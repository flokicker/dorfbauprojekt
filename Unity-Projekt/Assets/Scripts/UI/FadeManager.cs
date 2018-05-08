using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour {

	public Image fadeImage;
	public bool isInTransition = false;
	public float transition;
	public bool showing;

	private float durationFill, durationFade;
	private Color fadeColor = Color.white;//new Color(0.28f, 0.674f, 0.73f);

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(isInTransition && GameManager.IsSetup())
		{
			float fact = showing ? 1f : -1f;
			if(transition > 1)
			{
				transition += fact * Time.deltaTime * 1/durationFill; 
				fadeImage.color = fadeColor;
			}
			else
			{
				transition += fact * Time.deltaTime * 1/durationFade; 
				fadeImage.color = Color.Lerp(fadeColor*0f, fadeColor, transition);
			}

			fadeImage.raycastTarget = true;
			if(transition > 2 ||transition < 0)
			{
				fadeImage.raycastTarget = false;
				isInTransition = false;
				fadeImage.color = Color.Lerp(fadeColor*0f, fadeColor, transition - (showing ? 1f : 0f));
			}
		}
	}

	public void Fade(bool showing, float durationFill, float durationFade)
	{
		if(isInTransition) return;
		this.showing = showing;
		this.durationFill = durationFill;
		this.durationFade = durationFade;
		this.isInTransition = true;
		this.transition = showing ? 0 : 2;
	}
}
