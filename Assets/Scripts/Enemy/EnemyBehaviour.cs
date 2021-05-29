using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour {
	public bool PlayerIsAttacking;
	public bool Alive = true;
	EnemyCombat EC;
	// Use this for initialization
	void Start () {
		EC = GetComponent<EnemyCombat> ();
		FindObjectOfType<PlayerCombat>().attackEvent += IsPlayerAttacking;
		FindObjectOfType<PlayerCombat>().restartEvent += Restart;
		FindObjectOfType<EnemyCombat>().damagedEvent += Damage;
		StartCoroutine (Clap ());
	}
	
	// Update is called once per frame
	void Restart () {
		Alive = true;
		StartCoroutine (Clap ());

	}

	void TakeAction()
	{
		if (Alive) {
			int randomNumber = Random.Range (0, 100);
			if (EC.clapped) {
				if (EC.castPower > 0) {
					if (randomNumber < (40 + (EC.castPower * 10)) && EC.attackCD) {
						EC.Attack ();
					} else if (!PlayerIsAttacking) {
						EC.Cast ();				
					} else if (PlayerIsAttacking) {
						if (randomNumber < 40)
							EC.Cast ();
						else if(EC.castPower > 1)
							EC.Defend ();
						else
							EC.Cast ();
					}
				} else if (EC.castPower == 0) {
					if (!PlayerIsAttacking) {
						EC.Cast ();
					} else if (PlayerIsAttacking) {
						if (randomNumber < 40 && EC.castPower > 1)
							EC.Defend ();
						else
							EC.Cast ();
					}
				}
				// IF ALIVE

			}
			StartCoroutine ("Clap");
		}
	}

	virtual public IEnumerator Clap()
	{
		float wait = Random.Range (1, 3);
		yield return new WaitForSeconds (wait);
		if (Alive) {
			EC.combatEnumState = CombatClass.CombatEnum.Clap;
		}
		yield return new WaitForSeconds (.5f);
		TakeAction ();
	}

	private void IsPlayerAttacking()
	{
		PlayerIsAttacking = true;
	}

	public void Damage(bool alive)
	{
		Debug.Log ("DAMAGE");
		PlayerIsAttacking = false;
		Alive = alive;
	}
}
