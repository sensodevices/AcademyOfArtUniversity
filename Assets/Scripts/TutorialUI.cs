using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TutorialUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Vector3 location = new Vector3(parent.position.x, parent.position.y + offset, parent.position.z);
       // this.transform.position = location;
       // UIVisibility();
    }

	public void ParentManager(Transform[] child, Transform parent)
    {
		for (int i = child.Length -1; i > -1; i--) {
			child [i].parent = parent;
			child [i].transform.SetAsFirstSibling();
		}
    }



}
