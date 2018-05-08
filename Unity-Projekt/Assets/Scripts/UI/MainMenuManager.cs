using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

	[SerializeField]
	private Slider progressBar;
	[SerializeField]
	private FadeManager mainMenuFadeManager;

    [SerializeField]
    private Transform menuPanel, gameStatePanel, loadingPanel;
    [SerializeField]
    private GameObject gameStateButtonPrefab;
    [SerializeField]
    private InputField usernameInput;

    private static string messageAfterStart;
    [SerializeField]
    private Transform messageOverlay;

    void Start()
    {
        string un = PlayerPrefs.GetString("username");
        usernameInput.text = un;
        GameManager.username = un;

        menuPanel.GetChild(0).GetComponent<MenuEntry>().AddOnClickListener(() => ShowGameStates(true));
        menuPanel.GetChild(1).GetComponent<MenuEntry>().AddOnClickListener(() => ShowGameStates(false));
        menuPanel.GetChild(2).GetComponent<MenuEntry>().AddOnClickListener(() => {});
        menuPanel.GetChild(2).GetComponent<MenuEntry>().SetInteractable(false);
        menuPanel.GetChild(3).GetComponent<MenuEntry>().AddOnClickListener(() => CloseGame());

        Button b = messageOverlay.Find("MessageBox/Button").GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => OnCloseMessageBox());

        mainMenuFadeManager.Fade(false, 0.2f, 0.5f);
    }

    void Update()
    {
        if(messageAfterStart != null)
        {
            messageOverlay.gameObject.SetActive(true);
            messageOverlay.Find("MessageBox/Text").GetComponent<Text>().text = messageAfterStart;
            messageAfterStart = null;
        }
    }

    public void ShowGameStates(bool newGame)
    {
        menuPanel.GetComponent<Animator>().SetBool("active",false);
        for(int i = 0; i < menuPanel.childCount; i++)
            menuPanel.GetChild(i).GetComponent<MenuEntry>().SetInteractable(false);
        gameStatePanel.GetComponent<Animator>().SetBool("active",true);
        for(int i = 0; i < gameStatePanel.childCount; i++)
            gameStatePanel.GetChild(i).GetComponent<MenuEntry>().SetInteractable(true);
        
        for(int i = 0; i < gameStatePanel.childCount; i++)
        {
            Destroy(gameStatePanel.GetChild(i).gameObject);
        }

        for(int i = 0; i < SaveLoadManager.maxSaveStates+1; i++)
        {
            MenuEntry b = Instantiate(gameStateButtonPrefab, gameStatePanel).GetComponent<MenuEntry>();
            if(i == SaveLoadManager.maxSaveStates)
            {
                b.GetComponent<Text>().text = "Zurück";
                b.AddOnClickListener(HideGameStates);
            }
            else
            {
                bool exists = SaveLoadManager.SavedGame(i);
                b.GetComponent<Text>().text = "Spielstand "+(i+1) + (exists ? "" : " (leer)");
                if(!newGame && !exists) b.SetInteractable(false);
                int j = i;
                b.AddOnClickListener(() => OnGameState(newGame, j));
            }
        }
    }
    public void HideGameStates()
    {
        menuPanel.GetComponent<Animator>().SetBool("active",true);
        for(int i = 0; i < menuPanel.childCount; i++)
            menuPanel.GetChild(i).GetComponent<MenuEntry>().SetInteractable(i != 2);
        gameStatePanel.GetComponent<Animator>().SetBool("active",false);
        for(int i = 0; i < gameStatePanel.childCount; i++)
            gameStatePanel.GetChild(i).GetComponent<MenuEntry>().SetInteractable(false);
    }

    private void OnGameState(bool newGame, int state)
    {
        SaveLoadManager.saveState = state;
        if(newGame) SaveLoadManager.NewGame(state);
        LoadGame();
    }

    private void OnCloseMessageBox()
    {
        messageOverlay.gameObject.SetActive(false);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    void OnApplicationQuit()
    {
        SaveUsername();
    }

    void SaveUsername()
    {
        PlayerPrefs.SetString("username", usernameInput.text);
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
        if(usernameInput.text == "")
        {
            ShowMessage("Bitte gib einen Username ein!");
            return;
        }
        if(usernameInput.text.Length > 12)
        {
            ShowMessage("Dein Username ist zu lange!");
            return;
        }
        menuPanel.gameObject.SetActive(false);
        gameStatePanel.gameObject.SetActive(false);
        loadingPanel.gameObject.SetActive(true);

        SaveUsername();
        //Use a coroutine to load the Scene in the background
        StartCoroutine(LoadYourAsyncScene());
    }

    public static void ShowMessage(string message)
    {
        messageAfterStart = message;
    }

    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("VillageScene");
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (asyncLoad.progress < 0.9f) {
            progressBar.value = asyncLoad.progress / 0.9f;
            yield return null;
 
        }
        progressBar.value = 1f;

        mainMenuFadeManager.Fade(true, 0.1f, 1f);
        while(mainMenuFadeManager.isInTransition){
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;
    }
}
