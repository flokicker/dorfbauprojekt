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
    private Transform chatPanel, messagesPanel;

	// Use this for initialization
    void Start()
    {
        chatPanel = transform.Find("ChatInput");
        chatInput = chatPanel.GetComponentInChildren<InputField>();
        messagesPanel = transform.Find("Chat");
	}
	
	// Update is called once per frame
	void Update () {
        chatShowTime += Time.deltaTime;
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
        Color col = Color.cyan;
        if (msgText.StartsWith("/"))
        {
            // command
            col = Color.magenta;
            msgText = msgText.Substring(1);
            string[] arguments = new string[0];
            if(msgText.Contains(" "))
                arguments = msgText.Substring(msgText.IndexOf(' ')+1).Split(' ');
            msgText = msgText.Substring(0,msgText.IndexOf(' '));
            switch (msgText.ToLower())
            {
                case "help":
                    msgText = "command help";
                    break;
                case "give":
                    try
                    {
                        Debug.Log(arguments.Length);
                        int id = int.Parse(arguments[0]);
                        int am = int.Parse(arguments[1]);
                        PersonScript ps = PersonScript.FirstSelectedPerson();
                        if (ps)
                        {
                            ps.inventoryMaterial = new GameResources(id, am);
                            msgText = "added " + am + "x " + ps.inventoryMaterial.GetName() + " to " + ps.firstName + "s inventory";
                        }
                        else msgText = "no person selected";
                    }
                    catch
                    {
                        msgText = "wrong arguments";
                    }
                    break;
                default:
                    msgText = "unknown command";
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
        messageObj.GetComponentInChildren<Text>().text = "<color=#" + ColorUtility.ToHtmlStringRGB(col)+ ">" + msg + "</color>";
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
