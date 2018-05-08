using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private UnityAction onClick;

	private Text text;
	private Color textColor;

	private float targetScale = 1, targetAlpha = 0.8f;

    private bool isOver = false;
	private bool interactable = true;

	void Start()
	{
		text = GetComponent<Text>();
		textColor = text.color;
	}

	void Update()
	{
		transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one*targetScale, Time.deltaTime*5f);
		textColor.a = Mathf.Lerp(textColor.a, targetAlpha, Time.deltaTime*5f);
		text.color = textColor;
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
		if(!interactable) return;
		
		targetScale = 1.2f;
		targetAlpha = 1f;
        isOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
		if(!interactable) return;

		targetScale = 1;
		targetAlpha = 0.8f;
        isOver = false;
    }

	public void OnPointerClick(PointerEventData eventData)
	{
		if(!interactable) return;

		if(onClick != null)
			onClick.Invoke();
		
		targetScale = 1;
		targetAlpha = 0.8f;
	}

	public void AddOnClickListener(UnityAction action)
	{
		onClick = action;
	}

	public void SetInteractable(bool interactable)
	{
		this.interactable = interactable;
		if(!interactable)
		{
			targetAlpha = 0.3f;
			targetScale = 1f;
		}
		else
		{
			targetAlpha = 0.8f;
			targetScale = 1f;
		}
	}
}
