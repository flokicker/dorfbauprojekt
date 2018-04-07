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
	private DepthOfFieldModel dofm;
	private DepthOfFieldModel.Settings dofms;

	// Use this for initialization
	void Start () {
		cgm =  postProcessingBehaviour.profile.colorGrading;
		cgms = cgm.settings;
		dofm = postProcessingBehaviour.profile.depthOfField;
		dofms = dofm.settings;

		if(QualitySettings.GetQualityLevel() <= 2) postProcessingBehaviour.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		int season = GameManager.village.GetTwoSeason();
		if(season == 0 && !snowParticles.isEmitting)
			snowParticles.Play();
		if(season == 1 && snowParticles.isEmitting)
			snowParticles.Stop();

		int d = GameManager.village.GetDay();
		d = Mathf.Abs(365/2 - d);
		cgms.basic.temperature = -10 + 30*(1f-d/(365f/2f));
		
		//dofms.focusDistance = Mathf.Lerp(dofms.focusDistance, Mathf.Clamp(Camera.main.GetComponent<CameraController>().cameraDistance/10+1, 1, 10), Time.deltaTime*5);

		cgm.settings = cgms;
		postProcessingBehaviour.profile.colorGrading = cgm;
		dofm.settings = dofms;
		postProcessingBehaviour.profile.depthOfField = dofm;
	}
}
