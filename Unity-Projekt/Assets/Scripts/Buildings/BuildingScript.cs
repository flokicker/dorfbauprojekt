using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingScript : MonoBehaviour {

    // Collection of all buildings
    public static HashSet<BuildingScript> allBuildings = new HashSet<BuildingScript>();

    private Building thisBuilding;

    public bool blueprint;
    public Material[] buildingMaterial, blueprintMaterial;
    private Transform blueprintCanvas, rangeCanvas;
    private Image rangeImage;

    private MeshRenderer meshRenderer;

    private List<Transform> panelMaterial;
    private List<Text> textMaterial;

    public List<GameResources> resourceCost;

    // Reference to the clickableObject script
    private ClickableObject co;

    void Start()
    {
        // Update allBuildings collection
        allBuildings.Add(this);

        // make building a clickable object
        co = gameObject.AddComponent<ClickableObject>();
        co.clickable = false;

        // Disable Campfire script
        if(thisBuilding.id == 8) {
            gameObject.AddComponent<Campfire>().enabled = false;
        }

        meshRenderer = GetComponent<MeshRenderer>();

        // init blueprint
        blueprintCanvas = transform.Find("CanvasBlueprint").transform;
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

        resourceCost = new List<GameResources>();
        for(int i = 0; i < thisBuilding.GetAllMaterialCost().Length; i++)
        {
            int cost = thisBuilding.GetMaterialCost(i);
            if (cost > 0 && !GameManager.debugging) resourceCost.Add(new GameResources(i, cost));
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
            if(thisBuilding.id == Building.CAVE) range = thisBuilding.viewRange;
            if(thisBuilding.id == Building.WAREHOUSEFOOD) range = thisBuilding.foodRange;

            rangeCanvas.gameObject.SetActive(range != 0);
            rangeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, range*20+1+(thisBuilding.GetGridWidth() % 2 == 0 ? 0:10));
            rangeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, range*20+1+(thisBuilding.GetGridHeight() % 2 == 0 ? 0:10));
        } else rangeCanvas.gameObject.SetActive(false);
        
        if (blueprint)
        {
            int requiredCost = 0;
            foreach (GameResources r in resourceCost)
                requiredCost += r.GetAmount();
            if (requiredCost == 0)
            {
                meshRenderer.materials = buildingMaterial;
                blueprint = false;

                // Enable Campfire script
                if(thisBuilding.id == 8) {
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
            if(resourceCost.Count > 0)
            {
                for(int i = 0; i < resourceCost.Count; i++)
                {
                    int totCost = thisBuilding.GetMaterialCost(resourceCost[i].id);
                    int stillCost = resourceCost[i].GetAmount();
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

    public void SetBuilding(Building b)
    {
        thisBuilding = b;
    }
    public Building GetBuilding()
    {
        return thisBuilding;
    }

    // get factors influenced by this building
    public int LuxuryFactor()
    {
        if(thisBuilding.id == Building.CAMPFIRE)
        {
        }
        return 0;
    }
    public int HealthFactor()
    {
        if(thisBuilding.id == Building.CAMPFIRE)
        {
            if(GetComponent<Campfire>().fireBurning) return 2;
        }
        return 0;
    }

    // return wether to display lifebar
    public bool HasLifebar()
    {
        return thisBuilding.id == Building.CAMPFIRE;
    }
    public float LifebarFactor()
    {
        if(thisBuilding.id == Building.CAMPFIRE && GetComponent<Campfire>()) return GetComponent<Campfire>().GetHealthFactor();
        return 0;
    }    
}