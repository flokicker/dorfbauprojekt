using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantType
{
    Tree, Mushroom, MushroomStump, Reed, Crop, Rock
}
[System.Serializable]
public class PlantData : TransformData
{
    public PlantType type;
    public int specie, size, variation, material, miningTimes;
    public bool broken;

    public int gridX, gridY;
}
public class Plant : HideableObject
{
/*
    specie already defined by prefab -> size -> variation
 */

    // Name of the plant species
    private string[] specieNames;
    public string description;

    // Type of the plant
    public PlantType type;
    // Specie id
    private int specie;

    public int gridX, gridY;
    public int gridWidth, gridHeight;
    public bool walkable;

    // size and variation values/maxima
    private int size, maxSize, variation, maxVariation;
    public float radiusPerSize, radiusOffsetSize;
    private int[] meterPerSize, meterOffsetSize;

    private float materialFactor;
    public int materialID, material = -1, materialPerChop;
    private int[] materialPerSize;

    private float fallSpeed, breakTime;
    private int miningTimes = 0;
    private bool broken;

    private float shakingDelta, shakingTime = -1, shakingSpeed = 50f;

    // Growth factor (0=none) [/minute]
    private float growth;
    // Timer for growth and despawning
    private float growthTime, despawnTime;

    public Transform currentModel;
    private Transform[,] allModels;

    public Vector3 fallDirection = Vector3.forward;

    public int monthGrowStart, monthGrowStop;

    //private List<Vector2> entryPoints = new List<Vector2>();

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        fallSpeed = 0f;
        growthTime = 0f;
        /*for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if(dx != 0 && dy != 0)
                entryPoints.Add(new Vector2(dx, dy));*/

        broken = false;
    }

    // Fixed Update for animation
    void FixedUpdate()
    {
        if (broken) // Break/Fall animation
        {
            breakTime += Time.deltaTime;
            switch (type)
            {
                case PlantType.Tree:
                    if ((transform.eulerAngles.z+transform.eulerAngles.x) < 90f)
                    {
                        fallSpeed += 0.0007f*Time.deltaTime;
                        fallSpeed *= 1.07f;
                        //transform.Rotate(fallDirection, fallSpeed, Space.World);
                        /*Vector3 direction = new Vector3(0,0,1);
                        direction = transform.rotation Quaternion.
                        transform.Rotate(new Vector3(direction.z,0,-direction.x), fallSpeed);*/
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
    }

	// Update is called once per frame
	public override void  Update () {
        base.Update();

        if (material == 0 && broken) gameObject.SetActive(false);

        if(type == PlantType.Tree)
        {
            Material[] mats = GetComponentInChildren<MeshRenderer>().sharedMaterials;
            mats[2].color = GetLeavesColor();
            mats[2].SetFloat("_Cutoff",mats[2].color.a*0.2f+0.1f);
            GetComponentInChildren<MeshRenderer>().sharedMaterials = mats;
        }

        // check time of year for plant growing mode
        if(GrowMode() == 0)
        {
            despawnTime += Time.deltaTime;
            if(despawnTime >= 5)
            {
                despawnTime = 0;
                // randomly remove material
                if(Random.Range(0,4) == 0)
                {
                    material--;
                    if(material == 0) Break();
                }
            }
        }
        else if(growth != 0)
        {
            if(size == maxSize)
            {
                if(size > 1)
                    growth = 0;
                else
                {
                    growthTime += Time.deltaTime;
                    float gt = 60f / (growth);
                    if(growthTime >= gt)
                    {
                        growthTime -= gt;
                        if(material < materialPerSize[specie]) material++;
                    }
                }
            }
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

        material = 0;
        materialFactor = 1f + Random.Range(-1f, 1f) * 0.1f;
        materialPerChop = 1;

        radiusPerSize = 0;
        walkable = true;

        monthGrowStart = 0;
        monthGrowStop = 11;

        description = "Unbekannt";

        switch (type)
        {
            case PlantType.Tree:
                /*specieNames = new string[]{ "Fichte", "Birke", "Test-Birke" };
                description = "Kann von Holzfällern gefällt werden.";

                materialPerSize = new int[] { 12, 15, 17, 13 };
                materialID = GameResources.WOOD;

                float[] radiusPerSizes = { 0.1f, 0.05f, 0.08f, 0.08f };
                radiusPerSize = radiusPerSizes[specie];
                float[] radiusOffsetSizes = { 0.05f, 0.05f, 0.05f, 0.05f };
                radiusOffsetSize = radiusOffsetSizes[specie];
                meterPerSize = new int[] { 3, 2, 2, 2 };
                meterOffsetSize = new int[] { 3, 2, 2, 2 };
                int[] maxSizes = { 10, 7, 1, 1};
                maxSize = maxSizes[specie];
                maxVariation = 5;

                materialPerChop = 2;

                growth = 0.1f;*/
                specieNames = new string[]{ "Birke" };
                description = "Kann von Holzfällern gefällt werden.";

                materialPerSize = new int[] { 15 };
                materialID = GameResources.WOOD;

                float[] radiusPerSizes = { 0.05f };
                radiusPerSize = radiusPerSizes[specie];
                float[] radiusOffsetSizes = { 0.05f };
                radiusOffsetSize = radiusOffsetSizes[specie];
                meterPerSize = new int[] { 3 };
                meterOffsetSize = new int[] { 3 };
                int[] maxSizes = { 1 };
                maxSize = maxSizes[specie];
                maxVariation = 5;

                materialPerChop = 2;

                growth = 0.1f;

                break;
            case PlantType.Rock:
                specieNames = new string[] { "Fels", "Moosstein" };
                description = "Kann zu Stein abgebaut werden.";

                materialPerSize = new int[] { 50, 50 };
                materialID = GameResources.STONE;

                radiusOffsetSize = 2f;
                gridWidth = 3;
                gridHeight = 3;
                walkable = false;

                maxSize = 1;
                maxVariation = 3;

                growth = 0f;

                break;
            case PlantType.Crop:
                specieNames = new string[] { "Korn" };
                description = "Korn kann geerntet werden. Sammler können ausserhalb des Bauradius ernten.";

                materialPerSize = new int[] { 5 };
                materialID = GameResources.CROP;
                materialFactor = 1;

                radiusOffsetSize = 0.3f;
                maxSize = 1;
                maxVariation = 1;

                monthGrowStart = 3;
                monthGrowStop = 8;

                growth = 0f;

                break;
            case PlantType.Mushroom:
                specieNames = new string[] { "Pilz", "Steinpilz" };
                description = "Kann eingesammelt werden. Sammler suchen automatisch weiter Pilze.";

                materialPerSize = new int[] { 1, 1 };
                materialID = GameResources.MUSHROOM;
                materialFactor = 1;

                radiusOffsetSize = 0.05f;
                maxSize = 1;
                maxVariation = 5;

                growth = 0f;

                break;
            case PlantType.MushroomStump:
                specieNames = new string[] { "Pilzstrunk" };
                description = "Kann zu Pilzen abgebaut werden. Wächst wieder nach.";

                materialPerSize = new int[] { 30 };
                materialID = GameResources.MUSHROOM;

                radiusOffsetSize = 0.15f;
                maxSize = 1;
                maxVariation = 1;
                walkable = false;

                growth = 1f;

                break;
            case PlantType.Reed:
                specieNames = new string[] { "Fischgrund", "Fischgrund" };
                description = "Fischer können hier im Sommer rohen Fisch fangen.";

                materialPerSize = new int[] { 25, 25 };
                materialID = GameResources.RAWFISH;

                radiusOffsetSize = 0.5f; 
                maxSize = 1;
                maxVariation = 1;

                monthGrowStart = 2;
                monthGrowStop = 10;

                growth = 2f;

                break;
        }

        // Bring a little variation into the growth time, if there's any growth
        if(growth > float.Epsilon)
        {
            float factor = 1f + Random.Range(-0.4f,0.4f);
            growth *= factor;
        }

        // Bring variation into material count
        if(material == 0)
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
        if(maxSize > 1)
            growth = 0;

        if (!broken)
        {
            broken = true;
            breakTime = 0;
        }
    }

    // Mine the plant one time
    public void Mine()
    {
        // stop growing tress,rocks
        if(maxSize > 1)
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

    // 0=growth stop, 1=growing
    public int GrowMode()
    {
        int month = GameManager.GetMonth();
        if(monthGrowStart == monthGrowStop) {
            if(month != monthGrowStart) return 0;
        }
        else if(monthGrowStart < monthGrowStop) {
            if(month < monthGrowStart || month > monthGrowStop) return 0;
        }
        else {
            if(month < monthGrowStart && month > monthGrowStop) return 0;
        }
        return 1;
    }

    public PlantData GetPlantData()
    {
        PlantData pl = new PlantData();

        pl.SetPosition(transform.position);
        pl.SetRotation(transform.rotation);

        pl.type = type;
        pl.specie = specie;

        pl.material = material;
        pl.size = size;
        pl.miningTimes = miningTimes;
        pl.broken = broken;
        pl.variation = variation;
        pl.gridX = gridX;
        pl.gridY = gridY;

        return pl;
    }
    public void SetPlantData(PlantData pl)
    {
        transform.position = pl.GetPosition();
        transform.rotation = pl.GetRotation();

        Init(pl.type, pl.specie);

        variation = pl.variation;
        SetSize(pl.size);
        material = pl.material;
        miningTimes = pl.miningTimes;
        broken = pl.broken;

        gridX = pl.gridX;
        gridY = pl.gridY;
    }

    private Color GetLeavesColor()
    {
        Color summerColor = new Color(0.8f,1,0.6f,1);
        Color fallColor = new Color(1,0.5f,0.2f,1);
        Color springColor = new Color(0.8f,0.8f,0.7f,0.5f);

        int season = GameManager.GetFourSeason();
        float seasonPercentage = GameManager.GetFourSeasonPercentage();
        
        if(seasonPercentage >= 0.5f) 
        {
            season++;
            seasonPercentage --;
        }
        if(season > 3) season = 0;
        seasonPercentage += 0.5f;
        
        Color col = summerColor;
        switch(season)
        {
            case 0:
                col = fallColor;
                col.a = Mathf.Lerp(1,0,seasonPercentage/0.8f);
                if(seasonPercentage > 0.8f) col.a = 0;
                break;
            case 1:
                col = springColor;
                col.a = Mathf.Lerp(0,1,(seasonPercentage-0.2f)/0.8f);
                if(seasonPercentage < 0.2f) col.a = 0;
                break;
            case 2: 
                col = Color.Lerp(springColor,summerColor,seasonPercentage);
                break;
            case 3:
                col = Color.Lerp(summerColor,fallColor,seasonPercentage);
                break;
        }

        return col;
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
