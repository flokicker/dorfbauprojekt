using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public class AnimalScript : HideableObject
{
    // Collection of all animals
    public static HashSet<AnimalScript> allAnimals = new HashSet<AnimalScript>();

    private ClickableObject co;
    private Vector3 direction;
    private HerdCenter herdCenter;
    private float directionChangeTime;

    private Animator animator;

    // shore info and max distance to water
    private Node nearestShore;

    // Last personscript that attacked
    private PersonScript attacker;

    // jumping behaviour
    private float jumpTime;
    private float jumpDelta, jumpVelocity;

    private float checkHideableTimer = 0;

    public int Id
    {
        get { return Animal.id; }
    }
    public string Name
    {
        get { return Animal.name; }
    }
    public int DropResourceId
    {
        get { return Animal.dropResources[0].Id; }
    }
    public float MoveSpeed
    {
        get { return Animal.moveSpeed; }
    }
    public float MaxWaterDistance
    {
        get { return Animal.maxWaterDistance; }
    }
    public int MaxHerdDistance
    {
        get { return Animal.maxDistFromHerdCenter; }
    }
    public int MaxCountHerd
    {
        get { return Animal.maxCountHerd; }
    }
    public int HerdCount
    {
        get { return herdCenter.animalCount; }
    }
    public int MaxAge
    {
        get { return Animal.ageMax; }
    }
    public float ReproductionRate
    {
        get { return Mathf.Max(0.0001f, Animal.reproductionRate); }
    }
    public int PregnantTime
    {
        get { return Animal.pregnantTime; }
    }
    public int GrowUpTime
    {
        get { return Mathf.Max(1,Animal.growUpTime); }
    }
    public int LiveTime
    {
        get { return Animal.liveTime; }
    }
    public float StopRadius
    {
        get { return Animal.stopRadius; }
    }
    public bool Jumping
    {
        get { return Animal.jumping; }
    }
    public Sprite Icon
    {
        get { return Animal.icon; }
    }
    public Animal Animal
    {
        get { return gameAnimal.animal; }
    }

    public int HerdId
    {
        get { return gameAnimal.herdId; }
    }
    public int Age
    {
        get { return gameAnimal.age; }
    }
    public int Health
    {
        get { return gameAnimal.currentHealth; }
    }
    public int MaxHealth
    {
        get { return gameAnimal.maxHealth; }
    }
    public int DropResourceAmount
    {
        get { return gameAnimal.dropResourceAmount; }
    }
    public float CurrentGrowTime
    {
        get { return gameAnimal.currentGrowTime; }
    }
    public bool GrownUp
    {
        get { return gameAnimal.grownUp; }
    }
    public bool IsPregnant
    {
        get { return gameAnimal.isPregnant; }
    }
    public Gender Gender
    {
        get { return gameAnimal.gender; }
    }
    private GameAnimal gameAnimal;

    // Use this for initialization
    public override void Start()
    {
        // keep track of all animals
        if (gameAnimal.nr == -1) gameAnimal.nr = allAnimals.Count;
        allAnimals.Add(this);
        tag = Animal.Tag;

        Transform modelParent = transform.GetChild(0);

        // handles all outline/interaction stuff
        co = modelParent.gameObject.AddComponent<ClickableObject>();
        co.SetScriptedParent(transform);
        co.SetSelectionCircleRadius(Animal.selectionCircleRadius);

        if (!modelParent.GetComponent<Collider>()) modelParent.gameObject.AddComponent<BoxCollider>();


        animator = GetComponent<Animator>();

        nearestShore = null;

        onlyNoRenderOnHide = true;

        // get right herd center
        herdCenter = Nature.Instance.herdParent.GetChild(gameAnimal.herdId).GetComponent<HerdCenter>();
        herdCenter.animalCount++;

        StartCoroutine(UpdateRoutine());

        base.Start();
    }
    public override void OnDestroy()
    {
        allAnimals.Remove(this);
        base.OnDestroy();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        herdCenter.SetAnimalSelected(gameAnimal.nr, co.isSelected);

        if (IsDead())
        {
            if(gameAnimal.isLeader) ChooseNextLeader();

            herdCenter.animalCount--;

            gameObject.SetActive(false);
        }

        gameAnimal.currentLiveTime += Time.deltaTime;
        if(gameAnimal.currentLiveTime >= 60*10)
        {
            gameAnimal.currentLiveTime = 0;
            gameAnimal.age++;
            if(gameAnimal.age >= MaxAge)
            {
                gameAnimal.currentHealth = 0;
            }
        }

        UpdatePregnancy();
        UpdateNearestWater();
        UpdateMovement();
        UpdateGrowth();
    }

    // Update methods
    private void UpdatePregnancy()
    {
        if (!IsPregnant) return;

        gameAnimal.currentPregnantTime += Time.deltaTime;

        if(gameAnimal.currentPregnantTime >= PregnantTime)
        {
            gameAnimal.isPregnant = false;

            GameAnimal toSpawn = new GameAnimal(Animal);
            toSpawn.herdId = gameAnimal.herdId;
            toSpawn.SetPosition(transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)) * Grid.SCALE);
            toSpawn.SetRotation(Quaternion.Euler(0, Random.Range(0, 360), 0));
            toSpawn.grownUp = false;
            toSpawn.age = 0;
            UnitManager.SpawnAnimal(toSpawn);
        }
    }
    private void UpdateNearestWater()
    {
        // find nearest water source
        if (nearestShore == null && Nature.shore.Count > 0)
        {
            float minDist = float.MaxValue;
            foreach (Node n in Nature.shore)
            {
                float tmpDist = Vector3.Distance(Grid.ToWorld(n.gridX, n.gridY), transform.position);
                if (tmpDist < minDist)
                {
                    minDist = tmpDist;
                    nearestShore = n;
                }
            }
        }
    }
    private void UpdateMovement()
    {
        if (attacker != null)
        {
            direction = Vector3.zero;
            SetRunningAnimation(false);
        }

        transform.position += direction * Time.deltaTime;
        if (direction != Vector3.zero)
        {
            transform.localRotation = Quaternion.LookRotation(direction);
            jumpTime += Time.deltaTime;
        }
        else
        {
            jumpTime = 0;
        }

        // jumping delta update
        jumpDelta += jumpVelocity * Time.deltaTime;
        jumpVelocity -= Time.deltaTime * 7f;

        // has landed
        if (jumpDelta < 0)
        {
            if (direction != Vector3.zero && Jumping)
            {
                jumpVelocity = 0.7f;
            }
            else
            {
                jumpVelocity = 0;
            }
            jumpDelta = 0;
        }

        // reset y position of animal to match terrain or water level (0.53)
        float smph = Terrain.activeTerrain.SampleHeight(transform.position);
        smph = MaxWaterDistance > 0 ? Mathf.Max(smph, 0.53f) : smph;
        if (smph <= 0.6f)
        {
            jumpTime = 0;
        }
        
        transform.position = new Vector3(transform.position.x, Terrain.activeTerrain.transform.position.y + smph + jumpDelta, transform.position.z);

        if ((int)MaxWaterDistance == 0 && smph < 0.9f) // go away from water
        {
            directionChangeTime = 0;
            for (int x = -1; x < 1; x++)
            {
                for (int y = -1; y < 1; y++)
                {
                    if (Terrain.activeTerrain.SampleHeight(transform.position + Grid.SCALE * new Vector3(x, 0, y)) >= 0.95f)
                    {
                        direction = new Vector3(x, 0, y).normalized * Random.Range(MoveSpeed * 0.9f, MoveSpeed * 1.1f);
                        x = 1;
                        y = 1;
                        SetRunningAnimation(true);
                    }
                }
            }
        }

        // if in water range or maxwatdist=0, move randomly, otherwise go towards water
        bool inHerdRange = GameManager.InRange(transform.position, herdCenter.transform.position, MaxHerdDistance);
        if ((nearestShore == null || MaxWaterDistance == 0 || GameManager.InRange(transform.position, nearestShore.Position, MaxWaterDistance)) && inHerdRange)
        {
            directionChangeTime += Time.deltaTime;
            if (directionChangeTime >= 1)
            {
                directionChangeTime = 0;
                if (Random.Range(0, 4) == 0)
                {
                    if (direction == Vector3.zero || Random.Range(0, 3) == 0)
                    {
                        float dirX = Random.Range(MoveSpeed * 0.9f, MoveSpeed * 1.1f);
                        if (Random.Range(0, 2) == 0) dirX = -dirX;
                        float dirY = Random.Range(MoveSpeed * 0.9f, MoveSpeed * 1.1f);
                        if (Random.Range(0, 2) == 0) dirY = -dirY;
                        direction = new Vector3(dirX, 0, dirY);
                        SetRunningAnimation(true);

                    }
                    else
                    {
                        direction = Vector3.zero;
                        SetRunningAnimation(false);
                    }
                }
            }
        }
        else if(!inHerdRange)
        {
            direction = herdCenter.transform.position - transform.position;
            direction.Normalize();
            direction *= MoveSpeed;
        }
        else
        {
            direction = nearestShore.Position - transform.position;
            direction.Normalize();
            direction *= MoveSpeed;
        }
    }
    private void UpdateGrowth()
    {
        if (gameAnimal.grownUp)
            gameAnimal.currentGrowTime = 0;
        else
        {
            if (gameAnimal.currentGrowTime >= GameManager.secondsPerDay * GrowUpTime)
                gameAnimal.grownUp = true;
            else gameAnimal.currentGrowTime += Time.deltaTime;
        }
    }

    public void SetRunningAnimation(bool running)
    {
        if (!animator) return;
        if (!gameObject.activeSelf) return;

        animator.SetBool("running", running);
    }

    // HP
    public bool IsDead()
    {
        return Health <= 0;
    }
    public float HealthFactor()
    {
        return (float)Health / MaxHealth;
    }
    public void Hit(int damage, PersonScript attacker)
    {
        if (damage < 0) Debug.Log("taken damage < 0 for anmial");
        this.attacker = attacker;
        gameAnimal.currentHealth -= damage;
        if (gameAnimal.currentHealth < 0) gameAnimal.currentHealth = 0;
    }

    public override void ChangeHidden(bool hide)
    {
        if (hide)
        {
            direction = Vector3.zero;
            directionChangeTime = Random.Range(0.4f, 1f);
        }

        base.ChangeHidden(hide);
    }

    // Leader of herd
    public void SetLeader()
    {
        gameAnimal.isLeader = true;
    }
    public void ChooseNextLeader()
    {
        AnimalScript nextLeader = null;
        foreach (AnimalScript anis in allAnimals)
        {
            if (anis.HerdId == HerdId && (nextLeader == null || anis.GrownUp))
            {
                nextLeader = anis;
            }
        }

        // if no next leader, this herd has died out
        if (nextLeader)
        {
            nextLeader.SetLeader();
        }
    }

    public void SetAnimal(GameAnimal gameAnimal)
    {
        this.gameAnimal = gameAnimal;
    }

    public void UpdateSize()
    {
        if (gameAnimal.grownUp)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, gameAnimal.currentGrowTime / (GameManager.secondsPerDay * GrowUpTime));
        }
    }

    private IEnumerator UpdateRoutine()
    {
        while (!setup) { yield return null; }
        while (!IsDead())
        { 
            // update transform position rotation on save object
            gameAnimal.SetTransform(transform);

            UpdateBuildingViewRange();
            foreach (PersonScript ps in PersonScript.allPeople)
                ps.CheckHideableObject(this, transform);

            // check if herd is not already at maximum
            if(gameAnimal.grownUp && !gameAnimal.isPregnant && gameAnimal.gender == Gender.Female && 
                (int)Random.Range(0,60/ReproductionRate) == 0 && herdCenter.animalCount < MaxCountHerd)
            {
                gameAnimal.isPregnant = true;
                gameAnimal.currentPregnantTime = 0;
            }
            
            if(gameAnimal.isLeader)
            {
                herdCenter.transform.position = transform.position;
            }

            UpdateSize();

            yield return new WaitForSeconds(1);
        }
    }

    public static List<GameAnimal> AllGameAnimals()
    {
        List<GameAnimal> ret = new List<GameAnimal>();
        foreach (AnimalScript animal in allAnimals)
            ret.Add(animal.gameAnimal);
        return ret;
    }
    public static void DestroyAllAnimals()
    {
        foreach (AnimalScript a in allAnimals)
            Destroy(a.gameObject);
        allAnimals.Clear();
    }
}
