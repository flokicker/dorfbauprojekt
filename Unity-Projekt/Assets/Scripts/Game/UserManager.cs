using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserManager : Singleton<UserManager> {

    public static string username;
    public static int nationID = 0; //0=Ägypter

	// Use this for initialization
	void Start () {

        /*string url = "http://localhost/projektxy/get_user.php";
        WWW www = new WWW(url);
        //StartCoroutine(WaitForRequest(www));

        GameObject.Find("TextNation").GetComponent<Text>().text = Village.nationName[nationID];*/
	}

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            string s = www.text;
            Debug.Log(s);
            string text = "";
            if (s.StartsWith("0"))
            {
                text = "You're not logged in!";
            }
            else
            {
                string[] ss = s.Split('*');
                text = ss[2];
            }
            GameObject.Find("TextUser").GetComponent<Text>().text = text;
        }
        else
        {
            GameObject.Find("TextUser").GetComponent<Text>().text = "ERROR:" + www.error;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
