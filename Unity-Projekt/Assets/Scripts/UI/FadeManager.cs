using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : Singleton<FadeManager> {

	public Image fadeImage;
	public bool isInTransition;
	public float transition;
	public bool showing;

	private float duration = 1;
	private Color fadeColor = Color.white;

	// Use this for initialization
	void Start () {
		isInTransition = false;
		fadeImage.color = fadeColor;
	}
	
	// Update is called once per frame
	void Update () {
		if(isInTransition && GameManager.IsSetup())
		{
			float fact = showing ? 1f : -1f;
			transition += fact * Time.deltaTime * 1/duration; 
			fadeImage.color = Color.Lerp(fadeColor*0f, fadeColor, transition);

			fadeImage.raycastTarget = true;
			if(transition > 1 ||transition < 0)
			{
				fadeImage.raycastTarget = false;
				isInTransition = false;
			}
		}
	}

	public static void Fade(bool showing)
	{
		if(Instance.isInTransition) return;
		Instance.showing = showing;
		Instance.isInTransition = true;
		Instance.transition = showing ? 0 : 1;
	}
}
