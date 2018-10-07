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
	public static bool errorWhileLoading = false, errorWhileSaving = false;

	public static void SaveAnimals()
	{
        myGameState.animalData = AnimalScript.AllGameAnimals();
    }

	public static void LoadAnimals()
    {
        AnimalScript.DestroyAllAnimals();

        foreach (GameAnimal a in myGameState.animalData)
        {
            UnitManager.SpawnAnimal(a);
        }
	}

	public static void SaveItems()
	{
		myGameState.itemData = ItemScript.AllGameItems();
	}

	public static void LoadItems()
	{
        ItemScript.DestroyAllItems();

        foreach(GameItem it in myGameState.itemData)
        {
            ItemManager.SpawnItem(it);
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
		List<GamePerson> peopleData = new List<GamePerson>();
		foreach(PersonScript ps in PersonScript.allPeople)
			peopleData.Add(ps.GetPerson());

		myGameState.peopleData = peopleData;
	}

	public static void LoadPeople()
	{
		List<GamePerson> peopleData = new List<GamePerson>();
		peopleData = myGameState.peopleData;

		foreach(PersonScript ps in PersonScript.allPeople)
			Destroy(ps.gameObject);
		PersonScript.allPeople.Clear();
		foreach(GamePerson p in peopleData)
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

        myData.faithPoints = v.GetFaithPoints();
        myData.faithEnabled = UIManager.Instance.IsFaithBarEnabled();
        myData.techTreeEnabled = UIManager.Instance.IsTechTreeEnabled();

        myData.unlockedResources = new List<int>(ResourceData.unlockedResources);
        myData.unlockedBuildings = new List<int>(Building.unlockedBuilding);
        myData.unlockedJobs = new List<int>(Job.unlockedJobs);

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
        
        ResourceData.unlockedResources = new HashSet<int>(myData.unlockedResources);
        Building.unlockedBuilding = new HashSet<int>(myData.unlockedBuildings);
        Job.unlockedJobs = new HashSet<int>(myData.unlockedJobs);

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
			MainMenuManager.ShowMessage("Kann den Spielstand "+(state+1)+" nicht auslesen!\n(Wahrscheinlich ein Spielstand aus einer früheren Version)\n"+ex.Message+"\n"+ex.Source);
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
                Debug.LogError("error while loading state " + saveState + ":\n"+ex.Message);
                GameManager.CancelFade();
                UIManager.Instance.OnExitGame();
				MainMenuManager.ShowMessage("Kann den Spielstand nicht laden!\n"+ex.Message);
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
				Debug.LogError("can't save the game: error while loading state " + saveState);
				//LoadGame();
				return;
            }
            try
            {
                errorWhileSaving = false;

                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = new FileStream(Application.persistentDataPath + "/game" + saveState + ".sav", FileMode.Create);

                try
                {
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
                catch (Exception ex)
                {
                    errorWhileSaving = true;
                    Debug.LogError("error while saving state " + saveState + ":\n" + ex.Message);
                    GameManager.CancelFade();
                    UIManager.Instance.OnExitGame();
                    MainMenuManager.ShowMessage("Kann das Spiel nicht speichern!\n" + ex.Message);
                }
                finally
                {
                    stream.Close();
                }
            }
            catch(UnauthorizedAccessException ex)
            {
                errorWhileSaving = true;
                GameManager.CancelFade();
                UIManager.Instance.OnExitGame();
                MainMenuManager.ShowMessage("Kann das Spiel nicht speichern! (Nicht authorisiert)\n" + ex.Message);
                Debug.LogError("can't save the game: error while saving state " + saveState + " (UnauthorizedAccessException):\n" + ex.Message);
            }
		}
	}
}
