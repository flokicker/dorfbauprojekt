using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatMessage : MonoBehaviour {

    public bool initialTimeExpired;
    private float initialTime;
    private TextMeshProUGUI text;
    private Color myCol;

	// Use this for initialization
	void Start () {
        initialTime = 0;
        text = GetComponent<TextMeshProUGUI>();
        myCol = text.color;
        initialTimeExpired = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!initialTimeExpired)
        {
            initialTime += Time.deltaTime;
            myCol = text.color;
            if (initialTime < 2)
                myCol.a = 1;
            else if (initialTime < 3)
                myCol.a = Mathf.Lerp(1, 0, initialTime - 2);
            else
            {
                gameObject.SetActive(false);
                initialTimeExpired = true;
                myCol.a = 0;
            }
            text.color = myCol;
        }
    }
}
