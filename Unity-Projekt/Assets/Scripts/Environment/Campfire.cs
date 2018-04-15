﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : MonoBehaviour
{
    public string DisplayName = "Feuerstelle";
    public float woodAmount, maxWood;
    public bool fireBurning;
    private ParticleSystem fireParticles;

    private Light fireLight;
    private float burningTime, burningStopTime;

	// Use this for initialization
	void Start () {
        woodAmount = 0;
        maxWood = 30;
        fireParticles = transform.Find("Fire").GetComponent<ParticleSystem>();
        fireParticles.gameObject.SetActive(true);
        fireParticles.Stop();
        fireLight = transform.Find("FireLight").GetComponent<Light>();
        fireLight.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (woodAmount > 0)
        {
            fireBurning = true;
            int season = GameManager.village.GetTwoSeason();
            woodAmount -= Time.deltaTime / (season == 0 ? 8 : 10);
        }
        if (woodAmount <= 0)
        {
            woodAmount = 0;
            StopFire();
        }
        if (woodAmount > maxWood)
            woodAmount = maxWood;

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

    public int Restock(int restock)
    {
        if(restock <= maxWood-woodAmount)
        {
            woodAmount += restock;
            return restock;
        }
        restock = (int)(maxWood-woodAmount);
        woodAmount = maxWood;
        return restock;
        /*int n = (int)((1f - healthFactor) * 30f);
        if (n == 0) return 0;
        if (amountWood >= n)
        {
            healthFactor = 1;
            return n;
        }
        else
        {
            healthFactor += amountWood / 30f;
            return amountWood;
        }*/
    }
    public float GetHealthFactor()
    {
        return woodAmount / maxWood;
    }
}
