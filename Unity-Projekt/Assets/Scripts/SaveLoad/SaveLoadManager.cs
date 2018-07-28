using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadManager : MonoBehaviour {

	public static int maxSaveStates = 3;
	public static int saveState = -1;

	public static GameState myGameState = new GameState();
	public static bool errorWhileLoading = false;

	public static void SaveAnimals()
	{
		List<AnimalData> animalData = new List<AnimalData>();
		foreach(Animal a in Animal.allAnimals)
		{
			if(a && a.gameObject.activeSelf || a.isHidden)
				animalData.Add(a.GetAnimalData());
		}

		myGameState.animalData = animalData;
	}

	public static void LoadAnimals()
	{
		List<AnimalData> animalData = new List<AnimalData>();
		animalData = myGameState.animalData;

		foreach(Animal a in Animal.allAnimals)
		{
			Destroy(a.gameObject);
		}
		Animal.allAnimals.Clear();

		foreach(AnimalData a in animalData)
		{
			UnitManager.SpawnAnimal(a);
		}
	}

	public static void SaveItems()
	{
		List<ItemData> itemData = new List<ItemData>();
		foreach(Item i in Item.allItems)
		{
			if(i && i.gameObject.activeSelf || i.isHidden)
				itemData.Add(i.GetItemData());
		}

		myGameState.itemData = itemData;
	}

	public static void LoadItems()
	{
		List<ItemData> itemData = new List<ItemData>();
		itemData = myGameState.itemData;

		foreach(Item i in Item.allItems)
		{
			Destroy(i.gameObject);
		}
		Item.allItems.Clear();

		foreach(ItemData i in itemData)
		{
			ItemManager.SpawnItem(i);
		}
	}

	public static void SaveNature()
	{
		myGameState.natureData = NatureObjectScript.AllGameNatureObjects();
	}

	public static void LoadNature()
	{
		foreach(NatureObjectScript p in Nature.nature)
		{
			if(p && (p.gameObject.activeSelf || p.isHidden))
				Destroy(p.gameObject);
		}
		Nature.nature.Clear();
		
		foreach(GameNatureObject p in myGameState.natureData)
		{
			Nature.SpawnNatureObject(p);
		}
	}

	public static void SaveVillage()
	{
		myGameState.buildingdata = BuildingScript.AllGameBuildings();
	}

	public static void LoadVillage()
	{
        BuildingScript.DestroyAllBuildings();
		foreach(GameBuilding bd in myGameState.buildingdata)
		{
			BuildManager.SpawnBuilding(bd);
		}
	}

	public static void SavePeople()
	{
		List<PersonData> peopleData = new List<PersonData>();
		foreach(PersonScript ps in PersonScript.allPeople)
			peopleData.Add(ps.GetPersonData());

		myGameState.peopleData = peopleData;
	}

	public static void LoadPeople()
	{
		List<PersonData> peopleData = new List<PersonData>();
		peopleData = myGameState.peopleData;

		foreach(PersonScript ps in PersonScript.allPeople)
			Destroy(ps.gameObject);
		PersonScript.allPeople.Clear();
		foreach(PersonData p in peopleData)
		{
			UnitManager.SpawnPerson(p);
		}
	}

	public static void SaveGameData()
	{
		Village v = GameManager.village;
        GameData myData = GameManager.gameData;

		myData.foodFactor = v.GetFoodFactor();
		myData.roomspaceFactor = v.GetRoomspaceFactor();
		myData.healthFactor = v.GetHealthFactor();
		myData.fertilityFactor = v.GetFertilityFactor();
		myData.luxuryFactor = v.GetLuxuryFactor();
		myData.totalFactor = v.GetTotalFactor();
        myData.techTree = v.techTree;

        myData.achList = Achievement.GetList();

        myData.faithPoints = v.GetFaithPoints();
        myData.faithEnabled = UIManager.Instance.IsFaithBarEnabled();
        myData.techTreeEnabled = UIManager.Instance.IsTechTreeEnabled();

        myData.unlockedResources = new List<int>(ResourceData.unlockedResources);
        /* TODO: unlocked jobs, buildings */

        /*myData.unlockedBuildings = new bool[Building.COUNT];
		for(int i = 0; i < Building.COUNT; i++)
			myData.unlockedBuildings[i] = Building.IsUnlocked(i);

        myData.unlockedJobs = new bool[Job.COUNT];
        for (int i = 0; i < Job.COUNT; i++)
            myData.unlockedJobs[i] = Job.IsUnlocked(i);

        myData.unlockedResources = new bool[GameResources.COUNT];
		for(int i = 0; i < GameResources.COUNT; i++)
			myData.unlockedResources[i] = GameResources.IsUnlocked(i);*/

		myData.SetPosition(CameraController.LookAtTransform().position);
		myData.cameraRotation = CameraController.Instance.lookAtRotation;
		myData.cameraDistance = CameraController.Instance.cameraDistance;

		myGameState.gameData = myData;
	}

	public static void LoadGameData()
	{
		Village v = GameManager.village;
		GameData myData = myGameState.gameData;

        GameManager.gameData = myData;

		v.SetVillageData(myData);

        if (myData.achList != null)
            Achievement.SetList(myData.achList);
        else Achievement.SetupAchievements();
        
        ResourceData.unlockedResources = new HashSet<int>(myData.unlockedResources);
        /* TODO: set unlocked buildings, jobs */

       /* Building.ResetAllUnlocked();
        for (int i = 0; i < myData.unlockedBuildings.Length; i++)
			if(myData.unlockedBuildings[i])
				Building.Unlock(i);

        Job.ResetAllUnlocked();
        for (int i = 0; i < myData.unlockedJobs.Length; i++)
            if (myData.unlockedJobs[i])
                Job.Unlock(i);

        for (int i = 0; i < myData.unlockedResources.Length; i++)
			if(myData.unlockedResources[i])
				GameManager.UnlockResource(i);*/

		CameraController.SetCameraData(myData);
		
        Debug.Log("GameData loaded");
	}

	public static bool SavedGame(int state)
	{
		if(state == -1) return false;
		return File.Exists(Application.persistentDataPath +"/game"+state+".sav");
	}

	public static string SavedGameName(int state)
	{
		if(state == -1) return "unnamed";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/game" + state + ".sav", FileMode.Open);
        try
        {
			myGameState = bf.Deserialize(stream) as GameState;
			return myGameState.gameData.username;
		}
		catch(Exception ex)
		{
			MainMenuManager.ShowMessage("Corrupt save file!\n"+ex.Message+"\n"+ex.Source);
		}
        finally
        {
            stream.Close();
        }
		return "corrupt file";
	}

	public static void NewGame(int state)
	{
		File.Delete(Application.persistentDataPath +"/game"+state+".sav");
	}

	public static void LoadGame()
    {
        if (saveState == -1)
		{

		}
		else
		{
			errorWhileLoading = false;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(Application.persistentDataPath + "/game" + saveState + ".sav", FileMode.Open);
            try
            {
				myGameState = bf.Deserialize(stream) as GameState;

				LoadNature();
				LoadVillage();
				LoadPeople();
				LoadGameData();
				LoadItems();
				LoadAnimals();
			}
			catch(Exception ex)
			{
				errorWhileLoading = true;
                GameManager.CancelFade();
                UIManager.Instance.OnExitGame();
				MainMenuManager.ShowMessage("Corrupt save file!\n"+ex.Message);
			}
            finally
            {
                stream.Close();
            }
		}
	}

	public static void SaveGame()
	{
		if(saveState == -1)
		{
		}
		else
		{
			if(errorWhileLoading)
			{
				Debug.Log("Loading error state"+saveState);
				//LoadGame();
				return;
			}
			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream(Application.persistentDataPath +"/game"+saveState+".sav", FileMode.Create);

			myGameState = new GameState();

			SaveNature();
			SaveVillage();
			SavePeople();
			SaveGameData();
			SaveItems();
			SaveAnimals();

			bf.Serialize(stream, myGameState);
			stream.Close();
		}
	}
}
