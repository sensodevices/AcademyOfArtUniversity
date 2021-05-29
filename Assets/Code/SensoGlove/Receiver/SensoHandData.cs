using UnityEngine;
using SimpleJSON;

///
/// @brief Enumeration for all Senso position types
public enum ESensoPositionType
{
	Unknown, RightHand, LeftHand, PositionsCount
};

///
/// @brief Enumeration for fingers
public enum ESensoFingerType
{
	Thumb, Index, Middle, Third, Little
};

///
/// @brief Implements a container for Senso hand pose information.
///
public class SensoHandData {
    public ESensoPositionType handType { get; private set; }

	public Vector3 palmPosition;
	public Quaternion palmRotation;
	public Quaternion wristRotation;
	public Vector3[] fingerAngles = new Vector3[5];

    public Quaternion shoulderRotation;
    public bool hasShoulder;
    public Quaternion clavicleRotation;
    public bool hasClavicle;

    ///
    /// @brief Default constructor
    ///
    public SensoHandData () {
        handType = ESensoPositionType.Unknown;
        palmRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        wristRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        for (int i = 0; i < 5; ++i)
            fingerAngles[i] = new Vector3(0.0f, 0.0f, 0.0f);

        hasShoulder = false;
        shoulderRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        hasClavicle = false;
        clavicleRotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
    }

	///
	/// @brief Copy constructor
	///
	public SensoHandData (SensoHandData old) {
        handType = old.handType;
		palmPosition = old.palmPosition;
		palmRotation = old.palmRotation;
		wristRotation = old.wristRotation;
		for (int i = 0; i < 5; ++i) 
			fingerAngles[i] = old.fingerAngles[i];

        hasShoulder = old.hasShoulder;
        if (hasShoulder) shoulderRotation = old.shoulderRotation;

        hasClavicle = old.hasClavicle;
        if (hasClavicle) clavicleRotation = old.clavicleRotation;
    }

	///
	/// @brief Parses JSON node into internal properties
	///
	public void parseJSONNode (JSONNode data) {
		JSONArray anArr;

        var aType = data["type"].Value;
        if (aType.Equals("rh")) handType = ESensoPositionType.RightHand;
        else if (aType.Equals("lh")) handType = ESensoPositionType.LeftHand;
        else handType = ESensoPositionType.Unknown;

        // Wrist parsing
        anArr = data["wrist"]["quat"].AsArray;
		arrToQuat(ref anArr, ref wristRotation);

		// Palm parsing
		var palmNode = data["palm"];
		anArr = palmNode["quat"].AsArray;
		arrToQuat(ref anArr, ref palmRotation);
		anArr = palmNode["pos"].AsArray;
		arrToVec3(ref anArr, ref palmPosition);
        
		// Fingers parsing
		JSONArray fingersJArr = data["fingers"].AsArray;
		for (int i = 0; i < 5; ++i) {
			anArr = fingersJArr[i]["ang"].AsArray;
			arrToFingerAngles(ref anArr, ref fingerAngles[i]);
		}

        // Shoulder parsing
        if (data.KeyExists("shoulder"))
        {
            var shoulderNode = data["shoulder"];
            anArr = shoulderNode["quat"].AsArray;
            arrToQuat(ref anArr, ref shoulderRotation);
            hasShoulder = true;
        }
        else hasShoulder = false;

        // Clavicle parsing
        if (data.KeyExists("clavicle"))
        {
            var clavicleNode = data["clavicle"];
            anArr = clavicleNode["quat"].AsArray;
            arrToQuat(ref anArr, ref clavicleRotation);
            hasClavicle = true;
        }
        else hasClavicle = false;
    }

	static private void arrToQuat (ref JSONArray arr, ref Quaternion quat) {
		quat.w = arr[0].AsFloat;
		quat.x = arr[1].AsFloat;
		quat.y = -arr[3].AsFloat;
		quat.z = arr[2].AsFloat;
	}
	static private void arrToVec3 (ref JSONArray arr, ref Vector3 vec) {
		vec.x = arr[0].AsFloat;
		vec.y = arr[2].AsFloat;
		vec.z = arr[1].AsFloat;
	}

	static private void arrToFingerAngles (ref JSONArray arr, ref Vector3 angles) {
		angles.x = 0.0f;
		angles.y = arr[1].AsFloat * Mathf.Rad2Deg;
		angles.z = arr[0].AsFloat * Mathf.Rad2Deg;
	}

    public string ToJSON()
    {
        JSONClass baseNode = new JSONClass();
        if (handType == ESensoPositionType.RightHand) baseNode.Add("type", "rh");
        else if (handType == ESensoPositionType.LeftHand) baseNode.Add("type", "lh");
        else baseNode.Add("type", "unknown");

        JSONClass wristObj = new JSONClass();
        wristObj.Add("quat", quatToArr(wristRotation));
        baseNode.Add("wrist", wristObj);

        JSONClass palmObj = new JSONClass();
        palmObj.Add("quat", quatToArr(palmRotation));
        palmObj.Add("pos", vecToArr(palmPosition));
        baseNode.Add("palm", palmObj);

        JSONArray fingersArr = new JSONArray();
        JSONClass fingerObj;
        for (int i = 0; i < 5; ++i)
        {
            fingerObj = new JSONClass();
            fingerObj.Add("ang", fingerAnglesToArr(fingerAngles[i]));
            fingersArr.Add(fingerObj);
        }
        baseNode.Add("fingers", fingersArr);

        return baseNode.ToString();
    }

    static private JSONArray quatToArr(Quaternion quat)
    {
        var resArr = new JSONArray();
        resArr.Add(new JSONData(quat.w));
        resArr.Add(new JSONData(quat.x));
        resArr.Add(new JSONData(quat.y));
        resArr.Add(new JSONData(quat.z));
        return resArr;
    }

    static private JSONArray vecToArr(Vector3 vec)
    {
        var resArr = new JSONArray();
        resArr.Add(new JSONData(vec.x));
        resArr.Add(new JSONData(vec.y));
        resArr.Add(new JSONData(vec.z));
        return resArr;
    }

    static private JSONArray fingerAnglesToArr(Vector3 angles)
    {
        var resArr = new JSONArray();
        resArr.Add(new JSONData(angles.z * Mathf.Deg2Rad));
        resArr.Add(new JSONData(angles.y * Mathf.Deg2Rad));
        return resArr;
    }
}