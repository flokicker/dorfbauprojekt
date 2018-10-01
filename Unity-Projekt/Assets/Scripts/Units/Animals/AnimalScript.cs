using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimalScript : HideableObject
{
    // Collection of all animals
    public static HashSet<AnimalScript> allAnimals = new HashSet<AnimalScript>();

    private ClickableObject co;
    private Vector3 direction;
    private float directionChangeTime;

    private Animator animator;

    // shore info and max distance to water
    private Transform nearestShore;

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
    public List<GameResources> DropResources
    {
        get { return Animal.dropResources; }
    }
    public float MoveSpeed
    {
        get { return Animal.moveSpeed; }
    }
    public float MaxWaterDistance
    {
        get { return Animal.maxWaterDistance; }
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

    public int Health
    {
        get { return gameAnimal.currentHealth; }
    }
    public int MaxHealth
    {
        get { return gameAnimal.maxHealth; }
    }
    private GameAnimal gameAnimal;

    // Use this for initialization
    public override void Start()
    {
        // keep track of all animals
        allAnimals.Add(this);
        tag = Animal.Tag;

        // handles all outline/interaction stuff
        co = gameObject.AddComponent<ClickableObject>();

        if (!GetComponent<Collider>()) gameObject.AddComponent<BoxCollider>();

        animator = GetComponent<Animator>();

        nearestShore = null;

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

        checkHideableTimer += Time.deltaTime;
        if(checkHideableTimer >= 0.5f)
        {
            checkHideableTimer = 0;
            UpdateBuildingViewRange();
            foreach (PersonScript ps in PersonScript.allPeople)
                ps.CheckHideableObject(this, transform);
        }

        co.SetSelectionCircleRadius(Animal.selectionCircleRadius);

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
                    nearestShore = n.transform;
                }
            }
        }

        if (IsDead()) gameObject.SetActive(false);

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
            jumpVelocity = 0;
        }

        // jumping delta update
        jumpDelta += jumpVelocity * Time.deltaTime;
        jumpVelocity -= Time.deltaTime * 7f;

        // has landed
        if (jumpDelta < 0)
        {
            if (direction != Vector3.zero && Jumping)
            {
                jumpVelocity = 0.8f;
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

        if((int)MaxWaterDistance == 0 && smph < 0.9f) // go away from water
        {
            directionChangeTime = 0;
            for(int x = -1; x < 1; x++)
            {
                for (int y = -1; y < 1; y++)
                {
                    if(Terrain.activeTerrain.SampleHeight(transform.position + Grid.SCALE*new Vector3(x,0,y)) >= 0.95f)
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
        if (!nearestShore || MaxWaterDistance == 0 || GameManager.InRange(transform.position, nearestShore.position, MaxWaterDistance))
        {
            directionChangeTime += Time.deltaTime;
            if (directionChangeTime >= 1)
            {
                directionChangeTime = 0;
                if (Random.Range(0, 4) == 0)
                {
                    if (direction == Vector3.zero || Random.Range(0, 3) == 0)
                    {
                        float dirX = Random.Range(MoveSpeed*0.9f, MoveSpeed*1.1f);
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
        else
        {
            direction = nearestShore.position - transform.position;
            direction.Normalize();
            direction *= MoveSpeed;
        }
    }
    
    public void SetRunningAnimation(bool running)
    {
        if (!animator) return;
        if (!gameObject.activeSelf) return;

        animator.SetBool("running", running);
    }

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

    public void SetAnimal(GameAnimal gameAnimal)
    {
        this.gameAnimal = gameAnimal;
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
