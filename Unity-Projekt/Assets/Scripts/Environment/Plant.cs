using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO: move rock to other class */
public enum PlantType
{
    Tree, Mushroom, MushroomStump, Reed, Rock
}
public class Plant : HideableObject
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

    public int gridWidth, gridHeight;

    // size and variation values/maxima
    private int size, maxSize, variation, maxVariation;
    public float radiusPerSize, radiusOffsetSize;
    private int[] meterPerSize, meterOffsetSize;

    private float materialFactor;
    public int materialID, material = -1, materialPerChop;
    private int[] materialPerSize;

    private float fallSpeed, fallSpeedDelta, breakTime;
    private int miningTimes = 0;
    private bool broken;

    private float shakingDelta, shakingTime = -1, shakingSpeed = 50f;

    // Growth factor (0=none) [/minute]
    private float growth;
    // Timer for growth
    private float growthTime;

    public Transform currentModel;
    private Transform[,] allModels;

    //private List<Vector2> entryPoints = new List<Vector2>();

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        fallSpeed = 0f;
        fallSpeedDelta = 0;
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
        /*float minDistanceToPerson = float.MaxValue;
        foreach(PersonScript ps in PersonScript.allPeople)
        {
            float dist = Mathf.Abs(ps.transform.position.x - transform.position.x + ps.transform.position.y - transform.position.y);
            if(dist < minDistanceToPerson)
                minDistanceToPerson = dist;
        }
        Debug.Log(minDistanceToPerson < 5 || inBuildRadius);*/
        //gameObject.SetActive(minDistanceToPerson < 5 || inBuildRadius);

        if (material == 0 && broken) gameObject.SetActive(false);
        if (broken) // Break/Fall animation
        {
            breakTime += Time.deltaTime;
            switch (type)
            {
                case PlantType.Tree:
                    if (transform.eulerAngles.z < 90f)
                    {
                        fallSpeedDelta += 0.01f*Time.deltaTime;
                        fallSpeed += fallSpeedDelta;
                        fallSpeed *= 1.1f;
                        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + fallSpeed);
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
            shakingDelta = 0.08f * Mathf.Sin(Time.time * shakingSpeed) * (0.2f - shakingTime);
            transform.position = new Vector3(oldX + shakingDelta, transform.position.y, transform.position.z);
            if (shakingTime >= 0.2f) shakingTime = -1;
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

    public void Init(PlantType type, int specie)
    {
        this.type = type;
        this.specie = specie;

        gridWidth = 1;
        gridHeight = 1;
        materialFactor = 1f + Random.Range(-1f, 1f) * 0.1f;
        materialPerChop = 1;

        switch (type)
        {
            case PlantType.Tree:
                specieNames = new string[]{ "Fichte", "Birke" };

                materialPerSize = new int[] { 12, 15 };
                materialID = GameResources.WOOD;

                float[] radiusPerSizes = { 0.4f, 0.2f };
                radiusPerSize = radiusPerSizes[specie];
                float[] radiusOffsetSizes = { 0.5f, 0.2f };
                radiusOffsetSize = radiusOffsetSizes[specie];
                meterPerSize = new int[] { 3, 2 };
                meterOffsetSize = new int[] { 3, 2 };
                int[] maxSizes = { 10, 7};
                maxSize = maxSizes[specie];
                maxVariation = 1;

                materialPerChop = 4;

                growth = 1f;

                break;
            case PlantType.Rock:
                specieNames = new string[] { "Marmorstein", "Moosstein" };

                materialPerSize = new int[] { 50, 50 };
                materialID = GameResources.WOOD;

                radiusOffsetSize = 0.5f;
                gridWidth = 3;
                gridHeight = 3;

                maxSize = 3;
                maxVariation = 1;

                growth = 0f;

                break;
            case PlantType.Mushroom:
                specieNames = new string[] { "Pilz", "Steinpilz" };

                materialPerSize = new int[] { 1, 1 };
                materialID = GameResources.MUSHROOM;
                materialFactor = 1;

                radiusOffsetSize = 0.1f;
                maxSize = 1;
                maxVariation = 5;

                growth = 0f;

                break;
            case PlantType.MushroomStump:
                specieNames = new string[] { "Pilzstrunk" };

                materialPerSize = new int[] { 35 };
                materialID = GameResources.MUSHROOM;

                radiusOffsetSize = 0.4f;
                maxSize = 1;
                maxVariation = 1;

                growth = 0f;

                break;
            case PlantType.Reed:
                specieNames = new string[] { "Schilf", "Schilf" };

                materialPerSize = new int[] { 1, 1 };
                materialID = GameResources.RAWFISH;
                materialFactor = 1;

                radiusOffsetSize = 1f; 
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
        material = (int)(materialPerSize[specie] * materialFactor);
        variation = Random.Range(0,maxVariation);
        size = 1;
        
        // initializie all Models with a ClickableObject script and make them invisible, except for size 1
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
    public void SetRandSize(int maxRand)
    {
        SetSize(Random.Range(0,Mathf.Min(maxRand,maxSize)) + 1);
    }

    // sets the newSize and shows the correct model
    public void SetSize(int newSize)
    {
        if(newSize > maxSize) return;
        size = newSize;

        // Additional material due to the plants increased size
        material = (int)(materialPerSize[specie] * materialFactor * size);

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

    // Grow plant to next size
    public void Grow()
    {
        if(size >= maxSize) return;
        if(IsBroken() ||miningTimes > 0) return;

        // change model to appropriate size
        SetSize(size+1);
    }

    public void Break()
    {
        // Stop growing if broken
        growth = 0;

        if (!broken)
        {
            broken = true;
            breakTime = 0;
        }
    }
    public void Mine()
    {
        // If we start mining this plant, it stops growing
        growth = 0;
        
        if (!broken)
        {
            if(MineTimes() > 0) shakingTime = 0;
            miningTimes++;
            if (miningTimes > MineTimes()) Break();
        }
    }
    public bool IsBroken()
    {
        if (type == 0) return broken && transform.eulerAngles.z >= 90-float.Epsilon;
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
    public float GetRadiusInMeters()
    {
        return size * radiusPerSize + radiusOffsetSize;
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
