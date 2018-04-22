using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadManager : MonoBehaviour {

	public static int maxSaveStates = 3;
	public static int saveState;

	public static void SaveNature()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream stream = new FileStream(Application.persistentDataPath +"/nature"+saveState+".sav", FileMode.Create);

		List<PlantData> floraData = new List<PlantData>();
		foreach(Plant p in GameManager.village.nature.flora)
		{
			if(p && p.gameObject.activeSelf || p.isHidden)
				floraData.Add(p.GetPlantData());
		}

		bf.Serialize(stream, floraData);
		stream.Close();
	}

	public static void LoadNature()
	{
		if(File.Exists(Application.persistentDataPath +"/nature"+saveState+".sav"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream(Application.persistentDataPath +"/nature"+saveState+".sav", FileMode.Open);

			List<PlantData> floraData = new List<PlantData>();
			floraData = bf.Deserialize(stream) as List<PlantData>;

			if(GameManager.village.nature)
			{
				foreach(Plant p in GameManager.village.nature.flora)
				{
					Destroy(p.gameObject);
				}
				GameManager.village.nature.flora.Clear();
			}
			foreach(PlantData p in floraData)
			{
				Nature.SpawnPlant(p);
			}
			stream.Close();
		}
		else 
		{
			Debug.Log("file does not exist");
		}
	}

	public static void SaveVillage()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream stream = new FileStream(Application.persistentDataPath +"/buildings"+saveState+".sav", FileMode.Create);

		List<BuildingData> buildingdata = new List<BuildingData>();
		foreach(Building b in Building.allBuildings)
			buildingdata.Add(b.GetBuildingData());

		bf.Serialize(stream, buildingdata);
		stream.Close();
	}

	public static void LoadVillage()
	{
		if(File.Exists(Application.persistentDataPath +"/buildings"+saveState+".sav"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream(Application.persistentDataPath +"/buildings"+saveState+".sav", FileMode.Open);

			List<BuildingData> buildingdata = new List<BuildingData>();
			buildingdata = bf.Deserialize(stream) as List<BuildingData>;

			foreach(Building b in Building.allBuildings)
				Destroy(b.gameObject);
			Building.allBuildings.Clear();
			foreach(BuildingData bd in buildingdata)
			{
				Debug.Log(bd.resourceCurrent[0]);
				BuildManager.SpawnBuilding(bd);
			}
			stream.Close();
		}
		else 
		{
			Debug.Log("file does not exist");
		}
	}

	public static void SavePeople()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream stream = new FileStream(Application.persistentDataPath +"/people"+saveState+".sav", FileMode.Create);

		List<PersonData> peopleData = new List<PersonData>();
		foreach(PersonScript ps in PersonScript.allPeople)
			peopleData.Add(ps.GetPersonData());

		bf.Serialize(stream, peopleData);
		stream.Close();
	}

	public static void LoadPeople()
	{
		if(File.Exists(Application.persistentDataPath +"/people"+saveState+".sav"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream(Application.persistentDataPath +"/people"+saveState+".sav", FileMode.Open);

			List<PersonData> peopleData = new List<PersonData>();
			peopleData = bf.Deserialize(stream) as List<PersonData>;

			foreach(PersonScript ps in PersonScript.allPeople)
				Destroy(ps.gameObject);
			PersonScript.allPeople.Clear();
			foreach(PersonData p in peopleData)
			{
				UnitManager.SpawnPerson(p);
			}
			stream.Close();
		}
		else 
		{
			Debug.Log("file does not exist");
		}
	}

	public static void SaveGameData()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream stream = new FileStream(Application.persistentDataPath +"/game"+saveState+".sav", FileMode.Create);

		Village v = GameManager.village;
		GameData myData = new GameData();

		myData.coins = v.GetCoins();
		myData.currentDay = GameManager.GetTotDay();
		myData.foodFactor = v.GetFoodFactor();
		myData.roomspaceFactor = v.GetRoomspaceFactor();
		myData.healthFactor = v.GetHealthFactor();
		myData.fertilityFactor = v.GetFertilityFactor();
		myData.luxuryFactor = v.GetLuxuryFactor();
		myData.totalFactor = v.GetTotalFactor();

		myData.unlockedBuildings = new bool[Building.COUNT];
		for(int i = 0; i < Building.COUNT; i++)
			myData.unlockedBuildings[i] = Building.IsUnlocked(i);

		myData.unlockedResources = new bool[GameResources.COUNT];
		for(int i = 0; i < GameResources.COUNT; i++)
			myData.unlockedResources[i] = GameResources.IsUnlocked(i);

		bf.Serialize(stream, myData);
		stream.Close();
	}

	public static void LoadGameData()
	{
		if(File.Exists(Application.persistentDataPath +"/game"+saveState+".sav"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream stream = new FileStream(Application.persistentDataPath +"/game"+saveState+".sav", FileMode.Open);

			Village v = GameManager.village;
			GameData myData = bf.Deserialize(stream) as GameData;

			v.SetVillageData(myData);
			GameManager.SetDay(myData.currentDay);

			for(int i = 0; i < myData.unlockedBuildings.Length; i++)
				if(myData.unlockedBuildings[i])
					Building.Unlock(i);
					
			for(int i = 0; i < myData.unlockedResources.Length; i++)
				if(myData.unlockedResources[i])
        			GameManager.UnlockResource(i);

			stream.Close();
		}
		else 
		{
			Debug.Log("file does not exist");
		}
	}

	public static bool SavedGame(int state)
	{
		return File.Exists(Application.persistentDataPath +"/people"+state+".sav") && 
				File.Exists(Application.persistentDataPath +"/nature"+state+".sav") && 
				File.Exists(Application.persistentDataPath +"/buildings"+state+".sav") && 
				File.Exists(Application.persistentDataPath +"/game"+state+".sav");
	}

	public static void NewGame(int state)
	{
		File.Delete(Application.persistentDataPath +"/people"+state+".sav");
		File.Delete(Application.persistentDataPath +"/nature"+state+".sav");
		File.Delete(Application.persistentDataPath +"/buildings"+state+".sav");
		File.Delete(Application.persistentDataPath +"/game"+state+".sav");
	}

	public static void LoadGame()
	{
		LoadNature();
		LoadVillage();
		LoadPeople();
		LoadGameData();
	}

	public static void SaveGame()
	{
		SaveNature();
		SaveVillage();
		SavePeople();
		SaveGameData();
	}
}
