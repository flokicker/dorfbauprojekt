using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{
    BuildingMaterialProduction, Clothes, Food, Population, Storage, Luxury, Other, Crafting
}
public class Building : MonoBehaviour {

    public int nr;
    public bool prototype;

    // Collection of all buildings
    public static HashSet<Building> allBuildings = new HashSet<Building>();

    // Building info
    public int id;
    public BuildingType type;
    public string buildingName, description;
    public int stage;

    public int cost;
    public int[] materialCost, resourceCurrent, resourceStorage;
    public int jobId, workspace, populationRoom, populationCurrent;

    public int gridX, gridY;
    public int gridWidth, gridHeight;
    public int orientation;

    public int viewRange, foodRange, buildRange;

    public bool walkable, multipleBuildings;

    // Show grid-node under building
    public bool showGrid;

    public bool blueprint;
    public Material[] buildingMaterial, blueprintMaterial;
    private Transform blueprintCanvas, rangeCanvas;
    private Image rangeImage;

    private MeshRenderer meshRenderer;

    private List<Transform> panelMaterial;
    private List<Text> textMaterial;

    public List<GameResources> bluePrintBuildCost;

    // lsit of people that are working at this building
    public List<PersonScript> workingPeople;

    // Reference to the clickableObject script
    private ClickableObject co;

    void Start()
    {
        if(prototype) return;

        // Update allBuildings collection
        nr = allBuildings.Count;
        allBuildings.Add(this);

        workingPeople = new List<PersonScript>();

        // make building a clickable object
        co = gameObject.AddComponent<ClickableObject>();
        co.clickable = false;

        // Disable Campfire script
        if(id == 8) {
            gameObject.AddComponent<Campfire>().enabled = false;
        }

        meshRenderer = GetComponent<MeshRenderer>();

        // init blueprint
        blueprintCanvas = transform.Find("CanvasBlueprint");
        panelMaterial = new List<Transform>();
        textMaterial = new List<Text>();
        for(int i = 0; i < blueprintCanvas.childCount; i++)
        {
            Transform pm = blueprintCanvas.GetChild(i);
            panelMaterial.Add(pm);
            textMaterial.Add(pm.Find("TextMat").GetComponent<Text>());
        }
        buildingMaterial = meshRenderer.materials;
        blueprintMaterial = new Material[buildingMaterial.Length];
        for(int i = 0; i < buildingMaterial.Length; i++)
            blueprintMaterial[i] = BuildManager.Instance.blueprintMaterial;

        if(bluePrintBuildCost == null)
        {
            bluePrintBuildCost = new List<GameResources>();
            for(int i = 0; i < materialCost.Length; i++)
            {
                int cost = materialCost[i];
                if (cost > 0 && !GameManager.debugging) bluePrintBuildCost.Add(new GameResources(i, cost));
            }
        }

        // init range canvas
        rangeCanvas = transform.Find("CanvasRange").transform;
        rangeImage = rangeCanvas.Find("Image").GetComponent<Image>();

        //GetComponent<MeshCollider>().convex = true;
    }

    void Update()
    {
        // only clickable, if not in blueprint mode
        co.clickable = !blueprint;

        //GetComponent<MeshCollider>().isTrigger = thisBuilding.walkable;
        if(UIManager.Instance.GetSelectedBuilding() == this || BuildManager.placing)
        {
            int range = 0;
            if(id == CAVE) range = viewRange;
            if(id == WAREHOUSEFOOD) range = foodRange;

            rangeCanvas.gameObject.SetActive(range != 0);
            rangeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, range*20+1+(gridWidth % 2 == 0 ? 0:10));
            rangeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, range*20+1+(gridHeight % 2 == 0 ? 0:10));
        } else rangeCanvas.gameObject.SetActive(false);
        
        if (blueprint)
        {
            int requiredCost = 0;
            foreach (GameResources r in bluePrintBuildCost)
                requiredCost += r.GetAmount();
            if (requiredCost == 0)
            {
                meshRenderer.materials = buildingMaterial;
                blueprint = false;

                // Enable Campfire script
                if(id == 8) {
                    gameObject.GetComponent<Campfire>().enabled = true;
                }
                // Trigger unlock/achievement event
                GameManager.village.FinishBuildEvent(this);
            }
        }

        if (blueprint && meshRenderer.materials[0] != BuildManager.Instance.blueprintMaterial)
            meshRenderer.materials = blueprintMaterial;
    }

    void LateUpdate()
    {
        // Update UI canvas for blueprint
        if (blueprint)
        {
            Camera camera = Camera.main;
            blueprintCanvas.LookAt(blueprintCanvas.position + camera.transform.rotation * Vector3.forward * 0.0001f, camera.transform.rotation * Vector3.up);
            if(bluePrintBuildCost.Count > 0)
            {
                for(int i = 0; i < bluePrintBuildCost.Count; i++)
                {
                    int totCost = materialCost[bluePrintBuildCost[i].id];
                    int stillCost = bluePrintBuildCost[i].GetAmount();
                    panelMaterial[i].gameObject.SetActive(stillCost > 0);
                    textMaterial[i].text = (totCost - stillCost) + "/"+totCost;
                }
            }
        }
        else blueprintCanvas.gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        allBuildings.Remove(this);
    }
    
    public void FromID(int id)
    {
        this.id = id;
        switch (id)
        {
            case 0: Set(BuildingType.Storage, "Höhle", "Kleiner Unterschlupf mit Vorratslager", 0, new int[10], new int[] {25,25,0,0,0,0,0,0,0,0,0,0,0,0}, 0, 0, 5, 3, 3, true, false, 20, 0, 20); break;
            case 1: Set(BuildingType.Population, "Unterschlupf", "Erhöht den Wohnraum", 0, new int[] { 40, 10, 0, 0, 0 }, new int[20], 0, 0, 2, 4, 4, true, true, 0, 0, 0); break;
            case 2: Set(BuildingType.Storage, "Lagerplatz", "Lagert Holz, Steine und Fell", 0, new int[] { 25, 15, 0, 0,  }, new int[] {200,200,0,0,0,0,0,0,0,0,0,50,0,0}, 0, 0, 0, 4, 4, true, true, 0, 0, 0); break;
            case 3: Set(BuildingType.Storage, "Kornspeicher", "Lagert Getreide, Pilze und Fische", 0, new int[] { 20, 0, 0, 0, 0 }, new int[] {0,0,0,0,0,150,150,0,150,0,0,0,0,0}, 0, 0, 0, 2, 2, true, false, 0, 35, 0); break;
            case 4: Set(BuildingType.Food, "Fischerplatz", "Gefangene Fische (Wild) werden hier zu Fisch und Knochen verarbeitet", 0, new int[] { 25, 0, 0, 0, 0 }, new int[] { 0,0,0,0,0,0,50,0,0,50,50,0,0,0}, Job.FISHER, 2, 0, 4, 4, true, true, 0, 0, 0); break;
            case 5: Set(BuildingType.Other, "Holzlager", "Erlaubt die Holzverarbeitung", 0, new int[] { 35, 10, 0, 0, 0 }, new int[20], Job.LUMBERJACK, 0, 0, 4, 4, true, false, 0, 0, 0); break;
            case 6: Set(BuildingType.Crafting, "Jagdhütte", "Erlaubt das Jagen. Hier können Tiere zu Fleisch, Knochen und Fell verarbeitet werden", 0, new int[] { 45, 20, 0, 0, 0 }, new int[]{0,0,0,0,0,0,0,30,0,0,30,15,0,10}, Job.HUNTER, 1, 0, 4, 4, true, true, 0, 0, 0); break;
            case 7: Set(BuildingType.Crafting, "Steinzeit Schmied", "Lagerung von Knochen und Herstellung von Knochen-Werkzeug", 0, new int[] { 50, 35, 0, 0, 0 }, new int[] {0,0,0,0,0,0,0,0,0,0,40,0,10,0}, Job.BLACKSMITH, 1, 0, 8, 4, true, true, 0, 0, 0); break;
            case 8: Set(BuildingType.Luxury, "Lagerfeuer", "Bringe Holz, um das Feuer anzuzünden. Erhöht den Gesundheitsfaktor (+2)", 0, new int[] { 15, 5, 0, 0, 0 }, new int[20], Job.GATHERER, 0, 0, 2, 1, false, true, 0, 0, 10); break;
        }
    }
    private void Set(BuildingType type, string name, string description, int cost, int[] materialCost, int[] resourceStorage, int jobId, int workspace, int populationRoom,
        int gridWidth, int gridHeight, bool walkable, bool multipleBuildings, int viewRange, int foodRange, int buildRange)
    {
        showGrid = id == CAVE || id == CAMPFIRE;
        this.type = type;
        this.name = name;
        this.description = description;
        this.cost = cost;
        this.materialCost = materialCost;
        this.resourceStorage = resourceStorage;
        this.resourceCurrent = new int[20];

        this.jobId = jobId;
        this.workspace = workspace;
        this.populationRoom = populationRoom;

        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        this.stage = 0;
        this.populationCurrent = 0;
        this.gridX = 0;
        this.gridY = 0;

        this.walkable = walkable;
        this.multipleBuildings = multipleBuildings;

        this.viewRange = viewRange;
        this.foodRange = foodRange;
        this.buildRange = buildRange;
    }

    public string GetDescription()
    {
        string ret = description;
        if(!multipleBuildings && id > 0) ret += "\nKann nur einmal gebaut werden";
        return ret;
    }

    public int Restock(GameResources res, int amount)
    {
        int freeSpace = resourceStorage[res.id] - resourceCurrent[res.id];
        int restockRes = 0;
        if(freeSpace >= amount)
        {
            restockRes = amount;
        }
        else
        {
            restockRes = freeSpace;
        }
        resourceCurrent[res.id] += restockRes;
        return restockRes;
    }
    public int Restock(GameResources res)
    {
        return Restock(res, res.amount);
    }
    public int Take(GameResources res, int amount)
    {
        int takeRes = amount;
        if(resourceCurrent[res.id] < takeRes)
        {
            takeRes = resourceCurrent[res.id];
        }
        resourceCurrent[res.id] -= takeRes;
        return takeRes;
    }
    public int Take(GameResources res)
    {
        return Take(res, res.amount);
    }
    public int FreeStorage(int id)
    {
        return resourceStorage[id] - resourceCurrent[id];
    }

    public void SetPosition(int gx, int gy)
    {
        gridX = gx;
        gridY = gy;
    }

    // get factors influenced by this building
    public int LuxuryFactor()
    {
        if(id == CAMPFIRE)
        {
        }
        return 0;
    }
    public int HealthFactor()
    {
        if(id == CAMPFIRE)
        {
            if(GetComponent<Campfire>().fireBurning) return 2;
        }
        return 0;
    }

    // return wether to display lifebar
    public bool HasLifebar()
    {
        return id == CAMPFIRE;
    }
    public float LifebarFactor()
    {
        if(id == CAMPFIRE && GetComponent<Campfire>()) return GetComponent<Campfire>().GetHealthFactor();
        return 0;
    }

    public BuildingData GetBuildingData()
    {
        BuildingData bd = new BuildingData();

        bd.SetPosition(transform.position);
        bd.SetRotation(transform.rotation);

        bd.id = id;
        bd.nr = nr;
        bd.resourceCurrent = resourceCurrent;
        bd.bluePrintBuildCost = new int[GameResources.COUNT];
        foreach(GameResources res in bluePrintBuildCost)
            bd.bluePrintBuildCost[res.id] += res.amount;

        bd.gridX = gridX;
        bd.gridY = gridY;
        bd.gridWidth = gridWidth;
        bd.gridHeight = gridHeight;

        bd.blueprint = blueprint;

        bd.workingPeople = new List<int>();
        foreach(PersonScript ps in workingPeople)
            bd.workingPeople.Add(ps.nr);

        return bd;
    }
    public void SetBuildingData(BuildingData bd)
    {
        transform.position = bd.GetPosition();
        transform.rotation = bd.GetRotation();

        FromID(bd.id);

        nr = bd.nr;
        resourceCurrent = bd.resourceCurrent;
        bluePrintBuildCost = new List<GameResources>();
        for(int i = 0; i < GameResources.COUNT; i++)
            if(bd.bluePrintBuildCost[i] > 0)
                bluePrintBuildCost.Add(new GameResources(i, bd.bluePrintBuildCost[i]));

        gridX = bd.gridX;
        gridY = bd.gridY;
        gridWidth = bd.gridWidth;
        gridHeight = bd.gridHeight;

        blueprint = bd.blueprint;

        workingPeople = new List<PersonScript>();
        foreach(int wp in bd.workingPeople)
        {
            workingPeople.Add(PersonScript.Identify(wp));
        }
    }

    // identify buildingscript by nr
    public static Building Identify(int nr)
    {
        foreach (Building bs in allBuildings)
        {
            if(bs.nr == nr) return bs;
        }
        return null;
    }
    
    public static int COUNT = 9;
    public static int CAVE = 0;
    public static int SHELTER = 1;
    public static int WAREHOUSE = 2;
    public static int WAREHOUSEFOOD = 3;
    public static int FISHERMANPLACE = 4;
    public static int LUMBERJACK = 5;
    public static int HUNTINGLODGE = 6;
    public static int BLACKSMITH = 7;
    public static int CAMPFIRE = 8;
    private static bool[] unlocked = new bool[20];

    public static bool IsUnlocked(int id)
    {
        return unlocked[id];
    }
    public static void Unlock(int id)
    {
        unlocked[id] = true;
    }
}