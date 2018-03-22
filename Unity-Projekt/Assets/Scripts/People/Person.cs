using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Gender
{
    Male, Female
}

public class Person {

    private int nr;
    private string firstName, lastName;
    private Gender gender;
    private int age;
    private Job job;

    private GameResources inventory;

    public Person(int nr, string firstName, string lastName, Gender gender, int age, Job job)
    {
        this.nr = nr;
        this.firstName = firstName;
        this.lastName = lastName;
        this.gender = gender;
        this.age = age;
        this.job = job;
        this.inventory = null;
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
        return job.GetID() > 0;
    }
    public Job GetJob()
    {
        return job;
    }
    public GameResources GetInventory()
    {
        return inventory;
    }
    public int GetInventorySize()
    {
        return 10 + age;
    }
    public int AddToInventory(GameResources res)
    {
        int ret = 0;
        if (inventory == null || (res.GetID() != inventory.GetID() && inventory.GetAmount() == 0))
        {
            inventory = new GameResources(res.GetID());
        }
        
        if(res.GetID() == inventory.GetID())
        {
            int space = GetInventorySize() - inventory.GetAmount();
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
    public int GetFreeInventorySpace()
    {
        return GetInventorySize() - inventory.GetAmount();
    }
    public int GetCollectingRange()
    {
        return 30;
    }
    public int GetTreeCutRange()
    {
        return 80;
    }
    public float FoodUse()
    {
        if (age < 16) return 1;
        else return 0.5f;
    }

    public bool IsFertile()
    {
        if (gender == Gender.Male) return age >= 16 && age <= 60;
        else return age >= 16 && age <= 40;
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
