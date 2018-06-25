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
		List<PlantData> floraData = new List<PlantData>();
		foreach(Plant p in Nature.flora)
		{
			if(p && (p.gameObject.activeSelf || p.isHidden))
				floraData.Add(p.GetPlantData());
		}

		myGameState.floraData = floraData;
	}

	public static void LoadNature()
	{
		List<PlantData> floraData = new List<PlantData>();
		floraData = myGameState.floraData;

		foreach(Plant p in Nature.flora)
		{
			if(p && (p.gameObject.activeSelf || p.isHidden))
				Destroy(p.gameObject);
		}
		Nature.flora.Clear();
		
		foreach(PlantData p in floraData)
		{
			Nature.SpawnPlant(p);
		}
	}

	public static void SaveVillage()
	{
		List<BuildingData> buildingdata = new List<BuildingData>();
		foreach(Building b in Building.allBuildings)
			buildingdata.Add(b.GetBuildingData());
		
		myGameState.buildingdata = buildingdata;
	}

	public static void LoadVillage()
	{
		List<BuildingData> buildingdata = new List<BuildingData>();
		buildingdata = myGameState.buildingdata;

		foreach(Building b in Building.allBuildings)
			Destroy(b.gameObject);
		Building.allBuildings.Clear();
		foreach(BuildingData bd in buildingdata)
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
		GameData myData = new GameData();

		myData.username = GameManager.username;
		myData.coins = v.GetCoins();
		myData.currentDay = GameManager.GetTotDay();
		myData.foodFactor = v.GetFoodFactor();
		myData.roomspaceFactor = v.GetRoomspaceFactor();
		myData.healthFactor = v.GetHealthFactor();
		myData.fertilityFactor = v.GetFertilityFactor();
		myData.luxuryFactor = v.GetLuxuryFactor();
		myData.totalFactor = v.GetTotalFactor();

        myData.faithEnabled = UIManager.Instance.IsFaithBarEnabled();
        myData.faithPoints = v.GetFaithPoints();

        myData.peopleGroups = new List<int>[10];
        for (int i = 0; i < 10; i++)
            myData.peopleGroups[i] = GameManager.GetGameSettings().GetPeopleGroup(i);

        myData.unlockedBuildings = new bool[Building.COUNT];
		for(int i = 0; i < Building.COUNT; i++)
			myData.unlockedBuildings[i] = Building.IsUnlocked(i);

		myData.unlockedResources = new bool[GameResources.COUNT];
		for(int i = 0; i < GameResources.COUNT; i++)
			myData.unlockedResources[i] = GameResources.IsUnlocked(i);

		myData.SetPosition(CameraController.LookAtTransform().position);
		myData.cameraRotation = CameraController.Instance.lookAtRotation;
		myData.cameraDistance = CameraController.Instance.cameraDistance;

		myGameState.gameData = myData;
	}

	public static void LoadGameData()
	{
		Village v = GameManager.village;
		GameData myData = myGameState.gameData;

		v.SetVillageData(myData);

		GameManager.username = myData.username;
		GameManager.SetDay(myData.currentDay);

        for(int i = 0; i < myData.peopleGroups.Length; i++)
            GameManager.GetGameSettings().SetPeopleGroup(i, myData.peopleGroups[i]);

		for(int i = 0; i < myData.unlockedBuildings.Length; i++)
			if(myData.unlockedBuildings[i])
				Building.Unlock(i);
				
		for(int i = 0; i < myData.unlockedResources.Length; i++)
			if(myData.unlockedResources[i])
				GameManager.UnlockResource(i);

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
		try
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream(Application.persistentDataPath +"/game"+state+".sav", FileMode.Open);
			myGameState = bf.Deserialize(stream) as GameState;
			stream.Close();
			return myGameState.gameData.username;
		}
		catch(Exception ex)
		{
			MainMenuManager.ShowMessage("Corrupt save file!\n"+ex.Message);
		} 
		return "corrupt file";
	}

	public static void NewGame(int state)
	{
		File.Delete(Application.persistentDataPath +"/game"+state+".sav");
	}

	public static void LoadGame()
	{
		if(saveState == -1)
		{

		}
		else
		{
			errorWhileLoading = false;
			try
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream stream = new FileStream(Application.persistentDataPath +"/game"+saveState+".sav", FileMode.Open);
				myGameState = bf.Deserialize(stream) as GameState;
				stream.Close();

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
