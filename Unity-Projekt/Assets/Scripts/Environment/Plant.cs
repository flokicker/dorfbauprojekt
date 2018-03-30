using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO: move rock to other class */
public enum PlantType
{
    Tree, Mushroom, Reed, Rock
}
public class Plant : MonoBehaviour
{
/*
    specie already defined by prefab -> size -> variation
 */

    // Name of the plant species
    private string[] specieNames;

    // Type of the plant
    public PlantType type;
    // Specie id
    private int specie;

    // size and variation values/maxima
    private int size, maxSize, variation, maxVariation;
    public float radius;

    public int gridWidth, gridHeight;
    private int[] meterPerSize, meterOffsetSize;

    private float materialFactor;
    public int materialID, material = -1;
    private int[] materialPerSize;

    private float fallSpeed = 0.01f, breakTime;
    private int miningTimes = 0;
    private bool broken;

    private float shakingDelta, shakingTime = -1, shakingSpeed = 50f;

    // Growth factor (0=none) [/minute]
    private float growth;
    // Timer for growth
    private float growthTime;

    private Transform currentModel;
    private Transform[,] allModels;

    //private List<Vector2> entryPoints = new List<Vector2>();

    // Use this for initialization
    void Start()
    {
        growthTime = 0f;
        /*for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if(dx != 0 && dy != 0)
                entryPoints.Add(new Vector2(dx, dy));*/

        broken = false;
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

        if(growth != 0)
        {
            if(size == maxSize)
                growth = 0;
            else
            {
                growthTime += Time.deltaTime;
                float gt = 60f / (growth);
                if(growthTime >= gt)
                {
                    growthTime -= gt;
                    Grow();
                }
            }
        }
    }

    public void Init(PlantType type)
    {
        this.type = type;

        gridWidth = 1;
        gridHeight = 1;
        materialFactor = Random.Range(-1f, 1f) * 0.1f;

        switch (type)
        {
            case PlantType.Tree:
                specieNames = new string[]{ "Fichte", "Birke" };

                materialPerSize = new int[] { 12, 15 };
                materialID = 0;

                radius = size*0.5f;
                meterPerSize = new int[] { 3, 2 };
                meterOffsetSize = new int[] { 3, 2 };
                maxSize = 10;
                maxVariation = 1;

                growth = 1f;

                break;
            case PlantType.Rock:
                specieNames = new string[] { "Marmorstein", "Moosstein" };

                materialPerSize = new int[] { 50, 50 };
                materialID = 1;

                radius = 0.5f;
                gridWidth = 3;
                gridHeight = 3;

                maxSize = 3;
                maxVariation = 1;

                growth = 0f;

                break;
            case PlantType.Mushroom:
                specieNames = new string[] { "Pilz", "Steinpilz" };

                materialPerSize = new int[] { 1, 1 };
                materialID = GameResources.GetBuildingResourcesCount();
                materialFactor = 0;

                radius = 0.1f;
                maxSize = 1;
                maxVariation = 5;

                growth = 0f;

                break;
            case PlantType.Reed:
                specieNames = new string[] { "Schilf", "Schilf" };

                materialPerSize = new int[] { 1, 1 };
                materialID = GameResources.GetBuildingResourcesCount() + 1;
                materialFactor = 0;

                radius = 1f; 
                maxSize = 1;
                maxVariation = 1;

                growth = 0f;

                break;
        }

        // Bring a little variation into the growth time, if there's any growth
        if(growth > float.Epsilon)
        {
            float factor = 1f + Random.Range(-0.4f,0.4f);
            growth *= factor;
        }

        // Bring variation into material count
        int baseMaterial = materialPerSize[specie];
        material = (int)(baseMaterial * (1f + materialFactor));
        variation = Random.Range(0,maxVariation);
        size = 1;
        
        allModels = new Transform[maxSize,maxVariation];
        for(int i = 0; i < maxSize; i++)
        {
            for(int j = 0; j < maxVariation; j++)
            {
                allModels[i,j] = transform.GetChild(i*maxVariation + j);
                allModels[i,j].gameObject.AddComponent<ClickableObject>().SetScriptedParent(transform);
                allModels[i,j].gameObject.AddComponent<cakeslice.Outline>().enabled = false;
                allModels[i,j].gameObject.SetActive(i == 0 && j == 0);
            }
        }

        currentModel = allModels[0,0];
    }

    // Sets a random size
    public void SetRandSize()
    {
        SetSize(Random.Range(0,maxSize));
    }

    // sets the newSize and shows the correct model
    public void SetSize(int newSize)
    {
        if(newSize > maxSize) return;
        size = newSize;

        // make sure to save outlined state of model
        bool outlined = false;
        if(currentModel.GetComponent<cakeslice.Outline>())
            outlined = currentModel.GetComponent<cakeslice.Outline>().enabled;

        currentModel.gameObject.SetActive(false);
        currentModel = allModels[newSize-1,variation];
        currentModel.gameObject.SetActive(true);

        if(currentModel.GetComponent<cakeslice.Outline>())
            currentModel.GetComponent<cakeslice.Outline>().enabled = outlined;
    }
    public void Grow()
    {
        // change model to appropriate size
        SetSize(size+1);

        // Additional material due to the plants increased size
        material += materialPerSize[specie];
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

    public string GetName()
    {
        return specieNames[specie];
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
        return size * meterPerSize[specie] + meterOffsetSize[specie];
    }
    public void TakeMaterial(int takeAmount)
    {
        material -= takeAmount;
    }
    

    /*DestroyImmediate(GetComponent<cakeslice.Outline>());
    Destroy(GetComponent<MeshRenderer>());
    Destroy(GetComponent<MeshFilter>());
    UnityEditorInternal.ComponentUtility.CopyComponent(plantModels[(int)type][size + id - 1].GetComponent<MeshRenderer>());
    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameObject);
    UnityEditorInternal.ComponentUtility.CopyComponent(plantModels[(int)type][size + id - 1].GetComponent<MeshFilter>());
    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameObject);
    gameObject.AddComponent<cakeslice.Outline>();*/
    /*GetComponent<MeshFilter>().sharedMesh = plantModels[(int)type][size + id - 1].GetComponent<MeshFilter>().sharedMesh;
    GetComponent<MeshRenderer>().sharedMaterials = plantModels[(int)type][size + id - 1].GetComponent<MeshRenderer>().sharedMaterials;
    /*Destroy(GetComponent<MeshRenderer>());
    Destroy(GetComponent<MeshFilter>());
    MeshRenderer r = ;
    r = plantModels[(int)type][size + id - 1].GetComponent<MeshRenderer>();
    MeshFilter f = GetComponent<MeshFilter>();
    gameObject.AddComponent(plantModels[(int)type][size + id - 1].GetComponent<MeshFilter>());*/
    /*GameObject obj = (GameObject)Instantiate(plantModels[(int)type][size + id - 1], transform.position, transform.rotation, transform.parent);
    obj.AddComponent(typeof(cakeslice.Outline));
    Plant plant = obj.AddComponent<Plant>();
    plant = this;
    Destroy(this);*/
}
