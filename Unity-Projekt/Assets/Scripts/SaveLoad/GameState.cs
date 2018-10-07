using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState {

	public GameData gameData;
    public List<GameBuilding> buildingdata = new List<GameBuilding>();
	public List<GameNatureObject> natureData = new List<GameNatureObject>();
	public List<GamePerson> peopleData = new List<GamePerson>();
	public List<GameAnimal> animalData = new List<GameAnimal>();
	public List<GameItem> itemData = new List<GameItem>();

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
