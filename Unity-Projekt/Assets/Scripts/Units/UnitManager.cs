using System.Collections;
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
    public static void SpawnPerson(Person p)
    {
        GameObject obj = (GameObject)Instantiate(Instance.personPrefab, new Vector3(UnityEngine.Random.Range(-5, 5) * Grid.SCALE, 0, 
			UnityEngine.Random.Range(-5, 5) * Grid.SCALE), Quaternion.identity, Instance.peopleParentTransform);
        obj.AddComponent<PersonScript>().SetPerson(p);
    }

    // Spawn a animal
    public static void SpawnAnimal(int id, Vector3 position)
    {
        GameObject obj = (GameObject)Instantiate(Instance.animalPrefabs[id], position, Quaternion.identity, Instance.animalParentTransform);
        Animal a = obj.AddComponent<Animal>();
        a.Init(id);
    }
}
