using System.Collections.Generic;
using SimpleJSON;

public enum ESensoGestureType
{
    Unknown,
    PinchStart,
    PinchEnd,
    Gesture0Start,
    Gesture0End,
    Gesture1Start,
    Gesture1End,
    Gesture2Start,
    Gesture2End,
    Gesture3Start,
    Gesture3End,
    Gesture4Start,
    Gesture4End,
    Gesture5Start,
    Gesture5End,
    Gesture6Start,
    Gesture6End,
    Clap,
    Calibrate1,
    Calibrate2,
    Calibrate3,
    GesturesCount
};

public class SensoHandGesture
{
    public ESensoPositionType Hand { get; private set; }
    public ESensoFingerType[] Fingers { get; private set; }
    public ESensoGestureType Type { get; private set; }

    public SensoHandGesture (JSONNode data) {
        JSONArray arr = data["fingers"].AsArray;
        Fingers = new ESensoFingerType[arr.Count];
        for (int i = 0; i < arr.Count; ++i) {
            Fingers[i] = (ESensoFingerType)arr[i].AsInt;
        }

        string handStr = data["hand"].Value;
        if (handStr.Equals("rh")) Hand = ESensoPositionType.RightHand;
        else if (handStr.Equals("lh")) Hand = ESensoPositionType.LeftHand;
        else Hand = ESensoPositionType.Unknown;

        string typeStr = data["name"].Value;
        if (typeStr.Equals("pinch_start")) Type = ESensoGestureType.PinchStart;
        else if (typeStr.Equals("pinch_end")) Type = ESensoGestureType.PinchEnd;
        else if (typeStr.Equals("clap")) Type = ESensoGestureType.Clap;
        // Unnamed experimental gestures
        else if (typeStr.StartsWith("gesture_"))
        {
            int ind = 0;
            int last_ = typeStr.LastIndexOf('_');
            if (System.Int32.TryParse(typeStr.Substring(8, last_ - 8), out ind))
            {
                Type = (ESensoGestureType)((int)ESensoGestureType.Gesture0Start + (ind * 2));
                if (typeStr.EndsWith("end")) ++Type;
            }
        }
        else if (typeStr.StartsWith("calibrate_"))
        {
            int ind = 0;
            int last_ = typeStr.LastIndexOf('_');
            if (System.Int32.TryParse(typeStr.Substring(last_ + 1), out ind))
            {
                Type = (ESensoGestureType)((int)ESensoGestureType.Calibrate1 + ind - 1);
            }
        }
        else Type = ESensoGestureType.Unknown;
    }
};