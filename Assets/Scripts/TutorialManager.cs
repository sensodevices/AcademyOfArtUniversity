using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TutorialManager : MonoBehaviour {
	public TutorialUI mainUI;
	public Transform[] attImArray1, attImArray2, attImArray3, defArray, clapArray, healthText, castText;
	public Transform mask, finish;

	public int steps = 0;
	// Use this for initialization
	void Start () 
	{
		mainUI = FindObjectOfType<TutorialUI>();
        mainUI.gameObject.SetActive(false);
		mainUI.ParentManager (clapArray, mask);
		Step ();
	}
	
	// Update is called once per frame
	public void Step () 
	{
		if (steps == 0) 
		{
			mainUI.ParentManager (clapArray, mask);
		}

		if (steps == 1) 
		{
			mainUI.ParentManager (clapArray, mask);
			mainUI.ParentManager (castText, mask);
			Looper (clapArray, 1, false, 0);
			Looper (clapArray, 2, true, 0);
			StartCoroutine (Delay (3,clapArray [2].Find ("Tutorial3"), true));

		}

		if (steps == 2) 
		{
			mainUI.ParentManager (clapArray, mainUI.transform);
			mainUI.ParentManager (defArray, mask);
			mainUI.ParentManager (attImArray1, mask);
			Looper (clapArray, 3, false, 0);
			Looper (clapArray, 2, false, 0);

			Looper (attImArray1, 1, true, 0);

			Looper (defArray, 1, true, 0);
			Looper (defArray, 1, false, 4);
			StartCoroutine (Delay (4,defArray [2].Find ("Tutorial1"), false));
			StartCoroutine (Delay (4,attImArray1 [2].Find ("Tutorial1"), false));
			StartCoroutine (Delay (4,attImArray1 [2].Find ("Tutorial2"), true));
		}

		if (steps == 3) 
		{
			mainUI.ParentManager (attImArray1, mainUI.transform);
			mainUI.ParentManager (defArray, mainUI.transform);
			mainUI.ParentManager (castText, mainUI.transform);
			mask.gameObject.SetActive (false);
			StartCoroutine (Delay (2,mask, true));
			Looper (attImArray1, 1, false, 0);
			Looper (attImArray1, 2, false, 0);
			StartCoroutine (Delay (3,attImArray1 [2].Find ("Tutorial2"), false));
			StartCoroutine (DelayStep (2));
		}

		if (steps == 4) 
		{
			mainUI.ParentManager (attImArray2, mask);
			Looper (attImArray2, 1, true, 0);
		}

		if (steps == 5) 
		{
			mask.gameObject.SetActive (false);
			StartCoroutine (Delay (2,mask, true));
			mainUI.ParentManager (attImArray2, mainUI.transform);
			Looper (attImArray2, 1, false, 0);
			StartCoroutine (DelayStep (2));
		}

		if (steps == 6) 
		{
			mainUI.ParentManager (attImArray3, mask);
			Looper (attImArray3, 1, true, 0);
		}

		if (steps == 7) 
		{
			mask.gameObject.SetActive (false);
			mainUI.ParentManager (attImArray3, mainUI.transform);
			Looper (attImArray3, 1, false, 0);
		}

		if (steps == 8) 
		{
			mask.gameObject.SetActive (true);
			mainUI.ParentManager (defArray, mask);
			Looper (defArray, 2, true, 0);
		}

		if (steps == 9) 
		{
			mask.gameObject.SetActive (false);
			finish.gameObject.SetActive (true);
		}

	}

	private void Looper(Transform[] array, int no, bool choice, float delay)
	{
		StartCoroutine (Delay (delay,array [2].Find ("Tutorial" + no.ToString()), choice));
	}

	private IEnumerator Delay(float delay, Transform show, bool active)
	{
		yield return new WaitForSeconds(delay);
		show.gameObject.SetActive(active);
	}

	private IEnumerator DelayStep(float delay)
	{
		yield return new WaitForSeconds(delay);
		steps++;
		Step ();
	}
}
