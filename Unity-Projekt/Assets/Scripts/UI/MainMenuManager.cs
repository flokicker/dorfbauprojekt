using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : Singleton<MainMenuManager> {

    // store entered name in main menu
    public static string username = "Flo";
    public static Gender startGender = Gender.Male;

    [SerializeField]
	private Slider progressBar;
	[SerializeField]
	private FadeManager mainMenuFadeManager;

    [SerializeField]
    private Transform menuPanel, gameStatePanel, loadingPanel, changelogPanel;
    [SerializeField]
    private GameObject gameStateButtonPrefab;

    private static string messageAfterStart;
    [SerializeField]
    private Transform messageOverlay;

    private TMPro.TextMeshProUGUI textMessageUI;
    void Start()
    {
        menuPanel.GetChild(0).GetComponent<MenuEntry>().AddOnClickListener(() => ShowGameStates(true));
        menuPanel.GetChild(1).GetComponent<MenuEntry>().AddOnClickListener(() => ShowGameStates(false));
        menuPanel.GetChild(2).GetComponent<MenuEntry>().AddOnClickListener(() => {});
        menuPanel.GetChild(2).GetComponent<MenuEntry>().SetInteractable(false);
        menuPanel.GetChild(3).GetComponent<MenuEntry>().AddOnClickListener(() => CloseGame());

        Button b = messageOverlay.Find("MessageBox/Buttons/ButtonOk").GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => OnCloseMessageBox());
        b = messageOverlay.Find("MessageBox/Buttons/ButtonCopy").GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => OnCopyMessage());

        mainMenuFadeManager.Fade(false, 0.2f, 0.5f);
        menuPanel.GetComponent<Animator>().SetInteger("slideState",1);

        //TextAsset changelog = Resources.Load("Changelog") as TextAsset;
        TextAsset textAsset = (TextAsset)Resources.Load("Changelog");

        textMessageUI = messageOverlay.Find("MessageBox/Scroll/Viewport/Content/Text").GetComponent<TMPro.TextMeshProUGUI>();
        changelogPanel.Find("Scroll View/Viewport/Content/Text").GetComponent<TMPro.TextMeshProUGUI>().text = new StringReader(textAsset.text).ReadToEnd();

    }

    void Update()
    {
        if (messageAfterStart != null)
        {
            textMessageUI.text = messageAfterStart;
            messageOverlay.gameObject.SetActive(true);
            messageAfterStart = null;
        }
    }

    public void ShowGameStates(bool newGame)
    {
        menuPanel.GetComponent<Animator>().SetInteger("slideState",2);
        changelogPanel.GetComponent<Animator>().SetBool("active", false);
        for (int i = 0; i < menuPanel.childCount; i++)
            menuPanel.GetChild(i).GetComponent<MenuEntry>().SetInteractable(false);
        gameStatePanel.GetComponent<Animator>().SetInteger("slideState",1);
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
                b.GetComponent<Text>().text = "Spielstand "+(i+1) + " - "+(exists ? SaveLoadManager.SavedGameName(i) : " (leer)");
                if(!newGame && !exists) b.SetInteractable(false);
                int j = i;
                b.AddOnClickListener(() => OnGameState(newGame, j));
            }
        }
    }
    public void HideGameStates()
    {
        menuPanel.GetComponent<Animator>().SetInteger("slideState",1);
        changelogPanel.GetComponent<Animator>().SetBool("active", true);
        for (int i = 0; i < menuPanel.childCount; i++)
            menuPanel.GetChild(i).GetComponent<MenuEntry>().SetInteractable(i != 2);
        gameStatePanel.GetComponent<Animator>().SetInteger("slideState",0);
        for(int i = 0; i < gameStatePanel.childCount; i++)
            gameStatePanel.GetChild(i).GetComponent<MenuEntry>().SetInteractable(false);
    }

    private void OnGameState(bool newGame, int state)
    {
        SaveLoadManager.saveState = state;
        if (newGame) 
        {
            SaveLoadManager.NewGame(state);
            gameStatePanel.GetComponent<Animator>().SetInteger("slideState",2);
            GodSelectionManager.Instance.OnShow();
        }
        else
        {
            LoadGame();
        }
    }

    private void OnCloseMessageBox()
    {
        messageOverlay.gameObject.SetActive(false);
    }
    private void OnCopyMessage()
    {
        GUIUtility.systemCopyBuffer = textMessageUI.text;
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    void OnApplicationQuit()
    {
    }

    public void DeleteAllGameState()
    {
        for(int i = 0; i < SaveLoadManager.maxSaveStates; i++)
        {
            SaveLoadManager.NewGame(i);
        }
    }

    public void LoadGame()
    {
        menuPanel.gameObject.SetActive(false);
        gameStatePanel.gameObject.SetActive(false);
        loadingPanel.gameObject.SetActive(true);

        GameManager.gameData = null;

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

        int count = 0;
        // Wait until the asynchronous scene fully loads
        while (asyncLoad.progress < 0.9f) {
            progressBar.value = asyncLoad.progress / 0.9f;
            count++;
            if (count > 1000)
                break;
            yield return null;
 
        }
        progressBar.value = 1f;

        mainMenuFadeManager.Fade(true, 0.1f, 1f);
        count = 0;
        while(mainMenuFadeManager.isInTransition){
            count++;
            if (count > 1000)
                break;
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;

        Debug.Log("done loading");
    }
}
