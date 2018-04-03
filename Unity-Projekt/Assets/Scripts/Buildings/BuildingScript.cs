using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingScript : MonoBehaviour {

    private Building thisBuilding;

    public bool blueprint;
    public Material[] buildingMaterial, blueprintMaterial;
    private Transform blueprintCanvas;

    private MeshRenderer meshRenderer;

    private List<Transform> panelMaterial;
    private List<Text> textMaterial;

    public List<GameResources> resourceCost;

    // Reference to the clickableObject script
    private ClickableObject co;

    void Start()
    {
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
        for(int i = 1; i < blueprintCanvas.childCount; i++)
        {
            Transform pm = blueprintCanvas.GetChild(i);
            panelMaterial.Add(pm);
            textMaterial.Add(pm.Find("TextMat").GetComponent<Text>());
        }
        buildingMaterial = meshRenderer.materials;
        blueprintMaterial = new Material[buildingMaterial.Length];
        for(int i = 0; i < buildingMaterial.Length; i++)
            blueprintMaterial[i] = BuildManager.Instance.blueprintMaterial;
        blueprint = true;

        resourceCost = new List<GameResources>();
        for(int i = 0; i < thisBuilding.GetAllMaterialCost().Length; i++)
        {
            int cost = thisBuilding.GetMaterialCost(i);
            if (cost > 0 && !GameManager.debugging) resourceCost.Add(new GameResources(i, cost));
        }
    }

    void Update()
    {
        // only clickable, if not in blueprint mode
        co.clickable = !blueprint;
        
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
                GameManager.GetVillage().FinishBuildEvent(thisBuilding);
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

    public void SetBuilding(Building b)
    {
        thisBuilding = b;
    }
    public Building GetBuilding()
    {
        return thisBuilding;
    }
}