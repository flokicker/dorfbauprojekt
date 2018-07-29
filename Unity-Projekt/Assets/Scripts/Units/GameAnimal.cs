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

    public GameAnimal(string name) : this(Animal.Get(name)) { }
    public GameAnimal(int id) : this(Animal.Get(id)) { }
    public GameAnimal(Animal animal)
    {
        animalId = animal.id;

        maxHealth = animal.healthBase + Random.Range(-animal.healthVar, animal.healthVar);
        currentHealth = maxHealth;
    }
}