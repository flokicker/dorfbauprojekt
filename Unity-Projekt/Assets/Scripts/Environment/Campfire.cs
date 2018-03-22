using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : MonoBehaviour
{
    public string DisplayName = "Feuerstelle";
    private float healthFactor;
    public bool fireBurning;
    private ParticleSystem fireParticles;

	// Use this for initialization
	void Start () {
        fireParticles = transform.Find("Fire").GetComponent<ParticleSystem>();
        GetComponent<cakeslice.Outline>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (healthFactor > 0)
        {
            fireBurning = true;
            healthFactor -= 0.001f * Time.deltaTime;
        }
        if (healthFactor <= 0)
        {
            healthFactor = 0;
            fireBurning = false;
        }
        if (healthFactor > 1)
            healthFactor = 1;

        if (fireBurning && !fireParticles.isPlaying)
            fireParticles.Play();
        if (!fireBurning && fireParticles.isPlaying) fireParticles.Stop();
	}

    void OnMouseExit()
    {
        GetComponent<cakeslice.Outline>().enabled = false;
        VillageUIManager.Instance.OnHideObjectInfo();
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) GetComponent<cakeslice.Outline>().enabled = true;
        if (Input.GetMouseButton(0))
            VillageUIManager.Instance.OnShowObjectInfo(transform);
        else
            VillageUIManager.Instance.OnShowSmallObjectInfo(transform); 
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
