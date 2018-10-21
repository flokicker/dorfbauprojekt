using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization;
using SimpleFogOfWar;

[System.Serializable]
public enum Gender
{
    Male, Female
}
[System.Serializable]
public enum Disease
{
    None, Infirmity /* Altersschwäche */, Flu
}
public class PersonScript : HideableObject {

    // Collection of wandering people, all people and selected people
    public static HashSet<PersonScript> wildPeople = new HashSet<PersonScript>();
    public static HashSet<PersonScript> allPeople = new HashSet<PersonScript>();
    public static HashSet<PersonScript> selectedPeople = new HashSet<PersonScript>();

    // Person info
    /*public int Nr;
    public string FirstName, LastName;
    public Gender Gender;
    public int age, lifeTimeYears, lifeTimeDays;
    public Job job;
    public float Health, hunger;
    public Disease disease;
    public bool Wild;*/
    private GamePerson person;
    public int Nr
    {
        get { return person.nr; }
    }
    public string FirstName
    {
        get { return person.firstName; }
    }
    public string LastName
    {
        get { return person.lastName; }
    }
    public Gender Gender
    {
        get { return person.gender; }
    }
    public int Age
    {
        get { return person.age; }
    }
    public int LifeTimeYears
    {
        get { return person.lifeTimeYears; }
    }
    public int LifeTimeDays
    {
        get { return person.lifeTimeDays; }
    }
    public Job Job
    {
        get { return Job.Get(person.jobID); }
    }
    public float Health
    {
        get { return person.health; }
    }
    public float Hunger
    {
        get { return person.hunger; }
    }
    public float Saturation
    {
        get { return person.saturation; }
    }
    public float SaturationTimer
    {
        get { return person.saturationTimer; }
    }
    public Disease Disease
    {
        get { return person.disease; }
    }
    public bool Wild
    {
        get { return person.wild; }
    }
    // if no mother or dead, mother=null
    public int MotherNr
    {
        get { return person.motherNr; }
    }
    public Node lastNode
    {
        get { return Grid.GetNode(person.lastNodeX, person.lastNodeY); }
    }
    // women can be pregnant for 9 months = 270 days
    public bool Pregnant
    {
        get { return person.pregnant; }
    }
    public float PregnancyTime
    {
        get { return person.pregnancyTime; }
    }
    public List<Task> Routine
    {
        get { return person.routine; }
    }
    public GameResources InventoryMaterial
    {
        get { return person.inventoryMaterial; }
    }
    public GameResources InventoryFood
    {
        get { return person.inventoryFood; }
    }

    public float ActionRadius
    {
        get { return 0.3f; }
    }

    public bool selected, highlighted;
    
    // fog of war viewing
    public int viewDistance = 2;
    public SimpleFogOfWar.FogOfWarInfluence fogOfWarInfluence;

    // Building where this Person is living
    public BuildingScript workingBuilding;

    private int walkMode = 0;
    private float moveSpeed = 0.65f;
    private float currentMoveSpeed = 0f;

    // Animation controller
    private Animator animator;
    private Transform woodAxe, spear;

    private float scratchTimer;

    // time since last task
    private float noTaskTime = 0, checkCampfireTime = 0;

    private List<Node> currentPath;

    // Activity speeds/times
    private float putMaterialSpeed = 2f, buildSpeed = 4f, energySpotTakingSpeed = 1f;
    private float fishingTime = 1, processFishTime = 2f, collectingSpeed = 2f, hitTime = 1f, harvestCornTime = 2f;

    // Audio
    public AudioClip chopSound;
    private AudioSource audioSource;

    private Transform canvas;
    private Image imageHP, imageFood;

    // position of camera over shoulder
    private Transform shoulderCameraPos;

    private ClickableUnit clickableUnit;

    private bool inFoodRange = false;
    private BuildingScript noTaskBuilding;

    // Reference to the clickableObject script
    private ClickableObject co;

    // Use this for initialization
    public override void Start()
    {
        person.saturationTimer = 0;
        workingBuilding = null;

        woodAxe = transform.Find("Armature_004/Torso/Chest/Upper_Arm_R/Lower_Arm_R/Hand_R/Finger04_R/Axe");
        if (woodAxe) woodAxe.gameObject.SetActive(false);
        spear = transform.Find("Armature_004/Torso/Chest/Upper_Arm_L/Lower_Arm_L/Hand_L/Spear");
        if (spear) spear.gameObject.SetActive(false);

        // handles all outline/interaction stuff
        clickableUnit = GetComponentInChildren<SkinnedMeshRenderer>().gameObject.AddComponent<ClickableUnit>();
        clickableUnit.SetScriptedParent(transform);

        // Initialize fog of war influence if not Wild
        fogOfWarInfluence = gameObject.AddComponent<SimpleFogOfWar.FogOfWarInfluence>();
        fogOfWarInfluence.ViewDistance = viewDistance;
        fogOfWarInfluence.enabled = !Wild;

        // Animation
        animator = GetComponent<Animator>();

        canvas = GetComponentInChildren<Canvas>().transform;
        imageHP = canvas.Find("Health").Find("ImageHP").GetComponent<Image>();
        imageFood = canvas.Find("Food").Find("ImageHP").GetComponent<Image>();

        Vector3 gp = Grid.ToGrid(transform.position);
        person.lastNodeX = (int)gp.x;
        person.lastNodeY = (int)gp.z;

        if(person.nr == -1) person.nr = allPeople.Count;
        if (Wild)
            wildPeople.Add(this);
        else
            allPeople.Add(this);

        co = gameObject.AddComponent<ClickableObject>();
        co.SetSelectionCircleRadius(0.2f);
        //co.highlightable = Wild;
        co.clickable = Wild;
        //co.enabled = Wild;

        CheckAllHideableObjects();

        shoulderCameraPos = transform.Find("ShoulderCam");

        audioSource = GetComponent<AudioSource>();

        // start coroutine
        StartCoroutine(GamePersonObjectTransform());
        
        // check if tasks are already setup
        foreach (Task t in person.routine)
            if (!t.setup)
                t.SetupTarget();

        base.Start();
    }

    private bool rayCastOutline = false;
    // Update is called once per frame
    public override void Update()
    {
        if(!Wild) co.selectedOutline = selected;
        co.showSmallInfo = Wild;

        if (workingBuilding && !workingBuilding.gameObject.activeSelf) workingBuilding = null;

        if (rayCastOutline)
        {
            // check if person is behind object
            RaycastHit raycastHit;
            if (Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Camera.main.transform.position - (transform.position + new Vector3(0, 0.1f, 0)), out raycastHit, 100))
            {
                //Debug.Log(raycastHit.transform.tag);
                clickableUnit.tempOutline = true;
            }
        }

        UpdateSize();
        UpdatePregnancy();
        UpdateToDo();
        UpdateCondition();

        if (selected)
        {
            if (Input.GetKeyDown(KeyCode.X))
                walkMode++;
            if (walkMode > 2) walkMode = 0;
        }
        else walkMode = 0;

        // pickup nearby items
        if(ShoulderControl() && Input.GetKeyDown(KeyCode.F))
        {
            Transform nearestItem = null;  
            if(InventoryMaterial != null && InventoryMaterial.Amount > 0) nearestItem = GameManager.village.GetNearestItemInRange(transform.position, InventoryMaterial, Grid.SCALE);
            else nearestItem = GameManager.village.GetNearestItemInRange(transform.position, Grid.SCALE);

            if (nearestItem != null && person.routine.Count == 0)
            {
                SetTargetTransform(nearestItem, false);
                if (person.routine.Count == 2) NextTask();
            }
        }
        
        // position player at correct ground height on terrain
        Vector3 terrPos = transform.position;
        terrPos.y = Terrain.activeTerrain.SampleHeight(terrPos) + Terrain.activeTerrain.transform.position.y;
        transform.position = terrPos;

        // last visited node update
        lastNode.SetPeopleOccupied(true);

        scratchTimer += Time.deltaTime;

        /* TODO: scartching */
        /*animator.SetBool("scratching",false);
        if(scratchTimer >= Random.Range(20f,30f) && !animator.GetBool("walking"))
        {
            scratchTimer = 0;
            if(Random.Range(0,10) == 0)
            {
                animator.SetBool("scratching",true);
            }
        }*/

        base.Update();
	}

    // LateUpdate called after update
    void LateUpdate()
    {
        //GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity*0.95f;

        // Update UI canvas for Health bar
        Camera camera = Camera.main;
        canvas.LookAt(canvas.position + camera.transform.rotation * Vector3.forward * 0.0001f, camera.transform.rotation * Vector3.up);
        canvas.gameObject.SetActive(false);//highlighted || selected);
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
    public override void OnDestroy()
    {
        allPeople.Remove(this);
        selectedPeople.Remove(this);
        wildPeople.Remove(this);

        base.OnDestroy();
    }

    private float randomMovementTimer = 0;
    private void RandomMovement()
    {
        randomMovementTimer += Time.deltaTime;
        if(randomMovementTimer >= 5)
        {
            randomMovementTimer = 0;
            if (person.routine.Count < 3 && Random.Range(0,5) == 0)
            {
                Node lastTarget = Grid.GetNode(Grid.SpawnX, Grid.SpawnY);
                if (person.routine.Count > 0) lastTarget = Grid.GetNodeFromWorld(person.routine[person.routine.Count - 1].target);

                int rndx = lastTarget.gridX + Random.Range(-5, 5);
                int rndy = lastTarget.gridY + Random.Range(-5, 5);

                // make sure children only play in radius +-10 around center cave
                Node rndTarget = Grid.GetNode(Mathf.Clamp(rndx, Grid.SpawnX - 10, Grid.SpawnX + 10), Mathf.Clamp(rndy, Grid.SpawnY - 10, Grid.SpawnY + 10));
                AddRoutineTaskPosition(Grid.ToWorld(rndTarget.gridX, rndTarget.gridY), true, false);
            }
        }
    }

    // update methods
    private float eatTimer = 0, followReactionTimer = 0;
    private Transform oldTarget = null;
    private void UpdateCondition()
    {
        if (Disease != Disease.Infirmity)
        {
            if (Age > LifeTimeYears || Age == LifeTimeYears && GameManager.CurrentDay >= LifeTimeDays)
            {
                person.disease = Disease.Infirmity;
                ChatManager.Msg(FirstName + " ist krank geworden und mag nichts mehr essen!");
            }
        }

        person.saturationTimer += Time.deltaTime;

        float hungryFactor = Hunger <= 50 ? 0.1f : 1f;

        // person doesn't want to eat anymore
        if (Disease == Disease.Infirmity)
        {
            person.saturationTimer = 0;
            person.saturation = 1;
            hungryFactor = 1;
        }

        eatTimer += Time.deltaTime;
        if (eatTimer > 0.5f)
        {
            eatTimer = 0;

            // Eat after not being saturated anymore
            if (Random.Range(0,8) == 0 && person.saturationTimer >= person.saturation * hungryFactor && Hunger <= 95)
            {
                person.saturationTimer = 0;
                person.saturation = 0;

                // automatically take food from inventory first
                GameResources food = person.inventoryFood;

                // only take food from inventory if its food
                if (food != null)
                {
                    if(food.Amount > 0 && food.Edible) food.Take(1);
                    else food = null;
                }

                // check for food from warehouse
                /* TODO: check only for full (set param true */
                if (food == null) food = GameManager.village.TakeFoodForPerson(this, false);

                if (food != null && food.Amount > 0)
                {
                    person.health += food.Health * (Disease != Disease.None ? 0.3f : 1f);
                    person.hunger += food.Nutrition;
                    person.saturation = food.Nutrition * 2f;
                }
            }
        }


        // hp update
        float satFact = 0f;
        if (Hunger <= 0) satFact = 0.8f;
        else if (Hunger <= 10) satFact = 0.4f;
        else if (Hunger <= 20) satFact = 0.1f;

        person.health -= Time.deltaTime * satFact;

        // hunger update
        satFact = 0.03f;
        if (person.saturation == 0) satFact = 0.05f;

        if (GameManager.GetTwoSeason() == 0) satFact *= 1.5f;

        person.hunger -= Time.deltaTime * satFact;

        if (Hunger < 0) person.hunger = 0;
        if (Hunger > 100) person.hunger = 100;

        if (Health < 0) person.health = 0;
        if (Health > 100) person.health = 100;
    }
    private void UpdateToDo()
    {
        int ageState = AgeState();
        switch(ageState)
        {
            case 0: // random
                RandomMovement();
                break;
            case 1: // following mother
                PersonScript mother = Identify(MotherNr);

                // follow mother if she exists
                if (mother)
                {
                    randomMovementTimer = 0;

                    if ((transform.position - mother.transform.position).sqrMagnitude > 2)
                    {
                        Transform myTTT = GetFirstTargetTransform();
                        if (Routine.Count == 0 || oldTarget != null || myTTT != mother.transform)
                        {
                            // follow person
                            SetTargetTransform(mother.transform, true);
                            oldTarget = null;
                            followReactionTimer = 0;
                            //AddRoutineTaskTransform(mother.transform, mother.transform.position, true, false, true);
                        }
                    }
                    else if(mother.Routine.Count > 0)// when person is close enough to mother and mother does something
                    {
                        // only set task if not already same task as mother is doing
                        if (Routine.Count == 0 || (Routine.Count > 0 && Routine[Routine.Count-1].target != mother.Routine[mother.Routine.Count - 1].target))
                        {
                            // delay follow action by 1.5 second
                            Transform newTarget = mother.GetFirstTargetTransform();
                            if (newTarget)
                            {
                                if (newTarget != oldTarget && followReactionTimer >= 1.5f)
                                {
                                    SetTargetTransform(newTarget, true);
                                    followReactionTimer = 0;
                                    oldTarget = newTarget;
                                }
                                else if(newTarget == oldTarget) followReactionTimer += Time.deltaTime;
                            }
                        }
                    }
                }
                else if (randomMovementTimer >= 5)
                {
                    RandomMovement();
                }
                else randomMovementTimer += Time.deltaTime;
                break;
            case 2: // controllable
                break;
        }

        if(person.routine.Count == 0 && !ShoulderControl())
            animator.SetBool("walking", false);

        // check waht to do
        if (person.routine.Count > 0)
        {
            Task ct = person.routine[0];
            noTaskTime = 0;
            checkCampfireTime = 0;
            ExecuteTask(ct);
        }
        else if(ageState > 0)
        {
            noTaskTime += Time.deltaTime;

            // after 300 seconds go to nearest no-task-building and await commands or do no-task-people-activities (e.g. bring wood to campfire)
            if(noTaskTime >= 300)
            {
                BuildingScript ntb = GameManager.village.GetNearestBuildingNoTask(transform.position);
                if(ntb)
                {
                    SetTargetTransform(ntb.transform, true);
                    ntb.AddNoTaskPerson();

                    if (noTaskBuilding) noTaskBuilding.RemoveNoTaskPerson();
                    noTaskBuilding = ntb;

                    noTaskTime = 0;
                }
            }
            /*checkCampfireTime += Time.deltaTime;

            // every 2sec check campfire
            if (checkCampfireTime >= 2)
            {
                checkCampfireTime = 0;
                // after 300 sec, go to campfire, warmup and await new commands
                if (tf && tf.GetComponent<Campfire>().GetHealthFactor() < 0.5f && (GameManager.InRange(transform.position, tf.position, tf.GetComponent<BuildingScript>().BuildRange) && noTaskTime >= 300))
                {
                    if (person.inventoryMaterial != null && person.inventoryMaterial.Is("Holz") && person.inventoryMaterial.Amount > 0)
                    {
                        // go put wood into campfire
                        AddTargetTransform(tf, true);
                    }
                    else if (person.inventoryMaterial == null || person.inventoryMaterial.Amount == 0 || (person.inventoryMaterial.Is("Holz") && GetFreeMaterialInventorySpace() > 0))
                    {
                        Transform nearestStorage = GameManager.village.GetNearestStorageBuilding(transform.position, ResourceData.Id("Holz"), false);
                        BuildingScript bs = nearestStorage.GetComponent<BuildingScript>();
                        if (nearestStorage && bs.GetStorageCurrent("Holz") > 0)
                        {
                            // get wood and then put into campfire
                            AddResourceTask(TaskType.TakeFromWarehouse, bs, new GameResources(ResourceData.Id("Holz"), Mathf.Min(GetFreeMaterialInventorySpace(), bs.GetStorageCurrent("Holz"))));
                            AddTargetTransform(tf, true);
                        }
                    }
                }
            }*/
        }
    }
    private void UpdateSize()
    {
        float scale = 1f;
        float initialScale = 1f;
        if (Age < 18)
        {
            scale = (0.3f * (Age+GameManager.DayOfYear/365f) / 18f) + 0.7f;
        }
        transform.localScale = Vector3.one * scale * initialScale;
    }
    private void UpdatePregnancy()
    {
        if (Pregnant)
        {
            person.pregnancyTime += Time.deltaTime * GameManager.speedFactor;
            if (person.pregnancyTime >= 200) // 3minuten30sek schwanger
            {
                person.pregnant = false;
                GameManager.village.PersonBirth(Nr);
                person.pregnancyTime = 0;
            }
        }
        else person.pregnancyTime = 0;
    }

    // Do a given task 'ct'
    private void ExecuteTask(Task ct)
    {
        float moveSpeed = MoveSpeed();

        /*string taskStr = "";
        foreach(Task t in person.routine)
            taskStr += t.taskType.ToString()/*+"-"+t.targetTransform+";";
        Debug.Log("Tasks: "+taskStr);*/
        ct.taskTime += Time.deltaTime;
        NatureObjectScript NatureObjectScript = null;
        BuildingScript bs = null;
        //AnimalScript animalScript = null;
        Building b = null;
        BuildingScript parentBs = null;
        GameResources invFood = person.inventoryFood;
        GameResources invMat = person.inventoryMaterial;
        GameResources res = null;
        Village myVillage = GameManager.village;
        Transform nearestTrsf = null;
        BuildingScript nearestBuilding = null;
        AnimalScript nearestAnimal = null;
        List<GameResources> requirements = new List<GameResources>();
        List<GameResources> results = new List<GameResources>();
        Vector3 diff;
        if (ct.targetTransform != null)
        {
            NatureObjectScript = ct.targetTransform.GetComponent<NatureObjectScript>();
            bs = ct.targetTransform.GetComponent<BuildingScript>();
            if (bs)
            {
                b = bs.Building;
                parentBs = BuildingScript.Identify(bs.ParentBuildingNr);
            }
            
        }
        List<TaskType> buildingTasks = new List<TaskType>(new TaskType[]{ TaskType.BringToWarehouse, TaskType.TakeFromWarehouse,
                TaskType.Campfire, TaskType.Build, TaskType.Craft, TaskType.SacrificeResources, TaskType.WorkOnField });
        if(buildingTasks.Contains(ct.taskType))
        {
            if(!bs)
            {
                NextTask();
                return;
            }
            else if (!ct.checkFromFar && !GameManager.InRange(transform.position, bs.transform.position, Mathf.Max(bs.GridWidth, bs.GridHeight) + ActionRadius))
            {
                SetTargetTransform(ct.targetTransform, true);
                return;
            }
        }
        int am = 0;
        bool automaticNextTask = Controllable() && !ShoulderControl();

        List<GameResources> inventoryList = new List<GameResources>();
        if (person.inventoryMaterial != null && person.inventoryMaterial.Amount > 0) inventoryList.Add(person.inventoryMaterial);
        if (person.inventoryFood != null && person.inventoryFood.Amount > 0) inventoryList.Add(person.inventoryFood);

        switch (ct.taskType)
        {
            case TaskType.MineNatureObject:
                animator.SetBool("chopping", false);
                // if object is not there anymore, continue
                if (!NatureObjectScript)
                {
                    if (person.routine.Count <= 1)
                    {
                        if (GetFreeMaterialInventorySpace() > 0)
                            nearestTrsf = myVillage.GetNearestPlant(transform.position, GetTreeCutRange(), true);
                        else
                        {
                            StoreMaterialInventory();
                            break;
                        }

                        if (nearestTrsf && automaticNextTask)
                        {
                            SetTargetTransform(nearestTrsf, true);
                            break;
                        }
                    }
                    NextTask();
                    break;
                }
                if (!NatureObjectScript) // everyone can chop down trees //|| ct.taskType == TaskType.CutTree && !job.Is("Holzfäller")) { 
                {
                    //(!NatureObjectScript || ct.taskType == TaskType.CutTree && job.id != Job.LUMBERJACK) && !(ct.taskType == TaskType.CutTree && job.Is("Holzfäller"))) {
                    NextTask();
                    break;
                }
                if (NatureObjectScript.IsBroken() || NatureObjectScript.ChopTimes() == 0)
                {
                    // Amount of wood per one chop gained
                    res = NatureObjectScript.ResourcePerChop;

                    int freeSpace = 0;
                    ResourceType pmt = ResourceData.Get(res.Id).type;
                    if (pmt == ResourceType.Building) freeSpace = GetFreeMaterialInventorySpace();
                    if (pmt == ResourceType.Food) freeSpace = GetFreeFoodInventorySpace();

                    if (NatureObjectScript.ResourceCurrent.Amount > 0 && freeSpace > 0)
                    {
                        if (NatureObjectScript.Type != NatureObjectType.MushroomStump)
                            PlayAnimation(PersonAnimation.Chop);
                        else
                            PlayAnimation(PersonAnimation.GoDown);
                    }
                    else if (person.routine.Count <= 1 && automaticNextTask)
                    {
                        // find nearest natureobject of same type
                        nearestTrsf = myVillage.GetNearestPlant(transform.position, NatureObjectScript.Type, GetTreeCutRange(), true);
                        if (freeSpace == 0 || !nearestTrsf)
                        {
                            int rt = ResourceToInventoryType(NatureObjectScript.ResourceCurrent.Type);
                            if (rt == 1 ? StoreMaterialInventory() : StoreFoodInventory())
                            {
                                if (NatureObjectScript.ResourceCurrent.Amount > 0)
                                    AddTargetTransform(ct.targetTransform, true);
                                else if (nearestTrsf != null)
                                    AddTargetTransform(nearestTrsf, true);
                                NextTask();
                                //else ChatManager.Msg("Keine weiteren Bäume auffindbar in der Nähe!");
                            }
                            else
                            {
                                ChatManager.Msg("Alle " + NatureObjectScript.ResourceCurrent.Name + "lager sind voll!");
                                WalkToCenter();
                            }
                        }
                        else
                        {
                            if (nearestTrsf)
                            {
                                SetTargetTransform(nearestTrsf, true);
                            }
                            else
                            {
                                WalkToCenter();
                            }

                        }
                        ResetAnimations();
                        break;
                    }
                    else if(Routine.Count > 1)
                    {
                        NextTask();
                        break;
                    }

                    // Collect wood of fallen tree, by chopping it into pieces
                    if (ct.taskTime >= NatureObjectScript.ChopTime)
                    {
                        ct.taskTime = 0;
                        //Transform nearestTree = GameManager.village.GetNearestPlant(NatureObjectType.Tree, transform.position, GetTreeCutRange());

                        if (NatureObjectScript.ResourceCurrent.Amount > 0)
                        {
                            GameManager.UnlockResource(res.Id);
                            if (NatureObjectScript.ResourceCurrent.Amount < res.Amount)
                                res = new GameResources(NatureObjectScript.ResourceCurrent);
                            int mat = AddToInventory(res);
                            NatureObjectScript.TakeMaterial(mat);

                            GameManager.UpdateQuestAchievementCollectingResources(new GameResources(res));

                            //if (GameManager.IsDebugging()) ChatManager.Msg(mat + " added to inv");

                            // If still can mine NatureObjectScript, continue
                            if (mat != 0 && NatureObjectScript.ResourceCurrent.Amount > 0 && freeSpace > 0)
                            {
                                if (NatureObjectScript.Type != NatureObjectType.MushroomStump)
                                    audioSource.PlayOneShot(AudioManager.GetRandomChop());
                                break;
                            }

                        }
                    }
                }
                else if (!NatureObjectScript.IsFalling())
                {
                    if (NatureObjectScript.Type != NatureObjectType.MushroomStump)
                    {
                        PlayAnimation(PersonAnimation.Chop);
                    }
                    if (ct.taskTime >= NatureObjectScript.ChopTime)
                    {
                        if (NatureObjectScript.Type != NatureObjectType.MushroomStump)
                        {
                            audioSource.PlayOneShot(AudioManager.GetRandomChop());
                        }
                        ct.taskTime = 0;
                        NatureObjectScript.Mine();
                    }
                }
                else
                {
                    // if tree is falling reset taskTime
                    ct.taskTime = 0;
                }
                break;
            case TaskType.BringToWarehouse: // Bringing material to warehouse
                while (ct.taskRes.Count > 0 && ct.taskRes[0].Amount == 0)
                    ct.taskRes.RemoveAt(0);

                if (ct.taskRes.Count == 0)
                {
                    // only automatically find new tree to cut if person is a lumberjack
                    /*if (person.routine.Count <= 1 && ct.automated && job.Is("Holzfäller"))
                    {
                        nearestTrsf = myVillage.GetNearestPlant(transform.position, NatureObjectType.Tree, GetTreeCutRange(), true);
                        if (nearestTrsf != null) SetTargetTransform(nearestTrsf, true);
                        break;
                    }*/
                    NextTask();
                    break;
                }

                // store resources in building
                if (StoreResourceInBuilding(ct, bs, ct.taskRes[0].Id)) { }
                else ct.taskRes.RemoveAt(0);

                break;
            case TaskType.TakeFromWarehouse: // Taking material from warehouse
                while (ct.taskRes.Count > 0 && ct.taskRes[0].Amount == 0)
                    ct.taskRes.RemoveAt(0);
                if (ct.taskRes.Count == 0)
                {
                    NextTask();
                    break;
                }

                // Take res into inventory
                if (TakeIntoInventory(ct, bs, ct.taskRes[0].Id)) {
                }
                else ct.taskRes.RemoveAt(0);

                break;
            case TaskType.Campfire: // Restock campfire fire wood
                Campfire cf = ct.targetTransform.GetComponent<Campfire>();
                if (invMat != null && invMat.Is("Holz") && invMat.Amount > 0)
                {
                    PlayAnimation(PersonAnimation.GoDown);
                    if (ct.taskTime >= 1f / putMaterialSpeed)
                    {
                        ct.taskTime = 0;
                        int rstk = cf.Restock(1);
                        if (rstk == 0) NextTask();
                        else invMat.Take(rstk);
                    }
                }
                else
                {
                    ResetAnimations();
                    // nothing else to do
                    if (person.routine.Count == 1)
                    {
                        // after random time restock campfire if it is under half of its capacity in wood
                        if (ct.taskTime >= 1f)
                        {
                            ct.taskTime = 0;
                            // the less wood in the campfire the more the people want to bring wood
                            if (cf.GetHealthFactor() <= 0.5f && Random.Range(0, (int)(10 * cf.GetHealthFactor())) == 0)
                            {
                                if (StorageIntoInventory(new GameResources("Holz", GetMaterialInventorySize())))
                                {
                                    AddTargetTransform(ct.targetTransform, true);
                                }
                                else
                                {
                                    // go collect wood items
                                    nearestTrsf = myVillage.GetNearestItemInRange(transform.position, ResourceData.Id("Holz"), GetCollectingRange());
                                    if (nearestTrsf)
                                    {
                                        AddTargetTransform(nearestTrsf, true);
                                        AddTargetTransform(ct.targetTransform, true);
                                    }
                                    else
                                    {
                                        /* TODO: people complain that there is no mroe wood in reach */
                                    }
                                }
                                NextTask();
                                break;
                            }
                            break;
                        }
                        else
                        {
                            // warmup on campfire
                            break;
                        }
                    }
                    else NextTask();
                }
                break;
            case TaskType.Fishing: // Do Fishing
                                   /*if(!NatureObjectScript){
                                       NextTask();
                                       break;
                                   }*/
                ResetAnimations();
                if (/*job.Is("Fischer") && */(invFood == null || invFood.Amount == 0 || invFood.Is("Roher Fisch")))
                {
                    bool hasFish = bs.nearestLake.currentFish > 0;
                    if(hasFish)
                        PlayAnimation(PersonAnimation.Fishing);
                    if (ct.taskTime >= fishingTime && hasFish)
                    {
                        ct.taskTime = 0;
                        int season = GameManager.GetFourSeason();
                        // only fish in summer
                        /*if(season == 0)
                        {
                            ChatManager.Msg("Keine Fische im Winter");
                            NextTask();
                            break;
                        }
                        else*/
                        if (Random.Range(0, season == 0 ? 8 : 5) == 1)// && NatureObjectScript.ResourceCurrent.Amount >= 1)
                        {
                            res = new GameResources("Roher Fisch", 1);
                            int Amount = AddToInventory(res);

                            bs.nearestLake.TakeFish(Amount);

                            GameManager.UpdateQuestAchievementCollectingResources(new GameResources(res));
                            GameManager.UnlockResource(res.Id);

                            /*NatureObjectScript.ResourceCurrent.Take(Amount);
                            if(NatureObjectScript.ResourceCurrent.Amount == 0)
                                NatureObjectScript.Break();*/

                        }
                    }
                    if (person.routine.Count <= 1)
                    {
                        if (GetFreeFoodInventorySpace() == 0 || !hasFish)
                        {
                            // get parent fishermanPlace
                            if (parentBs)
                            {
                                SetTargetTransform(parentBs.transform, true);
                                AddTargetTransform(ct.targetTransform, true);
                                break;
                            }
                            else
                            {
                                ChatManager.Msg("Keine Fischerhütte mehr da! sollte nicht sein...");
                            }
                        }
                        /*else if(NatureObjectScript.ResourceCurrent.Amount == 0)
                        {
                            nearestTrsf = myVillage.GetNearestPlant(transform.position, NatureObjectType.Reed, GetReedRange(), true);
                            if(!nearestTrsf)
                                nearestTrsf = myVillage.GetNearestBuildingID(transform.position, 4).transform;
                        }*/
                    }
                    if (GetFreeFoodInventorySpace() == 0 || !hasFish) //|| NatureObjectScript.ResourceCurrent.Amount == 0)
                        NextTask();
                }
                else
                {
                    StoreFoodInventory();
                    AddTargetTransform(ct.targetTransform, true);
                }
                break;
            case TaskType.Build: // Put resources into blueprint building
                /* TODO: smart: fill up inventory */
                if (HasResourcesToBuild(bs))
                {
                    PlayAnimation(PersonAnimation.Building);
                    if (ct.checkFromFar)
                    {
                        ResetAnimations();
                        SetTargetTransform(ct.targetTransform, true, false);
                        break;
                    }
                    else if (ct.taskTime >= 1f / buildSpeed)
                    {
                        ct.taskTime = 0;
                        bool built = false;
                        foreach (GameResources r in bs.BlueprintBuildCost)
                        {
                            if (invMat.Id == r.Id && r.Amount > 0 && invMat.Amount > 0 && !built)
                            {
                                built = true;
                                invMat.Take(1);
                                r.Take(1);
                                if (r.Amount == 0)
                                {
                                    //if(!bs.BuildFinish())
                                    AddTargetTransform(ct.targetTransform, true);
                                    NextTask();
                                }
                            }
                        }

                        if (!built)
                        {
                            AddTargetTransform(ct.targetTransform, true);
                            NextTask();
                            ResetAnimations();
                        }
                    }
                }
                else
                {
                    if (FindResourcesForBuilding(bs) && automaticNextTask)
                    {
                        AddTargetTransform(ct.targetTransform, true);
                        if (ct.checkFromFar)
                        {
                            AddTargetTransform(ct.targetTransform, true, false);
                        }
                    }
                    NextTask();
                }
                break;
            case TaskType.CollectMushroom: // Collect the mushroom
                if(!NatureObjectScript){
                    NextTask();
                    break;
                }
                ResetAnimations();
                // add resources to persons inventory
                if (NatureObjectScript.gameObject.activeSelf && NatureObjectScript.ResourceCurrent.Amount > 0)
                {
                    PlayAnimation(PersonAnimation.GoDown);
                    if (ct.taskTime >= 1f/collectingSpeed)
                    {
                        ct.taskTime = 0;
                        res = new GameResources(NatureObjectScript.ResourceCurrent);
                        am = AddToInventory(new GameResources(res.Id, 1));
                        if(am > 0) 
                        {
                            NatureObjectScript.ResourceCurrent.Take(1);
                            GameManager.UpdateQuestAchievementCollectingResources(new GameResources(res.Id, am));
                            GameManager.UnlockResource(res.Id);
                            if(NatureObjectScript.ResourceCurrent.Amount == 0)
                            {
                                // Destroy collected mushroom
                                NatureObjectScript.Break();
                                NatureObjectScript.gameObject.SetActive(false);
                            }
                            else break;
                        }
                    }
                    else break;
                }

                NextTask();

                // Find another mushroom to collect
                if(person.routine.Count <= 1/* && job.Is("Sammler")*/ && automaticNextTask)
                {
                    Transform nearestMushroom = myVillage.GetNearestPlant(transform.position, NatureObjectType.Mushroom, GetCollectingRange(), true);
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
                ResetAnimations();
                // add resources to persons inventory
                if (NatureObjectScript && NatureObjectScript.gameObject.activeSelf)
                {
                    if (NatureObjectScript.ResourceCurrent.Amount == 0)
                    {
                        // Destroy collected NatureObjectScript
                        NatureObjectScript.Break();
                        NatureObjectScript.gameObject.SetActive(false);
                        NextTask();
                    }
                    else
                    {
                        PlayAnimation(PersonAnimation.GoDown);
                        if (ct.taskTime > NatureObjectScript.ChopTime)
                        {
                            ct.taskTime = 0;
                            res = new GameResources(NatureObjectScript.ResourceCurrent.Id, NatureObjectScript.MaterialAmPerChop);
                            am = AddToInventory(res);
                            if (am > 0)
                            {
                                NatureObjectScript.ResourceCurrent.Take(am);
                                GameManager.UnlockResource(res.Id);
                                GameManager.UpdateQuestAchievementCollectingResources(new GameResources(res.Id, am));
                            }
                        }
                    }
                    break;
                }
                NextTask();
                break;
            case TaskType.WorkOnField:
                ResetAnimations();
                if(GameManager.GetFourSeason() == 0)
                {
                    NextTask();
                    break;
                }
                if (bs.FieldFullyRotted())
                {
                    bs.StartSeeding();
                }
                else if(bs.FieldGrown())
                {
                    PlayAnimation(PersonAnimation.GoDown);
                    if (ct.taskTime >= harvestCornTime)
                    {
                        ct.taskTime = 0;
                        am = AddToInventory(new GameResources("Korn", 1));
                        if (am == 1)
                        {
                            am = bs.HarvestField();
                        }
                        
                        if(am == 0 || bs.FieldResource == 0)
                        {
                            nearestTrsf = BuildingScript.Identify(bs.ParentBuildingNr).transform;

                            if (nearestTrsf)
                            {
                                SetTargetTransform(nearestTrsf, true);
                                // go back to harvest more 
                                AddTargetTransform(ct.targetTransform, true);
                            }
                            else
                            {
                                ChatManager.Msg("Nicht genügend Platz im Inventar um Korn zu ernten", MessageType.Info);
                                NextTask();
                            }
                            animator.SetBool("goDown", false);
                        }
                    }
                    
                }
                else if(bs.FieldSeeded())
                {
                    // just wait for seeds to grow
                }
                else
                {
                    PlayAnimation(PersonAnimation.GoDown);
                    bs.UpdateFieldTime();
                }
                break;
            case TaskType.PickupItem: // Pickup the item
                if (person.routine[0].targetTransform == null)
                {
                    NextTask();
                    break;
                    //myVillage.GetNearestItemInRange(transform.position, )
                }
                ItemScript itemToPickup = person.routine[0].targetTransform.GetComponent<ItemScript>();
                //if(GameManager.IsDebugging()) ChatManager.Error("PickupItem1;" + itemToPickup.gameObject.activeSelf+";"+itemToPickup.Amount);
                ResetAnimations();
                if (itemToPickup != null && itemToPickup.gameObject.activeSelf && itemToPickup.Amount > 0)
                {
                    PlayAnimation(PersonAnimation.GoDown);
                    //if(GameManager.IsDebugging()) ChatManager.Error("PickupItem2");
                    if (ct.taskTime >= 1f/collectingSpeed)
                    {
                        //if(GameManager.IsDebugging()) ChatManager.Error("PickupItem3");
                        ct.taskTime = 0;
                        res = new GameResources(itemToPickup.ResId, 1);
                        am = AddToInventory(res);
                        if (am > 0)
                        {
                            GameManager.UnlockResource(itemToPickup.ResId);
                            GameManager.UpdateQuestAchievementCollectingResources(res);

                            itemToPickup.Take(1);
                            if (itemToPickup.Amount > 0) break;

                            itemToPickup.gameObject.SetActive(false);

                            int freeSpace = 0;
                            ResourceType pmt = itemToPickup.Resource.type;
                            if(pmt == ResourceType.Building) freeSpace = GetFreeMaterialInventorySpace();
                            if(pmt == ResourceType.Food) freeSpace = GetFreeFoodInventorySpace();
                            
                            // Automatically pickup other items in reach or go to warehouse if inventory is full
                            Transform nearestItem = GameManager.village.GetNearestItemInRange(transform.position, res, GetCollectingRange());
                            if (person.routine.Count == 1 && nearestItem != null && freeSpace > 0 && nearestItem.gameObject.activeSelf && automaticNextTask)
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
                            ChatManager.Msg(FirstName + " kann " + itemToPickup.ResName + " nicht aufsammeln");
                        }
                    } else break;
                }
                NextTask();
                break;
            case TaskType.Craft:
                if(bs == null || AgeState() == 0)
                {
                    NextTask();
                    break;
                }
                else if(bs.IsHut())
                {
                    if (bs.JobId == Job.Id("Jäger") || bs.JobId == Job.Id("Fischer") || bs.JobId == Job.Id("Bauer"))
                    {
                        bool doneSmth = false;

                        workingBuilding = bs;

                        // store
                        foreach (GameResources inv in inventoryList)
                        {
                            if (StoreResourceInBuilding(ct, bs, inv.Id))
                            {
                                doneSmth = true;
                                break;
                            }
                        }
                        if (doneSmth) break;

                        // check if process
                        bool hasProcessed = false;
                        foreach (GameResources st in bs.Storage)
                        {
                            if (st.Results.Count > 0 && bs.DoesProcessResource(st) && bs.GetStorageCurrent(st) > 0)
                            {
                                doneSmth = false;
                                requirements.Add(new GameResources(st.Id, 1));
                                results.AddRange(st.Results);

                                foreach (GameResources resures in results)
                                {
                                    if (bs.GetStorageFree(resures.Id) < resures.Amount)
                                    {
                                        doneSmth = true;
                                    }
                                }
                                if (doneSmth) continue;

                                //if (bs.processProgress > ct.taskTime / st.ProcessTime)
                                //    ct.taskTime = bs.processProgress * st.ProcessTime;
                                //bs.processProgress = ct.taskTime / st.ProcessTime;

                                bs.processProgress += Time.deltaTime / st.ProcessTime;

                                ProcessResource(ct, bs, requirements, results, st.ProcessTime);

                                hasProcessed = true;

                                break;
                            }
                            if(st.Requirements.Count > 0 && bs.DoesProduceResource(st) && bs.GetStorageFree(st) > 0)
                            {
                                doneSmth = false;
                                requirements.AddRange(st.Requirements);
                                results.Add(new GameResources(st.Id, 1));

                                foreach (GameResources requres in requirements)
                                {
                                    if (bs.GetStorageCurrent(requres) < requres.Amount)
                                    {
                                        doneSmth = true;
                                    }
                                }
                                if (doneSmth) continue;

                                //if (bs.processProgress > ct.taskTime / st.ProcessTime)
                                // ct.taskTime = bs.processProgress * st.ProcessTime;
                                //bs.processProgress =  ct.taskTime / st.ProcessTime;
                                bs.processProgress += Time.deltaTime / st.ProcessTime;

                                ProcessResource(ct, bs, requirements, results, st.ProcessTime);

                                hasProcessed = true;

                                break;
                            }
                        }

                        if (!hasProcessed)
                        {
                            /* TODO: check if hut storage available */
                            /*doneSmth = false;
                             * 
                            // check if hut storage available for the overload of items (if there is one)
                            foreach (GameResources st in bs.Storage)
                            {
                                if (st.Results.Count > 0 && bs.DoesProduceResource(st) && bs.GetStorageFree(st) == 0)
                                {
                                    if(TakeIntoInventory(ct, bs, st.Id))
                                    {
                                        doneSmth = true;
                                        break;
                                    }
                                }
                            }

                            if (doneSmth) break;*/

                            // we are done here
                            if (person.routine.Count > 1)
                            {
                                NextTask();
                            }
                            else
                            {
                                if (bs.JobId == Job.Id("Jäger"))
                                    nearestAnimal = myVillage.GetNearestAnimal(transform.position, Animal.Id("Huhn"));
                                if (bs.JobId == Job.Id("Fischer"))
                                    nearestBuilding = myVillage.GetNearestBuildingID(transform.position, Building.Id("Fischerbereich"));
                                if (bs.JobId == Job.Id("Bauer"))
                                    nearestBuilding = myVillage.GetNearestBuildingID(transform.position, Building.Id("Kornfeld"));

                                if (nearestAnimal) nearestTrsf = nearestAnimal.transform;
                                else if (nearestBuilding) nearestTrsf = nearestBuilding.transform;
                                else
                                {
                                    ChatManager.Msg("Nichts in der Nähe gefunden!");
                                    NextTask();
                                    break;
                                }

                                if (nearestTrsf)
                                {
                                    SetTargetTransform(nearestTrsf, true);
                                    /* TODO: save working building for person to go back here after all is done */

                                    //AddTargetTransform(ct.targetTransform, true); // come back here afterwards
                                }
                            }

                        }
                    }

                }
                else
                {
                    NextTask();
                }
                break;
            case TaskType.HuntAnimal:
                // make sure person is a hunter
                //if(job.Is("Jäger"))
                //{
                // only hit animal if in range

                if(!ct.targetTransform)
                {
                    NextTask();
                    break;
                }

                bool inRange = GameManager.InRange(transform.position, ct.targetTransform.position, GetHitRange());
                if (!inRange)
                {
                    SetTargetTransform(ct.targetTransform, true);
                    break;
                }

                diff = ct.targetTransform.position - transform.position;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * 5);
                if (ct.taskTime >= hitTime)
                {
                    ct.taskTime = 0;

                    // get animal from target
                    AnimalScript animal = ct.targetTransform.GetComponent<AnimalScript>();

                    // Hit animal for damage of this person
                    if (animal.Health > 0) animal.Hit(GetHitDamage(), this);

                    if (animal.IsDead())
                    {
                        foreach (GameResources drop in animal.DropResources)
                        {
                            int mat = AddToInventory(drop);
                            if (mat < drop.Amount)
                            {
                                // not enough space in inventory, drop res on ground
                                ItemManager.SpawnItem(drop.Id, drop.Amount - mat, transform.position, 0.8f, 0.8f);
                            }
                            GameManager.UnlockResource(drop.Id);

                            // if there's enough space in inventory, go search another animal

                            nearestTrsf = null;

                            if (CanTakeIntoInventory(drop))
                            {
                                AnimalScript tmp = myVillage.GetNearestAnimal(transform.position, animal.Id);
                                if(tmp != null) nearestTrsf = tmp.transform;
                            }

                            if (nearestTrsf == null)
                            {
                                nearestBuilding = myVillage.GetNearestHutJob(transform.position, Job.Get("Jäger"));
                                if (nearestBuilding != null) nearestTrsf = nearestBuilding.transform;
                            }

                            if (nearestTrsf == null)
                            {
                                ChatManager.Msg("Keine Jagdhütte in Reichweite um Tier zu verarbeiten");
                                NextTask();
                            }
                            else
                            {
                                SetTargetTransform(nearestTrsf, true);
                            }
                            ResetAnimations();

                            break;
                        }
                        
                    }
                }
                /*}
                else
                {
                    NextTask();
                }*/
                break;
            case TaskType.FollowPerson:
                if((ct.targetTransform.transform.position - ct.target).sqrMagnitude >= Grid.SCALE*1.2f)
                {
                    SetTargetTransform(ct.targetTransform, true);
                }
                break;
            case TaskType.TakeIntoVillage:
                PersonScript ps = ct.targetTransform.GetComponent<PersonScript>();
                ps.TakeIntoVillage();
                ChatManager.Msg("Du hast einen neuen Bewohner dazugewonnen!", MessageType.News);
                NextTask();
                break;
            case TaskType.SacrificeResources:
                if (person.inventoryFood != null && person.inventoryFood.Amount > 0 && person.inventoryFood.FaithPoints > 0)
                {
                    if(Mathf.CeilToInt(myVillage.GetFaithPoints()) >= 100)
                    {
                        ChatManager.Msg("Du hast bereits 100 Glaubenspunkte und kannst deshalb nichts mehr opfern!");
                        NextTask();
                        break;
                    }

                    // walk here if checking from far to see if has res to sacrifice
                    if(ct.checkFromFar)
                    {
                        SetTargetTransform(ct.targetTransform, true, false);
                        break;
                    }
                    if (ct.taskTime >= putMaterialSpeed)
                    {
                        ct.taskTime = 0;

                        person.inventoryFood.Take(1);
                        myVillage.ChangeFaithPoints(person.inventoryFood.FaithPoints);

                        ChatManager.Msg("Du hast " + person.inventoryFood.Name + " geopfert", MessageType.Info);
                    }
                    break;
                }
                NextTask();
                break;
            case TaskType.GoWork:
                if(bs)
                {
                    if(bs.IsHut())
                    {
                        bool doneSmth = false;

                        // actually store
                        foreach (GameResources inv in inventoryList)
                        {
                            if (StoreResourceInBuilding(ct, bs, inv.Id))
                            {
                                doneSmth = true;
                                break;
                            }
                        }
                        if (doneSmth) break;

                        if (bs.JobId == Job.Id("Holzfäller"))
                        {
                            nearestTrsf = myVillage.GetNearestPlant(transform.position, NatureObjectType.Tree, GetTreeCutRange(), true);
                        }
                        else if(bs.JobId == Job.Id("Steinmetz"))
                        {
                            nearestTrsf = myVillage.GetNearestPlant(transform.position, NatureObjectType.Rock, GetTreeCutRange(), true);
                        }
                        if (nearestTrsf)
                        {
                            SetTargetTransform(nearestTrsf, true);
                            workingBuilding = bs;
                            break;
                        }
                    }
                }
                NextTask();
                break;
            case TaskType.Walk: // Walk towards the given target
                
                // Firstly check if already in stopradius of targetObject
                float objectStopRadius = 0f;
                if(ct.targetTransform != null)
                {
                    // standard stop radius for objects
                    objectStopRadius = 0.8f;
                    // Set custom stop radius for trees
                    if (ct.targetTransform.tag == NatureObject.Tag && NatureObjectScript != null)
                    {
                        objectStopRadius = NatureObjectScript.GetRadiusInMeters();

                        //Debug.Log(natureObjectCollisions.Count);

                        if (natureObjectCollisions.Count > 0)
                        {
                            bool stopped = false;

                            foreach (NatureObjectScript collision in natureObjectCollisions)
                            {
                                //Debug.Log("collCheck:" + tarBs+"=?="+collision);
                                if (collision == NatureObjectScript)
                                {
                                    EndCurrentPath();
                                    stopped = true;
                                    break;
                                }
                            }
                            if (stopped) break;
                        }
                    }
                    else if (ct.targetTransform.tag == ItemScript.Tag)
                    {
                        objectStopRadius = 0.1f;
                    }
                    else if (ct.targetTransform.tag == Building.Tag)
                    {
                        ct.SetTarget(bs.Center());

                        if (bs.Walkable)
                            objectStopRadius = 0.5f;
                        else if (bs.Blueprint) objectStopRadius = 0.5f;
                        else if (bs.HasEntry)
                            objectStopRadius = 0.1f;
                        else
                        {
                            if (bs.CollisionRadius > float.Epsilon) // overwrite collision radius
                                objectStopRadius = bs.CollisionRadius;
                            else objectStopRadius = 0.01f;

                            //Debug.Log("collCheckCount: "+ buildingCollisions.Count);
                            if (buildingCollisions.Count > 0)
                            {
                                bool stopped = false;

                                foreach (BuildingScript collision in buildingCollisions)
                                {
                                    //Debug.Log("collCheck: " + bs + "=?=" + collision);
                                    if (collision == bs)
                                    {
                                        EndCurrentPath();
                                        stopped = true;
                                        break;
                                    }
                                }
                                if (stopped) break;
                            }
                            else if (buildingColliders.Count > 0)
                            {
                                bool stopped = false;

                                foreach (Collider collision in buildingColliders)
                                {
                                    if (collision.transform == bs.transform)
                                    {
                                        EndCurrentPath();
                                        stopped = true;
                                        break;
                                    }
                                }
                                if (stopped) break;
                            }
                        }

                        // old calculation of interaction radius
                        // objectStopRadius = Mathf.Min(tarBs.GridWidth,tarBs.GridHeight)+0.5f;
                    }
                    else if (ct.targetTransform.tag == "Person")
                    {
                        objectStopRadius = 0.1f;
                    }
                    else if (ct.targetTransform.tag == Animal.Tag)
                    {
                        AnimalScript tarAn = ct.targetTransform.GetComponent<AnimalScript>();
                        if(tarAn) objectStopRadius = tarAn.StopRadius;
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
                    nextTarget = nextNode.Position;//Grid.ToWorld(nextNode.GetX(), nextNode.GetY());
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
                float baseAnimationSpeed = 0.8f;
                if (currentPath.Count > 1 || distance > stopRadius)
                {
                    AnimatorClipInfo[] aci = animator.GetCurrentAnimatorClipInfo(0);
                    AnimatorTransitionInfo ati = animator.GetAnimatorTransitionInfo(0);
                    AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);
                    if (asi.IsName("Walking") || asi.IsName("Idle"))
                    {
                        // Update position/rotation towards target if animation of previous stuff is done
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * 5);

                        transform.position += diff.normalized * currentMoveSpeed * Time.deltaTime;
                    }
                    ResetAnimations();
                    animator.SetBool("walking", true);

                    float rotDiff = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(diff));
                    if (rotDiff >= 10)
                    {
                        currentMoveSpeed *= 0.6f + (180 - rotDiff) / 170f * (0.99f - 0.6f);
                        animator.speed = baseAnimationSpeed*0.5f;
                    }
                    /*else if (distance < stopRadius + moveSpeed*Time.deltaTime*5 && currentMoveSpeed > 0.2f * moveSpeed && currentPath.Count == 1)
                    {
                        currentMoveSpeed *= 0.95f;
                    }*/
                    else
                    {
                        currentMoveSpeed += 0.15f * moveSpeed;
                        animator.speed = baseAnimationSpeed*currentMoveSpeed / moveSpeed;
                    }
                    if (currentMoveSpeed > moveSpeed) currentMoveSpeed = moveSpeed;
                }
                if (distance <= stopRadius)
                {
                    lastTouchedObject = null;

                    if (currentPath.Count > 0)
                    {
                        if (currentPath[0].objectWalkable)
                        {
                            person.SetLastNode(currentPath[0]);
                        }
                        CheckAllHideableObjects();
                    }

                    // If path has ended, continue to next task
                    if (currentPath.Count == 0)
                    {
                        EndCurrentPath();
                        break;
                    }

                    // remove path node
                    currentPath.RemoveAt(0);
                }
                        
                        
                break;
        }
    }

    // continue to next task
    public void NextTask()
    {
        lastTouchedObject = null;

        ResetAnimations();

        if (person.routine.Count > 0)
        {
            if (person.routine[0].taskType == TaskType.Walk) animator.SetBool("walking", false);
            if (person.routine[0].taskType == TaskType.WorkOnField) animator.SetBool("goDown", false);
            if (person.routine[0].taskType == TaskType.MineNatureObject) animator.SetBool("chopping", false);
            // Remove current Task from person.routine
            person.routine.RemoveAt(0);
        }
        else
        {
            animator.SetBool("walking", false);
            animator.SetBool("goDown", false);
            animator.SetBool("chopping", false);
        }

        buildingCollisions.Clear();
        buildingColliders.Clear();
        natureObjectColliders.Clear();
        natureObjectCollisions.Clear();

        StartCurrentTask();
    }

    public enum PersonAnimation
    {
        Walk, Chop, GoDown, Fishing, Building
    }
    public void PlayAnimation(PersonAnimation an)
    {
        ResetAnimations();
        switch(an)
        {
            case PersonAnimation.Walk:
                animator.SetBool("walking", true);
                break;
            case PersonAnimation.Chop:
                animator.SetBool("chopping", true);
                if (woodAxe) woodAxe.gameObject.SetActive(true);
                break;
            case PersonAnimation.GoDown:
                animator.SetBool("goDown", true);
                break;
            case PersonAnimation.Fishing:
                animator.SetBool("fishing", true);
                if (spear) spear.gameObject.SetActive(true);
                break;
            case PersonAnimation.Building:
                animator.SetBool("building", true);
                break;
        }
    }

    public void ResetAnimations()
    {
        animator.speed = 1f;
        animator.SetBool("walking", false);
        animator.SetBool("goDown", false);
        animator.SetBool("chopping", false);
        animator.SetBool("fishing", false);
        animator.SetBool("building", false);
        if (woodAxe) woodAxe.gameObject.SetActive(false);
        if (spear) spear.gameObject.SetActive(false);
    }

    // find path for new current task
    public void StartCurrentTask()
    {
        // If a new path has to be walked, find it with astar
        if (person.routine.Count > 0 && person.routine[0].taskType == TaskType.Walk)
        {
            FindPath(person.routine[0].target, person.routine[0].targetTransform);
        }
    }

    private void EndCurrentPath()
    {
        Vector3 prevRot = transform.rotation.eulerAngles;
        if (person.routine[0].targetTransform != null)
        {
            transform.LookAt(person.routine[0].targetTransform);
            prevRot.y = transform.rotation.eulerAngles.y;
        }
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
        if(p.personIDs.Contains(Nr)) 
        {
            if(!inRadius)
            {
                p.personIDs.Remove(Nr);
                if(p.personIDs.Count == 0) {
                    p.ChangeHidden(true);
                    if(model.GetComponent<cakeslice.Outline>()) model.GetComponent<cakeslice.Outline>().enabled = false;
                    if (p.GetComponent<ClickableObject>()) p.GetComponent<ClickableObject>().outlined = false;
                }
            }
        }
        else
        {
            if(inRadius)
            { 
                p.personIDs.Add(Nr);
                p.ChangeHidden(false);
            }
        }
    }
    public void CheckAllHideableObjects()
    {
        if (Wild) return;

        foreach (NatureObjectScript p in Nature.nature)
        {
            CheckHideableObject(p, p.GetCurrentModel());
        }
        foreach (ItemScript p in ItemScript.allItemScripts)
        {
            CheckHideableObject(p, p.transform);
        }
        foreach (PersonScript p in wildPeople)
        {
            CheckHideableObject(p, p.transform);
        }
        foreach (AnimalScript a in AnimalScript.allAnimals)
        {
            CheckHideableObject(a, a.transform);
        }
    }

    // Target handlers
    public void SetTargetTransform(Transform target, bool automatic)
    {
        SetTargetTransform(target, automatic, true);
    }
    public void SetTargetTransform(Transform target, bool automatic, bool checkFromFar)
    {
        AddRoutineTaskTransform(target, target.position, automatic, checkFromFar, true);
    }
    public void AddTargetTransform(Transform target, bool automatic)
    {
        AddTargetTransform(target, automatic, true);
    }
    public void AddTargetTransform(Transform target, bool automatic, bool checkFromFar)
    {
        AddRoutineTaskTransform(target, target.position, automatic, checkFromFar, false);
    }
    public void AddRoutineTaskTransform(Transform target, Vector3 targetPosition, bool automatic, bool checkFromFar, bool clearRoutine)
    {
        Task walkTask = new Task(TaskType.Walk, targetPosition, target);
        if (target != null)
        {
            HideableObject ho = target.GetComponent<HideableObject>();
            if (ho && ho.isHidden) target = null;
        }
        Task targetTask = TargetTaskFromTransform(target, automatic, checkFromFar);
        if(target != null && targetTask == null) return;
        if(clearRoutine) person.routine.Clear();

        /*foreach(Task t in person.routine)
        {
            if(target != null && t.targetTransform == target ) return;
            if(t.target == targetPosition ) return;
        }*/

        ResetAnimations();

        int rc = person.routine.Count;
        if (targetTask != null && checkFromFar && targetTask.checkFromFar)
        {
            // dont walk there, just check from here
        }
        else
        {
            person.routine.Add(walkTask);

            if (rc == 0)
            {
                FindPath(targetPosition, target);
            }
        }
        if (targetTask != null) person.routine.Add(targetTask);
    }

    // Add task to walk toward target
    public void AddRoutineTaskPosition(Vector3 newTarget, bool automatic, bool clearRoutine)
    {
        Vector3 gridPos = Grid.ToGrid(newTarget);
        if(!Grid.ValidNode((int)gridPos.x, (int)gridPos.z)) return;
        Node n = Grid.GetNode((int)gridPos.x, (int)gridPos.z);
        if (n.nodeObject != null)
        {
            if (AgeState() != 0)
                AddRoutineTaskTransform(n.nodeObject, n.nodeObject.position, automatic, clearRoutine, false);
            return;
        }
        if(clearRoutine) person.routine.Clear();
        Task walkTask = new Task(TaskType.Walk, newTarget);
        ResetAnimations();
        person.routine.Add(walkTask);
        if(person.routine.Count == 1) FindPath(newTarget, null);
    }

    // Add resource task
    public bool AddResourceTask(TaskType type, BuildingScript bs, GameResources res)
    {
        if(!bs) return false;

        Task walkTask = new Task(TaskType.Walk, bs.transform.position, bs.transform);
        int maxInvSize = res.Type == ResourceType.Food ? GetFoodInventorySize() : GetMaterialInventorySize();

        GameResources inv = null;
        // if building is material storage, get material from inventory
        if(res.Type == ResourceType.Food) inv = person.inventoryFood;
        else inv = person.inventoryMaterial;

        if(type == TaskType.TakeFromWarehouse)
        {
            if(res.Amount > bs.GetStorageCurrent(res)) return false;
            if(inv != null && res.Amount > maxInvSize-inv.Amount) return false;
        }
        if(type == TaskType.BringToWarehouse)
        {
            if(inv == null || res.Amount > inv.Amount) return false;
            if(res.Amount > bs.GetStorageTotal(res) - bs.GetStorageCurrent(res)) return false;
        }

        List<GameResources> list = new List<GameResources>();
        list.Add(res);

        // if no inventory resource, take from warehouse
        if (res != null && res.Amount > 0) 
        {
            ResetAnimations();
            person.routine.Add(walkTask);
            person.routine.Add(new Task(type, bs.transform.position, bs.transform, list));
            if(person.routine.Count == 2)
                StartCurrentTask();
            return true;
        }
        return false;
    }
    public Task TargetTaskFromTransform(Transform target, bool automatic, bool checkFirst)
    {
        Task targetTask = null;

        List<GameResources> allResTaskList = new List<GameResources>();
        if (person.inventoryMaterial != null && person.inventoryMaterial.Amount > 0)
            allResTaskList.Add(new GameResources(person.inventoryMaterial));
        if (person.inventoryFood != null && person.inventoryFood.Amount > 0)
            allResTaskList.Add(new GameResources(person.inventoryFood));

        if (target != null)
        {
            switch (target.tag)
            {
                case "Person":
                    PersonScript ps = target.GetComponentInParent<PersonScript>();
                    if (ps.Nr != Nr)
                    {
                        if (ps.Wild)
                            targetTask = new Task(TaskType.TakeIntoVillage, target);
                        else
                            targetTask = new Task(TaskType.FollowPerson, target);
                    }
                    break;
                case Building.Tag:
                    BuildingScript bs = target.GetComponent<BuildingScript>();
                    if (bs.Blueprint)
                    {
                        // check out building from far if not already done
                        targetTask = new Task(TaskType.Build, target, true, checkFirst);
                    }
                    else
                    {
                        if (bs.IsHut())
                        {
                            bool control = Input.GetKey(KeyCode.LeftControl);
                            if (bs.JobId == Job.Id("Bauer") && !control)
                            {
                                targetTask = new Task(TaskType.Craft, target.position, target);
                            }
                            else if (bs.JobId == Job.Id("Jäger") && !control)
                            {
                                targetTask = new Task(TaskType.Craft, target.position, target);
                            }
                            else if (bs.JobId == Job.Id("Fischer") && !control)
                            {
                                targetTask = new Task(TaskType.Craft, target.position, target);
                            }
                            else if (bs.JobId == Job.Id("Holzfäller") && !control)
                            {
                                targetTask = new Task(TaskType.GoWork, target.position, target);
                            }
                            else if (bs.JobId == Job.Id("Steinmetz") && !control)
                            {
                                targetTask = new Task(TaskType.GoWork, target.position, target);
                            }
                            else
                            {
                                // If personscript decides automatically to walk there, just unload everything
                                foreach (GameResources res in allResTaskList)
                                {
                                    if (bs.GetStorageFree(res) > 0) automatic = true;
                                }

                                if (automatic)
                                {
                                    targetTask = new Task(TaskType.BringToWarehouse, target.position, target, allResTaskList);
                                }
                                else
                                {
                                    UIManager.Instance.OnShowObjectInfo(target);
                                    UIManager.Instance.TaskResRequest(this);
                                }
                            }
                        }
                        else
                        {
                            switch (bs.Type)
                            {
                                // Warehouse activity 
                                case BuildingType.Population:
                                    //Transform nearestTree = GameManager.village.GetNearestPlant(transform.position, NatureObjectType.Tree, GetTreeCutRange(), true);
                                    Debug.Log("no task");
                                    break;
                                case BuildingType.Storage:
                                case BuildingType.HutStorage:
                                    // If personscript decides automatically to walk there, just unload everything
                                    foreach (GameResources res in allResTaskList)
                                    {
                                        if (bs.GetStorageFree(res) > 0) automatic = true;
                                    }

                                    if (automatic)
                                    {
                                        targetTask = new Task(TaskType.BringToWarehouse, target.position, target, allResTaskList);
                                    }
                                    else
                                    {
                                        UIManager.Instance.OnShowObjectInfo(target);
                                        UIManager.Instance.TaskResRequest(this);
                                    }
                                    break;
                                case BuildingType.Religion:
                                    // sacrifice all material in inventory
                                    targetTask = new Task(TaskType.SacrificeResources, target, automatic, checkFirst);
                                    break;
                                case BuildingType.Food:
                                    break;
                                case BuildingType.Campfire:
                                    if (bs.HasFire) // Campfire
                                    {
                                        Campfire cf = target.GetComponent<Campfire>();
                                        if (cf != null)
                                        {
                                            targetTask = new Task(TaskType.Campfire, target);
                                        }
                                    }
                                    break;
                                case BuildingType.Luxury:
                                    if (bs.Name == "Schmuckfabrik")
                                    {
                                        targetTask = new Task(TaskType.Craft, target);
                                    }
                                    break;
                                case BuildingType.Field:
                                    if (bs.Name == "Kornfeld")
                                    {
                                        targetTask = new Task(TaskType.WorkOnField, target);
                                    }
                                    else if (bs.Name == "Fischerbereich")
                                    {
                                        targetTask = new Task(TaskType.Fishing, target);
                                    }
                                    break;
                                case BuildingType.Crafting:
                                    if (bs.Name == "Schmiede")
                                    {
                                        // check if person is a blacksmith, then he can craft
                                        if (Job.Is("Schmied"))
                                        {
                                            targetTask = new Task(TaskType.Craft, target);
                                        }
                                        // store bones ino that building
                                        else if (person.inventoryMaterial != null && person.inventoryMaterial.Is("Knochen"))
                                        {
                                            UIManager.Instance.OnShowObjectInfo(target);
                                            UIManager.Instance.TaskResRequest(this);
                                        }
                                        else ChatManager.Msg("Nichts zu tun bei der Schmiede");
                                    }
                                    else if (bs.Name == "Keulenwerkstatt")
                                    {
                                        targetTask = new Task(TaskType.Craft, target);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case NatureObject.Tag:
                    NatureObjectScript nos = target.GetComponent<NatureObjectScript>();
                    switch(nos.Type)
                    {
                        case NatureObjectType.Tree:
                        case NatureObjectType.MushroomStump:
                        case NatureObjectType.Rock:
                            targetTask = new Task(TaskType.MineNatureObject, target);
                            break;
                        case NatureObjectType.Mushroom:
                            targetTask = new Task(TaskType.CollectMushroom, target);
                            break;
                        case NatureObjectType.Crop:
                            targetTask = new Task(TaskType.Harvest, target);
                            break;

                    }
                    break;
                case ItemScript.Tag:
                    ItemScript it = target.GetComponent<ItemScript>();
                    if (it != null)
                    {
                        targetTask = new Task(TaskType.PickupItem, target);
                    }
                    break;
                case Animal.Tag:
                    AnimalScript animal = target.GetComponent<AnimalScript>();
                    if (animal != null)
                    {
                        //if(job.Is("Jäger"))
                        targetTask = new Task(TaskType.HuntAnimal, target);
                        targetTask.taskTime = hitTime;
                        //else ChatManager.Msg("Nur Jäger können Tiere jagen.");
                    }
                    break;
            }
        }
        if (targetTask != null)
        {
            targetTask.automated = automatic;
            if (noTaskBuilding && target != noTaskBuilding)
            {
                noTaskBuilding.RemoveNoTaskPerson();
                noTaskBuilding = null;
            }
        }
        return targetTask;
    }

    public Transform GetFirstTargetTransform()
    {
        for (int i = 0; i < Routine.Count; i++)
            if (Routine[i].taskType != TaskType.Walk && Routine[i].targetTransform != null) return Routine[i].targetTransform;

        return null;
    }

    // get corresponding inventory to resId
    public GameResources InventoryFromResId(int resId)
    {
        if(person.inventoryMaterial != null && person.inventoryMaterial.Id == resId) return person.inventoryMaterial;
        if(person.inventoryFood != null && person.inventoryFood.Id == resId) return person.inventoryFood;
        return null;
    }
    
    // Craft resource into other resources
    public bool ProcessResource(Task ct, BuildingScript bs, List<GameResources> requirements, List<GameResources> results, float processTime)
    {
        // check if building has enough resources stored
        bool enoughRes = true;
        foreach(GameResources res in requirements)
        {
            if(bs.GetStorageCurrent(res) < res.Amount) enoughRes = false;
        }
        // check if enough space in building storage
        bool storageSpace = true;
        foreach(GameResources res in results)
        {
            if(res.Amount > bs.GetStorageFree(res)) enoughRes = false;
        }
        if(enoughRes && storageSpace)
        {
            if(bs.processProgress >= 1f)
            {
                ct.taskTime = 0;
                bs.processProgress = 0;

                foreach(GameResources res in requirements)
                {
                    bs.Take(res);
                }
                foreach(GameResources res in results)
                {
                    bs.Restock(res);
                    GameManager.UnlockResource(res.Id);
                }
            }

            return true;
        }

        return false;
    }

    // Take resource from building into appropriate inventory
    public bool TakeIntoInventory(Task ct, BuildingScript bs, int resId)
    {
        GameResources takeRes = new GameResources(resId, 1);
        GameResources inventory = ResourceToInventory(takeRes.Type);

        // check if building has resource and enough inventory space is available
        if(bs.GetStorageCurrent(takeRes) > 0 && 
            (inventory == null || inventory.Amount == 0 || inventory.Id == resId && GetFreeInventorySpace(takeRes) > 0))
        {
            if(ct.taskTime >= 1f/putMaterialSpeed)
            {
                ct.taskTime = 0;

                int mat = AddToInventory(takeRes);
                if (mat > 0) {
                    bs.Take(takeRes);
                    if (ct.taskRes.Count > 0) ct.taskRes[0].Take(1);
                }
                else ChatManager.Error("TakeIntoInventory:"+bs.Name);
            }

            return true;
        }

        return false;
    }
    public bool CanTakeIntoInventory(GameResources res)
    {
        GameResources inventory = ResourceToInventory(res.Type);

        // check if building has resource and enough inventory space is available
        if ((inventory == null || inventory.Amount == 0 || inventory.Id == res.Id && GetFreeInventorySpace(res) > 0))
        {
            return true;
        }

        return false;
    }

    // Store resource in building
    public bool StoreResourceInBuilding(Task ct, BuildingScript bs, int resId)
    {
        GameResources store = new GameResources(resId, 1);
        GameResources inventory = InventoryFromResId(resId);
        
        if(inventory != null && inventory.Amount > 0 && bs.GetStorageFree(store) > 0)
        {
            if(ct.taskTime >= 1f/putMaterialSpeed)
            {
                ct.taskTime = 0;

                // store res in building
                bs.Restock(store);

                if (ct.taskRes.Count > 0) ct.taskRes[0].Take(1);

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
        if(person.inventoryMaterial == null) return false;
        return StoreResource(person.inventoryMaterial);
    }
    public bool StoreFoodInventory()
    {
        if(person.inventoryFood == null) return false;
        return StoreResource(person.inventoryFood);
    }
    public bool StoreResource(GameResources res)
    {
        BuildingScript bs = workingBuilding;
        if (bs != null)
        {
            if(bs.GetStorageFree(res) == 0)
            {
                bool foundOne = false;
                foreach(int id in bs.ChildBuildingStorage)
                {
                    BuildingScript storChild = BuildingScript.Identify(id);
                    if (!storChild || storChild.Blueprint) continue;
                    if (storChild.GetStorageFree(res) > 0)
                    {
                        bs = storChild;
                        foundOne = true;
                        break;
                    }
                }
                if(!foundOne)  bs = null;
            }
        }
        // check if an appropriate job building is available
        if (bs == null) bs = GameManager.village.GetNearestStorageBuildingJob(transform.position, res, true, false);
        if (bs == null) bs = GameManager.village.GetNearestStorageBuilding(transform.position, res.Id, true, false);
        if (bs == null) return false;
        Transform nearestStorage = bs.transform;
        if(nearestStorage == null) return false;
        ResetAnimations();
        person.routine.Add(new Task(TaskType.Walk, nearestStorage.position));
        person.routine.Add(new Task(TaskType.BringToWarehouse, nearestStorage.position, nearestStorage, new GameResources(res), true, false));
        return true;
    }

    // take res into inventory
    public bool StorageIntoInventory(GameResources res)
    {
        bool depositInventory = !(person.inventoryMaterial == null ||person.inventoryMaterial.Amount == 0 || person.inventoryMaterial.Id == res.Id);
        // check if we first need to store res in a storage building
        if(depositInventory)
        {
            if(!StoreMaterialInventory()) return false;
        }

        bool foundAtleastOne = false;
        List<BuildingScript> sortedBuildings = new List<BuildingScript>();
        sortedBuildings.AddRange(BuildingScript.allBuildingScripts);
        Vector2 gridPos = GridPosition();
        sortedBuildings.Sort((x, y) => Vector2.Distance(gridPos, x.GridPosition()).CompareTo(Vector2.Distance(gridPos, y.GridPosition())));

        int taken = 0;
        foreach (BuildingScript bs in sortedBuildings)
        {
            if(bs.Blueprint) continue;
            if(!GameManager.InRange(transform.position,bs.transform.position,GetStorageSearchRange())) continue;

            int stor = bs.GetStorageCurrent(res);
            if (stor > 0)
            {
                int am = Mathf.Min(stor, res.Amount);
                if (!depositInventory)
                {
                    // take at max the Amount of free inventory space
                    am = Mathf.Max(0,Mathf.Min(GetFreeInventorySpace(res) - taken, am));
                }
                else
                {
                    am = Mathf.Max(0, Mathf.Min(GetInventorySize(res) - taken, am));
                }

                if (AddResourceTask(TaskType.TakeFromWarehouse, bs, new GameResources(res.Id, am)))
                {
                    foundAtleastOne = true;
                    res.Take(am);
                    taken += am;
                }

                if(res.Amount == 0) break;
            }
        }

        return foundAtleastOne;
    }

    public bool FindResourcesForBuilding(BuildingScript toBuild)
    {
        // only sarch for resource building if still need to build
        if(!toBuild.Blueprint) return false;

        foreach(GameResources res in toBuild.BlueprintBuildCost)
        {
            if(res.Amount == 0) continue;
            if(StorageIntoInventory(new GameResources(res)))
            {
                return true;
            }
        }

        return false;
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
        //if(sx == ex && sy == ey) person.routine.RemoveAt(0);
        // if clicked on node with object, but not object
        if (Grid.GetNode(ex, ey).nodeObject != null)
        {
            targetTransform = Grid.GetNode(ex, ey).nodeObject;
        }

        // not walk through campfire, so we walk back to last walkable position
        if (!Grid.GetNode(sx, sy).StartFromHere())
        {
            sx = lastNode.gridX;
            sy = lastNode.gridY;
        }

        currentPath = AStar.FindPath(sx, sy, ex, ey);

        if (targetTransform && targetTransform.tag == Building.Tag)
        {
            BuildingScript bs = targetTransform.GetComponent<BuildingScript>();
            float distToCenter = Vector3.Distance(transform.position, bs.Center());
            float entryToCenter = Vector3.Distance(bs.EntryNode().Position, bs.Center());
            if(distToCenter < entryToCenter*0.7f && currentPath.Count <= 1)
            {
                currentPath.Clear();
            }
            return;
        }

        //if(currentPath != null && currentPath.Count > 1)
        //currentPath.RemoveAt(0);
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

        if (Input.GetMouseButtonDown(0) && !Wild) {
            OnClick();
        }
    }
    private void OnMouseExit()
    {
        highlighted = false;
    }

    private Transform lastTouchedObject;
    private List<BuildingScript> buildingCollisions = new List<BuildingScript>();
    private List<NatureObjectScript> natureObjectCollisions = new List<NatureObjectScript>();
    private void OnCollisionEnter(Collision collision)
    {
        lastTouchedObject = collision.transform;

        ClickableObject co = lastTouchedObject.GetComponent<ClickableObject>();
        if (co != null) lastTouchedObject = co.ScriptedParent();

        if (collision.gameObject.tag == Building.Tag) buildingCollisions.Add(collision.gameObject.GetComponent<BuildingScript>());
        if (collision.gameObject.tag == NatureObject.Tag) natureObjectCollisions.Add(collision.gameObject.GetComponent<NatureObjectScript>());
    }
    private void OnCollisionExit(Collision collision)
    {
        Transform collTrf = collision.transform;

        ClickableObject co = collTrf.GetComponent<ClickableObject>();
        if (co != null) collTrf = co.ScriptedParent();

        if (lastTouchedObject == collTrf) lastTouchedObject = null;
        if (collTrf.tag == Building.Tag) buildingCollisions.Remove(collTrf.gameObject.GetComponent<BuildingScript>());
        if (collTrf.tag == NatureObject.Tag) natureObjectCollisions.Remove(collision.gameObject.GetComponent<NatureObjectScript>());
    }

    private List<Collider> pathColliders = new List<Collider>();
    private List<Collider> natureObjectColliders = new List<Collider>();
    private List<Collider> buildingColliders = new List<Collider>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == Building.Tag)
            if (other.GetComponent<BuildingScript>().Type == BuildingType.Path) pathColliders.Add(other);
            else buildingColliders.Add(other);
        if (other.tag == NatureObject.Tag) natureObjectColliders.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == Building.Tag)
            if (other.GetComponent<BuildingScript>().Type == BuildingType.Path) pathColliders.Remove(other);
            else buildingColliders.Remove(other);
        if (other.tag == NatureObject.Tag) natureObjectColliders.Remove(other);
    }

    private bool CheckIfInFoodRange()
    {
        foreach(BuildingScript bs in BuildingScript.allBuildingScripts)
        {
            if(bs.FoodRange > 0 && !bs.Blueprint)
            {
                if (GameManager.InRange(bs.transform.position, transform.position, bs.FoodRange))
                {
                    foreach(GameResources res in bs.StorageCurrent)
                        if(res.Amount > 0 && res.Edible)
                            return true;
                }
            }
        }
        return false;
    }

    private void StopRoutine()
    {
        while (person.routine.Count > 0) NextTask();
    }
    // movement
    float oldDx, oldDy;
    public void Move(float dx, float dy)
    {
        float moveSpeed = MoveSpeed();

        if (!Controllable()) return;

        if (Mathf.Abs(dx) > float.Epsilon || Mathf.Abs(dy) > float.Epsilon)
        {
            StopRoutine();
            currentMoveSpeed += 0.15f * moveSpeed;
            animator.SetBool("walking", true);
            animator.speed = currentMoveSpeed / moveSpeed * 0.8f;
        }
        else
        {
            currentMoveSpeed *= 0.8f;
            animator.SetBool("walking", false);
            animator.speed = 0.5f;
        }
        if (currentMoveSpeed > moveSpeed) currentMoveSpeed = moveSpeed;

        float fact = 0.5f;
        oldDx = dx * fact + oldDx * (1f - fact);
        oldDy = dy * fact + oldDy * (1f - fact);

        transform.Translate(new Vector3(oldDx, 0, oldDy).normalized * currentMoveSpeed * Time.deltaTime);
        //GetComponent<Rigidbody>().MovePosition(transform.position + new Vector3(oldDx, 0, oldDy).normalized * currentMoveSpeed * Time.deltaTime);

    }
    public void Rotate(float da)
    {
        if (!Controllable()) return;

        float rotSpeed = 80f;
        transform.Rotate(new Vector3(0, da, 0) * rotSpeed * Time.deltaTime);
        //GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, da, 0) * rotSpeed *Time.deltaTime));
    }

    public float MoveSpeed()
    {
        float movFactor = 1f;
        if (walkMode == 1) movFactor = 0.5f; // crouching
        else if (walkMode == 2) movFactor = 1.4f; // running
        if (pathColliders.Count > 0) movFactor += 0.3f;

        switch(AgeState())
        {
            case 0: movFactor *= 1.1f; break; // kids running around
            case 1: movFactor *= 0.8f; break; // walking behind mother
        }

        // limit speed factor, otherwise buggy
        return moveSpeed * Mathf.Min(5f,GameManager.speedFactor) * movFactor;
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
        selected = true;
        selectedPeople.Add(this);
        clickableUnit.selected = true;
        transform.Find("Camera").gameObject.SetActive(true);
    }
    public void OnDeselect()
    {
        selected = false;
        clickableUnit.selected = false;
        transform.Find("Camera").gameObject.SetActive(false);
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
        return !Job.IsUnemployed();
    }
    public int GetMaterialInventorySize()
    {
        return AgeState() == 0 ? 5 : 20;
    }
    public int GetFoodInventorySize()
    {
        return AgeState() == 0 ? 5 : 25;
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
        return 1.2f;
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
    public int GetStorageSearchRange()
    {
        return 20;
    }
    public bool IsFertile()
    {
        if (Gender == Gender.Male) return Age >= 18 && Age <= 50;
        else return Age >= 18 && Age <= 40;
    }
    public bool IsDead()
    {
        return Health <= float.Epsilon;
    }
    public void AgeOneYear()
    {
        bool wasControllable = Controllable();

        person.age++;

        // display message if person now is controllable
        if(!wasControllable && Controllable())
        {
            ChatManager.Msg(FirstName + " ist nun ausgewachsen!", MessageType.News);
        }
    }
    public Transform GetShoulderCamPos()
    {
        return shoulderCameraPos;
    }

    public void TakeIntoVillage()
    {
        if(!Wild)
        {
            Debug.Log("already in village");
            return;
        }
        person.wild = false;
        wildPeople.Remove(this);
        allPeople.Add(this);
    }

    // Grid stuff
    public Vector2 GridPosition()
    {
        Vector3 gridPos = Grid.ToGrid(transform.position);
        return new Vector2(gridPos.x, gridPos.z);
    }

    public int AgeState()
    {
        if (Age < 12) return 0; // random movement
        if (Age < 16) return 1; // following mother
        return 2; // controllable
    }
    public bool ShoulderControl()
    {
        PersonScript ps = PersonScript.FirstSelectedPerson();
        return CameraController.Instance.cameraMode == 1 && ps != null && ps.Nr == Nr;
    }
    public bool Controllable()
    {
        return AgeState() == 2;
    }
    public bool CanGetPregnant()
    {
        return !Pregnant && IsFertile();
    }
    public void GetPregnant()
    {
        person.pregnancyTime = 0;
        person.pregnant = true;
    }

    public void SetPerson(GamePerson newPerson)
    {
        person = newPerson;
        foreach (Task t in newPerson.routine)
            t.setup = false;
    }
    public GamePerson GetPerson()
    {
        return person;
    }

    /*// Person Data
    public PersonData GetPersonData()
    {
        PersonData thisPerson = new PersonData();

        thisPerson.Nr = Nr;
        thisPerson.FirstName = FirstName;
        thisPerson.LastName = LastName;
        thisPerson.Gender = Gender;

        thisPerson.Health = Health;
        thisPerson.hunger = hunger;
        thisPerson.person.saturation = person.saturation;

        thisPerson.Age = Age;
        thisPerson.lifeTimeYears = lifeTimeYears;
        thisPerson.lifeTimeDays = lifeTimeDays;

        thisPerson.motherNr = motherNr;
        thisPerson.pregnant = pregnant;
        thisPerson.pregnancyTime = pregnancyTime;

        thisPerson.disease = disease;

        thisPerson.jobID = job.id;
        thisPerson.workingBuildingId = workingBuilding ? workingBuilding.Nr : -1;
        thisPerson.noTaskBuildingId = noTaskBuilding ? noTaskBuilding.Nr : -1;

        thisPerson.SetPosition(transform.position);
        thisPerson.SetRotation(transform.rotation);

        if(person.inventoryMaterial != null)
        {
            thisPerson.invMatId = person.inventoryMaterial.Id;
            thisPerson.invMatAm = person.inventoryMaterial.Amount;
        }
        else 
        {
            thisPerson.invMatId = 0;
            thisPerson.invMatAm = 0;
        }
        if(person.inventoryFood != null)
        {
            thisPerson.invFoodId = person.inventoryFood.Id;
            thisPerson.invFoodAm = person.inventoryFood.Amount;
        }
        else 
        {
            thisPerson.invFoodId = 0;
            thisPerson.invFoodAm = 0;
        }

        foreach(Task t in person.routine)
            thisPerson.person.routine.Add(t.GetTaskData());

        return thisPerson;
    }
    public void SetPersonData(PersonData person)
    {
        Nr = person.Nr;
        FirstName = person.FirstName;
        LastName = person.LastName;
        Gender = person.Gender;

        Health = person.Health;
        hunger = person.hunger;
        person.saturation = person.person.saturation;

        pregnant = person.pregnant;
        pregnancyTime = person.pregnancyTime;
        motherNr = person.motherNr;

        disease = person.disease;

        Wild = person.Wild;

        job = Job.Get(person.jobID);
        workingBuilding = BuildingScript.Identify(person.workingBuildingId);
        noTaskBuilding = BuildingScript.Identify(person.noTaskBuildingId);

        Age = person.Age;
        lifeTimeYears = person.lifeTimeYears;
        lifeTimeDays = person.lifeTimeDays;

        transform.position = person.GetPosition();
        transform.rotation = person.GetRotation();

        person.inventoryMaterial = new GameResources(person.invMatId, person.invMatAm);
        person.inventoryFood = new GameResources(person.invFoodId, person.invFoodAm);
        
        viewDistance = 2;
        
        foreach(TaskData td in person.person.routine)
            person.routine.Add(new Task(td));
        
        if(person.routine.Count > 0 && person.routine[0].taskType == TaskType.Walk)
        {
            FindPath(person.routine[0].target, person.routine[0].targetTransform);
        }
    }*/

    public void UnEmploy()
    {
        person.jobID = 0;
        if(workingBuilding != null)
        {
            workingBuilding.Unemploy(this);
            workingBuilding = null;
        }
    }

    // inventory handlers 0 = not handled, 1 = building material, 2 = food
    private int ResourceToInventoryType(ResourceType rt)
    {
        switch(rt)
        {
            case ResourceType.Building:
            case ResourceType.DeadAnimal:
            case ResourceType.AnimalParts:
            case ResourceType.Luxury:
            case ResourceType.Crafting: return 1;
            case ResourceType.RawFood:
            case ResourceType.Food: return 2;
        }
        return 0;
    }
    public GameResources ResourceToInventory(ResourceType rt)
    {
        int invrt = ResourceToInventoryType(rt);
        if(invrt == 1) return person.inventoryMaterial;
        if(invrt == 2) return person.inventoryFood;
        return null;
    }
    public int AddToInventory(GameResources res)
    {
        int invResType = ResourceToInventoryType(res.Type);
        int ret = 0;
        GameResources inventory = null;

        if(invResType == 0) {
            ChatManager.Error("Ressource-Typ kann nicht hinzugefügt werden: "+res.Type.ToString());
            return ret;
        }
        if(invResType == 1) inventory = person.inventoryMaterial;
        if(invResType == 2) inventory = person.inventoryFood;

        if (inventory == null || (res.Id != inventory.Id && inventory.Amount == 0))
        {
            if(invResType == 1) person.inventoryMaterial = new GameResources(res.Id);
            if(invResType == 2) person.inventoryFood = new GameResources(res.Id);
        }

        if(invResType == 1) inventory = person.inventoryMaterial;
        if(invResType == 2) inventory = person.inventoryFood;
        
        if(res.Id == inventory.Id)
        {
            int space = 0;
            if(invResType == 1) space = GetFreeMaterialInventorySpace();
            if(invResType == 2) space = GetFreeFoodInventorySpace();
            if (space >= res.Amount)
            {
                ret = res.Amount;
            }
            else if (space < res.Amount && space > 0)
            {
                ret = space;
            }
            inventory.Add(ret);
        }

        if(ret > 0)
        {
            GameManager.UnlockResource(res.Id);
        }

        return ret;
    }
    public int GetFreeMaterialInventorySpace()
    {
        int used = 0;
        if(person.inventoryMaterial != null) used = person.inventoryMaterial.Amount;
        return GetMaterialInventorySize() - used;
    }
    public int GetFreeFoodInventorySpace()
    {
        int used = 0;
        if(person.inventoryFood != null) used = person.inventoryFood.Amount;
        return GetFoodInventorySize() - used;
    }
    public int GetFreeInventorySpace(GameResources res)
    {
        int invrt = ResourceToInventoryType(res.Type);
        if(invrt == 1) return GetFreeMaterialInventorySpace();
        if(invrt == 2) return GetFreeFoodInventorySpace();

        return 0;
    }
    public int GetInventorySize(GameResources res)
    {
        int invrt = ResourceToInventoryType(res.Type);
        if (invrt == 1) return GetMaterialInventorySize();
        if (invrt == 2) return GetFoodInventorySize();

        return 0;
    }
    public bool HasResourcesToBuild(BuildingScript toBuild)
    {
        if (person.inventoryMaterial == null) return false;
        if (person.inventoryMaterial.Amount == 0) return false;

        foreach (GameResources res in toBuild.BlueprintBuildCost)
        {
            if (res.Amount == 0) continue;
            if (person.inventoryMaterial.Id == res.Id) return true;
        }
        return false;
    }
    public void SetInventory(GameResources res)
    {
        if (ResourceToInventoryType(res.Type) == 1) person.inventoryMaterial = new GameResources(res);
        else person.inventoryFood = new GameResources(res);
    }

    // Factors
    public float GetFoodFactor()
    {
        return Hunger / 100;
    }
    public float GetHealthFactor()
    {
        return Health / 100;
    }
    public int FoodUse()
    {
        return 100;  
    }

    // Get Condition of person (0=dead, 4=well)
    public int GetCondition()
    {
        if(Disease != Disease.None) return 1;

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
        if (Pregnant) return "Schwanger";
        int cond = GetCondition();
        switch(cond)
        {
            case 0: return "Tot";
            case 1: return "Krank";
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
    
    // Update transform once per second
    private IEnumerator GamePersonObjectTransform()
    {
        while (true)
        {
            // update transform position rotation on save object
            person.SetTransform(transform);

            // check for food every second
            inFoodRange = CheckIfInFoodRange();

            yield return new WaitForSeconds(1);
        }
    }

    // identify personscript by id
    public static PersonScript Identify(int Nr)
    {
        foreach (PersonScript ps in allPeople)
        {
            if(ps.Nr == Nr) return ps;
        }
        return null;
    }

    public static PersonScript FirstSelectedPerson()
    {
        if(selectedPeople.Count == 0) return null;
        return new List<PersonScript>(selectedPeople)[0];
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
        "Agnes", "Arthur", "Benno", "Clemens", "Emil", "Eugen", "Friedrich", "Henri", "Julius", "Karl", "Konstantin", "Ludwig", "Theodor" //"Finn", "Jan", "Jannik", "Jonas", "Leon", "Luca", "Niklas", "Tim", "Tom", "Alexander", "Christian", "Daniel", "Dennis", "Martin", "Michael"
    };
    private static string[] allFemaleNames = {
        "Antonia", "Edda", "Frieda", "Helene", "Ida", "Martha", "Thea", "Theresa", "Viktoria", "Wilhelmine"  //"Anna", "Hannah", "Julia", "Lara", "Laura", "Lea", "Lena", "Lisa", "Michelle", "Sarah", "Christina", "Katrin", "Melanie", "Nadine", "Nicole"
    };
}


// Old execute task switch statements

/*
 * 
                if (bs.Name == "Schmeide" && job.Is("Schmied"))
                {
                    // bone-tool requires 4 bones
                    res = new GameResources("Knochenwerkzeug", 1);
                    requirements.Add(new GameResources("Knochen", 4));
                    results.Add(res);

                    // store bones in building
                    if(StoreResourceInBuilding(ct, bs, ResourceData.Id("Knochen"))) { }
                    // craft tool
                    else if(ProcessResource(ct, bs, requirements, results, res.ProcessTime)) { }
                    else
                    {
                        ChatManager.Msg("Für ein Knochenwerkzeug brauchst du 4 Knochen!");
                        NextTask();
                    }
                }
                else if(bs.Name == "Keulenwerkstatt")
                {
                    res = new GameResources("Keule", 1);
                    requirements.Add(new GameResources("Holz", 5));
                    results.Add(res);

                    // store wood in building
                    if (StoreResourceInBuilding(ct, bs, ResourceData.Id("Holz"))) { }
                    // craft tool
                    else if (ProcessResource(ct, bs, requirements, results, res.ProcessTime)) { }
                    else
                    {
                        ChatManager.Msg("Für eine Keule brauchst du 5 Holz!");
                        NextTask();
                    }
                }
                else if(bs.Name == "Schmuckfabrik")
                {
                    res = new GameResources("Halskette", 1);
                    requirements.Add(new GameResources("Zahn", 10));
                    results.Add(res);

                    // store wood in building
                    if (StoreResourceInBuilding(ct, bs, ResourceData.Id("Zahn"))) { }
                    // craft tool
                    else if (ProcessResource(ct, bs, requirements, results, res.ProcessTime)) { }
                    else
                    {
                        ChatManager.Msg("Für eine Halskette brauchst du 10 Zähne!");
                        NextTask();
                    }
                }
                */

/*
 * // process animal
foreach(GameResources st in bs.Storage)
{
    if(bs.GetStorageFree(st) > 0 && st.Type == ResourceType.DeadAnimal)
    {
        requirements.Add(new GameResources(st.Id, 1));
        results.AddRange(st.Results);
        break;
    }
}
if(requirements.Count == 0)
{
    NextTask();
}
else
{

 foreach (GameResources st in bs.Storage)
{
    if (st.Type == ResourceType.DeadAnimal)
    {
        requirements.Add(new GameResources(st.Id, 1));
        results.AddRange(st.Results);
    }
}

if (bs.processProgress > ct.taskTime / requirements[0].ProcessTime)
    ct.taskTime = bs.processProgress * requirements[0].ProcessTime;
bs.processProgress = ct.taskTime / requirements[0].ProcessTime;

if (StoreResourceInBuilding(ct, bs, requirements[0].Id)) { }
// process animal
else if (ProcessResource(ct, bs, requirements, results, requirements[0].ProcessTime)) { }
else
{
     TODO: do we want to take res into inventory 

    bool stillReqs = false;
    foreach(GameResources req in requirements)
    {
        if (TakeIntoInventory(ct, bs, req.Id))
        {
            stillReqs = true;
            break;
        }
    }
    if(!stillReqs)
    {
        NextTask();
    }*
    bs.processProgress = 0;

    NextTask();
}
}
else if (bs.JobId == Job.Id("Fischer"))
{
// process raw fish
requirements.Add(new GameResources("Roher Fisch", 1));
results.AddRange(requirements[0].Results);

if (bs.processProgress > ct.taskTime / requirements[0].ProcessTime)
    ct.taskTime = bs.processProgress * requirements[0].ProcessTime;
bs.processProgress = ct.taskTime / requirements[0].ProcessTime;

if (StoreResourceInBuilding(ct, bs, requirements[0].Id)) { }
// process animal
else if (ProcessResource(ct, bs, requirements, results, requirements[0].ProcessTime)) { }
else
{
    bs.processProgress = 0;

    if(person.routine.Count > 1)
    {
        NextTask();
    }
    else
    {
        nearestTrsf = myVillage.GetNearestBuildingID(transform.position, Building.Id("Fischerbereich")).transform;
        SetTargetTransform(nearestTrsf, true);
    }
}
}*/

/*case TaskType.Fisherplace: // Making food out of fish
                 //TODO: decide if we want to check so that fisher can only work at his own workplace 

                // If person is not a fisher, he can't do anything here
                if (!job.Is("Fischer"))
                {
                    NextTask();
                }
                else
                {
                    // animal to process
                    requirements.Add(new GameResources("Roher Fisch", 1));
                    results.Add(new GameResources("Fisch", 1));
                    results.Add(new GameResources("Knochen", 1));

                    bool workingAlone = true;
                    if(workingBuilding != null && workingBuilding.WorkingPeople != null) workingAlone = workingBuilding.WorkingPeople.Count == 1;

                    // Check what to do (leave rawfish, process fish to edible/bones, take from fisherplace to storage)
                    if(StoreResourceInBuilding(ct, bs, ResourceData.Id("Roher Fisch"))) { }
                    else
                    {
                        bool goFishing = workingAlone || workingBuilding.WorkingPeople[0] == Nr;
bool processFish = workingAlone || workingBuilding.WorkingPeople[1] == Nr;
                        // convert rawfish into fish
                        if(ProcessResource(ct, bs, requirements, results, processFishTime) && processFish) { }
                        // take fish into inventory of person
                        else if(TakeIntoInventory(ct, bs, ResourceData.Id("Roher Fisch")) && processFish) { }
                        // take bones into inventory of person
                        else if(TakeIntoInventory(ct, bs, ResourceData.Id("Roher Fisch")) && processFish) { }
                        // walk automatically to warehouse
                        else
                        {
                            bool addedTask = false;
                            if(invFood != null && invFood.Is("Fisch") && invFood.Amount > 0 && processFish)
                            {
                                if(StoreFoodInventory()) addedTask = true;
                            }
                            if(invMat != null && invMat.Is("Knochen") && invMat.Amount > 0 && processFish)
                            {
                                if(StoreMaterialInventory()) addedTask = true;
                            }
                            if(processFish && !workingAlone && automaticNextTask)
                            {
                                // go back to working on fisherplace
                                AddTargetTransform(bs.transform, true);
NextTask();
                            }
                            else if(goFishing && (!workingAlone || addedTask) && automaticNextTask)
                            {
                                // automatically start fishing again
                                nearestTrsf = myVillage.GetNearestPlant(transform.position, NatureObjectType.Reed, GetReedRange(), true);
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
                break;*/

/*case TaskType.TakeEnergySpot:
    if (NatureObjectScript)
    {
        if (NatureObjectScript.IsBroken())
        {
            NextTask();
        }
        else if (ct.taskTime >= 1f / energySpotTakingSpeed)
        {
            ct.taskTime = 0;
            NatureObjectScript.Mine();
            if (NatureObjectScript.IsBroken())
            {
                ChatManager.Msg("Kraftort eingenommen");
                int add = 5;
                if (myVillage.CountEnergySpots() == 1) add = 10;
                myVillage.AddFaithPoints(add);
                NextTask();
            }
        }
    }
    else
    {
        NextTask();
    }
    break;*/

/*case TaskType.ProcessAnimal:
            if(job.Is("Jäger"))
            {
                // animal to process
                GameResources duck = new GameResources("Ente", 1);
requirements.Add(duck);
                results.Add(new GameResources("Fleisch", 6));
                results.Add(new GameResources("Knochen", Random.Range(2, 5)));
                results.Add(new GameResources("Wildzahn", Random.Range(12,15)));
                results.Add(new GameResources("Fell", 1));

                // store duck in building
                if(StoreResourceInBuilding(ct, bs, duck.Id)) { }
                // process animal
                else if(ProcessResource(ct, bs, requirements, results, duck.ProcessTime)) { }
                // take meat into inventory of person
                else if(TakeIntoInventory(ct, bs, ResourceData.Id("Fleisch"))) { }
                // take tooth into inventory of person
                else if (TakeIntoInventory(ct, bs, ResourceData.Id("Zahn"))) { }
                // take fur into inventory of person
                else if(TakeIntoInventory(ct, bs, ResourceData.Id("Fell"))) { }
                // take bones into inventory of person
                else if(TakeIntoInventory(ct, bs, ResourceData.Id("Knochen"))) { }
                // store resources
                else 
                {
                    // store mat inventory
                    if(StoreMaterialInventory() && invMat.Amount > 0) {  }
                    // store food inventory
                    if(StoreFoodInventory() && invFood.Amount > 0) { }

                    NextTask();
}
            }
            else
            {
                NextTask();
            }
            break;*/

/*public class PeopleSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var people = (List<PersonScript>)obj;
        info.AddValue("_count", people.Count);
        foreach(PersonScript personScript in people)
        {
            int Nr = personScript.Nr;
            info.AddValue(Nr + "_firstName", personScript.FirstName);
            info.AddValue(Nr + "_lastName", personScript.LastName);
            info.AddValue(Nr + "_age", personScript.age);
            Transform pt = personScript.transform;
            info.AddValue(Nr + "_posX", pt.position.x);
            info.AddValue(Nr + "_posY", pt.position.x);
            info.AddValue(Nr + "_posZ", pt.position.z);
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
        personScript.Nr = (int)info.GetValue("_nr", typeof(int));
        personScript.FirstName = (string)info.GetValue("_firstName", typeof(string));
        personScript.LastName = (string)info.GetValue("_lastName", typeof(string));
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
