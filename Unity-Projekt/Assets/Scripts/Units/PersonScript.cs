using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization;

[System.Serializable]
public enum Gender
{
    Male, Female
}
public class PersonScript : MonoBehaviour {

    // Collection of all people and selected people
    public static HashSet<PersonScript> allPeople = new HashSet<PersonScript>();
    public static HashSet<PersonScript> selectedPeople = new HashSet<PersonScript>();

    // Person info
    public int nr;
    public string firstName, lastName;
    public Gender gender;
    public int age;
    public Job job;
    public int viewDistance;
    public float health, hunger;

    // Inventory
    public GameResources inventoryMaterial, inventoryFood;

    /*public Person(int nr, string firstName, string lastName, Gender gender, int age, Job job)
    {
        this.nr = nr;
        this.firstName = firstName;
        this.lastName = lastName;
        this.gender = gender;
        this.age = age;
        this.job = job;
        this.inventoryMaterial = null;
        this.inventoryFood = null;
        viewDistance = 10;

        health = 80;
        hunger = 100;
    }*/

    public bool selected, highlighted;

    // Building where this Person is working at
    public Building workingBuilding;

    private float saturationTimer, saturation;

    private float moveSpeed = 0.7f;
    private float currentMoveSpeed = 0f;

    // Animation controller
    private Animator animator;

    // time since last task
    private float noTaskTime = 0, checkCampfireTime = 0;

    private List<Node> currentPath;
    public List<Task> routine = new List<Task>();
    private Node lastNode;

    // Activity speeds/times
    private float choppingSpeed = 0.8f, putMaterialSpeed = 2f, buildSpeed = 2f;
    private float fishingTime = 1, processFishTime = 2f, collectingSpeed = 2f, hitTime = 2f;

    private Transform canvas;
    private Image imageHP, imageFood;

    private bool automatedTasks = false;

    private ClickableUnit clickableUnit;

    private bool inFoodRange = false;

	// Use this for initialization
    void Start()
    {
        saturationTimer = 0;

        workingBuilding = null;

        // handles all outline/interaction stuff
        clickableUnit = GetComponentInChildren<SkinnedMeshRenderer>().gameObject.AddComponent<ClickableUnit>();
        clickableUnit.SetScriptedParent(transform);

        // Animation
        animator = GetComponent<Animator>();

        canvas = GetComponentInChildren<Canvas>().transform;
        imageHP = canvas.Find("Health").Find("ImageHP").GetComponent<Image>();
        imageFood = canvas.Find("Food").Find("ImageHP").GetComponent<Image>();

        Vector3 gp = Grid.ToGrid(transform.position);
        lastNode = Grid.GetNode((int)gp.x, (int)gp.z);

        nr = allPeople.Count;
        allPeople.Add(this);

        foreach(Plant p in Nature.flora)
        {
            CheckHideableObject(p,p.currentModel);
            
        }
        foreach(Item p in Item.allItems)
        {
            CheckHideableObject(p,p.transform);
        }
	}

	// Update is called once per frame
    void Update()
    {
        // last visited node update
        lastNode.SetPeopleOccupied(false);

        // check waht to do
        if (routine.Count > 0)
        {
            Task ct = routine[0];
            noTaskTime = 0;
            checkCampfireTime = 0;
            ExecuteTask(ct);
        }
        else
        {
            noTaskTime += Time.deltaTime;
            checkCampfireTime += Time.deltaTime;

            if(checkCampfireTime >= 1)
            {
                checkCampfireTime = 0;
                Transform tf = GameManager.village.GetNearestBuildingID(transform.position, Building.CAMPFIRE);
                    // after 300 sec, go to campfire, warmup and await new commands
                if(tf && tf.GetComponent<Campfire>().GetHealthFactor() < 0.5f && (GameManager.InRange(transform.position, tf.position, tf.GetComponent<Building>().buildRange) || noTaskTime >= 300))
                {
                    if(inventoryMaterial != null && inventoryMaterial.id == GameResources.WOOD && inventoryMaterial.amount > 0)
                    {
                        // go put wood into campfire
                        AddTargetTransform(tf, true);
                    }
                    else if(inventoryMaterial == null || inventoryMaterial.amount == 0 || (inventoryMaterial.id == GameResources.WOOD && GetFreeMaterialInventorySpace() > 0) )
                    {
                        Transform nearestStorage = GameManager.village.GetNearestStorageBuilding(transform.position, GameResources.WOOD, false);
                        Building bs = nearestStorage.GetComponent<Building>();
                        if(nearestStorage && bs.resourceCurrent[GameResources.WOOD] > 0)
                        {
                            // get wood and then put into campfire
                            AddResourceTask(TaskType.TakeFromWarehouse, bs, new GameResources(GameResources.WOOD, Mathf.Min(GetFreeMaterialInventorySpace(), bs.resourceCurrent[GameResources.WOOD])));
                            AddTargetTransform(tf, true);
                        }
                    }
                }
            }
        }
        
        saturationTimer += Time.deltaTime;

        inFoodRange = CheckIfInFoodRange();

        // Eat after not being saturated anymore
        if(saturationTimer >= saturation) {
            saturation = 0;
            saturationTimer = 0;

            // automatically take food from inventory first
            GameResources food = inventoryFood;

            // only take food from inventory if its food
            if(food != null && (food.amount == 0 || food.GetResourceType() != ResourceType.Food)) food = null;

            // check for food from warehouse
            if(food == null) food = GameManager.village.TakeFoodForPerson(this);
            
            if(food != null)
            {
                health += food.health;
                hunger += food.nutrition;
                saturation = food.nutrition;
                food.Take(1);
            }
        }

        // hp update
        float satFact = 0f;
        if(hunger <= 0) satFact = 1f;
        else if(hunger <= 10) satFact = 0.5f;
        else if(hunger <= 20) satFact = 0.2f;

        health -= Time.deltaTime * satFact;

        // hunger update
        satFact = 0.18f;
        if(saturation == 0) satFact = 1f;

        if(GameManager.GetTwoSeason() == 0) satFact *= 1.5f;

        hunger -= Time.deltaTime * satFact;

        if(hunger < 0) hunger = 0;
        if(hunger > 100) hunger = 100;

        if(health < 0) health = 0;
        if(health > 100) health = 100;

        // position player at correct ground height on terrain
        Vector3 terrPos = transform.position;
        terrPos.y = Terrain.activeTerrain.SampleHeight(terrPos) + Terrain.activeTerrain.transform.position.y;
        transform.position = terrPos;
        lastNode.SetPeopleOccupied(true);
	}

    // LateUpdate called after update
    void LateUpdate()
    {
        //GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity*0.95f;

        // Update UI canvas for health bar
        Camera camera = Camera.main;
        canvas.LookAt(canvas.position + camera.transform.rotation * Vector3.forward * 0.0001f, camera.transform.rotation * Vector3.up);
        canvas.gameObject.SetActive(highlighted || selected);
        float maxWidth = canvas.Find("Health").Find("ImageHPBack").GetComponent<RectTransform>().rect.width - 1;
        imageHP.rectTransform.offsetMax = new Vector2(-(0.5f+ maxWidth * (1f-GetHealthFactor())),-0.5f);
        imageHP.color = GetConditionCol();
        imageFood.rectTransform.offsetMax = new Vector2(-(0.5f+ maxWidth * (1f-GetFoodFactor())),-0.5f);
        imageFood.color = GetFoodCol();
        
        // Update outline component
        //outline.enabled = highlighted || selected;
        //outline.color = selected ? 1 : 0;
    }

    // update people list
    void OnDestroy()
    {
        allPeople.Remove(this);
        selectedPeople.Remove(this);
    }

    // Do a given task 'ct'
    void ExecuteTask(Task ct)
    {
        ct.taskTime += Time.deltaTime;
        Plant plant = null;
        Building bs = null;
        GameResources invFood = inventoryFood;
        GameResources invMat = inventoryMaterial;
        Village myVillage = GameManager.village;
        Transform nearestTrsf = null;
        List<GameResources> requirements = new List<GameResources>();
        List<GameResources> results = new List<GameResources>();
        if (ct.targetTransform != null)
        {
            plant = ct.targetTransform.GetComponent<Plant>();
            bs = ct.targetTransform.GetComponent<Building>();
        }
        int am = 0;
        switch (ct.taskType)
        {
            case TaskType.CutTree: // Chopping a tree
            case TaskType.CullectMushroomStump: // Collect the mushroom from stump
            case TaskType.MineRock: // Mine the rock to get stones
                if(!plant && ct.taskType == TaskType.CutTree && job.id == Job.LUMBERJACK)
                {
                    if(routine.Count <= 1)
                    {
                        // only automatically find new tree to cut if person is a lumberjack
                        if(ct.taskType == TaskType.CutTree && job.id == Job.LUMBERJACK)
                        {
                            if(GetFreeMaterialInventorySpace() > 0)
                                nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Tree, GetTreeCutRange());
                            else
                            {
                                StoreMaterialInventory();
                                break;
                            }
                        }
                        else if(ct.taskType == TaskType.CullectMushroomStump) { }
                        else if(ct.taskType == TaskType.MineRock) { }

                        if (nearestTrsf) 
                        {
                            SetTargetTransform(nearestTrsf, true);
                            break;
                        }
                    }
                    NextTask();
                    break;
                }
                if(!plant || ct.taskType == TaskType.CutTree && job.id != Job.LUMBERJACK) {
                    //(!plant || ct.taskType == TaskType.CutTree && job.id != Job.LUMBERJACK) && !(ct.taskType == TaskType.CutTree && job.id == Job.LUMBERJACK)) {
                    NextTask();
                    break;
                }
                if (plant.IsBroken())
                {
                    // Collect wood of fallen tree, by chopping it into pieces
                    if (ct.taskTime >= 1f / choppingSpeed)
                    {
                        ct.taskTime = 0;
                        //Transform nearestTree = GameManager.village.GetNearestPlant(PlantType.Tree, transform.position, GetTreeCutRange());

                        int freeSpace = 0;
                        ResourceType pmt = new GameResources(plant.materialID, 0).GetResourceType();
                        if(pmt == ResourceType.BuildingMaterial) freeSpace = GetFreeMaterialInventorySpace();
                        if(pmt == ResourceType.Food) freeSpace = GetFreeFoodInventorySpace();

                        if (plant.material > 0)
                        {
                            GameManager.UnlockResource(plant.materialID);

                            // Amount of wood per one chop gained
                            int mat = plant.materialPerChop;
                            if (plant.material < mat) mat = plant.material;
                            mat = AddToInventory(new GameResources(plant.materialID, mat));
                            plant.TakeMaterial(mat);

                            if(GameManager.debugging) GameManager.Msg(mat + " added to inv");

                            // If still can mine plant, continue
                            if(mat != 0 && plant.material > 0 && freeSpace > 0) break;
                        }
                        
                        if(routine.Count <= 1)
                        {
                            // only automatically find new tree to cut if person is a lumberjack
                            if(ct.taskType == TaskType.CutTree && job.id == Job.LUMBERJACK)
                            {
                                if(freeSpace > 0)
                                    nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Tree, GetTreeCutRange());
                                else
                                {
                                    StoreMaterialInventory();
                                    break;
                                }
                            }
                            else if(ct.taskType == TaskType.CullectMushroomStump)
                            {
                            }
                            else if(ct.taskType == TaskType.MineRock)
                            {
                            }

                            if (nearestTrsf) 
                            {
                                SetTargetTransform(nearestTrsf, true);
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
                /* TODO: decide if we want to check so that fisher can only work at his own workplace */

                // If person is not a fisher, he can't do anything here
                if (job.id != Job.FISHER)
                {
                    NextTask();
                }
                else
                {
                    // animal to process
                    requirements.Add(new GameResources(GameResources.RAWFISH, 1));
                    results.Add(new GameResources(GameResources.FISH, 1));
                    results.Add(new GameResources(GameResources.BONES, 1));

                    bool workingAlone = workingBuilding.workingPeople.Count == 1;

                    // Check what to do (leave rawfish, process fish to edible/bones, take from fisherplace to storage)
                    if(StoreResourceInBuilding(ct, bs, GameResources.RAWFISH)) { }
                    else
                    {
                        bool goFishing = workingAlone || workingBuilding.workingPeople[0] == this;
                        bool processFish = workingAlone || workingBuilding.workingPeople[1] == this;
                        // convert rawfish into fish
                        if(ProcessResource(ct, bs, requirements, results, processFishTime) && processFish) { }
                        // take fish into inventory of person
                        else if(TakeIntoInventory(ct, bs, GameResources.FISH) && processFish) { }
                        // take bones into inventory of person
                        else if(TakeIntoInventory(ct, bs, GameResources.BONES) && processFish) { }
                        // walk automatically to warehouse
                        else
                        {
                            bool addedTask = false;
                            if(invFood != null && invFood.id == GameResources.FISH && invFood.amount > 0 && processFish)
                            {
                                if(StoreFoodInventory()) addedTask = true;
                            }
                            if(invMat != null && invMat.id == GameResources.BONES && invMat.amount > 0 && processFish)
                            {
                                if(StoreMaterialInventory()) addedTask = true;
                            }
                            if(processFish && !workingAlone)
                            {
                                // go back to working on fisherplace
                                AddTargetTransform(bs.transform, true);
                                NextTask();
                            }
                            else if(goFishing && (!workingAlone || addedTask))
                            {
                                // automatically start fishing again
                                nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Reed, GetReedRange());
                                if(nearestTrsf) AddTargetTransform(nearestTrsf, true);
                                NextTask();
                            }
                            else
                            {
                                NextTask();
                            }
                        }
                    }
                }
                break;
            case TaskType.BringToWarehouse: // Bringing material to warehouse
                while(ct.taskRes.Count > 0 && ct.taskRes[0].amount == 0)
                    ct.taskRes.RemoveAt(0);
                if(ct.taskRes.Count == 0)
                {
                        // only automatically find new tree to cut if person is a lumberjack
                    if(routine.Count <= 1 && ct.automated && job.id == Job.LUMBERJACK)
                    {
                        nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Tree, GetTreeCutRange());
                        if(nearestTrsf != null) SetTargetTransform(nearestTrsf, true);
                        break;
                    }
                    NextTask();
                    break;
                }

                // store resources in building
                if(StoreResourceInBuilding(ct, bs, ct.taskRes[0].id)) { }
                else ct.taskRes.RemoveAt(0);

                break;
            case TaskType.TakeFromWarehouse: // Taking material from warehouse
                while(ct.taskRes.Count > 0 && ct.taskRes[0].amount == 0)
                    ct.taskRes.RemoveAt(0);
                if(ct.taskRes.Count == 0)
                {
                    NextTask();
                    break;
                }

                // Take res into inventory
                if(TakeIntoInventory(ct, bs, ct.taskRes[0].id)) { }
                else ct.taskRes.RemoveAt(0);

                break;
            case TaskType.Campfire: // Restock campfire fire wood
                Campfire cf = ct.targetTransform.GetComponent<Campfire>();
                if(invMat != null && invMat.id == GameResources.WOOD && invMat.amount > 0)
                {
                    if(ct.taskTime >= 1f/putMaterialSpeed)
                    {
                        ct.taskTime = 0;
                        invMat.Take(cf.Restock(1));
                    }
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.Fishing: // Do Fishing
                if(!plant){
                    NextTask();
                    break;
                }
                
                if (job.id == Job.FISHER && (invFood == null || invFood.GetAmount() == 0 || invFood.id == GameResources.RAWFISH))
                {
                    if (ct.taskTime >= fishingTime)
                    {
                        ct.taskTime = 0;
                        int season = GameManager.GetTwoSeason();
                        // only fish in summer
                        if(season == 0)
                        {
                            GameManager.Msg("Keine Fische im Winter");
                        }
                        else if (season == 1 && Random.Range(0, 3) == 1 && plant.material >= 1)
                        {
                            int amount = AddToInventory(new GameResources(GameResources.RAWFISH, 1));
                            
                            GameManager.UnlockResource(plant.materialID);
                            
                            plant.material -= amount;
                            if(plant.material == 0)
                                plant.Break();

                        }
                    }
                    if(routine.Count <= 1)
                    {
                        if(GetFreeFoodInventorySpace() == 0)
                        {
                            // get nearest fishermanPlace
                            nearestTrsf = myVillage.GetNearestBuildingID(transform.position, 4);
                        }
                        else if(plant.material == 0)
                        {
                            nearestTrsf = myVillage.GetNearestPlant(transform.position, PlantType.Reed, GetReedRange());
                            if(!nearestTrsf)
                                nearestTrsf = myVillage.GetNearestBuildingID(transform.position, 4);
                        }

                        if(nearestTrsf)
                        {
                            SetTargetTransform(nearestTrsf, true);
                            break;
                        }
                    }
                    if(GetFreeFoodInventorySpace() == 0 || plant.material == 0)
                        NextTask();
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.Build: // Put resources into blueprint building
                if (invMat == null) NextTask();
                else if(ct.taskTime >= 1f/buildSpeed)
                {
                    ct.taskTime = 0;
                    bool built = false;
                    foreach (GameResources r in bs.bluePrintBuildCost)
                    {
                        if (invMat.id == r.id && r.GetAmount() > 0 && invMat.GetAmount() > 0 && !built)
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
                if(!plant){
                    NextTask();
                    break;
                }
                // add resources to persons inventory
                if(plant.gameObject.activeSelf && plant.material > 0)
                {
                    if(ct.taskTime >= 1f/collectingSpeed)
                    {
                        ct.taskTime = 0;
                        am = AddToInventory(new GameResources(plant.materialID, 1));
                        if(am > 0) 
                        {
                            plant.material--;
                            GameManager.UnlockResource(plant.materialID);
                            if(plant.material == 0)
                            {
                                // Destroy collected mushroom
                                plant.Break();
                                plant.gameObject.SetActive(false);
                            }
                            else break;
                        }
                    }
                    else break;
                }

                NextTask();

                // Find another mushroom to collect
                if(routine.Count <= 1 && job.id == Job.GATHERER)
                {
                    Transform nearestMushroom = myVillage.GetNearestPlant(transform.position, PlantType.Mushroom, GetCollectingRange());
                    if (GetFreeFoodInventorySpace() == 0)
                    {
                        if (!StoreFoodInventory()) WalkToCenter();
                    }
                    else
                    {
                        if (nearestMushroom != null) SetTargetTransform(nearestMushroom, true);
                        else if (!StoreFoodInventory()) WalkToCenter();
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
                        am = AddToInventory(new GameResources(plant.materialID, plant.material));
                        if(am > 0) 
                        {
                            GameManager.UnlockResource(plant.materialID);
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
                if(GameManager.debugging) GameManager.Error("test1;"+itemToPickup.gameObject.activeSelf+";"+itemToPickup.resource.amount);
                if (itemToPickup != null && itemToPickup.gameObject.activeSelf && itemToPickup.resource.amount > 0)
                {
                    if(GameManager.debugging) GameManager.Error("test2");
                    if(ct.taskTime >= 1f/collectingSpeed)
                    {
                        if(GameManager.debugging) GameManager.Error("test3");
                        ct.taskTime = 0;
                        am = AddToInventory(new GameResources(itemToPickup.resource.id, 1));
                        if (am > 0)
                        {
                            GameManager.UnlockResource(itemToPickup.resource.id);

                            itemToPickup.resource.amount--;
                            if(itemToPickup.resource.amount > 0) break;

                            itemToPickup.gameObject.SetActive(false);

                            int freeSpace = 0;
                            ResourceType pmt = itemToPickup.GetResource().GetResourceType();
                            if(pmt == ResourceType.BuildingMaterial) freeSpace = GetFreeMaterialInventorySpace();
                            if(pmt == ResourceType.Food) freeSpace = GetFreeFoodInventorySpace();
                            
                            // Automatically pickup other items in reach or go to warehouse if inventory is full
                            Transform nearestItem = GameManager.village.GetNearestItemInRange(transform.position, itemToPickup, GetCollectingRange());
                            if (routine.Count == 1 && nearestItem != null && freeSpace > 0 && nearestItem.gameObject.activeSelf)
                            { 
                                SetTargetTransform(nearestItem, true);
                                break;
                            }
                            else
                            {
                                //Transform nearestItemStorage = GameManager.village.GetNearestBuildingType(transform.position, BuildingType.StorageFood);
                                //if (nearestFoodStorage != null) SetTargetTransform(nearestFoodStorage);
                                //if (nearestItem != null && nearestFoodStorage != null) AddTargetTransform(nearestItem);
                            }
                        }
                        else
                        {
                            GameManager.Msg(firstName + " kann " + itemToPickup.GetName() + " nicht aufsammeln");
                        }
                    } else break;
                }
                NextTask();
                break;
            case TaskType.Craft:
                if(job.id == Job.BLACKSMITH)
                {
                    // bone-tool requires 4 bones
                    GameResources tool = new GameResources(GameResources.TOOL_BONE, 1);
                    requirements.Add(new GameResources(GameResources.BONES, 4));
                    results.Add(tool);

                    // store bones in building
                    if(StoreResourceInBuilding(ct, bs, GameResources.BONES)) { }
                    // craft tool
                    else if(ProcessResource(ct, bs, requirements, results, tool.processTime)) { }
                    else
                    {
                        GameManager.Msg("Für ein Knochenwerkzeug brauchst du 4 Knochen!");
                        NextTask();
                    }
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.ProcessAnimal:
                if(job.id == Job.HUNTER)
                {
                    // animal to process
                    GameResources duck = new GameResources(GameResources.ANIMAL_DUCK, 1);
                    requirements.Add(duck);
                    results.Add(new GameResources(GameResources.MEAT, 2));
                    results.Add(new GameResources(GameResources.BONES, 4));
                    results.Add(new GameResources(GameResources.FUR, 1));

                    // store duck in building
                    if(StoreResourceInBuilding(ct, bs, duck.id)) { }
                    // process animal
                    else if(ProcessResource(ct, bs, requirements, results, duck.processTime)) { }
                    // take meat into inventory of person
                    else if(TakeIntoInventory(ct, bs, GameResources.MEAT)) { }
                    // take fur into inventory of person
                    else if(TakeIntoInventory(ct, bs, GameResources.FUR)) { }
                    // take bones into inventory of person
                    else if(TakeIntoInventory(ct, bs, GameResources.BONES)) { }
                    // store resources
                    else 
                    {
                        // store mat inventory
                        if(StoreMaterialInventory() && invMat.amount > 0) {  }
                        // store food inventory
                        if(StoreFoodInventory() && invFood.amount > 0) { }
                        
                        NextTask();
                    }
                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.HuntAnimal:
                // make sure person is a hunter
                if(job.id == Job.HUNTER)
                {
                    if(ct.taskTime >= hitTime)
                    {
                        // only hit animal if in range
                        bool inRange = GameManager.InRange(transform.position, ct.targetTransform.position, GetHitRange());
                        if(!inRange) {
                            SetTargetTransform(ct.targetTransform, true);
                            break;
                        }
                        ct.taskTime = 0;

                        // get animal from target
                        Animal animal = ct.targetTransform.GetComponent<Animal>();

                        // Hit animal for damage of this person
                        if(animal.health > 0) animal.Hit(GetHitDamage());

                        if(animal.IsDead())
                        {
                            GameResources drop = animal.Drop();
                            GameManager.UnlockResource(drop.id);
                            AddToInventory(drop);
                            NextTask();
                        }
                    }
                }
                else
                {
                    NextTask();
                }
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
                        Building tarBs = ct.targetTransform.GetComponent<Building>();
                        if(tarBs.walkable)
                            objectStopRadius = 1f;
                        else
                            objectStopRadius = 0.3f;
                    }
                    else if (ct.targetTransform.tag == "Animal")
                    {
                        Animal tarAn = ct.targetTransform.GetComponent<Animal>();
                        if(tarAn) objectStopRadius = tarAn.stopRadius;
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
                    FindPath(ct.target, ct.targetTransform);
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
                    animator.Play("Walking");

                    foreach(Plant p in Nature.flora)
                    {
                        CheckHideableObject(p,p.currentModel);
                        
                    }
                    foreach(Item p in Item.allItems)
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

    // continue to next task
    public void NextTask()
    {
        if(routine[0].taskType == TaskType.Walk) animator.Play("None");
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
        AddRoutineTaskPosition(Vector3.zero, true, true);
    }

    public void CheckHideableObject(HideableObject p, Transform model)
    {
        if(p.inBuildingViewRange) return;
        if(!p) return;
        bool inRadius = Mathf.Abs(transform.position.x - p.transform.position.x) <= viewDistance && Mathf.Abs(transform.position.z - p.transform.position.z) <= viewDistance;
        if(p.personIDs.Contains(nr)) 
        {
            if(!inRadius)
            {
                p.personIDs.Remove(nr);
                if(p.personIDs.Count == 0) {
                    p.ChangeHidden(true);
                    model.GetComponent<cakeslice.Outline>().enabled = false;
                }
            }
        }
        else
        {
            if(inRadius)
            { 
                p.personIDs.Add(nr);
                p.ChangeHidden(false);
            }
        }
    }

    // Target handlers
    public void SetTargetTransform(Transform target, bool automatic)
    {
        AddRoutineTaskTransform(target, target.position, automatic, true);
    }
    public void AddTargetTransform(Transform target, bool automatic)
    {
        AddRoutineTaskTransform(target, target.position, automatic, false);
    }
    public void AddRoutineTaskTransform(Transform target, Vector3 targetPosition, bool automatic, bool clearRoutine)
    {
        Task walkTask = new Task(TaskType.Walk, targetPosition, target);
        Task targetTask = TargetTaskFromTransform(target, automatic);
        if(target != null && targetTask == null) return;
        if(clearRoutine) routine.Clear();

        /*foreach(Task t in routine)
        {
            if(target != null && t.targetTransform == target ) return;
            if(t.target == targetPosition ) return;
        }*/
        int rc = routine.Count;
        routine.Add(walkTask);
        if (targetTask != null) routine.Add(targetTask);

        if (rc == 0)
        {
            FindPath(target.position, target);
        }
    }

    // Add task to walk toward target
    public void AddRoutineTaskPosition(Vector3 newTarget, bool automatic, bool clearRoutine)
    {
        Vector3 gridPos = Grid.ToGrid(newTarget);
        if(!Grid.ValidNode((int)gridPos.x, (int)gridPos.z)) return;
        Node n = Grid.GetNode((int)gridPos.x, (int)gridPos.z);
        if (n.nodeObject != null)
        {
            AddRoutineTaskTransform(n.nodeObject, n.nodeObject.position, automatic, clearRoutine);
            return;
        }
        if(clearRoutine) routine.Clear();
        Task walkTask = new Task(TaskType.Walk, newTarget);
        routine.Add(walkTask);
        if(routine.Count == 1) FindPath(newTarget, null);
    }

    // Add resource task
    public bool AddResourceTask(TaskType type, Building b, GameResources res)
    {
        if(!b) return false;

        Task walkTask = new Task(TaskType.Walk, b.transform.position, b.transform);
        int maxInvSize = res.GetResourceType() == ResourceType.Food ? GetFoodInventorySize() : GetMaterialInventorySize();

        GameResources inv = null;
        // if building is material storage, get material from inventory
        if(res.GetResourceType() == ResourceType.Food) inv = inventoryFood;
        else inv = inventoryMaterial;

        if(type == TaskType.TakeFromWarehouse)
        {
            if(res.amount > b.resourceCurrent[res.id]) return false;
            if(inv != null && res.amount > maxInvSize-inv.amount) return false;
        }
        if(type == TaskType.BringToWarehouse)
        {
            if(inv == null || res.amount > inv.amount) return false;
            if(res.amount > b.resourceStorage[res.id] - b.resourceCurrent[res.id]) return false;
        }

        List<GameResources> list = new List<GameResources>();
        list.Add(res);

        // if no inventory resource, take from warehouse
        if (res != null && res.GetAmount() > 0) 
        {
            routine.Add(walkTask);
            routine.Add(new Task(type,b.transform.position, b.transform, list));
            return true;
        }
        return false;
    }
    public Task TargetTaskFromTransform(Transform target, bool automatic)
    {
        Task targetTask = null;
        if (target != null)
        {
            switch (target.tag)
            {
                case "Building":
                    Building b = target.GetComponent<Building>();
                    if (b.blueprint)
                    {
                        /* TODO: building master needs to finish building */
                        targetTask = new Task(TaskType.Build, target);
                    }
                    else
                    {
                        switch (b.type)
                        {
                            // Warehouse activity 
                            case BuildingType.Storage:
                                // If personscript decides automatically to walk there, just unload everything
                                if(automatic)
                                {
                                   /* GameResources inv = null;
                                    // if building is material storage, get material from inventory
                                    if(b.GetBuildingType() == BuildingType.StorageFood) inv = inventoryFood;
                                    else inv = inventoryMaterial;

                                    List<GameResources> list = new List<GameResources>();
                                    if(inv != null)
                                        list.Add(new GameResources(inv.id, inv.amount));
                                    targetTask = new Task(TaskType.BringToWarehouse, target.position, target, list);*/
                                }
                                else
                                {
                                    UIManager.Instance.OnShowObjectInfo(target);
                                    UIManager.Instance.TaskResRequest(this);
                                }
                                break;
                            case BuildingType.Food:
                                if (b.id == Building.FISHERMANPLACE) // Fischerplatz
                                {
                                    targetTask = new Task(TaskType.Fisherplace, target);
                                }
                                break;
                            case BuildingType.Luxury:
                                if (b.id == Building.CAMPFIRE) // Campfire
                                {
                                    Campfire cf = target.GetComponent<Campfire>();
                                    if (cf != null)
                                    {
                                        targetTask = new Task(TaskType.Campfire, target);
                                    }
                                }
                                break;
                            case BuildingType.Crafting:
                                if (b.id == Building.BLACKSMITH)
                                {
                                    // check if person is a blacksmith, then he can craft
                                    if(job.id == Job.BLACKSMITH)
                                    {
                                        targetTask = new Task(TaskType.Craft, target);
                                    }
                                    // store bones ino that building
                                    else if(inventoryMaterial != null && inventoryMaterial.id == GameResources.BONES)
                                    {
                                        UIManager.Instance.OnShowObjectInfo(target);
                                        UIManager.Instance.TaskResRequest(this);
                                    }
                                    else GameManager.Msg("Nichts zu tun bei der Schmiede");
                                }
                                if (b.id == Building.HUNTINGLODGE)
                                {
                                    // check if person is a hunter, then he can process
                                    if(job.id == Job.HUNTER)
                                    {
                                        targetTask = new Task(TaskType.ProcessAnimal, target);
                                    }
                                    // store bones ino that building
                                    else if(inventoryMaterial != null && (inventoryMaterial.id == GameResources.BONES || inventoryFood.id == GameResources.MEAT))
                                    {
                                        UIManager.Instance.OnShowObjectInfo(target);
                                        UIManager.Instance.TaskResRequest(this);
                                    }
                                    else GameManager.Msg("Nichts zu tun bei der Jagdhütte");
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
                        if (job.id == Job.LUMBERJACK) //Holzfäller
                        {
                            targetTask = new Task(TaskType.CutTree, target);
                        }
                        else
                        {
                            GameManager.Msg(firstName + " kann keine Bäume fällen!");
                        }
                    }
                    else if (plant.type == PlantType.Mushroom)
                    {
                        targetTask = new Task(TaskType.CollectMushroom, target);
                    }
                    else if (plant.type == PlantType.MushroomStump)
                    {
                        targetTask = new Task(TaskType.CullectMushroomStump, target);
                    }
                    else if(plant.type == PlantType.Crop)
                    {
                        // can only harvest, if in build range or is a gatherer
                        if(GameManager.village.InBuildRange(target.position) || job.id == Job.GATHERER)
                            targetTask = new Task(TaskType.Harvest, target);
                        else GameManager.Msg("Nur Sammler können ausserhalb des Bau-Bereiches Korn ernten.");
                    }
                    else if (plant.type == PlantType.Reed)
                    {
                        if(job.id == Job.FISHER)
                            targetTask = new Task(TaskType.Fishing, target);
                        else
                            GameManager.Msg(firstName + " kann nicht fischen");
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
                case "Animal":
                    Animal animal = target.GetComponent<Animal>();
                    if (animal != null)
                    {
                        if(job.id == Job.HUNTER)
                            targetTask = new Task(TaskType.HuntAnimal, target);
                        else GameManager.Msg("Nur Jäger können Tiere jagen.");
                    }
                    break;
            }
        }
        if(targetTask != null) targetTask.automated = automatic;
        return targetTask;
    }

    // get corresponding inventory to resId
    public GameResources InventoryFromResId(int resId)
    {
        if(inventoryFood != null && inventoryFood.id == resId) return inventoryFood;
        if(inventoryMaterial != null && inventoryMaterial.id == resId) return inventoryMaterial;
        return null;
    }
    
    // Craft resource into other resources
    public bool ProcessResource(Task ct, Building bs, List<GameResources> requirements, List<GameResources> results, float processTime)
    {
        // check if building has enough resources stored
        bool enoughRes = true;
        foreach(GameResources res in requirements)
        {
            if(bs.resourceCurrent[res.id] < res.amount) enoughRes = false;
        }
        // check if enough space in building storage
        bool storageSpace = true;
        foreach(GameResources res in results)
        {
            if(res.amount > bs.FreeStorage(res.id)) enoughRes = false;
        }
        if(enoughRes && storageSpace)
        {
            if(ct.taskTime >= processTime)
            {
                ct.taskTime = 0;

                foreach(GameResources res in requirements)
                {
                    bs.resourceCurrent[res.id] -= res.amount;
                }
                foreach(GameResources res in results)
                {
                    bs.resourceCurrent[res.id] += res.amount;
                    GameManager.UnlockResource(res.id);
                }
            }

            return true;
        }

        return false;
    }

    // Take resource from building into appropriate inventory
    public bool TakeIntoInventory(Task ct, Building bs, int resId)
    {
        GameResources takeRes = new GameResources(resId, 1);
        GameResources inventory = ResourceToInventory(takeRes.GetResourceType());

        // check if building has resource and enough inventory space is available
        if(bs.resourceCurrent[resId] > 0 && 
            (inventory == null || inventory.amount == 0 || inventory.id == resId && GetFreeInventorySpace(takeRes) > 0))
        {
            if(ct.taskTime >= 1f/putMaterialSpeed)
            {
                ct.taskTime = 0;

                int mat = AddToInventory(takeRes);
                if(mat > 0) bs.resourceCurrent[resId]--;
                else GameManager.Error("TakeIntoInventory:"+bs.buildingName);
            }

            return true;
        }

        return false;
    }

    // Store resource in building
    public bool StoreResourceInBuilding(Task ct, Building bs, int resId)
    {
        GameResources inventory = InventoryFromResId(resId);

        if(inventory != null && inventory.GetAmount() > 0 && bs.FreeStorage(resId) > 0)
        {
            if(ct.taskTime >= 1f/putMaterialSpeed)
            {
                ct.taskTime = 0;

                // store raw fish in building
                int stockMat = bs.Restock(inventory, 1);
                
                // check if building storage is full
                if(stockMat == 0)
                {
                    return false;
                }

                // Take one resource from inventory
                inventory.Take(1);
            }

            return true;
        }
        return false;
    }

    // Store inventories
    public bool StoreMaterialInventory()
    {
        if(inventoryMaterial == null) return false;
        return StoreResource(inventoryMaterial);
    }
    public bool StoreFoodInventory()
    {
        if(inventoryFood == null) return false;
        return StoreResource(inventoryFood);
    }
    public bool StoreResource(GameResources res)
    {
        Transform nearestStorage = GameManager.village.GetNearestStorageBuilding(transform.position, res.id, true);
        if(nearestStorage == null) return false;
        routine.Add(new Task(TaskType.Walk, nearestStorage.position));
        routine.Add(new Task(TaskType.BringToWarehouse, nearestStorage.position, nearestStorage, res.Clone(), true));
        return true;
    }

    // use a* pathfinding to find path towards target
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
        if(currentPath != null && currentPath.Count > 1)
            currentPath.RemoveAt(0);
        // If path is empty and start node is not equal to end node, don't do anything
        int dx = ex - sx; int dy = ey - sy;
        if (currentPath.Count == 0 && ((dx * dx + dy * dy) > 1 || (sx == ex && sy == ey)))
        {
            NextTask();
        }
    }

    // Mouse handlers
    private void OnMouseOver()
    {
        if (CameraController.inputState != 2) return;
        highlighted = true;

        if (Input.GetMouseButtonDown(0)) {
            OnClick();
        }
    }
    private void OnMouseExit()
    {
        highlighted = false;
    }

    private bool CheckIfInFoodRange()
    {
        foreach(Building bs in Building.allBuildings)
        {
            if(bs.id == Building.WAREHOUSEFOOD)
            {
                return GameManager.InRange(bs.transform.position, transform.position, bs.foodRange);
            }
        }
        return false;
    }

    // click handlers
    public void OnClick()
    {
        if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            DeselectAll();
        OnSelect();
    }
    public void OnSelect()
    {
        selectedPeople.Add(this);
        clickableUnit.selected = true;
    }
    public void OnDeselect()
    {
        clickableUnit.selected = false;
    }
    public static void DeselectAll()
    {
        foreach(PersonScript ps in selectedPeople)
            ps.OnDeselect();
        
        selectedPeople.Clear();
    }

    // Person Properties
    public bool IsEmployed()
    {
        return job.id != Job.UNEMPLOYED;
    }
    public int GetMaterialInventorySize()
    {
        return 10;
    }
    public int GetFoodInventorySize()
    {
        return 20;
    }
    public int MaxHitDamage()
    {
        return 6;
    }
    public int GetHitDamage()
    {
        return (int)(MaxHitDamage() * (1f+Random.Range(-0.3f, 0.3f)));
    }
    public float GetHitRange()
    {
        return 0.5f;
    }
    public int GetCollectingRange()
    {
        return 50;
    }
    public int GetTreeCutRange()
    {
        return 80;
    }
    public int GetReedRange()
    {
        return 10;
    }
    public bool IsFertile()
    {
        if (gender == Gender.Male) return age >= 16 && age <= 60;
        else return age >= 16 && age <= 40;
    }
    public bool IsDead()
    {
        return health <= float.Epsilon;
    }
    public void AgeOneYear()
    {
        age++;
    }

    // Person Data
    public PersonData GetPersonData()
    {
        PersonData thisPerson = new PersonData();

        thisPerson.nr = nr;
        thisPerson.firstName = firstName;
        thisPerson.lastName = lastName;
        thisPerson.gender = gender;

        thisPerson.health = health;
        thisPerson.hunger = hunger;
        thisPerson.saturation = saturation;

        thisPerson.jobID = job.id;
        thisPerson.workingBuildingId = workingBuilding ? workingBuilding.nr : -1;

        thisPerson.SetPosition(transform.position);
        thisPerson.SetRotation(transform.rotation);

        if(inventoryMaterial != null)
        {
            thisPerson.invMatId = inventoryMaterial.id;
            thisPerson.invMatAm = inventoryMaterial.amount;
        }
        else 
        {
            thisPerson.invMatId = 0;
            thisPerson.invMatAm = 0;
        }
        if(inventoryFood != null)
        {
            thisPerson.invFoodId = inventoryFood.id;
            thisPerson.invFoodAm = inventoryFood.amount;
        }
        else 
        {
            thisPerson.invFoodId = 0;
            thisPerson.invFoodAm = 0;
        }
        return thisPerson;
    }
    public void SetPersonData(PersonData person)
    {
        nr = person.nr;
        firstName = person.firstName;
        lastName = person.lastName;
        gender = person.gender;

        health = person.health;
        hunger = person.hunger;
        saturation = person.saturation;

        job = new Job(person.jobID);
        workingBuilding = Building.Identify(person.workingBuildingId);

        transform.position = person.GetPosition();
        transform.rotation = person.GetRotation();

        inventoryMaterial = new GameResources(person.invMatId, person.invMatAm);
        inventoryFood = new GameResources(person.invFoodId, person.invFoodAm);
        
        viewDistance = 10;
    }

    // inventory handlers
    private int ResourceToInventoryType(ResourceType rt)
    {
        /*
        0 = not handled
        1 = building material
        2 = food
        */
        switch(rt)
        {
            case ResourceType.BuildingMaterial:
            case ResourceType.DeadAnimal:
            case ResourceType.Crafting: return 1;
            case ResourceType.RawFood:
            case ResourceType.Food: return 2;
        }
        return 0;
    }
    public GameResources ResourceToInventory(ResourceType rt)
    {
        int invrt = ResourceToInventoryType(rt);
        if(invrt == 1) return inventoryMaterial;
        if(invrt == 2) return inventoryFood;
        return null;
    }
    public int AddToInventory(GameResources res)
    {
        int invResType = ResourceToInventoryType(res.GetResourceType());
        int ret = 0;
        GameResources inventory = null;

        if(invResType == 0) { 
            GameManager.Error("Ressource-Typ kann nicht hinzugefügt werden: "+res.GetResourceType().ToString());
            return ret;
        }
        if(invResType == 1) inventory = inventoryMaterial;
        if(invResType == 2) inventory = inventoryFood;

        if (inventory == null || (res.id != inventory.id && inventory.GetAmount() == 0))
        {
            if(invResType == 1) inventoryMaterial = new GameResources(res.id);
            if(invResType == 2) inventoryFood = new GameResources(res.id);
        }

        if(invResType == 1) inventory = inventoryMaterial;
        if(invResType == 2) inventory = inventoryFood;
        
        if(res.id == inventory.id)
        {
            int space = 0;
            if(invResType == 1) space = GetFreeMaterialInventorySpace();
            if(invResType == 2) space = GetFreeFoodInventorySpace();
            if (space >= res.GetAmount())
            {
                ret = res.GetAmount();
            }
            else if (space < res.GetAmount() && space > 0)
            {
                ret = space;
            }
            inventory.Add(ret);
        }
        return ret;
    }
    public int GetFreeMaterialInventorySpace()
    {
        int used = 0;
        if(inventoryMaterial != null) used = inventoryMaterial.amount;
        return GetMaterialInventorySize() - used;
    }
    public int GetFreeFoodInventorySpace()
    {
        int used = 0;
        if(inventoryFood != null) used = inventoryFood.amount;
        return GetFoodInventorySize() - used;
    }
    public int GetFreeInventorySpace(GameResources res)
    {
        int invrt = ResourceToInventoryType(res.GetResourceType());
        if(invrt == 1) return GetFreeMaterialInventorySpace();
        if(invrt == 2) return GetFreeFoodInventorySpace();

        return 0;
    }

    // Factors
    public float GetFoodFactor()
    {
        return hunger / 100;
    }
    public float GetHealthFactor()
    {
        return health / 100;
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
             return new Color(1,0.6f,0.6f,0.6f);
            default: return new Color(0,0,0,0);
        }
    }
    // Convert hunger to color
    public Color GetFoodCol()
    {
        if(!inFoodRange) return new Color(1f,0.8f,0.15f,0.6f);
        return new Color(0,1,0.15f,0.6f);
    }

    // identify personscript by id
    public static PersonScript Identify(int nr)
    {
        foreach (PersonScript ps in allPeople)
        {
            if(ps.nr == nr) return ps;
        }
        return null;
    }

    // Name handlers
    public static string RandomMaleName()
    {
        return allMaleNames[Random.Range(0, allMaleNames.Length)];
    }
    public static string RandomFemaleName()
    {
        return allFemaleNames[Random.Range(0, allFemaleNames.Length)];
    }
    public static string RandomLastName()
    {
        return allLastNames[Random.Range(0, allLastNames.Length)];
    }
    private static string[] allLastNames = {
        "Müller", "Schmidt", "Schneider", "Fischer", "Weber", "Meyer", "Wagner", "Becker", "Schulz", "Hoffmann"
    };
    private static string[] allMaleNames = {
        "Finn", "Jan", "Jannik", "Jonas", "Leon", "Luca", "Niklas", "Tim", "Tom", "Alexander", "Christian", "Daniel", "Dennis", "Martin", "Michael"
    };
    private static string[] allFemaleNames = {
        "Anna", "Hannah", "Julia", "Lara", "Laura", "Lea", "Lena", "Lisa", "Michelle", "Sarah", "Christina", "Katrin", "Melanie", "Nadine", "Nicole"
    };
}

/*public class PeopleSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var people = (List<PersonScript>)obj;
        info.AddValue("_count", people.Count);
        foreach(PersonScript personScript in people)
        {
            int nr = personScript.nr;
            info.AddValue(nr + "_firstName", personScript.firstName);
            info.AddValue(nr + "_lastName", personScript.lastName);
            info.AddValue(nr + "_age", personScript.age);
            Transform pt = personScript.transform;
            info.AddValue(nr + "_posX", pt.position.x);
            info.AddValue(nr + "_posY", pt.position.x);
            info.AddValue(nr + "_posZ", pt.position.z);
        }
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        var people = (List<PersonScript>)obj;
        int count = (int)info.GetValue("_count", typeof(int));
        for(int i = 0; i < count; i++)
        {
            PersonScript personScript
        }
        personScript.nr = (int)info.GetValue("_nr", typeof(int));
        personScript.firstName = (string)info.GetValue("_firstName", typeof(string));
        personScript.lastName = (string)info.GetValue("_lastName", typeof(string));
        personScript.age = (int)info.GetValue("_age", typeof(int));
        Vector3 pos = Vector3.zero;
        pos.x = (float)info.GetValue("_posX", typeof(float));
        pos.y = (float)info.GetValue("_posY", typeof(float));
        pos.z = (float)info.GetValue("_posZ", typeof(float));
        personScript.transform.position = pos;
        return personScript;
    }
}
*/