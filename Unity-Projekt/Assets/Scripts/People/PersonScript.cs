using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonScript : MonoBehaviour {

    public static HashSet<PersonScript> allPeople = new HashSet<PersonScript>();
    public static HashSet<PersonScript> selectedPeople = new HashSet<PersonScript>();

    private Person thisPerson;
    public bool selected, highlighted;

    private float health, maxHealth;
    private float saturationTimer, saturation;

    private float moveSpeed = 1.5f;
    private float currentMoveSpeed = 0f;

    private List<Node> currentPath;
    public List<Task> routine = new List<Task>();
    private Node lastNode;

    // Activity speeds/times
    private float choppingSpeed = 0.8f;
    private float buildingTime = 0.3f, fishingTime = 1f;

    private Transform canvas;
    private Image imageHP;

    private bool automatedTasks = false;

    private cakeslice.Outline outline;

	// Use this for initialization
    void Start()
    {
        maxHealth = 100;
        health = maxHealth;
        saturationTimer = 0;

        outline = GetComponent<cakeslice.Outline>();
        outline.enabled = false;
        canvas = transform.Find("Canvas").transform;
        imageHP = canvas.Find("Health").Find("ImageHP").GetComponent<Image>();

        Vector3 gp = Grid.ToGrid(transform.position);
        lastNode = Grid.GetNode((int)gp.x, (int)gp.z);

        allPeople.Add(this);
	}

	// Update is called once per frame
    void Update()
    {
        lastNode.SetPeopleOccupied(false);

        if (routine.Count > 0)
        {
            Task ct = routine[0];
            ExecuteTask(ct);
        }
        
        saturationTimer+=Time.deltaTime;

        // Eat after not being saturated anymore
        if(saturationTimer >= saturation) {
            saturation = 0;
            saturationTimer = 0;

            // automatically take food from inventory first
            GameResources food = thisPerson.GetInventory();

            if(food == null || food.GetResourceType() != ResourceType.Food || food.GetAmount() == 0)
            {
                foreach(GameResources r in GameManager.GetVillage().resources)
                {    
                    if (r.GetResourceType() == ResourceType.Food && r.GetAmount() > 0)
                    {
                        food = r;
                        break;
                    }
                }
            }
            
            if(food != null && food.GetAmount() > 0 && food.GetResourceType() == ResourceType.Food)
            {
                saturation = food.GetNutrition();
                food.Take(1);
            }
        }

        float satFact = 1f;
        if(saturation == 0) satFact = -1f;
        else if(saturation > 10) satFact = 3f;

        health += Time.deltaTime*0.1f * satFact;

        if(health < 0) health = 0;
        if(health > maxHealth) health = maxHealth;

        // position player at correct ground height on terrain
        Vector3 terrPos = transform.position;
        terrPos.y = Terrain.activeTerrain.SampleHeight(terrPos) + Terrain.activeTerrain.transform.position.y;
        transform.position = terrPos;
        lastNode.SetPeopleOccupied(true);
	}

    void OnDestroy()
    {
        allPeople.Remove(this);
        selectedPeople.Remove(this);
    }

    void LateUpdate()
    {
        // Update UI canvas for health bar
        Camera camera = Camera.main;
        canvas.LookAt(canvas.position + camera.transform.rotation * Vector3.forward * 0.0001f, camera.transform.rotation * Vector3.up);
        canvas.gameObject.SetActive(highlighted || selected);
        imageHP.rectTransform.offsetMax = new Vector2(-(1+ 28f * (1f-GetHealthFactor())),-1);
        imageHP.color = GetConditionCol();
        //imageHP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,);
        
        // Update outline component
        outline.enabled = highlighted || selected;
        outline.color = selected ? 1 : 0;
    }

	
    // Do a given task 'ct'
    void ExecuteTask(Task ct)
    {
        ct.taskTime += Time.deltaTime;
        Plant plant = null;
        BuildingScript bs = null;
        GameResources res = thisPerson.GetInventory();
        if (ct.targetTransform != null)
        {
            plant = ct.targetTransform.GetComponent<Plant>();
            bs = ct.targetTransform.GetComponent<BuildingScript>();
        }
        int am = 0;
        switch (ct.taskType)
        {
            case TaskType.CutTree: // Chopping a tree
                if (plant.IsBroken())
                {
                    // Collect wood of fallen tree, by chopping it into pieces
                    if (ct.taskTime >= 1f / choppingSpeed)
                    {
                        ct.taskTime = 0;
                        //Transform nearestTree = GameManager.GetVillage().GetNearestPlant(PlantType.Tree, transform.position, thisPerson.GetTreeCutRange());
                        if (plant.material > 0)
                        {
                            // Amount of wood per one chop gained
                            int mat = 4;
                            if (plant.material < mat) mat = plant.material;
                            mat = thisPerson.AddToInventory(new GameResources(plant.materialID, mat));
                            plant.TakeMaterial(mat);
                            if(GameManager.debugging)
                            GameManager.GetVillage().NewMessage(mat + " added to inv");
                            if (mat == 0 || thisPerson.GetFreeInventorySpace() == 0) // inventory is full
                            {
                                NextTask();

                                if(automatedTasks)
                                {
                                    Transform nearestTreeStorage = GameManager.GetVillage().GetNearestBuildingType(transform.position, BuildingType.StorageMaterial);
                                    if (nearestTreeStorage != null) SetTargetTransform(nearestTreeStorage);
                                    else
                                        GameManager.GetVillage().NewMessage("Baue ein Lagerplatz für das Holz!");
                                    //if (nearestTree != null && nearestTreeStorage != null) AddTargetTransform(nearestTree);
                                }
                            }
                        }
                        else
                        {
                            NextTask();

                            // Find a new tree to cut down
                            //if (nearestTree != null && automatedTasks) SetTargetTransform(nearestTree);
                        }
                    }
                }
                else if (ct.taskTime >= 1f / choppingSpeed)
                {
                    ct.taskTime = 0;
                    plant.Mine();
                }
                break;
            case TaskType.Fisherplace: // Making food out of fish
                if (res == null || res.GetAmount() == 0 || res.GetID() != 6)
                {
                    NextTask();
                }
                else
                {
                    // Convert raw fish into real fish
                    GameResources fish = new GameResources(res.GetID() + 1, res.GetAmount());
                    GameManager.GetVillage().Restock(fish);
                    res.Take(res.GetAmount());
                }
                break;
            case TaskType.BringToWarehouse: // Bringing material to warehouse
                bool isRightWarehouse = false;
                if(res != null)
                {
                    if (bs.GetBuilding().GetBuildingType() == BuildingType.StorageFood && res.GetResourceType() == ResourceType.Food) isRightWarehouse = true;
                    else if (bs.GetBuilding().GetBuildingType() == BuildingType.StorageMaterial && res.GetResourceType() == ResourceType.BuildingMaterial) isRightWarehouse = true;
                }
                if (res == null || res.GetAmount() == 0 || !isRightWarehouse)
                {
                    NextTask();
                    /* TODO: GO out of warehouse */

                    /*Vector2 ep = bs.GetBuilding().GetEntryPoint(0);
                    SetTargetPosition(bs.transform.position + new Vector3(ep.x, 0, ep.y)*Grid.SCALE);
                    routine.Add(new Task(TaskType.Walk, lastPath));*/
                }
                else
                {
                    GameManager.GetVillage().Restock(res);
                    res.Take(res.GetAmount());
                }
                break;
            case TaskType.TakeFromWarehouse: // Taking material from warehouse
                if (res == null || res.GetAmount() == 0)
                {
                    GameResources takeRes = null;
                    if (bs.GetBuilding().GetBuildingType() == BuildingType.StorageMaterial) takeRes = new GameResources(0, thisPerson.GetInventorySize());
                    if (bs.GetBuilding().GetBuildingType() == BuildingType.Food) takeRes = new GameResources(5, thisPerson.GetInventorySize());
                    if (takeRes != null)
                    {
                        int take = GameManager.GetVillage().Take(takeRes);
                        takeRes.SetAmount(take);
                        thisPerson.AddToInventory(takeRes);
                    }
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.Campfire: // Restock campfire fire wood
                Campfire cf = routine[0].targetTransform.GetComponent<Campfire>();
                if(res != null && res.GetID() == 0)
                    res.Take(cf.Restock(res.GetAmount()));
                routine.RemoveAt(0);
                break;
            case TaskType.Fishing: // Do Fishing
                if (res == null || res.GetAmount() == 0 || res.GetID() == 6)
                {
                    if (ct.taskTime >= fishingTime)
                    {
                        ct.taskTime = 0;
                        if (Random.Range(0, 5) == 0)
                        {
                            thisPerson.AddToInventory(new GameResources(6, 1));
                        }
                    }
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.Build: // Put resources into blueprint building
                if (res == null) NextTask();
                else if(ct.taskTime >= buildingTime)
                {
                    ct.taskTime = 0;
                    bool built = false;
                    foreach (GameResources r in bs.resourceCost)
                    {
                        if (res.GetID() == r.GetID() && r.GetAmount() > 0 && res.GetAmount() > 0 && !built)
                        {
                            built = true;
                            res.Take(1);
                            r.Take(1);
                            if (r.GetAmount() == 0) NextTask();
                        }
                    }

                    if (!built) NextTask();
                }
                break;
            case TaskType.CollectMushroom: // Collect the mushroom
                // add resources to persons inventory
                if(plant.gameObject.activeSelf)
                {
                    am = thisPerson.AddToInventory(new GameResources(plant.materialID, plant.material));
                    if(am> 0) 
                    {
                        // Destroy collected mushroom
                        plant.Break();
                        plant.gameObject.SetActive(false);
                    }
                }
                NextTask();
                if(automatedTasks)
                {
                    Transform nearestMushroom = GameManager.GetVillage().GetNearestPlant(PlantType.Mushroom, transform.position, thisPerson.GetCollectingRange());
                    Transform nearestFoodStorage = GameManager.GetVillage().GetNearestBuildingType(transform.position, BuildingType.StorageFood);
                    if (thisPerson.GetFreeInventorySpace() == 0)
                    {
                        if (nearestFoodStorage != null) SetTargetTransform(nearestFoodStorage);
                        else
                            GameManager.GetVillage().NewMessage("Baue ein Kornlager für die Pilze!");
                        if (nearestMushroom != null && nearestFoodStorage != null) AddTargetTransform(nearestMushroom);
                    }
                    else
                    {
                        if (nearestMushroom != null) SetTargetTransform(nearestMushroom);
                        else if (nearestFoodStorage != null) SetTargetTransform(nearestFoodStorage);
                        else GameManager.GetVillage().NewMessage("Baue ein Kornlager für die Pilze!");
                    }
                }
                break;
            case TaskType.PickupItem: // Pickup the item
                Item itemToPickup = routine[0].targetTransform.GetComponent<Item>();
                NextTask();
                if (itemToPickup != null && itemToPickup.gameObject.activeSelf)
                {
                    am = thisPerson.AddToInventory(itemToPickup.GetResource());
                    if (am > 0)
                    {
                        itemToPickup.gameObject.SetActive(false);
                        // Only if automated tasks is enabled, find next item/warehouse
                        if(automatedTasks) 
                        {
                            Transform nearestItem = GameManager.GetVillage().GetNearestItemInRange(itemToPickup, transform.position, thisPerson.GetCollectingRange());
                            if (nearestItem != null && thisPerson.GetFreeInventorySpace() > 0) SetTargetTransform(nearestItem);
                            else
                            {
                                Transform nearestTreeStorage = GameManager.GetVillage().GetNearestBuildingType(transform.position, BuildingType.StorageFood);
                                if (nearestTreeStorage != null) SetTargetTransform(nearestTreeStorage);
                                else
                                    GameManager.GetVillage().NewMessage("Baue ein Lagerplatz für das Holz!");
                                if (nearestItem != null && nearestTreeStorage != null) AddTargetTransform(nearestItem);
                            }
                        }
                    }
                    else
                    {
                        GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " kann " + itemToPickup.GetName() + " nicht auflesen");
                    }
                }
                else
                {
                    GameManager.GetVillage().NewMessage("Wo ist der Gegenstand hin... ?");
                }
                break;
            case TaskType.Walk: // Walk towards the given target
                // Get next position to walk towards
                Vector3 nextTarget = ct.target;
                // Distance from taret at which we call it reached
                float stopRadius = 0.1f;
                // If still nodes to walk in path, get them as nextTarget
                if (currentPath != null && currentPath.Count > 0)
                {
                    Node nextNode = currentPath[0];
                    nextTarget = nextNode.transform.position;//Grid.ToWorld(nextNode.GetX(), nextNode.GetY());
                }
                else if (currentPath == null)
                {
                    break;
                }
                // Get forward vector towards target
                Vector3 diff = nextTarget - transform.position;
                diff.y = 0;
                float distance = Vector3.SqrMagnitude(diff);
                if (ct.targetTransform != null && currentPath.Count == 1)
                {
                    // standard stop radius for objects
                    stopRadius = 0.8f;
                    // Set custom stop radius for trees
                    if (ct.targetTransform.tag == "Plant" && plant != null)
                    {
                        stopRadius = plant.GetRadiusInMeters();
                    }
                    else if (ct.targetTransform.tag == "Item")
                    {
                        stopRadius = 0.5f;
                    }
                    else if (ct.targetTransform.tag == "Special")
                    {
                        stopRadius = 0.7f;
                    }
                    else if (ct.targetTransform.tag == "Building")
                    {
                        stopRadius = 1f;
                    }
                    else
                    {
                        Debug.Log("Unhandled stop radius");
                    }
                    //Debug.Log("dist/stopr:\t"+distance+"/"+stopRadius);
                }
                /* TODO: better factor */
                stopRadius *= 0.1f;
                if (currentPath.Count > 1 || distance > stopRadius)
                {
                    currentMoveSpeed += 0.05f * moveSpeed;
                    if (currentMoveSpeed > moveSpeed) currentMoveSpeed = moveSpeed;
                    // Update position/rotation towards target
                    transform.rotation = Quaternion.LookRotation(diff);
                    /*if (currentPath.Count == 1)
                        transform.position = Vector3.Lerp(transform.position, nextTarget, Time.deltaTime * 2);
                    else*/
                        transform.position += diff.normalized * moveSpeed * Time.deltaTime;
                }

                if (distance <= stopRadius && currentPath.Count > 0)
                {
                    lastNode = currentPath[0];
                    /* IF LAST PATH, ADD ACTIVITY */
                    //lastPath = currentPath[0].transform.position;
                    currentPath.RemoveAt(0);

                    /*// If last element was removed, check what to do
                    if (currentPath.Count == 1 && currentPath[0].IsPeopleOccupied())
                        currentPath.RemoveAt(0);*/

                    if (currentPath.Count == 0)
                    {
                        Vector3 prevRot = transform.rotation.eulerAngles;
                        if (routine[0].targetTransform != null)
                            transform.LookAt(routine[0].targetTransform);
                        prevRot.y = transform.rotation.eulerAngles.y;
                        transform.rotation = Quaternion.Euler(prevRot);
                        NextTask();
                        currentMoveSpeed = 0f;
                    }
                }
                        
                        
                break;
        }
    }
    public void NextTask()
    {
        // Remove current Task from routine
        routine.RemoveAt(0);

        // If a new path has to be walked, find it with astar
        if (routine.Count > 0 && routine[0].taskType == TaskType.Walk)
        {
            FindPath(routine[0].target, routine[0].targetTransform);
        }
    }

    public Person GetPerson()
    {
        return thisPerson;
    }
    public void SetPerson(Person person)
    {
        thisPerson = person;
    }

    public void SetTargetTransform(Transform target)
    {
        SetTargetTransform(target, target.position);
    }
    public void AddTargetTransform(Transform target)
    {
        AddTargetTransform(target, target.position);
    }
    public void AddTargetTransform(Transform target, Vector3 targetPosition)
    {
        Task walkTask = new Task(TaskType.Walk, targetPosition, target);
        Task targetTask = TargetTaskFromTransform(target);

        int rc = routine.Count;
        routine.Add(walkTask);
        if (targetTask != null) routine.Add(targetTask);

        if (rc == 0)
        {
            FindPath(target.position, target);
        }
    }
    public void SetTargetTransform(Transform target, Vector3 targetPosition)
    {
        routine.Clear();
        AddTargetTransform(target, targetPosition);
    }
    public void SetTargetPosition(Vector3 newTarget)
    {
        Vector3 gridPos = Grid.ToGrid(newTarget);
        if (Grid.GetNode((int)gridPos.x, (int)gridPos.z).nodeObject != null)
        {
            SetTargetTransform(Grid.GetNode((int)gridPos.x, (int)gridPos.y).nodeObject);
            return;
        }
        routine.Clear();
        Task walkTask = new Task(TaskType.Walk, newTarget);
        routine.Add(walkTask);
        FindPath(newTarget, null);
    }

    public Task TargetTaskFromTransform(Transform target)
    {
        Task targetTask = null;
        if (target != null)
        {
            switch (target.tag)
            {
                case "Building":
                    BuildingScript bs = target.GetComponent<BuildingScript>();
                    if (bs.bluePrint)
                    {
                        /* TODO: building master needs to finish building */
                        targetTask = new Task(TaskType.Build, target);
                    }
                    else
                    {
                        Building b = bs.GetBuilding();
                        switch (b.GetBuildingType())
                        {
                            // Warehouse activity 
                            case BuildingType.StorageMaterial:
                            case BuildingType.StorageFood:
                                GameResources res = thisPerson.GetInventory();
                                if (res != null && res.GetAmount() > 0)
                                {
                                    targetTask = new Task(TaskType.BringToWarehouse, target);
                                }
                                else
                                {
                                    targetTask = new Task(TaskType.TakeFromWarehouse, target);
                                }
                                break;
                            case BuildingType.Food:
                                if (b.GetID() == 4) // Fischerplatz
                                {
                                    targetTask = new Task(TaskType.Fisherplace, target);
                                }
                                break;
                        }
                    }
                    break;
                case "Plant":
                    Plant plant = target.GetComponent<Plant>();
                    if (plant.type == PlantType.Tree)
                    {
                        if (thisPerson.GetJob().GetID() == 2) //Holzfäller
                        {
                            targetTask = new Task(TaskType.CutTree, target);
                        }
                        else
                        {
                            GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " kann keinen Baum fällen!");
                        }
                    }
                    else if (plant.type == PlantType.Mushroom)
                    {
                        targetTask = new Task(TaskType.CollectMushroom, target);
                    }
                    else if (plant.type == PlantType.Reed)
                    {
                        targetTask = new Task(TaskType.Fishing, target);
                    }
                    break;
                case "Special":
                    Campfire cf = target.GetComponent<Campfire>();
                    if (cf != null)
                    {
                        targetTask = new Task(TaskType.Campfire, target);
                    }
                    break;
                case "Item":
                    Item it = target.GetComponent<Item>();
                    if (it != null)
                    {
                        targetTask = new Task(TaskType.PickupItem, target);
                    }
                    break;
            }
        }
        return targetTask;
    }

    public void FindPath(Vector3 targetPosition, Transform targetTransform)
    {
        currentMoveSpeed = 0.05f;
        Vector3 start = Grid.ToGrid(transform.position);
        Vector3 end = Grid.ToGrid(targetPosition);
        int sx = (int)start.x; int sy = (int)start.z;
        int ex = (int)end.x; int ey = (int)end.z;
        if (!Grid.ValidNode(sx, sy) || !Grid.ValidNode(ex, ey)) return;
        // if clicked on node with object, but not object
        if (Grid.GetNode(ex, ey).nodeObject != null)
        {
            //Debug.Log("test");
            targetTransform = Grid.GetNode(ex, ey).nodeObject;
        }
        currentPath = AStar.FindPath(sx, sy, ex, ey);
        // If path is empty and start node is not equal to end node, don't do anything
        int dx = ex - sx; int dy = ey - sy;
        if (currentPath.Count == 0 && ((dx * dx + dy * dy) > 1 || (sx == ex && sy == ey)))
        {
            routine.RemoveAt(0);
        }

        // Debug Path
        /*for(int i = 0; i < currentPath.Count; i++)
            Debug.Log("path"+i+"; "+currentPath[i].GetX() +";"+ currentPath[i].GetY());*/
    }

    void OnMouseOver()
    {
        if (CameraController.inputState != 2) return;
        highlighted = true;

        if (Input.GetMouseButtonDown(0)) {
            OnClick();
        }
    }
    void OnMouseExit()
    {
        highlighted = false;
    }

    public void OnClick()
    {
        if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            DeselectAll();
        OnSelect();
    }
    public void OnSelect()
    {
        selectedPeople.Add(this);
        selected = true;
    }

    public void OnDeselect()
    {
        selected = false;
    }

    public static void DeselectAll()
    {
        foreach(PersonScript ps in selectedPeople)
            ps.OnDeselect();
        
        selectedPeople.Clear();
    }

    public float GetHealthFactor()
    {
        return health / maxHealth;
    }

    // Get Condition of person (0=dead, 4=well)
    public int GetCondition()
    {
        float hf = GetHealthFactor();
        if(hf > 0.75f) return 4;
        if(hf > 0.5f) return 3;
        if(hf > 0.15f) return 2;
        if(hf > 0) return 1;

        return 0;
    }
    // Convert condition to string
    public string GetConditionStr()
    {
        int cond = GetCondition();
        switch(cond)
        {
            case 0: return "Tot";
            case 1: return "Hungersnot";
            case 2: return "Hungrig";
            case 3: return "Gesund";
            case 4: return "Gesund";
            default: return "unknown";
        }
    }
    // Convert condition to color
    public Color GetConditionCol()
    {
        int cond = GetCondition();
        switch(cond)
        {
            case 0: return new Color(0,0,0,0.6f);
            case 1: return new Color(1,0,0,0.6f);
            case 2: return new Color(1,0.6f,0.15f,0.6f);
            case 3: case 4:
             return new Color(0,1,0.15f,0.6f);
            default: return new Color(0,0,0,0);
        }
    }
}


/*if (gotoTarget)
{
    if (currentPath.Count == 0)
    {
        float stopRadius = 1f;
        transform.rotation = Quaternion.LookRotation(diff);
        if (distance > stopRadius)
        {
            transform.position += diff.normalized * moveSpeed * Time.deltaTime * 60f;
        }
        else
        {
            gotoTarget = false;

            if (targetTransform != null)
            {
                if (targetTransform.tag == "Tree")
                {
                    if (thisPerson.GetJob().GetID() == 2) //Holzfäller
                    {
                        GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " hat einen Baum gefällt!");
                        Tree tree = targetTransform.GetComponent<Tree>();
                        tree.Fall();
                        int mat = thisPerson.AddToInventory(GameResources.Wood(tree.GetMaterial()));
                        tree.TakeMaterial(mat);
                        activity = 22;
                    }
                    else
                    {
                        GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " kann keinen Baum fällen!");
                    }
                }
                else if (targetTransform.tag == "Building")
                {
                    activity = 1;
                }
            }
        }
    }
    else
    {
        Vector3 diff = Grid.ToWorld(nextNode.GetX(), nextNode.GetY()) - transform.position;
        transform.rotation = Quaternion.LookRotation(diff);
        diff.y = 0;
        float distance = Vector3.SqrMagnitude(diff);
        if (distance > 1f)
        {
            transform.position += diff.normalized * moveSpeed * Time.deltaTime * 60f;
        }
        else
        {
            if (currentPath.Count > 0) currentPath.RemoveAt(0);
            if (currentPath.Count == 0)
            {
                if (targetTransform != null)
                {
                    if (targetTransform.tag == "Building" && thisPerson.GetInventory() == null || thisPerson.GetInventory().GetAmount() == 0)
                    {
                        GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " hat nichts im Inventar!");
                        gotoTarget = false;
                    }
                }
                else
                {
                    gotoTarget = false;
                }
            }
        }
    }*/

/*if (currentPath[0].IsOccupied())
                            {
                                FindPath(routine[0].target, routine[0].targetTransform);
                            }*/
//Debug.Log("finished task");

/*if (targetTransform != null)
{
    switch (targetTransform.tag)
    {
        case "Building":
            Building b = targetTransform.GetComponent<BuildingScript>().GetBuilding();
            switch (b.GetID())
            {
                // If target is a warehouse, only let person in, if he has anything in inventory
                case (int)BuildingID.Warehouse:
                    if ((thisPerson.GetInventory() == null || thisPerson.GetInventory().GetAmount() == 0))
                    {
                        GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " hat nichts im Inventar!");
                        gotoTarget = false;
                    }
                    break;
            }
            break;
    }
}
else
{
    gotoTarget = false;
}*/
//}
/* else if (targetTransform != null)
 {
     switch (targetTransform.tag)
     {
         case "Building":
             Building b = targetTransform.GetComponent<BuildingScript>().GetBuilding();
             switch (b.GetID())
             {
                 // Warehouse activity 
                 case (int)BuildingID.Warehouse:
                     activity = 1;
                     break;
             }
             break;
         case "Tree":
             if (thisPerson.GetJob().GetID() == 2) //Holzfäller
             {
                 activity = 0;
             }
             else
             {
                 GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " kann keinen Baum fällen!");
             }
             break;
     }
     gotoTarget = false;
 }
 else
 {
     gotoTarget = false;
 }*/
