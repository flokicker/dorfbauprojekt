using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum MessageType
{
    PlayerChat, Info, Error, News, Debug
}
public class ChatManager : Singleton<ChatManager> {

    private List<string> chatMessages = new List<string>();
    
    private float newMessageTime = 0;
    private float chatShowTime = 0;
    private bool chatActive = false;

    [SerializeField]
    private GameObject chatMessagePrefab;

    private TMP_InputField chatInput;
    private Transform chatPanel, messagesPanel, messagesContent;

	// Use this for initialization
    void Start()
    {
        chatPanel = transform.Find("ChatInput");
        chatInput = chatPanel.GetComponentInChildren<TMP_InputField>();
        messagesPanel = transform.Find("Chat");
        messagesContent = messagesPanel.Find("Viewport/Content");

        string helpText = "Willkommen in deinem Dorf!\n" +
            "Mit der Eingabetaste öffnest du den Chat.\n" +
            "Benutze ASDW oder fahre mti der Maus an den Rand um die Kamera zu bewegen.\n" +
            "Drücke E/Q oder bewge die Maus mit dem Mausrad gehalten um die Kamera zu drehen.\n" +
            "Drehe das Mausrad um zu zoomen.\n" +
            "Mit der linken Maustaste wählst du Bewohenr und Objekte aus.\n" +
            "Mit der rechten Maustaste befiehlst die Bewohner herum und interagierst mit Objekten.\n" +
            "Wenn du einen Bewohner ausgewählt hast, kannst du rechts unten den Bau-Knopf betätigen oder (B) drücken\n" +
            "Viel Erfolg!";
        Msg(helpText, Color.yellow);
    }
	
	// Update is called once per frame
	void Update () {
        chatShowTime += Time.deltaTime;
        newMessageTime += Time.deltaTime;
        if (chatActive || !GameManager.HasFaded()) chatShowTime = 0;

        Instance.messagesPanel.Find("ScrollVert").GetComponent<Scrollbar>().interactable = chatActive;

        Color colPanel = messagesPanel.GetComponent<Image>().color;
        ColorBlock colVertScroll = messagesPanel.Find("ScrollVert").GetComponent<Scrollbar>().colors;
        float alpha = 0.8f;
        if(chatShowTime < 2)
        {
            newMessageTime = 2;
        }
        if (chatShowTime > 1)
        {
            alpha = Mathf.Lerp(0.8f, 0, chatShowTime - 1);
        }
        if(chatShowTime >= 2)
        {
            alpha = 0;
        }
        colPanel.a = alpha;
        Color c = colVertScroll.normalColor;
        c.a = alpha;
        colVertScroll.normalColor = c;
        c = colVertScroll.highlightedColor;
        c.a = alpha;
        colVertScroll.highlightedColor = c;
        c = colVertScroll.disabledColor;
        c.a = alpha*0.5f;
        colVertScroll.disabledColor = c;
        c = messagesPanel.Find("ScrollVert").GetComponent<Image>().color;
        c.a = alpha;
        messagesPanel.Find("ScrollVert").GetComponent<Image>().color = c;
        messagesPanel.GetComponent<Image>().color = colPanel;
        messagesPanel.Find("ScrollVert").GetComponent<Scrollbar>().colors = colVertScroll;
        if (newMessageTime < 2)
        {
            alpha = 0.8f;
            if (newMessageTime > 1)
            {
                alpha = Mathf.Lerp(0.8f, 0, newMessageTime - 1);
            }
        }
        for (int i = 0; i < messagesContent.childCount; i++)
        {
            TextMeshProUGUI tmp = messagesContent.GetChild(i).GetComponent<TextMeshProUGUI>();
            c = tmp.color;
            c.a = alpha;
            if (chatShowTime < 2 || tmp.GetComponent<ChatMessage>().initialTimeExpired)
            {
                tmp.GetComponent<ChatMessage>().initialTimeExpired = true;
                tmp.gameObject.SetActive(chatShowTime < 2);
                tmp.color = c;
            }

            if(i == messagesContent.childCount-1)
            {
                c = messagesContent.GetComponent<Image>().color;
                c.a = chatShowTime < 2 ? 0 : tmp.alpha*0.8f;
                messagesContent.GetComponent<Image>().color = c;
            }

            /*string text = messagesContent.GetChild(i).GetComponent<Text>().text;
            string colT = text.Substring(8, 6);
            string rest = text.Substring(16);
            ColorUtility.TryParseHtmlString("#"+colT, out c);
            c.a = alpha;
            messagesContent.GetChild(i).GetComponent<Text>().text = "<color=#" + ColorUtility.ToHtmlStringRGBA(c) + rest;*/
        }

        //messagesPanel.gameObject.SetActive(chatShowTime < 2 || newMessageTime < 2);
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
        MessageType type = MessageType.PlayerChat;
        if (msgText.StartsWith("/") && GameManager.IsDebugging())
        {
            // command
            type = MessageType.Debug;
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
                    for (int i = 0; i < ResourceData.allResources.Count; i++)
                        resText += i + "=" + ResourceData.allResources[i].name + " ,";
                    resText = resText.Substring(0,resText.Length - 2);
                    msgText =
                        "Spielgeschwindigkeit verändern = /speed [faktor]" +
                        "\nJahre vergehen lassen = /years [jahre]" +
                        "\nRessourcen an ausgewählte Person = /give [resNr] [anzahl]\n" + resText +
                        "\nMännlichen Bewohner spawnen = /bornman [alter]" +
                        "\nWeiblichen Bewohner spawnen = /bornwoman [alter]" +
                        "\nGebäudekosten (de)aktivieren = /tcost";
                    break;
                case "speed":
                    try
                    {
                        float fact = float.Parse(arguments[0]);
                        fact = Mathf.Clamp(fact, 0.1f, 5000);
                        GameManager.speedFactor = fact;
                        msgText = "Spielgeschwindigkeit auf " + fact + " gesetzt";
                    }
                    catch
                    {
                        msgText = "Falsche Argumente!";
                        type = MessageType.Error;
                    }
                    break;
                case "years":
                    try
                    {
                        int yrs = int.Parse(arguments[0]);
                        yrs = Mathf.Clamp(yrs, 1,100);
                        GameManager.PassYears(yrs);
                        msgText = yrs + " Jahre sind vergangen";
                    }
                    catch
                    {
                        msgText = "Falsche Argumente!";
                        type = MessageType.Error;
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
                            ps.SetInventory(new GameResources(id, am));
                            msgText = am + "x " + ps.InventoryMaterial.Name + " wurden " + ps.FirstName + "s Inventar hinzugefügt";
                        }
                        else
                        {
                            msgText = "Keine Person ausgewählt";
                            type = MessageType.Error;
                        }
                    }
                    catch
                    {
                        msgText = "Falsche Argumente!";
                        type = MessageType.Error;
                    }
                    break;
                case "bornman":
                case "bornwoman":
                    try
                    {
                        Gender Gender = (msgText.ToLower() == "bornman") ? Gender.Male : Gender.Female;
                        int age = int.Parse(arguments[0]);
                        age = Mathf.Clamp(age, 0, 80);
                        GamePerson p = GameManager.village.PersonBirth(-1, Gender, age);
                        msgText = "Bewohner gespawnt: Name="+p.firstName+",Alter="+p.age;
                    }
                    catch
                    {
                        msgText = "Falsche Argumente!";
                        type = MessageType.Error;
                    }
                    break;
                case "tcost":
                    GameManager.noCost = !GameManager.noCost;
                    msgText = "Gebäudekosten " + (GameManager.noCost ? "de" : "") + "aktiviert";
                    break;
                default:
                    msgText = "Falscher Befehl!";
                    type = MessageType.Error;
                    break;
            }
        }
        else
        {
            msgText = GameManager.Username + ": " + msgText;
        }

        Msg(msgText, type);
        Instance.chatInput.text = "";
        Instance.chatShowTime = 0;
    }

    public static void Msg(string msg)
    {
        Msg(msg, MessageType.Info);
    }
    public static void Msg(string msg, MessageType type)
    {
        Color col = Color.green;
        switch(type)
        {
            case MessageType.Error:
                col = Color.red;
                break;
            case MessageType.Info:
                col = Color.white;
                break;
            case MessageType.PlayerChat:
                col = new Color(0.8f, 0.8f, 1f);
                break;
            case MessageType.News:
                col = Color.cyan;
                break;
            case MessageType.Debug:
                col = Color.magenta;
                break;
        }
        Msg(msg, col);
    }
    private static void Msg(string msg, Color col)
    {
        Instance.chatMessages.Add(msg);
        GameObject messageObj = (GameObject)Instantiate(Instance.chatMessagePrefab, Instance.messagesPanel.Find("Viewport/Content"));
        TextMeshProUGUI tmp = messageObj.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = msg;// "<color=#" + ColorUtility.ToHtmlStringRGBA(col)+ ">" + msg + "</color>";
        tmp.color = col;
        Instance.StartCoroutine(ScrollToTop());
        Instance.newMessageTime = 0;
    }
    public static void Error(string errorMsg)
    {
        Msg(errorMsg, Color.red);
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
