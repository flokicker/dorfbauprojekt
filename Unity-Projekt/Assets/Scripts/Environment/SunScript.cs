using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunScript : MonoBehaviour {

	// Time in seconds for one day to pass
	private static float dayTime = 300f;

	private Light sunLight;
	private Color lightColor;

	// intensity = [0,1.5]
	private float maxIntensity, minIntensity;

	public bool isMoon;

	// Use this for initialization
	void Start () {
		if(isMoon)
		{
			maxIntensity= 0.9f;
			minIntensity = 0.6f;
		}
		else
		{
			maxIntensity= 1.7f;
			minIntensity = 0.3f;
		}

		sunLight = GetComponent<Light>();
		lightColor = sunLight.color;

        transform.position = NewPosition();
    }

    public Vector3 NewPosition()
    {
        float rotation = (GameManager.DayOfYear/365f)/1f * 360f;

        float offset = 0f;
        rotation += offset;

        if (rotation >= 360) rotation -= 360;
        if (rotation < 0) rotation += 360;

        float dayNight = 6f;
        float length = 50f;
        float max = (360 - length / dayNight);

        if (rotation > length) rotation = (rotation - length) / length + max;
        else rotation = rotation / length * max;

        rotation -= offset;

        if (isMoon) rotation += 180;

        rotation = Mathf.Deg2Rad * rotation;
        return new Vector3(Mathf.Sin(rotation), Mathf.Cos(rotation), 0) * 200;
    }
	
	// Update is called once per frame
	void Update () {
        // Rotate light
        //transform.RotateAround(Vector3.zero,Vector3.right,360f*Time.deltaTime/dayTime);
        transform.position = Vector3.Lerp(transform.position, NewPosition(), Time.deltaTime*2f);

        transform.LookAt(Vector3.zero);

		// calculate sun intensity for day/night cycle
		float rotX = transform.rotation.eulerAngles.x;
		if(rotX < 20)
			sunLight.intensity = rotX/20f*(maxIntensity-minIntensity) + minIntensity;
		else if(rotX < 160)
			sunLight.intensity = maxIntensity;
		else if(rotX < 180)
			sunLight.intensity = (180f-rotX)/20f*(maxIntensity-minIntensity) + minIntensity;
		else sunLight.intensity = minIntensity;
		
		if(!isMoon)
		{
			RenderSettings.ambientIntensity = sunLight.intensity*0.9f+0.05f;
            int day = GameManager.DayOfYear;
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
