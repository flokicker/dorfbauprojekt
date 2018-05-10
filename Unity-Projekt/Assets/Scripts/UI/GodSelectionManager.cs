using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GodSelectionManager : Singleton<GodSelectionManager> {

	private float ellipseAxisA = 200, ellipseAxisB = 50;
	private float currentRotation = 0, targetRotation = 0;

	[SerializeField]
	private Canvas myCanvas;
	[SerializeField]
	private FadeManager myFadeManager;

	[SerializeField]
	private GameObject godItem;

	private Transform circularList;
	private List<RectTransform> godListItems;
	private Text infoText;
	
	public InputField usernameInput;

	// Use this for initialization
	void Start () {
		circularList = myCanvas.transform.Find("GodSelection/CircularList");

		infoText = myCanvas.transform.Find("GodSelection/Panel/Info/Text").GetComponent<Text>();

		godListItems = new List<RectTransform>();
		for(int i = 0; i < 12; i++)
		{
			godListItems.Add(Instantiate(godItem, circularList.transform).GetComponent<RectTransform>());
		}
		for(int i = 0; i < godListItems.Count/2; i++)
		{
			godListItems[i].SetAsFirstSibling();
			godListItems[godListItems.Count-i-1].SetAsFirstSibling();
		}
		
		myCanvas.transform.Find("GodSelection/ButtonLeft").GetComponent<Button>().onClick.AddListener(() => OnArrow(true));
		myCanvas.transform.Find("GodSelection/ButtonRight").GetComponent<Button>().onClick.AddListener(() => OnArrow(false));

		myCanvas.transform.Find("GodSelection/Panel/Buttons/ButtonCancel").GetComponent<Button>().onClick.AddListener(() => OnCancelButton());
		myCanvas.transform.Find("GodSelection/Panel/Buttons/ButtonCreate").GetComponent<Button>().onClick.AddListener(() => OnConfirmGod());
		myCanvas.transform.Find("CharacterSelection/Panel/ButtonBack").GetComponent<Button>().onClick.AddListener(() => OnBackButton());
		myCanvas.transform.Find("CharacterSelection/Panel/ButtonOk").GetComponent<Button>().onClick.AddListener(() => OnCreateGame());

		usernameInput = myCanvas.transform.Find("CharacterSelection/Panel/InputField").GetComponent<InputField>();

        myFadeManager.Fade(false, 0.2f, 0.5f);
	}
	
	// Update is called once per frame
	void Update () {
		currentRotation = Mathf.Lerp(currentRotation, targetRotation, Time.deltaTime*4f);

		float minY = float.MaxValue, maxY = float.MinValue;
		int minInd = 0, maxInd = 0;
		for(int i = 0; i < godListItems.Count; i++)
		{
			float rot = (currentRotation+i*2*Mathf.PI/godListItems.Count) % (Mathf.PI * 2);
			Vector2 pos = godListItems[i].anchoredPosition;
			pos.x = Mathf.Sin(rot) * ellipseAxisA;
			pos.y = -Mathf.Cos(rot) * ellipseAxisB;
			godListItems[i].anchoredPosition = pos;
			float scale = -pos.y / (ellipseAxisB*2f) + 1f;
			godListItems[i].localScale = Vector3.one * Mathf.Clamp(scale, 0.8f, 1f);
			if(pos.y < minY) {
				minY = pos.y;
				minInd = i;
			}
			if(pos.y > maxY) {
				maxY = pos.y;
				maxInd = i;
			}
		}
		godListItems[minInd].SetAsLastSibling();
		infoText.text = "Gott"+minInd;
		godListItems[maxInd].SetAsFirstSibling();
	}

	public void OnArrow(bool left)
	{
		targetRotation += 2*Mathf.PI/godListItems.Count * (left ? 1f : -1f);
	}

	public void OnBackButton()
	{
		myCanvas.transform.Find("GodSelection").GetComponent<Animator>().SetInteger("slideState",1);
		myCanvas.transform.Find("CharacterSelection").GetComponent<Animator>().SetInteger("slideState",0);
	}

	public void OnCancelButton()
	{
		MainMenuManager.Instance.ShowGameStates(true);
		myCanvas.transform.Find("GodSelection").GetComponent<Animator>().SetInteger("slideState",0);
		myCanvas.transform.Find("CharacterSelection").GetComponent<Animator>().SetInteger("slideState",0);
	}

	public void OnConfirmGod()
	{
		myCanvas.transform.Find("GodSelection").GetComponent<Animator>().SetInteger("slideState",2);
		myCanvas.transform.Find("CharacterSelection").GetComponent<Animator>().SetInteger("slideState",1);
	}

	public void OnShow()
	{
		myCanvas.transform.Find("GodSelection").GetComponent<Animator>().SetInteger("slideState",1);
		myCanvas.transform.Find("CharacterSelection").GetComponent<Animator>().SetInteger("slideState",0);
	}

	public void OnCreateGame()
	{
        if(usernameInput.text == "")
        {
            MainMenuManager.ShowMessage("Bitte gib einen Charakternamen ein!");
            return;
        }
        if(usernameInput.text.Length > 12)
        {
            MainMenuManager.ShowMessage("Dein Charaktername ist zu lange!");
            return;
        }
		GameManager.username = usernameInput.text;
        myCanvas.transform.Find("GodSelection").gameObject.SetActive(false);
        myCanvas.transform.Find("CharacterSelection").gameObject.SetActive(false);
    	MainMenuManager.Instance.LoadGame();
	}
}
