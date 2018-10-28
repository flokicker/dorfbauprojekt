using UnityEngine;

[System.Serializable]
public class GameAnimal : TransformData
{
    public Animal animal
    {
        get { return Animal.Get(animalId); }
    }
    public int animalId;

    public int currentHealth, maxHealth;

    public bool grownUp;
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

        maxHealth = animal.healthBase + Random.Range(-animal.healthVar, animal.healthVar);
        currentHealth = maxHealth;
    }
}