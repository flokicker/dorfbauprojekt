using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingScript : MonoBehaviour {

    private Building thisBuilding;

    public bool bluePrint;
    public Material[] buildingMaterial, bluePrintMaterial;
    private Transform bluePrintCanvas;

    private MeshRenderer meshRenderer;

    private Text textMaterial;

    public List<GameResources> resourceCost;

    // Reference to the clickableObject script
    private ClickableObject co;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        bluePrintCanvas = transform.Find("CanvasBluePrint").transform;
        textMaterial = bluePrintCanvas.Find("TextMat").GetComponent<Text>();
        co = gameObject.AddComponent<ClickableObject>();
        co.clickable = false;
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
        // only clickable, if not in blueprint mode
        co.clickable = !bluePrint;
        
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
            {
                int totCost = thisBuilding.GetMaterialCost(0);
                textMaterial.text = (totCost - resourceCost[0].GetAmount()) + "/"+totCost;
            }
        }
        else bluePrintCanvas.gameObject.SetActive(false);
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