using UnityEngine;
using System;

public class SensoEventArgs : EventArgs
{
    public SensoHand Hand;
    public SensoEventArgs (SensoHand aHand)
    {
        Hand = aHand;
    }
}

///
/// @brief Arguments for fingers pinch event
///
public class SensoPinchEventArgs : SensoEventArgs
{
  public SensoFingerTip[] Fingers { get; private set; }

  public SensoPinchEventArgs(SensoHand aHand, SensoFingerTip finger1, SensoFingerTip finger2) : base(aHand)
  {
        Fingers = new SensoFingerTip[2] { finger1, finger2 };
  }
}

///
/// @brief Arguments for hand grabbing
///
public class SensoGrabArgs : SensoEventArgs
{
    public int FingersCount { get; private set; }
    public SensoGrabArgs(SensoHand aHand, int fingersCount) : base (aHand)
    {
        FingersCount = fingersCount;
    }
}

///
/// @brief Argument for unnamed gestures
///
public class SensoGestureEventArgs : SensoEventArgs
{
    public int GestureId { get; private set; }
    public SensoGestureEventArgs(SensoHand aHand, int gestureId) : base(aHand)
    {
        GestureId = gestureId;
    }
}

public abstract class SensoHand : MonoBehaviour
{
    public ESensoPositionType HandType;
    protected SensoHandData latestSample;
    protected bool sampleChanged = false;

    WeakReference m_sensoManager;

    public SensoManager Senso {
        get {
            if (m_sensoManager.IsAlive) return m_sensoManager.Target as SensoManager;
            else return null;
        }
    }
    public bool Grabbing { get; private set; }

    ///////////// FUNCTIONS
    public void Start () 
    {
         BroadcastMessage("SetHand", this, SendMessageOptions.DontRequireReceiver);
    }

    public void SensoPoseChanged (SensoHandData aData)
    {
        latestSample = aData;
        sampleChanged = true;

        for (ESensoFingerType fingerType = ESensoFingerType.Thumb; fingerType <= ESensoFingerType.Little; ++fingerType) {
            var tip = GetFingerTip(fingerType);
            if (tip != null) tip.SetRelativePitch(aData.fingerAngles[(int)fingerType].z);
        }
    }

    public SensoPalm GetPalm () {
        var palm = GetComponentInChildren<SensoPalm>();
        return palm;
    }
    ///
    /// @brief Return FingerTip with specified type
    ///
    public SensoFingerTip GetFingerTip (ESensoFingerType fingerType) {
        var fingers = GetComponentsInChildren<SensoFingerTip>();
        foreach (var finger in fingers) {
            if (finger.FingerType == fingerType) return finger;
        }
        return null;
    }

    public void SetSensoManager (SensoManager manager) {
        m_sensoManager = new WeakReference(manager, false);
    }

    public void VibrateFinger (ESensoFingerType finger, ushort duration, byte strength) {
  
        if (m_sensoManager.IsAlive) {
            SensoManager man = m_sensoManager.Target as SensoManager;
            man.SendVibro(HandType, finger, duration, strength);
        }
    }

    public void ToggleVibrateAll(byte strength)
	{
        var fingers = this.GetComponentsInChildren<SensoFingerTip>();
        foreach (var finger in fingers) {
            finger.Vibrate(strength);
        }
	}

    // Events
    public event EventHandler<SensoPinchEventArgs> OnPinchStart = delegate {}; // Is fired when two fingers started pinching
    public event EventHandler<SensoPinchEventArgs> OnPinchEnd = delegate {}; // Is fired when two fingers stopped pinching
    public event EventHandler<SensoGestureEventArgs> OnClap = delegate { }; // Is fired when hand has clapped
    public event EventHandler<SensoGrabArgs> OnGrabStart = delegate {}; // Is fired when hand has began grabbing
	public event EventHandler<SensoGrabArgs> OnGrabEnd = delegate {}; // Is fired when hand has ended grabbing
    public event EventHandler<SensoGestureEventArgs> OnGestureStart = delegate { }; // Is fired when hand has began grabbing
    public event EventHandler<SensoGestureEventArgs> OnGestureEnd = delegate { }; // Is fired when hand has ended grabbing

    public void TriggerPinch (ESensoFingerType finger1Type, ESensoFingerType finger2Type, bool stop = false) 
    {
        SensoFingerTip finger1 = GetFingerTip(finger1Type < finger2Type ? finger1Type : finger2Type);
        SensoFingerTip finger2 = GetFingerTip(finger2Type > finger1Type ? finger2Type : finger1Type);
        var args = new SensoPinchEventArgs(this, finger1, finger2);
        if (!stop) {
            OnPinchStart(this, args);
        } else {
            OnPinchEnd(this, args);
        }
    }

    public void TriggerClap()
    {
        var args = new SensoGestureEventArgs(this, 0);
        OnClap(this, args);
    }

    public void TriggerGrab (int fingersCount, bool stop = false) {
        var args = new SensoGrabArgs(this, fingersCount);
        if (!stop) {
            Grabbing = true;
            OnGrabStart(this, args);
        } else {
            Grabbing = false;
            OnGrabEnd(this, args);
        }
    }

    public void TriggerGesture(int gestureId, bool stop)
    {
        var args = new SensoGestureEventArgs(this, gestureId);
        if (!stop)
        {
            OnGestureStart(this, args);
        }
        else
        {
            OnGestureEnd(this, args);
        }
    }
}