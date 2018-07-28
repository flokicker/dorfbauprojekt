using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureObjectScript : HideableObject
{
    // Reference to the clickableObject script
    private ClickableObject co;

    public int Id
    {
        get { return NatureObject.id; }
    }
    public string Name
    {
        get { return NatureObject.name; }
    }
    public NatureObjectType Type
    {
        get { return NatureObject.type; }
    }
    public string Description
    {
        get { return NatureObject.description; }
    }
    public GameResources ResourceCurrent
    {
        get { return gameNatureObject.resourceCurrent; }
    }
    public GameResources ResourcePerSize
    {
        get { return new GameResources(NatureObject.materialPerSize); }
    }
    public GameResources ResourceMax
    {
        get { return new GameResources(NatureObject.materialPerSize.Id, NatureObject.materialPerSize.Amount * (1+MaxSize)); }
    }
    public int MaterialAmPerChop
    {
        get { return NatureObject.materialAmPerChop; }
    }
    public int GridWidth
    {
        get { return NatureObject.gridWidth; }
    }
    public int GridHeight
    {
        get { return NatureObject.gridHeight; }
    }
    public int MaxSize
    {
        get { return NatureObject.sizes; }
    }
    public int MaxVariation
    {
        get { return NatureObject.variations; }
    }
    public int MeterPerSize
    {
        get { return NatureObject.meterPerSize; }
    }
    public int MeterOffsetSize
    {
        get { return NatureObject.meterOffsetSize; }
    }
    public float RadiusPerSize
    {
        get { return NatureObject.radiusPerSize; }
    }
    public float RadiusOffsetSize
    {
        get { return NatureObject.radiusOffsetSize; }
    }
    public Sprite Icon
    {
        get { return NatureObject.icon; }
    }
    public NatureObject NatureObject
    {
        get { return gameNatureObject.natureObject; }
    }

    public int Size
    {
        get { return gameNatureObject.size; }
    }
    public int Variation
    {
        get { return gameNatureObject.variation; }
    }
    public int Growth
    {
        get { return (int)gameNatureObject.currentGrowth; }
    }
    public int GridX
    {
        get { return gameNatureObject.gridX; }
    }
    public int GridY
    {
        get { return gameNatureObject.gridY; }
    }

    private GameNatureObject gameNatureObject;

    private float shakingDelta, shakingTime = -1, shakingSpeed = 50f;
    private Transform currentModel;
    public Vector3 fallDirection = Vector3.forward;

    // Use this for initialization
    public override void Start()
    {
        SetGroundY();
        if (ChopTimes() == 0) Break();
        base.Start();
    }

    // Fixed Update for animation
    void FixedUpdate()
    {

        if (gameNatureObject.broken && NatureObject.tilting) // Break/Fall animation
        {
            gameNatureObject.breakTime += Time.deltaTime;
            if ((transform.eulerAngles.z + transform.eulerAngles.x) < 90f)
            {
                gameNatureObject.fallSpeed += 0.0007f * Time.deltaTime;
                gameNatureObject.fallSpeed *= 1.07f;
                //transform.Rotate(fallDirection, fallSpeed, Space.World);
                /*Vector3 direction = new Vector3(0,0,1);
                direction = transform.rotation Quaternion.
                transform.Rotate(new Vector3(direction.z,0,-direction.x), fallSpeed);*/
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + gameNatureObject.fallSpeed);
            }

            if (transform.eulerAngles.z > 90f)
            {
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 90);
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
    public override void Update()
    {
        base.Update();

        if (IsBroken() && ResourceCurrent.Amount == 0) gameObject.SetActive(false);

        if (Type == NatureObjectType.Tree)
        {
            Material[] mats = GetComponentInChildren<MeshRenderer>(false).sharedMaterials;
            int leavesIndex = -1;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].name.StartsWith("Leaves"))
                {
                    leavesIndex = i;
                    break;
                }
            }
            if (leavesIndex >= 0)
            {
                mats[leavesIndex].color = GetLeavesColor();
                mats[leavesIndex].SetFloat("_Cutoff", mats[leavesIndex].color.a * 0.2f + 0.1f);
                GetComponentInChildren<MeshRenderer>().sharedMaterials = mats;
            }
        }

        // check time of year for NatureObjectScript growing mode
        int gm = GrowMode();
        if (gm == -1)
        {
            gameNatureObject.despawnTime += Time.deltaTime;
            if (gameNatureObject.despawnTime >= 5)
            {
                gameNatureObject.despawnTime = 0;
                // randomly remove material
                if (Random.Range(0, 4) == 0 && ResourceCurrent.Amount > 0)
                {
                    ResourceCurrent.Take(1);
                    if (ResourceCurrent.Amount == 0) Break();
                }
            }
        }
        else if (Growth != 0)
        {
            float gt = 60f / (Growth);
            if (Size == MaxSize)
            {
                if (Size > 1)
                    gameNatureObject.StopGrowth();
                else
                {
                    gameNatureObject.growthTime += Time.deltaTime * GameManager.speedFactor;
                    if (gameNatureObject.growthTime >= gt)
                    {
                        gameNatureObject.growthTime -= gt;
                        if (ResourceCurrent.Amount < ResourceMax.Amount) ResourceCurrent.Add(1);
                    }
                }
            }
            else
            {
                gameNatureObject.growthTime += Time.deltaTime * GameManager.speedFactor;
                transform.localScale = Vector3.one * (0.8f + 0.4f * ((float)Size + gameNatureObject.growthTime / gt) / MaxSize);
                if (gameNatureObject.growthTime >= gt)
                {
                    gameNatureObject.growthTime -= gt;
                    Grow();
                }
            }
        }
    }
    void LateUpdate()
    {
        if (Type == NatureObjectType.EnergySpot)
        {
            currentModel.GetComponent<cakeslice.Outline>().color = IsBroken() ? 1 : 0;
            currentModel.GetComponent<cakeslice.Outline>().enabled = true;
        }
    }

    /*public void Init(NatureObjectType type, int specie)
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
            case NatureObjectType.Tree:
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

                growth = 0.1f;
                specieNames = new string[]{ "Birke" };
                description = "Kann von Holzfällern gefällt werden.";

                materialPerSize = new int[] { 15 };
                materialID = 0;

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
            case NatureObjectType.Rock:
                specieNames = new string[] { "Fels", "Moosstein" };
                description = "Kann zu Stein abgebaut werden.";

                materialPerSize = new int[] { 50, 50 };
                materialID = 1;

                radiusOffsetSize = 2f;
                gridWidth = 3;
                gridHeight = 3;
                walkable = false;

                maxSize = 1;
                maxVariation = 3;

                growth = 0f;

                break;
            case NatureObjectType.Crop:
                specieNames = new string[] { "Korn" };
                description = "Korn kann geerntet werden. Sammler können ausserhalb des Bauradius ernten.";

                materialPerSize = new int[] { 5 };
                materialID = 2;
                materialFactor = 1;

                radiusOffsetSize = 0.3f;
                maxSize = 1;
                maxVariation = 1;

                monthGrowStart = 3;
                monthGrowStop = 8;

                growth = 0f;

                break;
            case NatureObjectType.Mushroom:
                specieNames = new string[] { "Pilz", "Steinpilz" };
                description = "Kann eingesammelt werden. Sammler suchen automatisch weiter Pilze.";

                materialPerSize = new int[] { 1, 1 };
                materialID = 3;
                materialFactor = 1;

                radiusOffsetSize = 0.05f;
                maxSize = 1;
                maxVariation = 5;

                growth = 0f;

                break;
            case NatureObjectType.MushroomStump:
                specieNames = new string[] { "Pilzstrunk" };
                description = "Kann zu Pilzen abgebaut werden. Wächst wieder nach.";

                materialPerSize = new int[] { 30 };
                materialID = 4;

                radiusOffsetSize = 0.15f;
                maxSize = 1;
                maxVariation = 1;
                walkable = false;

                growth = 1f;

                break;
            case NatureObjectType.Reed:
                specieNames = new string[] { "Fischgrund", "Fischgrund" };
                description = "Fischer können hier im Sommer rohen Fisch fangen.";

                materialPerSize = new int[] { 25, 25 };
                materialID = 5;

                radiusOffsetSize = 0.5f; 
                maxSize = 1;
                maxVariation = 1;

                monthGrowStart = 2;
                monthGrowStop = 10;

                growth = 2f;

                break;

            case NatureObjectType.EnergySpot:
                specieNames = new string[] { "Kraftort" };
                description = "Kann von einem Priester eingenommen werden um Glaubenspunkt ezu erhalten.";

                materialPerSize = new int[] { 10 };
                materialID = 0;

                radiusOffsetSize = 0.5f;
                maxSize = 1;
                maxVariation = 1;

                growth = 0;

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
    }*/

    public void SetNatureObject(GameNatureObject gameNatureObject)
    {
        this.gameNatureObject = gameNatureObject;
        SetSize(Size);
    }

    public void SetGroundY()
    {
        float smph = Terrain.activeTerrain.SampleHeight(transform.position);
        Vector3 pos = transform.position;
        pos.y = Terrain.activeTerrain.transform.position.y + smph;
        transform.position = pos;
    }

    // Sets a random size
    public void SetRandSize(int maxRand)
    {
        SetSize(Random.Range(0, Mathf.Min(maxRand, MaxSize)) + 1);
    }
    // sets the newSize and shows the correct model
    public void SetSize(int newSize)
    {
        if (Size >= MaxSize) return;

        // Additional material due to the plants increased size
        if(newSize > Size)
            gameNatureObject.resourceCurrent.Add((int)(NatureObject.materialPerSize.Amount * (newSize - Size) * NatureObject.materialVarFactor));

        gameNatureObject.size = newSize;

        transform.localScale = Vector3.one * (0.8f + 0.4f * (float)Size / MaxSize);

        // make sure to save outlined state of model
        bool outlined = false;
        if (currentModel)
        {
            if (currentModel.GetComponent<cakeslice.Outline>())
                outlined = currentModel.GetComponent<cakeslice.Outline>().enabled;

            currentModel.gameObject.SetActive(false);
        }
        /* TODO: implement right size model */
        currentModel = GetCurrentModel(); ;
        currentModel.gameObject.SetActive(true);

        if (!currentModel.GetComponent<cakeslice.Outline>())
        {
            currentModel.gameObject.AddComponent<cakeslice.Outline>();
            
            // automatically add box colliders if none attached
            if (!currentModel.GetComponent<Collider>()) currentModel.gameObject.AddComponent<BoxCollider>();

            co = currentModel.gameObject.AddComponent<ClickableObject>();
            co.SetScriptedParent(transform);
        }
        currentModel.GetComponent<cakeslice.Outline>().enabled = outlined;
    }
    // Grow NatureObjectScript to next size
    public void Grow()
    {
        if (Size >= MaxSize) return;
        if (IsBroken()) return;

        // change model to appropriate size
        SetSize(Size + 1);
    }

    // Mine the NatureObjectScript one time
    public void Mine()
    {
        // stop growing
        gameNatureObject.StopGrowth();

        if (!IsBroken())
        {
            if (NatureObject.chopShake) shakingTime = 0;
            gameNatureObject.miningTimes++;
            if (gameNatureObject.miningTimes >= ChopTimes()) Break();
        }
    }
    public int ChopTimes()
    {
        return NatureObject.chopTimesPerSize * Size + NatureObject.chopTimesOffsetSize;
    }
    public void Break()
    {
        // stop growing
        gameNatureObject.StopGrowth();

        if (!gameNatureObject.broken)
        {
            gameNatureObject.broken = true;
            gameNatureObject.breakTime = 0;
        }
    }
    public bool IsBroken()
    {
        if (NatureObject.tilting) return gameNatureObject.broken && transform.eulerAngles.z >= 90 - float.Epsilon;
        return gameNatureObject.broken;
    }

    /*private int MineTimes()
    {
        switch (type)
        {
            case NatureObjectType.Tree: // Spruce
                return size/2 + 3;
            case NatureObjectType.EnergySpot:
                return 10;
        }
        return 0;
    }*/

    public Transform GetCurrentModel()
    {
        return transform.childCount <= Size ? transform : transform.GetChild(Size);
    }
    public int GetSizeInMeter()
    {
        return Size * MeterPerSize + MeterOffsetSize;
    }
    public float GetRadiusInMeters()
    {
        return Size * RadiusPerSize + RadiusOffsetSize;
    }

    public void TakeMaterial(int takeAmount)
    {
        ResourceCurrent.Take(takeAmount);
    }

    // 0=growth stop, 1=growing
    public int GrowMode()
    {
        int month = GameManager.GetMonth();
        foreach (IntegerInterval i in NatureObject.growingMonths)
        {
            if (i.Contains(month))
                return i.value;
        }
        return 0;
    }

    private Color GetLeavesColor()
    {
        Color summerColor = new Color(0.8f, 1, 0.6f, 1);
        Color fallColor = new Color(1, 0.5f, 0.2f, 1);
        Color springColor = new Color(0.8f, 0.8f, 0.7f, 0.5f);

        int season = GameManager.GetFourSeason();
        float seasonPercentage = GameManager.GetFourSeasonPercentage();

        if (seasonPercentage >= 0.5f)
        {
            season++;
            seasonPercentage--;
        }
        if (season > 3) season = 0;
        seasonPercentage += 0.5f;

        Color col = summerColor;
        switch (season)
        {
            case 0:
                col = fallColor;
                col.a = Mathf.Lerp(1, 0, seasonPercentage / 0.8f);
                if (seasonPercentage > 0.8f) col.a = 0;
                break;
            case 1:
                col = springColor;
                col.a = Mathf.Lerp(0, 1, (seasonPercentage - 0.2f) / 0.8f);
                if (seasonPercentage < 0.2f) col.a = 0;
                break;
            case 2:
                col = Color.Lerp(springColor, summerColor, seasonPercentage);
                break;
            case 3:
                col = Color.Lerp(summerColor, fallColor, seasonPercentage);
                break;
        }

        return col;
    }

    public static List<GameNatureObject> AllGameNatureObjects()
    {
        List<GameNatureObject> ret = new List<GameNatureObject>();
        foreach (NatureObjectScript nos in Nature.nature)
            ret.Add(nos.gameNatureObject);
        return ret;
    }
    public static void DestroyAllNatureObjects()
    {
        foreach (NatureObjectScript nos in Nature.nature)
            Destroy(nos.gameObject);
        Nature.nature.Clear();
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
    NatureObjectScript NatureObjectScript = obj.AddComponent<NatureObjectScript>();
    NatureObjectScript = this;
    Destroy(this);*/
}
