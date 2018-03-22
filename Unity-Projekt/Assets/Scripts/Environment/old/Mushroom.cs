using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    private static float radius = 1f;

    private static string[] namesList = {
        "Fliegenpilz"
    };
    private int type;
    private float materialFactor;
    private int currentMaterial = -1;

    public int MaterialID = 0; // Stone

    private int choppingTimes = 0;

    private bool broken;

    private float shakingDelta, shakingTime = -1, shakingSpeed = 50f;

	// Use this for initialization
    void Start()
    {
        broken = false;
        GetComponent<cakeslice.Outline>().enabled = false;
	}
	
	// Update is called once per frame
    void Update()
    {
        if (currentMaterial == -1)
        {
            currentMaterial = (int)(50 * (1f + materialFactor));
        }
        else if (currentMaterial == 0 && broken) gameObject.SetActive(false);
        if (broken)
        {
            // Broken animation
        }
        if (shakingTime >= 0)
        {
            shakingTime += Time.deltaTime;
            float oldX = transform.position.x - shakingDelta;
            shakingDelta = 0.3f*Mathf.Sin(Time.time * shakingSpeed) * (0.4f-shakingTime);
            transform.position = new Vector3(oldX + shakingDelta, transform.position.y, transform.position.z);
            if (shakingTime >= 0.4f) shakingTime = -1;
        }
	}

    void OnMouseExit()
    {
        GetComponent<cakeslice.Outline>().enabled = false;
        VillageUIManager.Instance.OnHideObjectInfo();
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) GetComponent<cakeslice.Outline>().enabled = true;
        VillageUIManager.Instance.OnShowObjectInfo(transform);
    }

    public void BreakRock()
    {
        if (!broken)
        {
            broken = true;
        }
    }
    public void Mine()
    {
        if (!broken)
        {
            shakingTime = 0;
            choppingTimes++;
            if (choppingTimes > 10) BreakRock();
        }
    }
    public bool IsBroken()
    {
        return broken;
    }

    public void SetProperties(int type)
    {
        this.type = type;
        name = namesList[type];
        materialFactor = Random.Range(-1f, 1f) * 0.1f;
    }
    public int GetRockType()
    {
        return type;
    }
    public string GetName()
    {
        return namesList[type];
    }
    public int GetMaterial()
    {
        return currentMaterial;
    }
    public void TakeMaterial(int takeAmount)
    {
        currentMaterial -= takeAmount;
    }
    public float GetRadius()
    {
        return radius;
    }
}
