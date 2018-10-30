using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Singleton<UnitManager> {

    [SerializeField]
    private Transform peopleParentTransform, animalParentTransform;
    [SerializeField]
    private GameObject manPrefab, womanPrefab;
    [SerializeField]
    private List<GameObject> animalPrefabs;

    // Use this for initialization
    public override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	// Spawn a person
    public static void SpawnPerson(GamePerson p)
    {
        GameObject obj = Instantiate(p.gender == Gender.Male ? Instance.manPrefab : Instance.womanPrefab, p.GetPosition(), p.GetRotation(), Instance.peopleParentTransform);
        obj.AddComponent<PersonScript>().SetPerson(p);

        Node personNode = Grid.GetNodeFromWorld(p.GetPosition());
        if(personNode != null) personNode.SetPeopleOccupied(true);

        if (PersonScript.allPeople.Count >= 15 && !Building.IsUnlocked("Grosse Feuerstelle"))
            GameManager.UnlockBuilding(Building.Get("Grosse Feuerstelle"));
        GameManager.UpdateAchievementPerson();

    }

    public static AnimalScript SpawnAnimal(GameAnimal gameAnmial)
    {
        // Spawn prefab
        GameObject newAnimal = Instantiate(gameAnmial.animal.model,
            gameAnmial.GetPosition(), gameAnmial.GetRotation(), Instance.animalParentTransform);

        // Add AnimalScript
        AnimalScript animalScript = newAnimal.AddComponent<AnimalScript>();
        animalScript.SetAnimal(gameAnmial);

        return animalScript;
    }

    /*// Spawn a animal
    public static void SpawnAnimal(int id, Vector3 position)
    {
        GameObject obj = (GameObject)Instantiate(Instance.animalPrefabs[id], position, Quaternion.identity, Instance.animalParentTransform);
        Animal a = obj.AddComponent<Animal>();
        a.Init(id);
    }

    // Spawn a animal
    public static void SpawnAnimal(AnimalData animalData)
    {
        GameObject obj = (GameObject)Instantiate(Instance.animalPrefabs[animalData.id], animalData.GetPosition(), Quaternion.identity, Instance.animalParentTransform);
        obj.AddComponent<Animal>().SetAnimalData(animalData);
    }*/
}
