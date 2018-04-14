using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Gender
{
    Male, Female
}

public class Person {

    public int nr;
    public string firstName, lastName;
    public Gender gender;
    public int age;
    public Job job;
    public int viewDistance;
    public float health, hunger;

    public GameResources inventoryMaterial, inventoryFood;

    public Person(int nr, string firstName, string lastName, Gender gender, int age, Job job)
    {
        this.nr = nr;
        this.firstName = firstName;
        this.lastName = lastName;
        this.gender = gender;
        this.age = age;
        this.job = job;
        this.inventoryMaterial = null;
        this.inventoryFood = null;
        viewDistance = 10;

        health = 80;
        hunger = 100;
    }

    public int GetNr()
    {
        return nr;
    }
    public string GetFirstName()
    {
        return firstName;
    }
    public string GetLastName()
    {
        return lastName;
    }
    public Gender GetGender()
    {
        return gender;
    }
    public int GetAge()
    {
        return age;
    }
    public bool IsEmployed()
    {
        return job.id != Job.UNEMPLOYED;
    }
    public Job GetJob()
    {
        return job;
    }
    public int GetMaterialInventorySize()
    {
        return 10;
    }
    public int GetFoodInventorySize()
    {
        return 20;
    }


    /*
    0 = not handled
    1 = building material
    2 = food
     */
    private int ResourceToInventoryType(ResourceType rt)
    {
        switch(rt)
        {
            case ResourceType.BuildingMaterial: return 1;
            case ResourceType.RawFood:
            case ResourceType.Food: return 2;
        }
        return 0;
    }
    public int AddToInventory(GameResources res)
    {
        int invResType = ResourceToInventoryType(res.GetResourceType());
        int ret = 0;
        GameResources inventory = null;

        if(invResType == 0) { 
            GameManager.Error("Ressource-Typ kann nicht hinzugefügt werden: "+res.GetResourceType().ToString());
            return ret;
        }
        if(invResType == 1) inventory = inventoryMaterial;
        if(invResType == 2) inventory = inventoryFood;

        if (inventory == null || (res.id != inventory.id && inventory.GetAmount() == 0))
        {
            if(invResType == 1) inventoryMaterial = new GameResources(res.id);
            if(invResType == 2) inventoryFood = new GameResources(res.id);
        }

        if(invResType == 1) inventory = inventoryMaterial;
        if(invResType == 2) inventory = inventoryFood;
        
        if(res.id == inventory.id)
        {
            int space = 0;
            if(invResType == 1) space = GetFreeMaterialInventorySpace();
            if(invResType == 2) space = GetFreeFoodInventorySpace();
            if (space >= res.GetAmount())
            {
                ret = res.GetAmount();
            }
            else if (space < res.GetAmount() && space > 0)
            {
                ret = space;
            }
            inventory.Add(ret);
        }
        return ret;
    }
    /*public int GetFreeInventorySpace()
    {
        int usedSpace = 0;
        if(inventoryMaterial != null) usedSpace += inventoryMaterial.GetAmount();
        if(inventoryFood != null) usedSpace += inventoryFood.GetAmount();
        return GetInventorySize() - usedSpace;
    }*/

    public int GetFreeMaterialInventorySpace()
    {
        int used = 0;
        if(inventoryMaterial != null) used = inventoryMaterial.amount;
        return GetMaterialInventorySize() - used;
    }
    public int GetFreeFoodInventorySpace()
    {
        int used = 0;
        if(inventoryFood != null) used = inventoryFood.amount;
        return GetFoodInventorySize() - used;
    }
    public int GetCollectingRange()
    {
        return 50;
    }
    public int GetTreeCutRange()
    {
        return 80;
    }
    public int GetReedRange()
    {
        return 10;
    }
    /*public float FoodUse()
    {
        if (age < 16) return 1;
        else return 0.5f;
    }*/

    public bool IsFertile()
    {
        if (gender == Gender.Male) return age >= 16 && age <= 60;
        else return age >= 16 && age <= 40;
    }
    public bool IsDead()
    {
        return health <= float.Epsilon;
    }

    public void AgeOneYear()
    {
        age++;
    }

    public static string getRandomMaleName()
    {
        return allMaleNames[Random.Range(0, allMaleNames.Length)];
    }
    public static string getRandomFemaleName()
    {
        return allFemaleNames[Random.Range(0, allFemaleNames.Length)];
    }
    public static string getRandomLastName()
    {
        return allLastNames[Random.Range(0, allLastNames.Length)];
    }
    private static string[] allLastNames = {
        "Müller", "Schmidt", "Schneider", "Fischer", "Weber", "Meyer", "Wagner", "Becker", "Schulz", "Hoffmann"
    };
    private static string[] allMaleNames = {
        "Finn", "Jan", "Jannik", "Jonas", "Leon", "Luca", "Niklas", "Tim", "Tom", "Alexander", "Christian", "Daniel", "Dennis", "Martin", "Michael"
    };
    private static string[] allFemaleNames = {
        "Anna", "Hannah", "Julia", "Lara", "Laura", "Lea", "Lena", "Lisa", "Michelle", "Sarah", "Christina", "Katrin", "Melanie", "Nadine", "Nicole"
    };
}
