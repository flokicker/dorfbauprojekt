using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantType
{
    Tree, Rock, Mushroom, Reed
}
public class Plant : MonoBehaviour
{
    public PlantType type;
    public int id, size;
    public float radius;

    public int gridWidth, gridHeight;

    private string[] namesList;
    private int[] meterPerSizeList;

    private float materialFactor;
    public int materialID, material = -1;

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
        if (material == 0 && broken) gameObject.SetActive(false);
        if (broken) // Break/Fall animation
        {
            breakTime += Time.deltaTime;
            switch (type)
            {
                case PlantType.Tree:
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
                case PlantType.Rock: // Rock
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

    public void Init(PlantType type, int id, int size)
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
            case PlantType.Tree:
                namesList = new string[]{ "Fichte", "Birke" };
                int[] matPerSizeList = { 12, 15 };
                meterPerSizeList = new int[] { 3, 2 };
                baseMaterial = size * matPerSizeList[id];
                materialID = 0;
                radius = size*0.5f;
                break;
            case PlantType.Rock:
                namesList = new string[] { "Marmorstein", "Moosstein" };
                baseMaterial = 50;
                materialID = 1;
                radius = 0.5f;
                gridWidth = 3;
                gridHeight = 3;
                break;
            case PlantType.Mushroom:
                namesList = new string[] { "Pilz", "Steinpilz" };
                baseMaterial = 1;
                materialID = GameResources.GetBuildingResourcesCount();
                materialFactor = 0;
                radius = 0.1f;
                break;
            case PlantType.Reed:
                namesList = new string[] { "Schilf" };
                baseMaterial = 1;
                materialID = GameResources.GetBuildingResourcesCount() + 1;
                materialFactor = 0;
                radius = 1f; 
                break;
        }

        name = namesList[id];

        material = (int)(baseMaterial * (1f + materialFactor));
    }

    /* TODO: unify handler for interactable objects */
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
        /* TODO: implement good numbers */
        switch (type)
        {
            case PlantType.Tree: // Spruce
                return size/2 + 3;
            case PlantType.Rock: // Rock
                return 5;
        }
        return 0;
    }

    public int GetSizeInMeter()
    {
        return size * meterPerSizeList[id];
    }
    public void TakeMaterial(int takeAmount)
    {
        material -= takeAmount;
    }
}
