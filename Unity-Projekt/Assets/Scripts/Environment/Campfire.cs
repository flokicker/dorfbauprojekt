using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : MonoBehaviour
{
    public string DisplayName = "Feuerstelle";
    private float healthFactor;
    public bool fireBurning;
    private ParticleSystem fireParticles;

    private Light fireLight;
    private float burningTime, burningStopTime;

	// Use this for initialization
	void Start () {
        fireParticles = transform.Find("Fire").GetComponent<ParticleSystem>();
        fireParticles.Stop();
        fireLight = transform.Find("FireLight").GetComponent<Light>();
        fireLight.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (healthFactor > 0)
        {
            fireBurning = true;
            healthFactor -= 0.004f * Time.deltaTime;
        }
        if (healthFactor <= 0)
        {
            healthFactor = 0;
            StopFire();
        }
        if (healthFactor > 1)
            healthFactor = 1;

        if (fireBurning && !fireParticles.isPlaying)
            fireParticles.Play();
        if (!fireBurning && fireParticles.isPlaying) fireParticles.Stop();

        burningTime += Time.deltaTime;
        burningStopTime += Time.deltaTime;
        if(!fireBurning)
        {
            burningTime = 0;
            if(burningStopTime > 4)
                fireLight.gameObject.SetActive(false);
            else
                fireLight.intensity = (1f-burningStopTime/4f)*1f;
        }
        else
        {
            fireLight.gameObject.SetActive(true);
            if(burningTime < 2)
                fireLight.intensity = burningTime/2f*1f;
            else
                fireLight.intensity = 1f;
        }
	}

    public void StopFire()
    {
        if(!fireBurning) return;

        burningStopTime = 0;
        fireBurning = false;
    }

    public int Restock(int amountWood)
    {
        int n = (int)((1f - healthFactor) / 0.2f);
        if (n == 0) return 0;
        if (amountWood >= n)
        {
            healthFactor = 1;
            return n;
        }
        else
        {
            healthFactor += amountWood * 0.2f;
            return amountWood;
        }
    }
    public int GetHealthPercentage()
    {
        return (int)(healthFactor * 100f);
    }
}
