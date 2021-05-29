using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Tutorial : CombatClass {
	TutorialManager tm;
	private bool clapTut, att1Tut, att2Tut, att3Tut;
	public delegate void AttackDel();
	public AttackDel attackEvent;
	SensoHand myHand;
	public Image castIm, defIm, attIm1, attIm2, attIm3
	,defHi, att1Hi, att2Hi, att3Hi;

	public Text castPowerText, healthText;

	public Transform l_origin, r_origin;
	private GameObject target;
    private bool needCalibration = true;

	// Use this for initialization
	void Start () {
		tm = FindObjectOfType<TutorialManager> ();
		target = GameObject.FindGameObjectWithTag("Enemy");
		shieldVFX = transform.Find("ShieldEffect").GetComponent<ParticleSystem>();
		castCD = false;
        FindObjectOfType<SensoManager>().ToggleHandleCalibration(true);
        StartCoroutine (Cooldown (1, CastCooldown, castIm));
	}
    public void StartTutorial()
    {
        tm.mainUI.gameObject.SetActive(true);
    }
	// Update is called once per frame
	void Update () {
        if (needCalibration)
        {
            FindObjectOfType<SensoManager>().ForceCalibration();
            needCalibration = false;
        }

        HighlightManager ();

		castPowerText.text = "Cast Power : " + (float)(Mathf.RoundToInt (castPower * 10)) / 10;
		healthText.text = "Health : " + health;


		switch (combatEnumState) {
		case CombatEnum.Idle:
			break;

		case CombatEnum.Clap:
			clapped = true;

			//mainUI.TimeUpdater (5f);

			break;

		case CombatEnum.Cast:
			clapped = false;
			castPower += castPowerIn;
			StartCoroutine (Cooldown (castTime, CastCooldown, castIm));
			combatEnumState = CombatEnum.Idle;
			break;

		case CombatEnum.Defend:
			StartCoroutine (Cooldown (defTime, DefendCooldown, defIm));
			clapped = false;
			castPower--;
			StartCoroutine(tempDef (target));
			shieldVFX.Play ();
			combatEnumState = CombatEnum.Idle;
			//Acitvates Defend for 0.5 Second
			break;

		case CombatEnum.Attack:
			attackEvent ();
			combatEnumState = CombatEnum.Idle;
			//Attack enemy by conjuring spell towards them. 
			break;
		}

	}

	private void MagicAttack(int power, bool left)
	{
		combatEnumState = CombatEnum.Attack;
        if(left)
			SpawnProjectile (power, l_origin, target, 1);
        else
			SpawnProjectile (power, r_origin, target, 1);
    }

	public void SubscribeEvents(SensoHand hand)
	{
		hand.OnGestureStart += onGestureStart;
		hand.OnGestureEnd += onGestureEnd;
		hand.OnClap += onClap;
		myHand = hand;

	}

	public void UnsubscribeEvents(SensoHand hand)
	{
		hand.OnGestureStart -= onGestureStart;
	}

	public void onClap(object sender, SensoGestureEventArgs args)
	{
		if (castCD && (tm.steps == 0 || tm.steps == 1 || tm.steps ==9))
		{
			combatEnumState = CombatEnum.Cast;
			castCD = false;
            if (tm.steps == 1)
				StartCoroutine (Cooldown (4, AttackCooldown, attIm1));
			if (tm.steps == 9)
				SceneManager.LoadScene ("Game");
			tm.steps++;
			tm.Step();
		}
	}
	public void onGestureStart(object sender, SensoGestureEventArgs args)
	{
        
		if (combatEnumState != CombatEnum.Cast) {
			if (args.GestureId == 2 && defHi.enabled && tm.steps == 8) {
				if (isPaused)
					TogglePause ();
				defHi.enabled = false;
                args.Hand.ToggleVibrateAll(8);
				combatEnumState = CombatEnum.Defend;
				tm.steps = 9;
				tm.Step ();
				StartCoroutine (Cooldown (3, CastCooldown, castIm));

			}
				
			if (args.GestureId == 5 && att1Hi.enabled && tm.steps == 2) {
                if (args.Hand.HandType == ESensoPositionType.LeftHand) MagicAttack(1, true);
                else MagicAttack(1, false);
                VibrateFinger(ESensoFingerType.Index, args);
                VibrateFinger(ESensoFingerType.Little, args);
                att1Hi.enabled = false;
				tm.steps = 3;
				tm.Step ();
				castPower = 2;
				StartCoroutine (Cooldown (attackTime, AttackCooldown, attIm1));
			}

			if (args.GestureId == 1 && att2Hi.enabled && tm.steps == 4) {
                if (args.Hand.HandType == ESensoPositionType.LeftHand) MagicAttack(2, true);
                else MagicAttack(2, false);
                VibrateFinger(ESensoFingerType.Index, args);
                VibrateFinger(ESensoFingerType.Middle, args);
                att2Hi.enabled = false;
				tm.steps = 5;
				tm.Step ();
				castPower = 3;
				StartCoroutine (Cooldown (attackTime, AttackCooldown, attIm2));
			}

			if (args.GestureId == 4 && att3Hi.enabled && tm.steps == 6) {
                if (args.Hand.HandType == ESensoPositionType.LeftHand) MagicAttack(3, true);
                else MagicAttack(3, false);
                VibrateFinger(ESensoFingerType.Index, args);
                VibrateFinger(ESensoFingerType.Thumb, args);
                tm.steps = 7;
				tm.Step ();
				TogglePause ();
				att3Hi.enabled = false;
				castPower = 2;
				StartCoroutine (Cooldown (attackTime, AttackCooldown, attIm3));
			}
		}
	}
	public void onGestureEnd(object sender, SensoGestureEventArgs args)
	{
        Release();
		combatEnumState = CombatEnum.Idle;
        args.Hand.ToggleVibrateAll(0);
    }


	void VibrateFinger(ESensoFingerType finger, SensoGestureEventArgs args)
	{
        args.Hand.GetFingerTip(finger).Vibrate();
    }

	public override void Defend (GameObject target, float time)
	{
		base.Defend (target, time);
		attackEvent ();
	}

	private void HighlightManager()
	{			

		defHi.enabled = att1Hi.enabled = att2Hi.enabled = att3Hi.enabled = false;
		if (castPower > .9f) {
			if(defCD)
				defHi.enabled = true;
			if (attackCD) {
				att1Hi.enabled = true;
				if (castPower > 1.9f) {
					att2Hi.enabled = true;
				}

				if (castPower > 2.9f) {
					att3Hi.enabled = true;
				}
			}
		}
	}

	IEnumerator tempDef(GameObject target)
	{
		yield return new WaitForSeconds (0.2f);
		for (int j = 0; j < 3; j++) {
			Collider[] hits = Physics.OverlapSphere (transform.position, 10);

			for (int i = 0; i < hits.Length; i++) {
				GameObject obj = hits [i].transform.gameObject;
				if (obj.GetComponent<Projectile> () != null) {
					Projectile bullet = obj.GetComponent<Projectile> ();
					if (bullet.target != target) {
						//bullet.deflected = false;
						bullet.target = target;
						//obj.GetComponent<Rigidbody> ().velocity *= -3;// (obj.transform.forward * -100, ForceMode.Impulse);
						obj.GetComponent<Rigidbody> ().AddForce(transform.forward * 200, ForceMode.Impulse);
						obj.GetComponent<Projectile>().damage = 50;
						obj.GetComponent<SphereCollider>().radius = 2;
					}
				}
			}

			yield return new WaitForSeconds (0.1f);
		}
	}



}
