﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Singleton<UnitManager> {

    [SerializeField]
    private Transform peopleParentTransform, animalParentTransform;
    [SerializeField]
    private GameObject personPrefab;
    [SerializeField]
    private List<GameObject> animalPrefabs;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	// Spawn a person
    public static void SpawnPerson(PersonData p)
    {
        GameObject obj = (GameObject)Instantiate(Instance.personPrefab, Vector3.zero, Quaternion.identity, Instance.peopleParentTransform);
        obj.AddComponent<PersonScript>().SetPersonData(p);
    }

    // Spawn a animal
    public static void SpawnAnimal(int id, Vector3 position)
    {
        GameObject obj = (GameObject)Instantiate(Instance.animalPrefabs[id], position, Quaternion.identity, Instance.animalParentTransform);
        Animal a = obj.AddComponent<Animal>();
        a.Init(id);
    }
}
