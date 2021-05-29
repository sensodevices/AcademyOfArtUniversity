using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PlayerCombat : CombatClass {
	public UIManager mainUI;
    public Image credit;

	public delegate void playerDel();
	public playerDel attackEvent;
	public playerDel restartEvent;

	SensoHand myHand;
	public Image castIm, defIm, attIm1, attIm2, attIm3, creditIm
	,defHi, att1Hi, att2Hi, att3Hi;
	public Text castPowerText, healthText;

    public Transform l_origin, r_origin;
    private GameObject target;

	private float spriteAlpha;

	public Transform[] damages;
	// Use this for initialization
	void Start () {
		target = GameObject.FindGameObjectWithTag("Enemy");
		shieldVFX = transform.Find("ShieldEffect").GetComponent<ParticleSystem>();
		clapVFX = transform.Find ("ClapVFX").GetComponent<ParticleSystem> ();
		restartEvent += Restart;
        
        mainUI = FindObjectOfType<UIManager>();

		FindObjectOfType<EnemyCombat>().damagedEvent += CreditScene;
        FindObjectOfType<SensoManager>().ToggleHandleCalibration(false);
    }

	public override void Restart ()
	{
		base.Restart ();
		health = 25;
	}
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Q)) {
			SceneManager.LoadScene (0);
		}

        if (combatEnumState != CombatEnum.Dead)
        {
            HighlightManager();
            DamageSprite();

            castPowerText.text = "Cast Power : " + (float)(Mathf.RoundToInt(castPower * 10)) / 10;
            healthText.text = "Health : " + health;


            switch (combatEnumState)
            {
			case CombatEnum.Idle:
				CreditScene (true);
                break;

            case CombatEnum.Clap:
                clapped = true;

                //mainUI.TimeUpdater (5f);

                break;

            case CombatEnum.Cast:
                clapped = false;
                clapVFX.Play();
                castPower += castPowerIn;
                StartCoroutine(Cooldown(castTime, CastCooldown, castIm));
                combatEnumState = CombatEnum.Idle;
                break;

            case CombatEnum.Defend:
                StartCoroutine(Cooldown(defTime, DefendCooldown, defIm));
                clapped = false;
                castPower -= .5f;
                Defend(target, .25f);
                shieldVFX.Play();
                combatEnumState = CombatEnum.Idle;
                //Acitvates Defend for 0.5 Second
                break;

            case CombatEnum.Attack:
                attackEvent();
                combatEnumState = CombatEnum.Idle;
                //Attack enemy by conjuring spell towards them. 
                break;

			case CombatEnum.Damaged:
				CreditScene (false);
				
                break;
            case CombatEnum.Dead:

                break;
            }
        }


	}

    private void CreditScene(bool dead)
    {
		if (!dead) {
			credit.gameObject.SetActive (true);
			credit.GetComponent<Credit> ().Activate ();
			combatEnumState = CombatEnum.Dead;
		} else {
			credit.gameObject.SetActive(false);
		}
    }
    private void MagicAttack(int power, bool left)
    {

        combatEnumState = CombatEnum.Attack;
        if (left)
			SpawnProjectile(power, l_origin, target, 1);
        else
			SpawnProjectile(power, r_origin, target, 1);
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
		if (castCD && combatEnumState != CombatEnum.Dead)
		{
			combatEnumState = CombatEnum.Cast;
		}
	}
	public void onGestureStart(object sender, SensoGestureEventArgs args)
	{
		if (combatEnumState != CombatEnum.Cast && combatEnumState != CombatEnum.Dead) {
			if (args.GestureId == 2 && defHi.enabled) {
				defHi.enabled = false;
				combatEnumState = CombatEnum.Defend;
			}

			if (args.GestureId == 5 && att1Hi.enabled) {
                if (args.Hand.HandType == ESensoPositionType.LeftHand) MagicAttack(1, true);
                else MagicAttack(1, false);
                VibrateFinger(ESensoFingerType.Index, args);
                VibrateFinger(ESensoFingerType.Little, args);
                att1Hi.enabled = false;
				StartCoroutine (Cooldown (attackTime, AttackCooldown, attIm1));
			}

			if (args.GestureId == 1 && att2Hi.enabled) {
                if (args.Hand.HandType == ESensoPositionType.LeftHand) MagicAttack(2, true);
                else MagicAttack(2, false);
                VibrateFinger(ESensoFingerType.Index, args);
                VibrateFinger(ESensoFingerType.Middle, args);
                att2Hi.enabled = false;
				StartCoroutine (Cooldown (attackTime, AttackCooldown, attIm2));
			}

			if (args.GestureId == 4 && att3Hi.enabled) {
                if (args.Hand.HandType == ESensoPositionType.LeftHand) MagicAttack(3, true);
                else MagicAttack(3, false);
                VibrateFinger(ESensoFingerType.Index, args);
                VibrateFinger(ESensoFingerType.Third, args);
                att3Hi.enabled = false;
				StartCoroutine (Cooldown (attackTime, AttackCooldown, attIm3));
			}

            if (args.GestureId == 6 && combatEnumState == CombatEnum.Dead)
            {
                SceneManager.LoadScene(0);
            }


        }

		else if (args.GestureId == 3)
		{
			StartCoroutine (RestartDelay ());
			StartCoroutine (Cooldown (2, AttackCooldown, creditIm));
		}

	}
	public void onGestureEnd(object sender, SensoGestureEventArgs args)
	{
        if (combatEnumState != CombatEnum.Dead)
        {
			
            Release();
            combatEnumState = CombatEnum.Idle;
            args.Hand.ToggleVibrateAll(0);
        }
    }

	private IEnumerator RestartDelay()
	{
		yield return new WaitForSeconds (2);
		restartEvent();
		Debug.Log ("Restart");
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

	public override void Damage (int power)
	{
        if (combatEnumState != CombatEnum.Dead)
        {
            base.Damage(power);
            StartCoroutine(DamageIndicator());
            if (health < 0)
            {
                health = 0;
                combatEnumState = CombatEnum.Damaged;
            }
        }
    }

	private IEnumerator DamageIndicator()
	{
		spriteAlpha = 1;
		for (;;) {
			yield return new WaitForSeconds (0.05f);
			if (spriteAlpha > 0.1) {
				spriteAlpha -= 0.1f;
			} else {
				spriteAlpha = 0;
				break;
			}
		}
	}

	private void DamageSprite()
	{
		Color temp = damages [0].GetComponent<SpriteRenderer> ().color;
		temp.a = spriteAlpha;
		for (int i = 0; i < damages.Length; i++) 
		{
			damages [i].GetComponent<SpriteRenderer> ().color = temp;
		}
	}




}
