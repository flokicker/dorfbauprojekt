using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour {

    private static float radius = 5f;

    private static string[] namesList = {
        "Fichte", "Birke"
    };
    private static int[] meterPerSizeList = {
        3, 2   
    };
    private static int[] matPerSizeList = {
        50, 75    
    };
    private int type;
    private int size;
    private float materialFactor;
    private int currentMaterial = -1;

    public int MaterialID = 0; // Wood

    private int choppingTimes = 0;

    private float fallSpeed = 0.01f, fallenTime = 0;
    private bool fallen;
    private float currFallingRot;

    private float shakingDelta, shakingTime = -1, shakingSpeed = 50f;

	// Use this for initialization
    void Start()
    {
        fallen = false;
        currFallingRot = 0f;
        GetComponent<cakeslice.Outline>().enabled = false;
	}
	
	// Update is called once per frame
    void Update()
    {
        if (currentMaterial == -1)
        {
            int maxMat = size * matPerSizeList[type];
            currentMaterial = (int)(maxMat * (1f + materialFactor));
        }
        else if (currentMaterial == 0 && fallenTime >= 2) gameObject.SetActive(false);
        if (fallen)
        {
            Vector3 currRot = transform.rotation.eulerAngles;
            if (currFallingRot < 90f)
            {
                currFallingRot += fallSpeed * Time.deltaTime * 60f;
                fallSpeed *= 1.05f;
                transform.rotation = Quaternion.Euler(currRot.x, currRot.y, currFallingRot);
            }
            else
            {
                fallenTime += Time.deltaTime;
                currFallingRot = 90f;
                transform.rotation = Quaternion.Euler(currRot.x, currRot.y, currFallingRot);
            }
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

    public void Fall()
    {
        if (!fallen)
        {
            fallen = true;
            currFallingRot = 0f;
        }
    }
    public void Chop()
    {
        if (!fallen)
        {
            shakingTime = 0;
            choppingTimes++;
            if (choppingTimes > size + 3) Fall();
        }
    }
    public bool HasFallen()
    {
        return fallen && currFallingRot >= 90f;
    }

    public void SetProperties(int type, int size)
    {
        this.type = type;
        name = namesList[type];
        this.size = size;
        materialFactor = Random.Range(-1f, 1f) * 0.1f;
    }
    public int GetTreeType()
    {
        return type;
    }
    public string GetName()
    {
        return namesList[type];
    }
    public int GetSizeInMeter()
    {
        return size * meterPerSizeList[type];
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
        return radius * size;
    }
}
