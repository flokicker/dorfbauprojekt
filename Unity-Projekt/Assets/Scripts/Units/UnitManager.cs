using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Singleton<UnitManager> {

    [SerializeField]
    private Transform peopleParentTransform;
    [SerializeField]
    private GameObject personPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	// Spawn a person
    public static void SpawnPerson(Person p)
    {
        GameObject obj = (GameObject)Instantiate(Instance.personPrefab, new Vector3(UnityEngine.Random.Range(-5, 5) * Grid.SCALE, 0, 
			UnityEngine.Random.Range(-5, 5) * Grid.SCALE), Quaternion.identity, Instance.peopleParentTransform);
        obj.AddComponent<PersonScript>().SetPerson(p);
    }
}
