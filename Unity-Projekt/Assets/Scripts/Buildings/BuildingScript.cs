using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingScript : MonoBehaviour {

    private Building thisBuilding;
    public bool bluePrint;
    public Material[] buildingMaterial, bluePrintMaterial;
    private MeshRenderer meshRenderer;
    private Transform bluePrintCanvas;
    private Text textMaterial;
    public List<GameResources> resourceCost;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        bluePrintCanvas = transform.Find("CanvasBluePrint").transform;
        textMaterial = bluePrintCanvas.Find("TextMat").GetComponent<Text>();
        GetComponent<cakeslice.Outline>().enabled = false;
        buildingMaterial = meshRenderer.materials;
        bluePrintMaterial = new Material[buildingMaterial.Length];
        for(int i = 0; i < buildingMaterial.Length; i++)
            bluePrintMaterial[i] = BuildManager.Instance.bluePrintMaterial;
        bluePrint = true;

        resourceCost = new List<GameResources>();
        for(int i = 0; i < thisBuilding.GetAllMaterialCost().Length; i++)
        {
            int cost = thisBuilding.GetMaterialCost(i);
            if (cost > 0) resourceCost.Add(new GameResources(i, cost));
        }
    }

    void Update()
    {
        if (bluePrint)
        {
            int requiredCost = 0;
            foreach (GameResources r in resourceCost)
                requiredCost += r.GetAmount();
            if (requiredCost == 0)
            {
                meshRenderer.materials = buildingMaterial;
                bluePrint = false;

                // Trigger unlock/achievement event
                GameManager.GetVillage().FinishBuildEvent(thisBuilding);
            }
        }

        if (bluePrint && meshRenderer.materials[0] != BuildManager.Instance.bluePrintMaterial)
            meshRenderer.materials = bluePrintMaterial;
    }

    void LateUpdate()
    {
        // Update UI canvas for blueprint
        if (bluePrint)
        {
            Camera camera = Camera.main;
            bluePrintCanvas.LookAt(bluePrintCanvas.position + camera.transform.rotation * Vector3.forward * 0.0001f, camera.transform.rotation * Vector3.up);
            if(resourceCost.Count > 0)
            textMaterial.text = resourceCost[0].GetAmount().ToString();
        }
        else bluePrintCanvas.gameObject.SetActive(false);
    }

    void OnMouseExit()
    {
        GetComponent<cakeslice.Outline>().enabled = false;
        VillageUIManager.Instance.OnHideSmallObjectInfo();
    }
    void OnMouseOver()
    {
        if (CameraController.inputState == 2) GetComponent<cakeslice.Outline>().enabled = true;
        if (Input.GetMouseButtonDown(0) && !bluePrint)
            VillageUIManager.Instance.OnShowBuildingInfo(transform);
        else
            VillageUIManager.Instance.OnShowSmallObjectInfo(transform);
    }

    public void SetBuilding(Building b)
    {
        thisBuilding = b;
    }
    public Building GetBuilding()
    {
        return thisBuilding;
    }
}

/* public Building b = new Building();
 public int gridX, gridY;
 public int populationCurrent;

 void Start () {
 }
	
 void Update () {
 }

 public void Initialize(string buildingStr)
 {
     string[] buildingInfoStr = buildingStr.Split('*');
     b.FromType(int.Parse(buildingInfoStr[0]));
     gridX = int.Parse(buildingInfoStr[1]);
     gridY = int.Parse(buildingInfoStr[2]);
 }
 public void Clone(BuildingScript bs)
 {
     b.FromType(bs.b.type);
     gridX = bs.gridX;
     gridY = bs.gridY;
 }*/
/*public class Building
{
    public int type;
    public string name;
    public int stage;

    public int cost;
    public int[] materialUse, materialProduce;
    public int populationUse, populationRoom, foodRationsProduce;
    public int gridSizeX, gridSizeY;

    public void FromType(int type)
    {
        this.type = type;
        this.name = building_names[UserManager.nationID, type];
        materialUse = new int[3];
        materialProduce = new int[3];
        gridSizeX = 1;
        gridSizeY = 1;
        switch (type)
        {
            case 0: // Haupthaus
                gridSizeX = 4;
                gridSizeY = 6;
                populationUse = 0;
                populationRoom = 30;
                cost = 0;
                materialUse = new int[] { 5, 0, 0 };
                break;
            case 1: //Wohnhaus
                gridSizeX = 2;
                gridSizeY = 4;
                populationUse = 0;
                populationRoom = 5;
                cost = 5;
                materialUse = new int[] { 15, 0, 0 };
                break;
            case 2: //Bauernhof
                populationUse = 0;
                populationRoom = 0;
                cost = 12;
                materialUse = new int[] { 22, 0, 0 };
                foodRationsProduce = 2;
                break;
            case 3: //Holzfäller
                gridSizeX = 2;
                gridSizeY = 2;
                populationUse = 3;
                populationRoom = 0;
                cost = 40;
                materialUse = new int[] { 3, 2, 0 };
                materialProduce = new int[] { 10, 0, 0 };
                break;
            case 4: // Lehmgrube
                gridSizeX = 2;
                gridSizeY = 2;
                populationUse = 4;
                populationRoom = 0;
                cost = 45;
                materialUse = new int[] { 4, 3, 0 };
                materialProduce = new int[] { 0, 12, 0 };
                break;
        }
    }

    public static List<Building> GetAllBuildings()
    {
        List<Building> list = new List<Building>();
        for (int i = 1; i < building_names.GetLength(1); i++) // can't build main
        {
            Building b = new Building();
            b.FromType(i);
            list.Add(b);
        }
        return list;
    }

    private static string[,] building_names = { 
        { "Haupthaus", "Wohnhaus", "Bauernhof", "Holzfäller", "Lehmgrube", "Jagdhütte", "Schafszucht", "Schneider", "Vorratslager", "Truppenunterkunft", "Tüftler" }, // Wikinger
        { "Haupthaus", "Wohnhaus", "Bauernhof", "Holzfäller", "Lehmgrube", "Jagdhütte", "Schafszucht", "Schneider", "Vorratslager", "Truppenunterkunft", "Tüftler" }  // Ägypter
    };
}
*/