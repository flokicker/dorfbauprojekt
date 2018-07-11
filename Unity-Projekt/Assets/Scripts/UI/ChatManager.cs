using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChatManager : Singleton<ChatManager> {

    private List<string> chatMessages = new List<string>();

    private float chatShowTime = 0;
    private bool chatActive = false;

    [SerializeField]
    private GameObject chatMessagePrefab;

    private InputField chatInput;
    private Transform chatPanel, messagesPanel, messagesContent;

	// Use this for initialization
    void Start()
    {
        chatPanel = transform.Find("ChatInput");
        chatInput = chatPanel.GetComponentInChildren<InputField>();
        messagesPanel = transform.Find("Chat");
        messagesContent = messagesPanel.Find("Viewport/Content");

    }
	
	// Update is called once per frame
	void Update () {
        chatShowTime += Time.deltaTime * 0.8f;
        if (chatActive) chatShowTime = 0;

        Instance.messagesPanel.Find("ScrollVert").GetComponent<Scrollbar>().interactable = chatActive;

        Color colPanel = messagesPanel.GetComponent<Image>().color;
        ColorBlock colVertScroll = messagesPanel.Find("ScrollVert").GetComponent<Scrollbar>().colors;
        float alpha = 0.8f;
        if (chatShowTime > 1)
        {
            alpha = Mathf.Lerp(0.8f, 0, chatShowTime - 1);
        }
        colPanel.a = alpha;
        Color c = colVertScroll.normalColor;
        c.a = alpha;
        colVertScroll.normalColor = c;
        c = colVertScroll.highlightedColor;
        c.a = alpha;
        colVertScroll.highlightedColor = c;
        c = messagesPanel.Find("ScrollVert").GetComponent<Image>().color;
        c.a = alpha;
        messagesPanel.Find("ScrollVert").GetComponent<Image>().color = c;
        messagesPanel.GetComponent<Image>().color = colPanel;
        messagesPanel.Find("ScrollVert").GetComponent<Scrollbar>().colors = colVertScroll;
        for (int i = 0; i < messagesContent.childCount; i++)
        {
            string text = messagesContent.GetChild(i).GetComponent<Text>().text;
            string colT = text.Substring(8, 6);
            string rest = text.Substring(16);
            ColorUtility.TryParseHtmlString("#"+colT, out c);
            c.a = alpha;
            messagesContent.GetChild(i).GetComponent<Text>().text = "<color=#" + ColorUtility.ToHtmlStringRGBA(c) + rest;
        }

        messagesPanel.gameObject.SetActive(chatShowTime < 2);
        chatPanel.gameObject.SetActive(chatActive);
	}

    public static void ToggleChat()
    {
        Instance.chatActive = !Instance.chatActive;

        Instance.chatPanel.gameObject.SetActive(Instance.chatActive);
        Instance.chatShowTime = 0;
        Instance.chatInput.enabled = Instance.chatActive;
        Instance.messagesPanel.GetComponent<ScrollRect>().enabled = Instance.chatActive;
        if(Instance.chatActive)
            Instance.messagesPanel.Find("ScrollVert").GetComponent<Scrollbar>().interactable = true;
        if (Instance.chatActive)
        {
            EventSystem.current.SetSelectedGameObject(Instance.chatInput.gameObject);
        }
        else
        {
            Instance.chatInput.text = "";
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public static void CommitMsg()
    {
        if (!IsChatActive()) return;

        string msgText = Instance.chatInput.text;
        if (msgText.Length == 0) return;
        Color col = Color.cyan;
        if (msgText.StartsWith("/") && GameManager.IsDebugging())
        {
            // command
            col = new Color(0.7f, 0.7f, 0.7f);
            msgText = msgText.Substring(1);
            string[] arguments = new string[0];
            if (msgText.Contains(" "))
            {
                arguments = msgText.Substring(msgText.IndexOf(' ') + 1).Split(' ');
                msgText = msgText.Substring(0, msgText.IndexOf(' '));
            }
            switch (msgText.ToLower())
            {
                case "help":
                    string resText = "";
                    for (int i = 0; i < GameResources.names.Length; i++)
                        resText += i + "=" + GameResources.names[i] + " ,";
                    resText = resText.Substring(0,resText.Length - 2);
                    msgText = 
                        "Spielgeschwindigkeit verändern = /speed [faktor]" +
                        "\nJahre vergehen lassen = /years [jahre]" +
                        "\nRessourcen an ausgewählte Person = /give [resNr] [anzahl]\n" + resText + 
                        "\nMännlichen Bewohner spawnen = /bornman [alter]" +
                        "\nWeiblichen Bewohner spawnen = /bornwoman [alter]";
                    break;
                case "speed":
                    try
                    {
                        float fact = float.Parse(arguments[0]);
                        fact = Mathf.Clamp(fact, 0.1f, 5000);
                        GameManager.speedFactor = fact;
                        msgText += "Spielgeschwindigkeit auf " + fact + " gesetzt";
                    }
                    catch
                    {
                        msgText = "Falsche Argumente!";
                        col = Color.red;
                    }
                    break;
                case "years":
                    try
                    {
                        int yrs = int.Parse(arguments[0]);
                        yrs = Mathf.Clamp(yrs, 1,100);
                        GameManager.PassYears(yrs);
                        msgText += yrs + " Jahre sind vergangen";
                    }
                    catch
                    {
                        msgText = "Falsche Argumente!";
                        col = Color.red;
                    }
                    break;
                case "give":
                    try
                    {
                        int id = int.Parse(arguments[0]);
                        int am = int.Parse(arguments[1]);
                        am = Mathf.Clamp(am, 0, 1000);
                        PersonScript ps = PersonScript.FirstSelectedPerson();
                        if (ps)
                        {
                            ps.inventoryMaterial = new GameResources(id, am);
                            msgText = am + "x " + ps.inventoryMaterial.GetName() + " wurden " + ps.firstName + "s Inventar hinzugefügt";
                        }
                        else
                        {
                            msgText = "Keine Person ausgewählt";
                            col = Color.red;
                        }
                    }
                    catch
                    {
                        msgText = "Falsche Argumente!";
                        col = Color.red;
                    }
                    break;
                case "bornman":
                case "bornwoman":
                    try
                    {
                        Gender gender = (msgText.ToLower() == "bornman") ? Gender.Male : Gender.Female;
                        int age = int.Parse(arguments[0]);
                        age = Mathf.Clamp(age, 0, 80);
                        PersonData p = GameManager.village.PersonBirth(-1, gender, age);
                        msgText += "Bewohner gespawnt: Name="+p.firstName+",Alter="+p.age;
                    }
                    catch
                    {
                        msgText = "Falsche Argumente!";
                        col = Color.red;
                    }
                    break;
                default:
                    msgText = "Falsche Argumente!";
                    col = Color.red;
                    break;
            }
        }
        else
        {
            msgText = GameManager.username + ": " + msgText;
        }

        Msg(msgText, col);
        Instance.chatInput.text = "";
    }

    public static void Msg(string msg)
    {
        Msg(msg, Color.white);
    }
    public static void Msg(string msg, Color col)
    {
        Instance.chatMessages.Add(msg);
        GameObject messageObj = (GameObject)Instantiate(Instance.chatMessagePrefab, Instance.messagesPanel.Find("Viewport/Content"));
        messageObj.GetComponentInChildren<Text>().text = "<color=#" + ColorUtility.ToHtmlStringRGBA(col)+ ">" + msg + "</color>";
        Instance.StartCoroutine(ScrollToTop());
        Instance.chatShowTime = 0;
    }

    private static IEnumerator ScrollToTop()
    {
        Instance.messagesPanel.GetComponent<ScrollRect>().enabled = true;
        yield return new WaitForEndOfFrame();
        Instance.messagesPanel.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
    }


    public static bool IsChatActive()
    {
        return Instance.chatActive;
    }
}
