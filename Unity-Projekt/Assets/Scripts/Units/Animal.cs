﻿using System.Collections;
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

	// Use this for initialization
	public override void Start () {
		base.Start();

		tag = "Animal";
		
        // handles all outline/interaction stuff
        gameObject.AddComponent<ClickableObject>();

		// keep track of all animals
		allAnimals.Add(this);
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

		if(IsDead()) gameObject.SetActive(false);

		transform.position += direction * Time.deltaTime;
		if(direction != Vector3.zero)
		transform.localRotation = Quaternion.LookRotation(direction);

		directionChangeTime += Time.deltaTime;
		if(directionChangeTime >= 1)
		{
			directionChangeTime = 0;
			if(Random.Range(0,4)==0)
			{
				if(direction == Vector3.zero || Random.Range(0,3)==0)
				{
					direction = new Vector3(Random.Range(0f,0.2f),0,Random.Range(0f,0.2f));
				}
				else direction = Vector3.zero;
			}
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