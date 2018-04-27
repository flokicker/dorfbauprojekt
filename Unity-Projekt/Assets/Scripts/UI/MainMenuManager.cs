using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

	[SerializeField]
	private Slider progressBar;

    [SerializeField]
    private Transform menuPanel, gameStatePanel;
    [SerializeField]
    private GameObject gameStateButtonPrefab;

    void Start()
    {
    }

    void Update()
    {
    }

    public void ShowGameStates(bool newGame)
    {
        menuPanel.gameObject.SetActive(false);
        gameStatePanel.gameObject.SetActive(true);
        
        for(int i = 0; i < gameStatePanel.childCount; i++)
        {
            Destroy(gameStatePanel.GetChild(i).gameObject);
        }

        for(int i = 0; i < SaveLoadManager.maxSaveStates+1; i++)
        {
            Button b = Instantiate(gameStateButtonPrefab, gameStatePanel).GetComponent<Button>();
            if(i == SaveLoadManager.maxSaveStates)
            {
                b.GetComponentInChildren<Text>().text = "Zurück";
                b.onClick.AddListener(HideGameStates);
            }
            else
            {
                bool exists = SaveLoadManager.SavedGame(i);
                b.GetComponentInChildren<Text>().text = "Spielstand "+(i+1) + (exists ? "" : " (leer)");
                if(!newGame && !exists) b.interactable = false;
                int j = i;
                b.onClick.AddListener(() => OnGameState(newGame, j));
            }
        }
    }
    public void HideGameStates()
    {
        menuPanel.gameObject.SetActive(true);
        gameStatePanel.gameObject.SetActive(false);
    }

    private void OnGameState(bool newGame, int state)
    {
        SaveLoadManager.saveState = state;
        if(newGame) SaveLoadManager.NewGame(state);
        LoadGame();
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void DeleteAllGameState()
    {
        for(int i = 0; i < SaveLoadManager.maxSaveStates; i++)
        {
            SaveLoadManager.NewGame(i);
        }
    }

    private void LoadGame()
    {
        //Use a coroutine to load the Scene in the background
        StartCoroutine(LoadYourAsyncScene());
    }

    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("VillageScene");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone) {
            progressBar.value = asyncLoad.progress / 0.9f; //Async progress returns always 0 here    
            yield return null;
 
        }
    }
}
