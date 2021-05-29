using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Credit : MonoBehaviour {
    float spriteAlpha = 0;
    // Use this for initialization
    public void Activate () {
        StartCoroutine(DamageIndicator());
    }
	
	// Update is called once per frame
	void Update () {
        Color temp = GetComponent<Image>().color;
        temp.a = spriteAlpha;
        GetComponent<Image>().color = temp;
	}

    private IEnumerator DamageIndicator()
    {
        spriteAlpha = 0;
        for (;;)
        {
            yield return new WaitForSeconds(0.05f);
            if (spriteAlpha < 0.9)
            {
                spriteAlpha += 0.05f;
            }
            else
            {
                spriteAlpha = 1;
                break;
            }
        }
    }
}
