using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyTutorial : PauseManager {
	public bool PlayerIsAttacking;
	private bool Alive = true;
	EnemyCombat EC;
	// Use this for initialization
	void Start () {
		EC = GetComponent<EnemyCombat> ();
		FindObjectOfType<Tutorial>().attackEvent += IsPlayerAttacking;
		FindObjectOfType<EnemyCombat>().damagedEvent += Damage;
		if (!isPaused)
			TogglePause ();
		StartCoroutine (Clap ());
	}
	
	// Update is called once per frame
	void Update () {
	}

	void TakeAction()
	{
		if (Alive && !isPaused) {
			int randomNumber = Random.Range (0, 100);
			if (EC.clapped) 
			{
				EC.Attack ();
			}
            StartCoroutine(Clap());
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
		PlayerIsAttacking = false;
		Alive = alive;
	}
}
