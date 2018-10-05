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

        // Day=12min (80%) Night=3min (20%)
        float dayTime = 12; //min
        float nightTime = 3; //min

        float time = ((GameManager.CurrentDay + GameManager.GetDayPercentage() + 380) * GameManager.secondsPerDay / ((dayTime+nightTime)*60f)) % 1f;

        float dayPerc = dayTime / (dayTime + nightTime);
        float nightPerc = 1f - dayPerc;

        // 0time = zenit -> no offset 
        time = (time + 0) % 1f;
        // day=[0,dayPerc/2] v [1-dayPerc/2,1]
        // night=[dayPerc/2,dayPerc/2+nightPerc]

        // offset for easier calculations 
        time = (time - dayPerc/2 + 1f) % 1f;
        // day=[0,dayPerc]
        // night=[dayPerc,1]

        float rotation = 0;

        if(time < dayPerc)
        {
            rotation = 180f + (time / dayPerc) * 180;
        }
        else
        {
            rotation = (time / nightPerc) * 180;
        }
        rotation += 90f;


        /*dayPerc -= 0.4f;

        float offset = -20f;
        rotation += offset;

        if (rotation >= 360) rotation -= 360;
        if (rotation < 0) rotation += 360;

        float dayNight = 4f;
        float length = 150f;
        float max = (360 - length / dayNight);

        if (rotation > length) rotation = (rotation - length) / length + max;
        else rotation = rotation / length * max;

        rotation -= offset;*/

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
