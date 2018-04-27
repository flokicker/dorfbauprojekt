using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState {

	public GameData gameData;
	public List<BuildingData> buildingdata = new List<BuildingData>();
	public List<PlantData> floraData = new List<PlantData>();
	public List<PersonData> peopleData = new List<PersonData>();
	public List<AnimalData> animalData = new List<AnimalData>();
	public List<ItemData> itemData = new List<ItemData>();

	public int CountTotalGameObjects()
	{
		int count = 0;
		count += buildingdata.Count;
		count += floraData.Count;
		count += peopleData.Count;
		count += itemData.Count;
		count += animalData.Count;
		return count;
	}
}
