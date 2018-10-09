using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : MonoBehaviour
{
    public float woodAmount = 0, maxWood;
    public bool fireBurning;
    private ParticleSystem fireParticles;

    private Light fireLight;
    private float burningTime, burningStopTime, takeWoodTime;

    private bool IsBigCampfire;
    private float maxIntensity = 1f;
    private float lightRange = 2f;

    // Use this for initialization
    void Start () {
        maxWood = 30;
        fireParticles = transform.Find("Fire").GetComponent<ParticleSystem>();
        fireParticles.gameObject.SetActive(true);
        fireParticles.Stop();
        fireLight = transform.Find("FireLight").GetComponent<Light>();
        fireLight.gameObject.SetActive(false);
        fireLight.range = lightRange;
    }
	
	// Update is called once per frame
	void Update () {
        if (woodAmount > 0)
        {
            fireBurning = true;
            int season = GameManager.GetTwoSeason();
            woodAmount -= Time.deltaTime / (season == 0 ? 10 : 14);
        }
        if (woodAmount <= 0)
        {
            woodAmount = 0;
            StopFire();
        }
        if (woodAmount > maxWood)
            woodAmount = maxWood;

        takeWoodTime += Time.deltaTime;
        if (woodAmount < maxWood/2 && takeWoodTime >= 3f && woodAmount > 0.5f)
        {
            takeWoodTime = 0;
            List<GameResources> takeWd = new List<GameResources>();
            takeWd.Add(new GameResources("Holz", 1));
            if(GameManager.village.TakeResources(takeWd))
            {
                woodAmount += takeWd[0].Amount;
            }
        }

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
                fireLight.intensity = (1f-burningStopTime/4f)* maxIntensity;
        }
        else
        {
            fireLight.gameObject.SetActive(true);
            if(burningTime < 2)
                fireLight.intensity = burningTime/2f* maxIntensity;
            else
                fireLight.intensity = maxIntensity;
        }
	}

    public void StopFire()
    {
        if(!fireBurning) return;

        burningStopTime = 0;
        fireBurning = false;
    }

    public void SetIsBigCampfire(bool isBig)
    {
        IsBigCampfire = isBig;
        lightRange = isBig ? 3f : 2f;
        maxIntensity = isBig ? 3f : 2f;
    }

    public int Restock(int restock)
    {
        if(restock <= maxWood-woodAmount)
        {
            woodAmount += restock;
            return restock;
        }
        return 0;
    }
    public float GetHealthFactor()
    {
        if(maxWood == 0) return 0;
        return woodAmount / maxWood;
    }
}
