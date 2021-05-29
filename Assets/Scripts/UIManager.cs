using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {
    public Transform parent;
    public float offset;
    public float timer;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Vector3 location = new Vector3(parent.position.x, parent.position.y + offset, parent.position.z);
       // this.transform.position = location;
       // UIVisibility();
    }

    public void TimeUpdater(float time)
    {
        timer = time;
    }
    private void UIVisibility()
    {
        if(timer > 0)
        {
            GetComponent<Canvas>().enabled = true;
            timer -= 1f * Time.deltaTime;
        }

        else
            GetComponent<Canvas>().enabled = false;

    }
}
