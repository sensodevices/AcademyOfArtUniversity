using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class SensoManager : MonoBehaviour 
{
    public enum ESensoManagerState
    {
        Calibrate0,
        Calibrate1,
        Calibrate2,
        Normal,
        Count
    }

    private bool m_rightEnabled;
    private bool m_leftEnabled;
    public Transform[] Hands;

    private LinkedList<SensoHand> sensoHands;

    private string GetSensoHost () {
        return searchCLIArgument("sensoHost", SensoHost);
    }
    private Int32 GetSensoPort () {
        var portStr = searchCLIArgument("sensoPort", SensoPort.ToString());
        Int32 port;
        if (!Int32.TryParse(portStr, out port)) {
            port = SensoPort;
        }
        return port;
    }
    public string SensoHost = "127.0.0.1"; //!< IP address of the Senso Server instane
    public Int32 SensoPort = 53450; //!< Port of the Senso Server instance

    private SensoNetworkThread sensoThread;

    public Transform orientationSource;
    private DateTime orientationNextSend;
    private double orientationSendEveryMS = 100.0f;

    public bool RecordMovements = false;
    public string RecordFile = "movements.txt";
    private FileStream m_recordStream;

    private ESensoManagerState[] currentState = { ESensoManagerState.Normal, ESensoManagerState.Normal };
    private SensoTutorialReplayer tutorialComp;
    public Material TutorialMaterial;
    private bool m_handleCalibration = true;
    private SensoHandData StartHandData;

    public Renderer[] HandRenderers;
    private List<Material> StartMaterials;

    private void Awake()
    {
        saveStartMaterial();
    }

    void Start () {
        if (Hands != null && Hands.Length > 0) {
            sensoHands = new LinkedList<SensoHand>();
            for (int i = 0; i < Hands.Length; ++i) {
                Component[] components = Hands[i].GetComponents(typeof(SensoHand));
                for (int j = 0; j < components.Length; ++j) {
                    var hand = components[j] as SensoHand;
                    sensoHands.AddLast(hand);
                    if (!m_rightEnabled && hand.HandType == ESensoPositionType.RightHand) m_rightEnabled = true;
                    else if (!m_leftEnabled && hand.HandType == ESensoPositionType.LeftHand) m_leftEnabled = true;
                }
            }
        }
        sensoThread = new SensoNetworkThread(GetSensoHost(), GetSensoPort());
        sensoThread.StartThread();
        BroadcastMessage("SetSensoManager", this);

        tutorialComp = GetComponent<SensoTutorialReplayer>();
        if (tutorialComp != null) // Perform very start calibration
        {
            tutorialComp.Switch(ESensoPositionType.RightHand, 0);
            StartHandData = tutorialComp.GetCurrentData(ESensoPositionType.RightHand);
        }
    }

    void OnDisable () {
        sensoThread.StopThread();
        if (m_recordStream != null) m_recordStream.Close();
    }

    void FixedUpdate() {
        SensoHandData leftSample = null, rightSample = null;

        if (RecordMovements && m_recordStream == null)
            m_recordStream = File.OpenWrite(Application.persistentDataPath + "/" + RecordFile);

        /// Right hand
        if (m_rightEnabled)
        {
            if (currentState[0] == ESensoManagerState.Calibrate0 && tutorialComp != null)
            {
                rightSample = new SensoHandData(StartHandData);
            }
            else if ((currentState[0] == ESensoManagerState.Calibrate1 || currentState[0] == ESensoManagerState.Calibrate2) && tutorialComp != null)
            {
                rightSample = tutorialComp.GetCurrentData(ESensoPositionType.RightHand);
            }
            else
            {
                rightSample = sensoThread.GetSample(ESensoPositionType.RightHand);
                if (RecordMovements && rightSample.handType != ESensoPositionType.Unknown)
                {
                    byte[] sampleBytes = Encoding.ASCII.GetBytes(rightSample.ToJSON() + "\n");
                    m_recordStream.Write(sampleBytes, 0, sampleBytes.Length);
                }
            }
        }

        /// Left hand
        if (m_leftEnabled)
        {
            if (currentState[1] == ESensoManagerState.Calibrate0 && tutorialComp != null)
            {
                leftSample = new SensoHandData(StartHandData);
            }
            else if ((currentState[1] == ESensoManagerState.Calibrate1 || currentState[1] == ESensoManagerState.Calibrate2) && tutorialComp != null)
            {
                leftSample = tutorialComp.GetCurrentData(ESensoPositionType.LeftHand);
            }
            else
            {
                leftSample = sensoThread.GetSample(ESensoPositionType.LeftHand);
                if (RecordMovements && leftSample.handType != ESensoPositionType.Unknown)
                {
                    byte[] sampleBytes = Encoding.ASCII.GetBytes(leftSample.ToJSON() + "\n");
                    m_recordStream.Write(sampleBytes, 0, sampleBytes.Length);
                }
            }
        }

        if (sensoHands != null) {
            foreach (var hand in sensoHands) {
                if (hand.HandType == ESensoPositionType.RightHand && m_rightEnabled) {
                    hand.SensoPoseChanged(rightSample);
                } else if (hand.HandType == ESensoPositionType.LeftHand && m_leftEnabled) {
                    hand.SensoPoseChanged(leftSample);
                }
            }
        }
        
        // Gestures
        var gestures = sensoThread.GetGestures();
        if (gestures != null) {
            for (int i = 0; i < gestures.Length; ++i) {
                if (gestures[i].Type == ESensoGestureType.PinchStart || gestures[i].Type == ESensoGestureType.PinchEnd) {
                    fingerPinch(gestures[i].Hand, gestures[i].Fingers[0], gestures[i].Fingers[1], gestures[i].Type == ESensoGestureType.PinchEnd);
                } else
                { // Unnamed experimental gestures ... and clap ... and calibration gestures
                    handleGesture(gestures[i].Hand, gestures[i].Type);
                }
            }
        }

        if (orientationSource != null && DateTime.Now >= orientationNextSend) {
            sensoThread.SendHMDOrientation(orientationSource.transform.localEulerAngles.y);
            orientationNextSend = DateTime.Now.AddMilliseconds(orientationSendEveryMS);
        }
    }

	///
	/// @brief Send vibration command to the server
	///
    public void SendVibro(ESensoPositionType hand, ESensoFingerType finger, ushort duration, byte strength)
    {
        sensoThread.VibrateFinger(hand, finger, duration, strength);
    }

    ///
    /// @brief Searches for the parameter in arguments list
    ///
    private static string searchCLIArgument (string param, string def = "") {
        if (Application.platform == RuntimePlatform.Android) {
            return def;
        }
        var args = System.Environment.GetCommandLineArgs();
        int i;
        string[] searchArgs = { param, "-" + param, "--" + param };

        for (i = 0; i < args.Length; ++i) {
            if (Array.Exists(searchArgs, elem => elem.Equals(args[i])) && args.Length > i + 1 ) {
                return args[i + 1];
            }
        }
        return def;
    }

    /// Events
    public void fingerPinch(ESensoPositionType handType, ESensoFingerType finger1Type, ESensoFingerType finger2Type, bool stop = false)
    {
        int handInd = (int)handType - (int)ESensoPositionType.RightHand;
        if (currentState[handInd] != ESensoManagerState.Normal) return;
        SensoHand aHand = null;
        foreach (var hand in sensoHands) 
            if (hand.HandType == handType) {
                aHand = hand;
                break;
            }
        
        if (aHand != null) {
            aHand.TriggerPinch(finger1Type, finger2Type, stop);
        }
    }

    public void handleGesture(ESensoPositionType handType, ESensoGestureType type)
    {
        int handInd = (int)handType - (int)ESensoPositionType.RightHand;
        if (handType == ESensoPositionType.RightHand && !m_rightEnabled) return;
        if (handType == ESensoPositionType.LeftHand && !m_leftEnabled) return;
        if (currentState[handInd] != ESensoManagerState.Normal && (type < ESensoGestureType.Calibrate1 || type > ESensoGestureType.Calibrate3)) return;
        SensoHand aHand = null;
        foreach (var hand in sensoHands)
            if (hand.HandType == handType)
            {
                aHand = hand;
                break;
            }

        if (aHand != null)
        {
            if (type == ESensoGestureType.Clap)
            {
                aHand.TriggerClap();
            } else if (tutorialComp != null && type >= ESensoGestureType.Calibrate1 && type <= ESensoGestureType.Calibrate3 
                && (m_handleCalibration || (currentState[handInd] >= ESensoManagerState.Calibrate0 && currentState[handInd] <= ESensoManagerState.Calibrate2)))
            {                
                if (type == ESensoGestureType.Calibrate1 && currentState[handInd] != ESensoManagerState.Calibrate1)
                {
                    setTutorialMaterial(handType, true);
                    tutorialComp.Switch(handType, 0);
                    currentState[handInd] = ESensoManagerState.Calibrate1;
                }
                else if (type == ESensoGestureType.Calibrate2)
                {
                    tutorialComp.Switch(handType, 1);
                    currentState[handInd] = ESensoManagerState.Calibrate2;
                }
                else if (type == ESensoGestureType.Calibrate3)
                {
                    setTutorialMaterial(handType, false);
                    currentState[handInd] = ESensoManagerState.Normal;
                    if (currentState[0] == ESensoManagerState.Normal && currentState[1] == ESensoManagerState.Normal) FindObjectOfType<Tutorial>().StartTutorial();
                    ToggleHandleCalibration(false);
                }
            }
            else
            {
                int gestureInd = ((int)type - (int)ESensoGestureType.Gesture0Start);
                bool stop = (gestureInd % 2) == 1;
                gestureInd = (int)Math.Floor((double)gestureInd / 2.0);
                aHand.TriggerGesture(gestureInd, stop);
            }
        }
    }

    private void setTutorialMaterial(ESensoPositionType handType, bool tutorial)
    {
        int handInd = (int)handType - (int)ESensoPositionType.RightHand;
        if (handInd >= HandRenderers.Length) return;
        if (tutorial)
        {
            //for (int i = 0; i < HandRenderers.Length; ++i)
                HandRenderers[handInd].material = TutorialMaterial;
        } else
        {
            //List<Material>.Enumerator matEnum = StartMaterials.GetEnumerator();
            //for (int i = 0; i < HandRenderers.Length; ++i)
            //{
                //matEnum.MoveNext();
                HandRenderers[handInd].material = StartMaterials[handInd];
            //}
        }
    }

    private void saveStartMaterial()
    {
        StartMaterials = new List<Material>(HandRenderers.Length);
        for (int i = 0; i < HandRenderers.Length; ++i)
        {
            StartMaterials.Add(HandRenderers[0].material);
        }
    }

    public void ToggleHandleCalibration(bool toggle)
    {
        m_handleCalibration = toggle;
    }

    public void ForceCalibration()
    {
        if (m_rightEnabled)
        {
            setTutorialMaterial(ESensoPositionType.RightHand, true);
            currentState[0] = ESensoManagerState.Calibrate0;
        }
        if (m_leftEnabled) { 
            setTutorialMaterial(ESensoPositionType.LeftHand, true);
            currentState[1] = ESensoManagerState.Calibrate0;           
        }
    }
}