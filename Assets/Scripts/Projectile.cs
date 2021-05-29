using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : PauseManager {
	[SerializeField] private GameObject[] cols = new GameObject[0];
	public GameObject target;
	public ParticleSystem end;
	int power = 0;
	public bool deflected = true, released = false;

    private float acc, offset, speed = 1.5f;
	private float delay;
	public int damage;

    // Use this for initialization
	public void Initialized (int _power, GameObject _target, float _delay, float difIncrease) {
		delay = _delay;
		target = _target;
		transform.LookAt(target.transform);
		StartCoroutine (Release (delay));
        Renderer tempMat = gameObject.GetComponent<Renderer> ();
		power = _power;
		cols [power-1].SetActive(true);
		transform.localRotation = Quaternion.Euler (0, 0, 0);
		damage = power;
		speed *= difIncrease;
        acc = 1f;
        offset = 1;
    }
	
	// Update is called once per frame
	void Update () {
		if (target != null) {
			if (target.tag == "Player") {
				if (!isPaused) {
					Move ();
				} else
					GetComponent<Rigidbody> ().isKinematic = true;
			} else {
				Move ();
			}
		}
	}

	private void Move()
	{
		GetComponent<Rigidbody> ().isKinematic = false;
		if (released) {
			if (acc < 10)
				acc += acc * Time.deltaTime;
			Vector3 velocity = transform.forward * speed * acc;

			if (acc > 9.9f)
				Destroy (gameObject);

			if (deflected)
				GetComponent<Rigidbody> ().velocity += velocity;

			if (target != null) {
				TargettingEnemy ();
			}
		}
	}

    private void TargettingEnemy()
    {
		Quaternion rotation = Quaternion.LookRotation ( target.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * 15);
    }

	public IEnumerator Release(float choice)
	{
        if (delay < .9f)
        {
            yield return new WaitForSeconds(delay);
            GetComponent<Rigidbody>().AddForce(target.transform.forward * -10, ForceMode.Impulse);
            transform.parent = null;
            released = true;
        }

        else if (choice > 3)
        {
            GetComponent<Rigidbody>().AddForce(target.transform.forward * -20, ForceMode.Impulse);
            transform.parent = null;
            released = true;
        }


    }

	private void OnCollisionEnter(Collision obj)
	{
		if (obj.gameObject == target) {
			cols [power-1].SetActive(false);
			end.Play ();
			Destroy (gameObject, .5f);
			if (obj.gameObject.GetComponent<PlayerCombat> () != null)
				obj.gameObject.GetComponent<PlayerCombat> ().Damage (damage);
		
			if (obj.gameObject.GetComponentInChildren<EnemyCombat> () != null)
				obj.gameObject.GetComponentInChildren<EnemyCombat> ().Damage (damage);
		} else if (obj.gameObject.name == "Bullet") {
			GetComponent<Rigidbody> ().AddForce (transform.position - obj.transform.position * 100, ForceMode.Force);
		}
	}
	private void OnTriggerEnter(Collider obj)
	{
		if (obj.gameObject.tag == "Tutorial" && target.tag == "Player") {
			TogglePause ();
			FindObjectOfType<TutorialManager> ().steps = 8;
			FindObjectOfType<TutorialManager> ().Step ();
		}
	}
}
