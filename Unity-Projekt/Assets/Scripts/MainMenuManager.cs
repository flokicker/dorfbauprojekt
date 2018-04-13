using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

	[SerializeField]
	private Slider progressBar;

    void Start()
    {
        //Use a coroutine to load the Scene in the background
        StartCoroutine(LoadYourAsyncScene());
    }

    void Update()
    {
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
