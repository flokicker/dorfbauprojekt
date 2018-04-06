using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class WeatherManager : MonoBehaviour {

	[SerializeField]
	private ParticleSystem snowParticles;

	[SerializeField]
	private PostProcessingBehaviour postProcessingBehaviour;

	private ColorGradingModel cgm;
	private ColorGradingModel.Settings cgms;

	// Use this for initialization
	void Start () {
		cgm =  postProcessingBehaviour.profile.colorGrading;
		cgms = cgm.settings;

		if(QualitySettings.GetQualityLevel() <= 2) postProcessingBehaviour.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		int season = GameManager.GetVillage().GetTwoSeason();
		if(season == 0 && !snowParticles.isEmitting)
			snowParticles.Play();
		if(season == 1 && snowParticles.isEmitting)
			snowParticles.Stop();

		int d = GameManager.GetVillage().GetDay();
		d = Mathf.Abs(365/2 - d);
		cgms.basic.temperature = -10 + 30*(1f-d/(365f/2f));

		cgm.settings = cgms;
		postProcessingBehaviour.profile.colorGrading = cgm;
	}
}
