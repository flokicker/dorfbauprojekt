using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NatureElementType
{
    Tree, Rock, Mushroom, Reed
}
public class NatureElement : MonoBehaviour
{
    private NatureElementType type;
    private int id, size;
    private float radius;

    private int gridWidth, gridHeight;

    private string[] namesList;
    private int[] meterPerSizeList;

    private float materialFactor;
    private int MaterialID, currentMaterial = -1;

    private float fallSpeed = 0.01f, breakTime;
    private int miningTimes = 0;
    private bool broken;

    private float shakingDelta, shakingTime = -1, shakingSpeed = 50f;

    private List<Vector2> entryPoints = new List<Vector2>();

    // Use this for initialization
    void Start()
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if(dx != 0 && dy != 0)
                entryPoints.Add(new Vector2(dx, dy));
        broken = false;
        GetComponent<cakeslice.Outline>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentMaterial == 0 && broken) gameObject.SetActive(false);
        if (broken) // Break/Fall animation
        {
            breakTime += Time.deltaTime;
            switch (type)
            {
                case NatureElementType.Tree:
                    if (transform.eulerAngles.z < 90f)
                    {
                        fallSpeed *= 1.2f;
                        fallSpeed += 0.1f * Time.deltaTime;
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + fallSpeed * Time.deltaTime);
                    }

                    if (transform.eulerAngles.z > 90f)
                    {
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 90);
                    }
                    break;
                case NatureElementType.Rock: // Rock
                    break;
            }
        }
        if (shakingTime >= 0)
        {
            shakingTime += Time.deltaTime;
            float oldX = transform.position.x - shakingDelta;
            shakingDelta = 0.1f * Mathf.Sin(Time.time * shakingSpeed) * (0.4f - shakingTime);
            transform.position = new Vector3(oldX + shakingDelta, transform.position.y, transform.position.z);
            if (shakingTime >= 0.4f) shakingTime = -1;
        }
    }

    public void Init(NatureElementType type, int id, int size)
    {
        this.type = type;
        this.id = id;
        this.size = size;

        int baseMaterial = 0;
        gridWidth = 1;
        gridHeight = 1;
        materialFactor = Random.Range(-1f, 1f) * 0.1f;

        switch (type)
        {
            case NatureElementType.Tree:
                namesList = new string[]{ "Fichte", "Birke" };
                int[] matPerSizeList = { 50, 75 };
                meterPerSizeList = new int[] { 3, 2 };
                baseMaterial = size * matPerSizeList[id];
                MaterialID = 0;
                radius = size*0.5f;
                break;
            case NatureElementType.Rock:
                namesList = new string[] { "Marmorstein", "Moosstein" };
                baseMaterial = 50;
                MaterialID = 1;
                radius = 0.5f;
                gridWidth = 3;
                gridHeight = 3;
                break;
            case NatureElementType.Mushroom:
                namesList = new string[] { "Pilz", "Steinpilz" };
                baseMaterial = 1;
                MaterialID = GameResources.GetBuildingResourcesCount();
                materialFactor = 0;
                radius = 0.1f;
                break;
            case NatureElementType.Reed:
                namesList = new string[] { "Schilf" };
                baseMaterial = 1;
                MaterialID = GameResources.GetBuildingResourcesCount() + 1;
                materialFactor = 0;
                radius = 1; 
                break;
        }

        name = namesList[id];

        currentMaterial = (int)(baseMaterial * (1f + materialFactor));
    }


    void OnMouseExit()
    {
        GetComponent<cakeslice.Outline>().enabled = false;
        VillageUIManager.Instance.OnHideSmallObjectInfo();
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) GetComponent<cakeslice.Outline>().enabled = true;
        if (Input.GetMouseButton(0))
            VillageUIManager.Instance.OnShowObjectInfo(transform);
        else
            VillageUIManager.Instance.OnShowSmallObjectInfo(transform);
    }

    public void Break()
    {
        if (!broken)
        {
            broken = true;
            breakTime = 0;
        }
    }
    public void Mine()
    {
        if (!broken)
        {
            shakingTime = 0;
            miningTimes++;
            if (miningTimes > MineTimes()) Break();
        }
    }
    public bool IsBroken()
    {
        if (type == 0) return broken && transform.eulerAngles.z >= 90;
        return broken;
    }

    private int MineTimes()
    {
        switch (type)
        {
            case NatureElementType.Tree: // Spruce
                return size/2 + 3;
            case NatureElementType.Rock: // Rock
                return 5;
        }
        return 0;
    }

    public int GetID()
    {
        return id;
    }
    public NatureElementType GetNatureElementType()
    {
        return type;
    }
    public string GetName()
    {
        return namesList[id];
    }
    public int GetSizeInMeter()
    {
        return size * meterPerSizeList[id];
    }
    public int GetMaterial()
    {
        return currentMaterial;
    }
    public int GetMaterialID()
    {
        return MaterialID;
    }
    public void TakeMaterial(int takeAmount)
    {
        currentMaterial -= takeAmount;
    }
    public float GetRadius()
    {
        return radius;
    }
    public int GetGridWidth()
    {
        return gridWidth;
    }
    public int GetGridHeight()
    {
        return gridHeight;
    }
}
