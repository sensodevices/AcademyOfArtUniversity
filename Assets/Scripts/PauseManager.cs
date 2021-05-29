using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour {
	public static bool isPaused = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.P)) {
			TogglePause ();
		}
	}

	public void TogglePause()
	{
		if (isPaused) 
		{
			isPaused = false;
			enemyTutorial EB = FindObjectOfType < enemyTutorial> ();
			StartCoroutine (EB.Clap ());
		}

		else
			isPaused = true;
	}
		



}
