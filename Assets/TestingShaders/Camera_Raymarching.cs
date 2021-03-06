using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Camera_Raymarching : MonoBehaviour {





	private void OnPreRender()
	{


		//Shader.SetGlobalVector("_CamPos", this.transform.position);

		Shader.SetGlobalVector ("_CamRight", this.transform.right);
		Shader.SetGlobalVector ("_CamUp", this.transform.up);
		Shader.SetGlobalVector ("_CamForward", this.transform.forward);

		Shader.SetGlobalFloat ("_AspectRatio", (float)Screen.width / (float)Screen.height);

		Shader.SetGlobalFloat ("_FieldOfView", Mathf.Tan (Camera.main.fieldOfView * Mathf.Deg2Rad * 0.5f) * 2f);
	

	}




}
