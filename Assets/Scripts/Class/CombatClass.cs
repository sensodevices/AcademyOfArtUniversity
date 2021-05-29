using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CombatClass : PauseManager {
	public enum CombatEnum{Idle, Clap, Cast, Defend, Attack, Damaged, Dead};
	public CombatEnum combatEnumState;

	public float health = 10;
	public float castPower = 0;
	public float castPowerIn = 1;
	private float difIn = 1;
	public int minPower = 1;
	public int maxPower = 4;

	public float attackTime, castTime, defTime;
	public bool clapped = false;

	public GameObject magicProjectile, bullTemp;
	public ParticleSystem shieldVFX, clapVFX;
	public delegate void CooldownDelegate(bool i);

	public bool attackCD = true, castCD = true, defCD = true;

	public void CastCooldown(bool b) {castCD = b;}
	public void AttackCooldown(bool b) {attackCD = b;}
	public void DefendCooldown(bool b) {defCD = b;}
    // Use this for initialization


	public virtual void Restart()
	{
		health = 15;
		castPower = 0;
		combatEnumState = CombatEnum.Idle;
		//difIn *= 1.5f;
	}
	public virtual void Damage(int power)
	{
		health -= power;

	}

	public void SpawnProjectile(int power, Transform origin, GameObject target, float delay)
	{
		if(castPower >= power && castPower != 0)
		{
            Mathf.Clamp(castPower, minPower, maxPower);
			StartCoroutine(Cooldown (attackTime,  AttackCooldown, null));

			clapped = false;
			castPower -= power;

			if (magicProjectile != null) 
			{
                bullTemp = Instantiate (magicProjectile, origin.position, Quaternion.identity);
                bullTemp.transform.parent = origin;
				bullTemp.GetComponent<Projectile> ().Initialized (power,target,delay,difIn);


			}

			else
				Debug.LogWarning ("Magic Projectiles is NULL");
		}
	}

    public void Release()
    {
        if(bullTemp != null)
        {
            StartCoroutine(bullTemp.GetComponent<Projectile>().Release(5));
        }
    }

	public virtual void Defend(GameObject target, float time)
	{
		StartCoroutine (Delay (target, time));
	}

	IEnumerator Delay(GameObject target, float time)
	{
		yield return new WaitForSeconds (time);

		for (int j = 0; j < 5; j++) {
			Collider[] hits = Physics.OverlapSphere (transform.position, 10);

			for (int i = 0; i < hits.Length; i++) {
				GameObject obj = hits [i].transform.gameObject;
				if (obj.GetComponent<Projectile> () != null) {
					Projectile bullet = obj.GetComponent<Projectile> ();
					if (bullet.target != target) {
						bullet.deflected = false;
						bullet.target = target;
						obj.GetComponent<Rigidbody> ().velocity *= -2;// (obj.transform.forward * -100, ForceMode.Impulse);
						obj.GetComponent<Rigidbody> ().velocity += new Vector3 (0, Random.Range (-5, 5), 0);
					}
				}
			}

			yield return new WaitForSeconds (0.1f);
		}
	}

	public IEnumerator Cooldown(float time, CooldownDelegate cdDelegate, Image background)
	{
		float maxTime = time;
		float curTime = 0;
		if(background!= null)
			background.fillAmount = curTime;
		for (;;)
		{
			cdDelegate (false);
			yield return new WaitForSeconds (0.1f);
			if (time > 0)
			{
				time -= 0.1f;
				curTime += 0.1f;
				if(background!= null)
					background.fillAmount = curTime / maxTime;
			}

			else
			{
				cdDelegate(true);
				break;
			}
		}


	}
}
