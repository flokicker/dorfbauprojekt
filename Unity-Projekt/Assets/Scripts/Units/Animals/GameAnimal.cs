using UnityEngine;

[System.Serializable]
public class GameAnimal : TransformData
{
    public Animal animal
    {
        get { return Animal.Get(animalId); }
    }
    public int animalId;

    public int nr = -1;

    public bool isLeader;

    public int currentHealth, maxHealth;

    public int age;

    public int dropResourceAmount;

    public bool grownUp, isPregnant;
    public float currentGrowTime, currentPregnantTime, currentLiveTime;

    public int herdId;

    public Gender gender;

    public GameAnimal(string name) : this(Animal.Get(name)) { }
    public GameAnimal(int id) : this(Animal.Get(id)) { }
    public GameAnimal(Animal animal)
    {
        animalId = animal.id;

        gender = (Gender)Random.Range(0, 2);

        grownUp = Random.Range(0, 4) > 0;
        if (grownUp)
            age = Random.Range(animal.ageMax / 8, animal.ageMax);
        else
            age = Random.Range(0, animal.ageMax / 8);

        dropResourceAmount = 1 + Random.Range(age/8, animal.dropResources[0].Amount + age/4);

        isLeader = false;

        maxHealth = animal.healthBase + Random.Range(-animal.healthVar, animal.healthVar);
        currentHealth = maxHealth;
    }
}