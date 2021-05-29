using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EnemyCombat : CombatClass {
	public delegate void damagedDel(bool alive);
	public damagedDel damagedEvent;

    public Image healthIm;
    private float maxHealth;
	public Transform r_origin, l_origin;

    public Text cpText;
	private Animator anim;
	private GameObject target;

	void Start () {
		anim = GetComponentInChildren<Animator> ();
        maxHealth = health;
        shieldVFX = transform.Find("ShieldEffect").GetComponent<ParticleSystem>();
		target = GameObject.FindGameObjectWithTag ("Player");
		if(target.GetComponent<PlayerCombat>() != null)
			target.GetComponent<PlayerCombat>().restartEvent += Restart;
	}

	public override void Restart ()
	{
		base.Restart ();
		anim.SetBool("Dead", false);
		health *= 1.5f;
		maxHealth = health;
	}

	// Update is called once per frame
	void Update () {
        if(cpText != null)
            cpText.text = "Cast Power : " + castPower;
        anim.speed = 1;
		healthIm.fillAmount = health / maxHealth;

		switch (combatEnumState) {
		case CombatEnum.Idle:

			break;

		case CombatEnum.Clap:
			clapped = true;

			break;

		case CombatEnum.Cast:
			anim.SetBool ("Casting", true);
			clapped = false;
			castCD = false;
			castPower++;
			combatEnumState = CombatEnum.Idle;
		//Increase casting power
			break;

		case CombatEnum.Defend:
			StartCoroutine (Cooldown (defTime, DefendCooldown, null));
			Defend (target, 0.5f);
			shieldVFX.Play ();
		//Acitvates Defend for 0.5 Second
			break;

		case CombatEnum.Attack:
			combatEnumState = CombatEnum.Idle;
		//Attack enemy by conjuring spell towards them. 
			break;

		case CombatEnum.Damaged:
		anim.SetBool("Hit", true);
		combatEnumState = CombatEnum.Idle;

			//Attack enemy by conjuring spell towards them. 
			break;


		case CombatEnum.Dead:

		//Attack enemy by conjuring spell towards them. 
			break;
		} 

	}

	public void MagicAttack(int power)
	{
		if(power < 2)
			SpawnProjectile (power, r_origin, target, .55f);
		else
			SpawnProjectile (power, l_origin, target, .40f);
	}


	public override void Damage (int power)
	{
		base.Damage (power);
		combatEnumState = CombatEnum.Damaged;
		if (health > 0 && damagedEvent!=null)
			damagedEvent (true);
		else {
			damagedEvent (false);
			anim.SetBool("Dead", true);
		}


	}
	public void Attack()
	{
		combatEnumState = CombatEnum.Attack;
		int PowerPicker = Random.Range (minPower, (int)castPower);
		if (PowerPicker > maxPower) PowerPicker = maxPower;

		if (PowerPicker < 2) 
		{
			anim.SetBool ("Attacking", true);
			MagicAttack (PowerPicker);
		}

		else
			anim.SetBool ("Attacking2", true);
	}

	public void Cast()
	{
		combatEnumState = CombatEnum.Cast;
	}

	public void Defend()
	{
		combatEnumState = CombatEnum.Defend;
	}

	public void Done(string name)
	{
		anim.SetBool (name, false);
	}
}
