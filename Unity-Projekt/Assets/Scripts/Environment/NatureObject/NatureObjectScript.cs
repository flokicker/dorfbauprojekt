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
    public GameResources ResourcePerChop
    {
        get { return new GameResources(NatureObject.materialPerSize.Id, MaterialAmPerChop); }
    }
    public GameResources ResourceMax
    {
        get { return new GameResources(NatureObject.materialPerSize.Id, NatureObject.materialPerSize.Amount * (MaxSize)); }
    }
    public int MaterialAmPerChop
    {
        get { return NatureObject.materialAmPerChop; }
    }
    public float ChopTime
    {
        get { return NatureObject.chopTime; }
    }
    public bool Walkable
    {
        get { return NatureObject.walkable; }
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
    public float ResourceGrowth
    {
        get { return NatureObject.resourceGrowth; }
    }
    public bool Destroyable
    {
        get { return true; }
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
    public float Growth
    {
        get { return gameNatureObject.currentGrowth; }
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

    private Collider collider;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    // audio
    private AudioSource audioSource;

    // Use this for initialization
    public override void Start()
    {
        tag = NatureObject.Tag;

        SetGroundY();

        // TODO: check if commenting next line didnt break anything
        //if (ChopTimes() == 0) Break();

        if (currentModel == null) SetCurrentModel();

        audioSource = Instantiate(AudioManager.Instance.buildingAudioPrefab, transform).GetComponent<AudioSource>();

        // start coroutine
        StartCoroutine(GameNatureObjectTransform());

        base.Start();
    }

    // Fixed Update for animation
    void FixedUpdate()
    {
        if (gameNatureObject.broken && NatureObject.tilting) // Break/Fall animation
        {
            //GetComponent<Collider>().enabled = false;
            gameNatureObject.breakTime += Time.deltaTime;
            if (transform.eulerAngles.z < 90f - float.Epsilon)
            {
                gameNatureObject.fallSpeed += 0.0002f * Time.deltaTime;
                gameNatureObject.fallSpeed *= 1.065f;
                //transform.Rotate(fallDirection, fallSpeed, Space.World);
                /*Vector3 direction = new Vector3(0,0,1);
                direction = transform.rotation Quaternion.
                transform.Rotate(new Vector3(direction.z,0,-direction.x), fallSpeed);*/
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + gameNatureObject.fallSpeed);
            }

            if (transform.eulerAngles.z > 90f + float.Epsilon)
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
        UpdateGrowth();
        CheckDestroy();

        base.Update();
    }
    void LateUpdate()
    {
        if (Type == NatureObjectType.EnergySpot)
        {
            cakeslice.Outline outline = currentModel.GetComponent<cakeslice.Outline>();
            if (!outline) outline = currentModel.gameObject.AddComponent<cakeslice.Outline>();
            outline.color = IsBroken() ? 1 : 0;
            outline.enabled = true;
        }
    }

    // update methods
    private void UpdateColliders()
    {
        if (Type == NatureObjectType.Tree && (IsBroken() || IsFalling()))
        {
            // make sure that player wont get stuck in mesh collider of tree
            if (meshCollider && meshCollider.enabled) meshCollider.enabled = false;
            if (collider && !collider.isTrigger) collider.isTrigger = true;
        }
        if (Type == NatureObjectType.Reed)
        {
            if (collider.enabled) collider.enabled = false;
        }
        else if (Type != NatureObjectType.Tree)
        {
            if (meshCollider && !meshCollider.convex) meshCollider.convex = Walkable;
            if (collider && !collider.isTrigger) collider.isTrigger = Walkable;
        }
    }
    private void UpdateGrowth()
    {
        // check time of year for NatureObjectScript growing mode
        float gt = 0;
        if (CurrentGrowMode == -1)
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
        else if (Size == MaxSize || Growth <= float.Epsilon)
        {
            if (Size > 1)
                gameNatureObject.StopGrowth();
            else
            {
                gt = 60f / ResourceGrowth;
                gameNatureObject.growthTime += Time.deltaTime * GameManager.speedFactor;
                if (gameNatureObject.growthTime >= gt)
                {
                    gameNatureObject.growthTime -= gt;
                    if (ResourceCurrent.Amount < ResourceMax.Amount) ResourceCurrent.Add(1);
                }
            }
        }
        else if (Growth > float.Epsilon)
        {
            gt = 60f / (Growth);
            gameNatureObject.growthTime += Time.deltaTime * GameManager.speedFactor;
            transform.localScale = Vector3.one * (NatureObject.minSize + (NatureObject.maxSize - NatureObject.minSize) * (Size + gameNatureObject.growthTime / gt) / MaxSize);
            if (gameNatureObject.growthTime >= gt)
            {
                gameNatureObject.growthTime -= gt;
                Grow();
            }
        }
    }
    private void CheckDestroy()
    {
        if (ResourceCurrent.Amount <= 0 && Type != NatureObjectType.EnergySpot && Destroyable)
        {
            gameObject.SetActive(false);
            Grid.GetNode(GridX, GridY).SetNodeObject(null);
        }
    }

    public override void OnDestroy()
    {
        Nature.nature.Remove(this);
        base.OnDestroy();
    }

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

        transform.localScale = Vector3.one * (0.8f + 0.4f * Size / MaxSize);
        
        SetCurrentModel();

        // update clickable object selection circle radius
        co.SetSelectionCircleRadius(GetRadiusInMeters());
    }
    // Grow NatureObjectScript to next size
    public void Grow()
    {
        if (Size >= MaxSize)
        {
            return;
        }
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

        if (NatureObject.tilting) audioSource.PlayOneShot(AudioManager.Instance.fallingTree);

        // if falling tree, we need to update the colliders
        UpdateColliders();
    }
    public bool IsBroken()
    {
        if (NatureObject.tilting) return gameNatureObject.broken && transform.eulerAngles.z >= 90 - float.Epsilon;
        return gameNatureObject.broken;
    }
    public bool IsFalling()
    {
        if (NatureObject.tilting) return gameNatureObject.broken && transform.eulerAngles.z < 90;
        return false;
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

    public void SetCurrentModel()
    {
        currentModel = GetCurrentModel(); ;
        currentModel.gameObject.SetActive(true);

        if (!currentModel.gameObject.GetComponent<ClickableObject>())
        {
            // automatically add box colliders if none attached
            meshCollider = currentModel.GetComponent<MeshCollider>();
            collider = currentModel.GetComponent<Collider>();
            if (!collider && Type != NatureObjectType.Water) collider = currentModel.gameObject.AddComponent<BoxCollider>();

            co = currentModel.gameObject.AddComponent<ClickableObject>();
            co.SetScriptedParent(transform);

            if (IsBroken())
            {
                co.SetOriginalPosition(transform.position + Vector3.up);
            }
        }
        if(co) co.keepOriginalPos = true;

        // update clickable object selection circle radius
        co.SetSelectionCircleRadius(GetRadiusInMeters() * 1.5f + 0.2f);

        // get mesh renderer for trees to change leaves color
        meshRenderer = GetComponentInChildren<MeshRenderer>(false);

        // set colliders
        UpdateColliders();
    }
    public Transform GetCurrentModel()
    {
        return transform;//transform.childCount <= Size ? transform : transform.GetChild(Size);
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
    private int CurrentGrowMode = 0;
    public void RecalculateGrowMode()
    {
        int month = GameManager.GetMonth();
        if (NatureObject.growingMonths.Count == 0)
        {
            CurrentGrowMode = 1;
            return;
        }
        foreach (IntegerInterval i in NatureObject.growingMonths)
        {
            if (i.Contains(month))
            {
                CurrentGrowMode = i.value;
                return;
            }
        }
        CurrentGrowMode = 0;
    }

    private Color GetLeavesColor()
    {
        Color summerColor = new Color(0.8f, 1, 0.6f, 1);
        Color fallColor = new Color(1, 0.5f, 0.2f, 1);
        Color springColor = new Color(0.8f, 0.8f, 0.7f, 0.5f);

        int season = GameManager.GetFourSeason();
        float seasonPercentage = GameManager.GetFourSeasonPercentage();

        //Debug.Log(seasonPercentage);

        if (seasonPercentage >= 0.5f)
        {
            season++;
            seasonPercentage -= 0.5f;
        }
        else
        {
            seasonPercentage += 0.5f;
        }
        if (season > 3) season = 0;

        Color col = summerColor;
        switch (season)
        {
            case 0: // fall -> winter
                col = fallColor;
                if (seasonPercentage < 0.8f)
                    col.a = Mathf.Lerp(1, 0, seasonPercentage / 0.8f);
                else col.a = 0;
                break;
            case 1: // winter -> spring
                col = springColor;
                if (seasonPercentage < 0.5f)
                    col.a = 0;
                else
                    col.a = Mathf.Lerp(0, 1, (seasonPercentage-0.5f)/0.5f);
                break;
            case 2: // spring -> summer
                col = Color.Lerp(springColor, summerColor, seasonPercentage);
                break;
            case 3: // summer -> fall
                col = Color.Lerp(summerColor, fallColor, seasonPercentage);
                break;
        }

        return col;
    }
    
    // Update transform once per second
    private IEnumerator GameNatureObjectTransform()
    {
        while (true)
        {
            // update transform position rotation on save object
            gameNatureObject.SetTransform(transform);
            UpdateLeavesColor();
            RecalculateGrowMode();

            yield return new WaitForSeconds(1);
        }
    }
    // update leaves color only every second
    private void UpdateLeavesColor()
    {
        if (Type == NatureObjectType.Tree && Name == "Birke" && meshRenderer != null && false)
        {
            Material[] mats = meshRenderer.sharedMaterials;
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
                meshRenderer.sharedMaterials = mats;
            }
        }
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
