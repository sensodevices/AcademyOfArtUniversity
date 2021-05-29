using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ballsofpowerscript : MonoBehaviour {

    [SerializeField]
    private GameObject mainParticle;

    [SerializeField]
    private float[] turnSpeed;

    private void Update()
    {
		if (mainParticle != null) {
			this.gameObject.transform.RotateAround (mainParticle.transform.position, this.transform.up, turnSpeed [0]);
			this.gameObject.transform.RotateAround (mainParticle.transform.position, this.transform.right, turnSpeed [1]);
			this.gameObject.transform.RotateAround (mainParticle.transform.position, this.transform.forward, turnSpeed [2]);

		}

    }
}
