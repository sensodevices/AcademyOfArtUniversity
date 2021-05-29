using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class SensoTutorialReplayer : MonoBehaviour {

    public string[] calibrateFiles = { "calibrate1", "calibrate2" };

    private LinkedList<string>[] m_recordedLines;
    private LinkedList<string>.Enumerator[] m_recordedLinesEnum;
    private int[] m_recordedLinesCount;

    private int[] m_currentTutorial = { 0, 0 };
    

	// Use this for initialization
	void Awake () {
        m_recordedLines = new LinkedList<string>[calibrateFiles.Length];
        m_recordedLinesCount = new int[calibrateFiles.Length];
        m_recordedLinesEnum = new LinkedList<string>.Enumerator[2];
        for (int i = 0; i < calibrateFiles.Length; ++i)
        {
            m_recordedLines[i] = new LinkedList<string>();
            m_recordedLinesCount[i] = 0;
            var aFile = Resources.Load(string.Format("Senso/{0}", calibrateFiles[i])) as TextAsset;
            //string absolutePath = string.Format("{0}/{1}", Application.dataPath, calibrateFiles[i]);

            if (aFile)
            {
                string[] lines = aFile.text.Split(new Char[] { '\n' });
                foreach (string line in lines)
                {
                    m_recordedLines[i].AddLast(line);
                    ++m_recordedLinesCount[i];
                }
            } else
            {
                Debug.LogError("Couldn't find calibrate file: " + calibrateFiles[i]);
            }
        }
	}
	
    public SensoHandData GetCurrentData(ESensoPositionType handType)
    {
        int handInd = (int)handType - (int)ESensoPositionType.RightHand;

        if (m_recordedLinesCount[m_currentTutorial[handInd]] == 0) return new SensoHandData();
        if (!m_recordedLinesEnum[handInd].MoveNext())
        {
            m_recordedLinesEnum[handInd] = m_recordedLines[m_currentTutorial[handInd]].GetEnumerator();
            m_recordedLinesEnum[handInd].MoveNext();
        }
        string curLine = m_recordedLinesEnum[handInd].Current;
        if (curLine.Length == 0) return GetCurrentData(handType);
        
        SensoHandData aData = new SensoHandData();
        try
        {
            JSONNode parsedData = JSON.Parse(curLine);
            aData.parseJSONNode(parsedData);
        }
        catch (Exception ex)
        {
            Debug.LogError("Cannot parse recorder line: " + curLine);
        }
        return aData;
    }

    public void Switch(ESensoPositionType handType, int ind)
    {
        int handInd = (int)handType - (int)ESensoPositionType.RightHand;

        if (ind < 0) ind = 0;
        else if (ind >= m_recordedLines.Length) ind = m_recordedLines.Length - 1;
        m_currentTutorial[handInd] = ind;
        m_recordedLinesEnum[handInd] = m_recordedLines[m_currentTutorial[handInd]].GetEnumerator();
    }
    
}
