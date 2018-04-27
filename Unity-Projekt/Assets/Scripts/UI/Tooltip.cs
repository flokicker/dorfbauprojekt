using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour {

	public static bool shown = false;
	private Transform toolTip;

	public string text;

	// Use this for initialization
	void Start () {
		toolTip = GetComponentInParent<Canvas>().transform.Find("Tooltip");

		EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
		EventTrigger.Entry evt = new EventTrigger.Entry();
		evt.eventID = EventTriggerType.PointerEnter;
		evt.callback.AddListener((data) => PointerEnter());
		eventTrigger.triggers.Add(evt);
		evt = new EventTrigger.Entry();
		evt.eventID = EventTriggerType.PointerExit;
		evt.callback.AddListener((data) => PointerExit());
		eventTrigger.triggers.Add(evt);
	}
	
	// Update is called once per frame
	void Update () {
	}


	public void PointerEnter()
	{
		if(!shown && enabled)
		{
			shown = true;
			toolTip.gameObject.SetActive(true);
			toolTip.transform.position = transform.position + new Vector3(0,65,0);
			toolTip.GetComponentInChildren<Text>().text = text;
		}
	}

	public void PointerExit()
	{
		if(shown)
		{
			shown = false;
			toolTip.gameObject.SetActive(false);
		}
	}
}
