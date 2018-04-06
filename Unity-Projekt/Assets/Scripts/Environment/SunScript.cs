using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunScript : MonoBehaviour {

	public Light sunLight;

	// intensity = [0,1.5]
	private float maxIntensity = 1.5f;

	// Use this for initialization
	void Start () {
		sunLight = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround(Vector3.zero,Vector3.right,1f*Time.deltaTime);

		// calculate sun intensity for day/night cycle
		float rotX = transform.rotation.eulerAngles.x;
		if(rotX < 20)
			sunLight.intensity = rotX/20f*maxIntensity;
		else if(rotX < 160)
			sunLight.intensity = maxIntensity;
		else if(rotX < 180)
			sunLight.intensity = (180f-rotX)/20f*maxIntensity;
		else sunLight.intensity = 0;

		transform.LookAt(Vector3.zero);
	}
}
