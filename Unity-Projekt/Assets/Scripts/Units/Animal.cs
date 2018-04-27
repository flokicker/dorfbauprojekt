using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : HideableObject {

    // Collection of all animals
    public static HashSet<Animal> allAnimals = new HashSet<Animal>();

	// id of the animal
	public int id;

	// name of the animal
	public string animalName;

	// health of animal
	public int health, maxHealth;
	public float stopRadius;

	// list of dropped resources when killed
    public List<GameResources> resources = new List<GameResources>();

	private Vector3 direction;
	private float directionChangeTime;
	private float moveSpeed;

	// shor info and max distance to water
	private Transform nearestShore;
	private float waterDistance;
	
	// jumping behaviour
	private float jumpTime;
	private float jumpDelta, jumpVelocity;

	// Use this for initialization
	public override void Start () {
		base.Start();

		tag = "Animal";
		
        // handles all outline/interaction stuff
        gameObject.AddComponent<ClickableObject>();

		// keep track of all animals
		allAnimals.Add(this);

		nearestShore = null;
	}
    void OnDestroy()
    {
        allAnimals.Remove(this);
    }

	private void Set(string animalName, int maxHealth, float stopRadius)
	{
		this.animalName = animalName;

		this.maxHealth = (int)(maxHealth * (1f + Random.Range(-0.3f,0.3f)));
		this.health = this.maxHealth;
		this.stopRadius = stopRadius;

		this.moveSpeed = 0.3f;

		this.waterDistance = 10;
	}

	private void Add(int resId, int resAm)
	{
		resources.Add(new GameResources(resId, resAm));
	}

	public void Init(int id)
	{
		this.id = id;
		List<GameResources> res = new List<GameResources>();
		switch(id)
		{
			case 0: 
				Add(GameResources.ANIMAL_DUCK, 1);
				Set("Ente", 25, 0.1f); 
				break;
		}

	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();

		// find nearest water source
		if(nearestShore == null && GameManager.village.nature.shore.Count > 0)
		{
			float minDist = float.MaxValue;
			foreach(Node n in GameManager.village.nature.shore)
			{
				float tmpDist = Vector3.Distance(Grid.ToWorld(n.gridX, n.gridY), transform.position);
				if(tmpDist < minDist)
				{
					minDist = tmpDist;
					nearestShore = n.transform;
				}
			}
		}

		if(IsDead()) gameObject.SetActive(false);

		transform.position += direction * Time.deltaTime;
		if(direction != Vector3.zero)
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
		jumpDelta += jumpVelocity*Time.deltaTime;
		jumpVelocity -= Time.deltaTime*7f;

		// has landed
		if(jumpDelta < 0)
		{
			if(direction != Vector3.zero)
				jumpVelocity = 0.8f;
			else
				jumpVelocity = 0;
			jumpDelta = 0;
		}

		// reset y position of animal to match terrain or water level (0.53)
        float smph = Mathf.Max(Terrain.activeTerrain.SampleHeight(transform.position), 0.53f);
		if(smph <= 0.6f)
		{
			jumpTime = 0;
		}
        transform.position = new Vector3(transform.position.x, Terrain.activeTerrain.transform.position.y + smph + jumpDelta, transform.position.z);

		// if in water range, move randomly, otherwise go towards water
		if(GameManager.InRange(transform.position, nearestShore.position, waterDistance))
		{
			directionChangeTime += Time.deltaTime;
			if(directionChangeTime >= 1)
			{
				directionChangeTime = 0;
				if(Random.Range(0,4)==0)
				{
					if(direction == Vector3.zero || Random.Range(0,3)==0)
					{
						direction = new Vector3(Random.Range(-moveSpeed,moveSpeed),0,Random.Range(-moveSpeed,moveSpeed));
					}
					else direction = Vector3.zero;
				}
			}
		}
		else
		{
			direction = nearestShore.position - transform.position;
			direction.Normalize();
			direction *= moveSpeed;
		}
	}

	public float GetHealthFact()
	{
		return (float)health / (float)maxHealth;
	}

	public void Hit(int damage)
	{
		direction = Vector3.zero;
		directionChangeTime = -2;
		health -= damage;
		if(health < 0) health = 0;
	}

	public bool IsDead()
	{
		return health <= 0;
	}

	public GameResources Drop()
	{
		return resources[0];
	}

	public static int COUNT = 1;
	public static int DUCK = 0;
}
