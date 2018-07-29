using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimalScript : HideableObject
{
    // Collection of all animals
    public static HashSet<AnimalScript> allAnimals = new HashSet<AnimalScript>();

    private Vector3 direction;
    private float directionChangeTime;

    // shore info and max distance to water
    private Transform nearestShore;

    // jumping behaviour
    private float jumpTime;
    private float jumpDelta, jumpVelocity;

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
        gameObject.AddComponent<ClickableObject>();

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
            if (direction != Vector3.zero)
                jumpVelocity = 0.8f;
            else
                jumpVelocity = 0;
            jumpDelta = 0;
        }

        // reset y position of animal to match terrain or water level (0.53)
        float smph = Mathf.Max(Terrain.activeTerrain.SampleHeight(transform.position), 0.53f);
        if (smph <= 0.6f)
        {
            jumpTime = 0;
        }
        transform.position = new Vector3(transform.position.x, Terrain.activeTerrain.transform.position.y + smph + jumpDelta, transform.position.z);

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
                        direction = new Vector3(Random.Range(-MoveSpeed, MoveSpeed), 0, Random.Range(-MoveSpeed, MoveSpeed));
                    }
                    else direction = Vector3.zero;
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
    
    public bool IsDead()
    {
        return Health <= 0;
    }
    public float HealthFactor()
    {
        return (float)Health / MaxHealth;
    }

    public void Hit(int damage)
    {
        if (damage < 0) Debug.Log("taken damage < 0 for anmial");
        direction = Vector3.zero;
        directionChangeTime = -2;
        gameAnimal.currentHealth -= damage;
        if (gameAnimal.currentHealth < 0) gameAnimal.currentHealth = 0;
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
