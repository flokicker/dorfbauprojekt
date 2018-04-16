using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunScript : MonoBehaviour {

	// Time in seconds for one day to pass
	private static float dayTime = 300f;

	private Light sunLight;
	private Color lightColor;

	// intensity = [0,1.5]
	private float maxIntensity;

	public bool isMoon;

	// Use this for initialization
	void Start () {
		if(isMoon)
		{
			 maxIntensity= 1f;
		}
		else
		{
			 maxIntensity= 1.3f;
		}

		sunLight = GetComponent<Light>();
		lightColor = sunLight.color;
	}
	
	// Update is called once per frame
	void Update () {
		// Rotate light
		transform.RotateAround(Vector3.zero,Vector3.right,360f*Time.deltaTime/dayTime);
		transform.LookAt(Vector3.zero);

		// calculate sun intensity for day/night cycle
		float rotX = transform.rotation.eulerAngles.x;
		if(rotX < 20)
			sunLight.intensity = rotX/20f*maxIntensity+0.2f;
		else if(rotX < 160)
			sunLight.intensity = maxIntensity;
		else if(rotX < 180)
			sunLight.intensity = (180f-rotX)/20f*maxIntensity+0.2f;
		else sunLight.intensity = 0.2f;
		
		if(!isMoon)
		{

			int day = GameManager.GetDay();
			if(day < 31 || day > 365-2*31)
				lightColor.b = 1f;
			else if(day < 59)
				lightColor.b = 1f-(day-31)/28f*0.5f;
			else if(day < 365-3*31)
				lightColor.b = 0.5f;
			else
				lightColor.b = 0.5f+(day-(365-3*31))/31f*0.5f;

			sunLight.color = lightColor;
		}
	}
}
