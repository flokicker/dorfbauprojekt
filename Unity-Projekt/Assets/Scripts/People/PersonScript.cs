using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonScript : MonoBehaviour {

    public static HashSet<PersonScript> allPeople = new HashSet<PersonScript>();
    public static HashSet<PersonScript> selectedPeople = new HashSet<PersonScript>();

    public int ID;

    private Person thisPerson;
    public bool selected, highlighted;

    public float health, maxHealth;
    private float saturationTimer, saturation;

    private float moveSpeed = 1.2f;
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
        health = maxHealth/3f;
        saturationTimer = 0;

        outline = GetComponent<cakeslice.Outline>();
        outline.enabled = false;
        canvas = transform.Find("Canvas").transform;
        imageHP = canvas.Find("Health").Find("ImageHP").GetComponent<Image>();

        Vector3 gp = Grid.ToGrid(transform.position);
        lastNode = Grid.GetNode((int)gp.x, (int)gp.z);

        ID = allPeople.Count;
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
            GameResources food = thisPerson.inventoryFood;

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
        float maxWidth = canvas.Find("Health").Find("ImageHPBack").GetComponent<RectTransform>().rect.width - 4;
        //personInfoHealthbar.rectTransform.offsetMax = new Vector2(-(2+ maxWidth * (1f-ps.GetHealthFactor())),-2);
        imageHP.rectTransform.offsetMax = new Vector2(-(1+ maxWidth * (1f-GetHealthFactor())),-1);
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
        GameResources invFood = thisPerson.inventoryFood;
        GameResources invMat = thisPerson.inventoryMaterial;
        Village myVillage = GameManager.GetVillage();
        Transform nearestTrsf = null;
        if (ct.targetTransform != null)
        {
            plant = ct.targetTransform.GetComponent<Plant>();
            bs = ct.targetTransform.GetComponent<BuildingScript>();
        }
        int am = 0;
        switch (ct.taskType)
        {
            case TaskType.CutTree: // Chopping a tree
            case TaskType.CullectMushroomStump: // Collect the mushroom from stump
            case TaskType.MineRock: // Mine the rock to get stones
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
                            int mat = plant.materialPerChop;
                            if (plant.material < mat) mat = plant.material;
                            mat = thisPerson.AddToInventory(new GameResources(plant.materialID, mat));
                            plant.TakeMaterial(mat);
                            if(GameManager.debugging)
                            GameManager.GetVillage().NewMessage(mat + " added to inv");

                            // If still can mine plant, continue
                            if(mat != 0 && thisPerson.GetFreeInventorySpace() > 0) break;
                        }
                        
                        if(routine.Count <= 1)
                        {
                            // only automatically find new tree to cut if person is a lumberjack
                            if(ct.taskType == TaskType.CutTree && thisPerson.job.id == Job.LUMBERJACK)
                            {
                                if(thisPerson.GetFreeInventorySpace() > 0)
                                    nearestTrsf = myVillage.GetNearestPlant(PlantType.Tree, transform.position, thisPerson.GetTreeCutRange());
                                else
                                    nearestTrsf = myVillage.GetNearestBuildingType(transform.position, BuildingType.StorageMaterial);
                            }
                            else if(ct.taskType == TaskType.CullectMushroomStump)
                            {
                            }
                            else if(ct.taskType == TaskType.MineRock)
                            {
                            }

                            if (nearestTrsf) 
                            {
                                SetTargetTransform(nearestTrsf);
                                break;
                            }
                        }

                        NextTask();
                    }
                }
                else if (ct.taskTime >= 1f / choppingSpeed)
                {
                    ct.taskTime = 0;
                    plant.Mine();
                }
                break;
            case TaskType.Fisherplace: // Making food out of fish
                // Look for raw fish in foodInventory
                if (invFood == null || invFood.GetAmount() == 0 || invFood.id != GameResources.RAWFISH)
                {
                    NextTask();
                }
                else
                {
                    // Convert raw fish into real fish
                    thisPerson.inventoryFood = new GameResources(GameResources.FISH, invFood.amount);
                    
                    // Walk to nearest food storage
                    Transform nearestFoodStorage = myVillage.GetNearestBuildingType(transform.position, BuildingType.StorageFood);
                    if(nearestFoodStorage != null) SetTargetTransform(nearestFoodStorage);
                    // else WalkToCenter();
                }
                break;
            case TaskType.BringToWarehouse: // Bringing material to warehouse
                if(invMat != null)
                {
                    if (bs.GetBuilding().GetBuildingType() == BuildingType.StorageMaterial && invMat.GetResourceType() == ResourceType.BuildingMaterial
                        && invMat.GetAmount() > 0) 
                    {
                        GameManager.GetVillage().Restock(invMat);
                        invMat.Take(invMat.GetAmount());

                        if(invMat.id == GameResources.WOOD && routine.Count <= 1 && thisPerson.job.id == Job.LUMBERJACK)
                        {
                            nearestTrsf = myVillage.GetNearestPlant(PlantType.Tree, transform.position, thisPerson.GetTreeCutRange());
                            if(nearestTrsf)
                            {
                                SetTargetTransform(nearestTrsf);
                                break;
                            }
                        }
                    }
                }
                if(invFood != null)
                {
                    if (bs.GetBuilding().GetBuildingType() == BuildingType.StorageFood && invFood.GetResourceType() == ResourceType.Food
                        && invFood.amount > 0) 
                    {
                        GameManager.GetVillage().Restock(invFood);
                        invFood.Take(invFood.GetAmount());

                        if(invFood.id == GameResources.FISH && routine.Count <= 1 && thisPerson.job.id == Job.FISHER)
                        {
                            nearestTrsf = myVillage.GetNearestPlant(PlantType.Reed, transform.position, thisPerson.GetCollectingRange());
                            if(nearestTrsf)
                            {
                                SetTargetTransform(nearestTrsf);
                                break;
                            }
                        }
                    }
                }
                NextTask();
                break;
            case TaskType.TakeFromWarehouse: // Taking material from warehouse
                GameResources takeRes = null; 
                // If building is material storage, if building is food storage, get mushrooms
                if (bs.GetBuilding().GetBuildingType() == BuildingType.StorageMaterial)
                {
                    takeRes = new GameResources(GameResources.WOOD, thisPerson.GetInventorySize());
                }
                else if (bs.GetBuilding().GetBuildingType() == BuildingType.StorageFood)
                {
                    takeRes = new GameResources(GameResources.MUSHROOM, thisPerson.GetInventorySize());
                }
                // Take resources from storage into person's inventory
                if (takeRes != null)
                {
                    int take = GameManager.GetVillage().Take(takeRes);
                    takeRes.SetAmount(take);
                    thisPerson.AddToInventory(takeRes);
                }
                NextTask();
                break;
            case TaskType.Campfire: // Restock campfire fire wood
                Campfire cf = routine[0].targetTransform.GetComponent<Campfire>();
                if(invMat != null && invMat.GetID() == GameResources.WOOD)
                    invMat.Take(cf.Restock(invMat.GetAmount()));

                NextTask();
                break;
            case TaskType.Fishing: // Do Fishing
                if (thisPerson.job.id == Job.FISHER && (invFood == null || invFood.GetAmount() == 0 || invFood.GetID() == GameResources.RAWFISH))
                {
                    if (ct.taskTime >= fishingTime)
                    {
                        ct.taskTime = 0;
                        if (Random.Range(0, 5) == 0 && plant.material >= 1)
                        {
                            plant.Break();
                            int amount = thisPerson.AddToInventory(new GameResources(GameResources.RAWFISH, 1));
                            plant.material -= amount;
                        }
                    }
                    if(routine.Count <= 1)
                    {
                        if(thisPerson.GetFreeInventorySpace() == 0)
                        {
                            // get nearest fishermanPlace
                            nearestTrsf = myVillage.GetNearestBuildingID(transform.position, 4);
                        }
                        else if(plant.material == 0)
                        {
                            nearestTrsf = myVillage.GetNearestPlant(PlantType.Reed, transform.position, thisPerson.GetReedRange());
                            if(!nearestTrsf)
                                nearestTrsf = myVillage.GetNearestBuildingID(transform.position, 4);
                        }

                        if(nearestTrsf)
                        {
                            SetTargetTransform(nearestTrsf);
                            break;
                        }
                    }
                    if(thisPerson.GetFreeInventorySpace() == 0 || plant.material == 0)
                        NextTask();
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.Build: // Put resources into blueprint building
                if (invMat == null) NextTask();
                else if(ct.taskTime >= buildingTime)
                {
                    ct.taskTime = 0;
                    bool built = false;
                    foreach (GameResources r in bs.resourceCost)
                    {
                        if (invMat.GetID() == r.GetID() && r.GetAmount() > 0 && invMat.GetAmount() > 0 && !built)
                        {
                            built = true;
                            invMat.Take(1);
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

                // Find another mushroom to collect
                if(routine.Count <= 1)
                {
                    Transform nearestMushroom = GameManager.GetVillage().GetNearestPlant(PlantType.Mushroom, transform.position, thisPerson.GetCollectingRange());
                    Transform nearestFoodStorage = GameManager.GetVillage().GetNearestBuildingType(transform.position, BuildingType.StorageFood);
                    if (thisPerson.GetFreeInventorySpace() == 0)
                    {
                        if (nearestFoodStorage != null) SetTargetTransform(nearestFoodStorage);
                        else WalkToCenter();
                    }
                    else
                    {
                        if (nearestMushroom != null) SetTargetTransform(nearestMushroom);
                        else if (nearestFoodStorage != null) SetTargetTransform(nearestFoodStorage);
                        else WalkToCenter();
                    }
                }
                break;
            case TaskType.Harvest:
                // add resources to persons inventory
                if(plant.gameObject.activeSelf)
                {
                    if(plant.material == 0)
                    {
                        plant.Break();
                        plant.gameObject.SetActive(false);
                    }
                    else
                    {
                        am = thisPerson.AddToInventory(new GameResources(plant.materialID, plant.material));
                        if(am > 0) 
                        {
                            // Destroy collected plant
                            plant.Break();
                            plant.gameObject.SetActive(false);
                        }
                    }
                }
                NextTask();
                break;
            case TaskType.PickupItem: // Pickup the item
                Item itemToPickup = routine[0].targetTransform.GetComponent<Item>();
                if (itemToPickup != null && itemToPickup.gameObject.activeSelf)
                {
                    am = thisPerson.AddToInventory(itemToPickup.GetResource());
                    if (am > 0)
                    {
                        itemToPickup.gameObject.SetActive(false);

                        // Automatically pickup other items in reach or go to warehouse if inventory is full
                        Transform nearestItem = GameManager.GetVillage().GetNearestItemInRange(itemToPickup, transform.position, thisPerson.GetCollectingRange());
                        if (routine.Count == 1 && nearestItem != null && thisPerson.GetFreeInventorySpace() > 0 && nearestItem.gameObject.activeSelf)
                        { 
                            SetTargetTransform(nearestItem);
                            break;
                        }
                        else
                        {
                            //Transform nearestFoodStorage = GameManager.GetVillage().GetNearestBuildingType(transform.position, BuildingType.StorageFood);
                            //if (nearestFoodStorage != null) SetTargetTransform(nearestFoodStorage);
                            //if (nearestItem != null && nearestFoodStorage != null) AddTargetTransform(nearestItem);
                        }
                    }
                    else
                    {
                        GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " kann " + itemToPickup.GetName() + " nicht auflesen");
                    }
                }
                NextTask();
                break;
            case TaskType.Walk: // Walk towards the given target
                
                // Firstly check if already in stopradius of targetObject
                float objectStopRadius = 0f;
                Vector3 diff;
                if(ct.targetTransform != null)
                {
                    // standard stop radius for objects
                    objectStopRadius = 0.8f;
                    // Set custom stop radius for trees
                    if (ct.targetTransform.tag == "Plant" && plant != null)
                    {
                        objectStopRadius = plant.GetRadiusInMeters();
                    }
                    else if (ct.targetTransform.tag == "Item")
                    {
                        objectStopRadius = 0.1f;
                    }
                    else if (ct.targetTransform.tag == "Building")
                    {
                        BuildingScript tarBs = ct.targetTransform.GetComponent<BuildingScript>();
                        if(tarBs.GetBuilding().walkable)
                            objectStopRadius = 1f;
                        else
                            objectStopRadius = 0.3f;
                    }
                    else
                    {
                        Debug.Log("Unhandled stop radius");
                    }
                    diff = ct.targetTransform.position - transform.position;
                    float tmpDist = Vector3.SqrMagnitude(diff);
                    if(tmpDist < objectStopRadius * Grid.SCALE)
                    {
                        EndCurrentPath();
                        break;
                    }
                }
                // Get next position to walk towards
                Vector3 nextTarget = ct.target;
                // Distance from taret at which we call it reached
                float stopRadius = 0.1f;
                // If still nodes to walk in path, get them as nextTarget
                if (currentPath != null && currentPath.Count > 0)
                {
                    Node nextNode = currentPath[0];
                    // Debug.Log(Vector3.Distance(Grid.ToWorld(nextNode.gridX,nextNode.gridY),Grid.ToWorld(lastNode.gridX,lastNode.gridY)));
                    nextTarget = nextNode.transform.position;//Grid.ToWorld(nextNode.GetX(), nextNode.GetY());
                    //finalTargetNode = currentPath[currentPath.Count-1];
                }
                else if (currentPath == null)
                {
                    break;
                }
                // Get forward vector towards target
                diff = nextTarget - transform.position;
                diff.y = 0;
                if (ct.targetTransform != null)
                {
                    if(currentPath.Count == 0) stopRadius = objectStopRadius;
                }

                float distance = Vector3.SqrMagnitude(diff);

                /* TODO: better factor */
                stopRadius *= Grid.SCALE;

                //Debug.Log(distance.ToString("F2") + " / "+stopRadius.ToString("F2"));
                if (currentPath.Count > 1 || distance > stopRadius)
                {
                    currentMoveSpeed += 0.05f * moveSpeed;
                    if (currentMoveSpeed > moveSpeed) currentMoveSpeed = moveSpeed;
                    
                    // Update position/rotation towards target
                    transform.rotation = Quaternion.LookRotation(diff);

                    transform.position += diff.normalized * moveSpeed * Time.deltaTime;

                    foreach(Plant p in GameManager.GetVillage().nature.flora)
                    {
                        CheckHideableObject(p,p.currentModel);
                        
                    }
                    foreach(Item p in GameManager.GetVillage().items)
                    {
                        CheckHideableObject(p,p.transform);
                    }
                }

                if (distance <= stopRadius)
                {
                    // If path has ended, continue to next task
                    if (currentPath.Count == 0)
                    {
                        EndCurrentPath();
                        break;
                    }

                    if(currentPath[0].objectWalkable)
                        lastNode = currentPath[0];

                    // remove path node
                    currentPath.RemoveAt(0);
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

    private void EndCurrentPath()
    {
        Vector3 prevRot = transform.rotation.eulerAngles;
        if (routine[0].targetTransform != null)
            transform.LookAt(routine[0].targetTransform);
        prevRot.y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(prevRot);
        NextTask();
        currentMoveSpeed = 0f;
    }

    // Walk back to center
    public void WalkToCenter()
    {
        SetTargetPosition(Vector3.zero);
    }

    public void CheckHideableObject(HideableObject p, Transform model)
    {
        if(p.inBuildRadius) return;
        if(!p) return;
        float dist = Mathf.Abs(transform.position.x - p.transform.position.x) + Mathf.Abs(transform.position.z - p.transform.position.z);
        bool inRadius = dist < 10;
        if(p.personIDs.Contains(ID)) 
        {
            if(!inRadius)
            {
                p.personIDs.Remove(ID);
                if(p.personIDs.Count == 0) {
                    p.gameObject.SetActive(false);
                    p.isHidden = true;
                    model.GetComponent<cakeslice.Outline>().enabled = false;
                }
            }
        }
        else
        {
            if(inRadius)
            { 
                p.personIDs.Add(ID);
                p.gameObject.SetActive(true);
                p.isHidden = false;
            }
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

        /*int foundSameWalk = 0;
        // Make sure not to redo same task twice
        foreach(Task t in routine)
            if(t.taskType == TaskType.Walk)
            {
                if(Grid.ToGrid(t.target) - Grid.ToGrid(targetPosition)) 
                {
                    if(foundSameWalk > 0) return;
                    else foundSameWalk = 1;
                }
                else foundSameWalk = 0;
            }*/

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
        routine.Clear();
        AddTargetPosition(newTarget);
    }
    public void AddTargetPosition(Vector3 newTarget)
    {
        Vector3 gridPos = Grid.ToGrid(newTarget);
        if (Grid.GetNode((int)gridPos.x, (int)gridPos.z).nodeObject != null)
        {
            AddTargetTransform(Grid.GetNode((int)gridPos.x, (int)gridPos.z).nodeObject);
            return;
        }
        Task walkTask = new Task(TaskType.Walk, newTarget);
        routine.Add(walkTask);
        if(routine.Count == 1) FindPath(newTarget, null);
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
                    if (bs.blueprint)
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
                                GameResources res = null;

                                // if building is material storage, get material from inventory
                                if(b.GetBuildingType() == BuildingType.StorageMaterial) res = thisPerson.inventoryMaterial;
                                else res = thisPerson.inventoryFood;

                                // if no inventory resource, take from warehouse
                                if (res != null && res.GetAmount() > 0) targetTask = new Task(TaskType.BringToWarehouse, target);
                                else targetTask = new Task(TaskType.TakeFromWarehouse, target);

                                break;
                            case BuildingType.Food:
                                if (b.GetID() == 4) // Fischerplatz
                                {
                                    targetTask = new Task(TaskType.Fisherplace, target);
                                }
                                break;
                            case BuildingType.Luxury:
                                if (b.GetID() == 8) // Campfire
                                {
                                    Campfire cf = target.GetComponent<Campfire>();
                                    if (cf != null)
                                    {
                                        targetTask = new Task(TaskType.Campfire, target);
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case "Plant":
                    Plant plant = target.GetComponent<Plant>();
                    if (plant.type == PlantType.Tree)
                    {
                        // Every person can cut trees
                        targetTask = new Task(TaskType.CutTree, target);
                        /*if (thisPerson.GetJob().GetID() == 2) //Holzfäller
                        {
                            targetTask = new Task(TaskType.CutTree, target);
                        }
                        else
                        {
                            GameManager.GetVillage().NewMessage(thisPerson.GetFirstName() + " kann keinen Baum fällen!");
                        }*/
                    }
                    else if (plant.type == PlantType.Mushroom)
                    {
                        targetTask = new Task(TaskType.CollectMushroom, target);
                    }
                    else if (plant.type == PlantType.MushroomStump)
                    {
                        targetTask = new Task(TaskType.CullectMushroomStump, target);
                    }
                    else if(plant.type == PlantType.Corn)
                    {
                        targetTask = new Task(TaskType.Harvest, target);
                    }
                    else if (plant.type == PlantType.Reed)
                    {
                        if(thisPerson.job.id == Job.FISHER)
                            targetTask = new Task(TaskType.Fishing, target);
                        else
                            GameManager.GetVillage().NewMessage(thisPerson.firstName + " kann nicht fischen");
                    }
                    else if (plant.type == PlantType.Rock)
                    {
                        targetTask = new Task(TaskType.MineRock, target);
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
        //if(sx == ex && sy == ey) routine.RemoveAt(0);
        // if clicked on node with object, but not object
        if (Grid.GetNode(ex, ey).nodeObject != null)
        {
            targetTransform = Grid.GetNode(ex, ey).nodeObject;
        }
        if(!Grid.GetNode(sx, sy).objectWalkable)
        {
            sx = lastNode.gridX;
            sy = lastNode.gridY;
        }
        currentPath = AStar.FindPath(sx, sy, ex, ey);
        // If path is empty and start node is not equal to end node, don't do anything
        int dx = ex - sx; int dy = ey - sy;
        if (currentPath.Count == 0 && ((dx * dx + dy * dy) > 1 || (sx == ex && sy == ey)))
        {
            NextTask();
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
