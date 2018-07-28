using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState {

	public GameData gameData;
    public List<GameBuilding> buildingdata = new List<GameBuilding>();
	public List<GameNatureObject> natureData = new List<GameNatureObject>();
	public List<PersonData> peopleData = new List<PersonData>();
	public List<AnimalData> animalData = new List<AnimalData>();
	public List<ItemData> itemData = new List<ItemData>();

	public int CountTotalGameObjects()
	{
		int count = 0;
		count += buildingdata.Count;
		count += natureData.Count;
		count += peopleData.Count;
		count += itemData.Count;
		count += animalData.Count;
		return count;
	}
}
