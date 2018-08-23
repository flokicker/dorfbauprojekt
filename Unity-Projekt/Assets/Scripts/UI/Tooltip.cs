using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour {

    public static Tooltip tooltipTransform;

	public static bool shown = false;
	private Transform tooltip;

    private Vector3 mouseOffset = new Vector3(0, 45, 0);

    public string text;

	// Use this for initialization
	void Start () {
        tooltip = GetComponentInParent<Canvas>().transform.Find("Tooltip");

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
	void Update ()
    {
        tooltip.transform.position = Input.mousePosition + mouseOffset;
        if (tooltipTransform != null && !tooltipTransform.gameObject.activeInHierarchy)
        {
            PointerExit();
        }
	}


	public void PointerEnter()
	{
		if(!shown && enabled)
		{
            tooltipTransform = this;
            shown = true;
            tooltip.gameObject.SetActive(true);
            tooltip.transform.position = Input.mousePosition + mouseOffset;
            tooltip.GetComponentInChildren<Text>().text = text;
		}
	}

	public void PointerExit()
	{
		if(shown)
        {
            tooltipTransform = null;
            shown = false;
            tooltip.gameObject.SetActive(false);
		}
	}
}
