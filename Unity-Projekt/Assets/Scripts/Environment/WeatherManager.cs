using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

public class WeatherManager : MonoBehaviour {

	[SerializeField]
	private ParticleSystem snowParticles;

	[SerializeField]
	private PostProcessingBehaviour postProcessingBehaviour;

	private ColorGradingModel cgm;
	private ColorGradingModel.Settings cgms;
	private DepthOfFieldModel dofm;
	private DepthOfFieldModel.Settings dofms;

	[SerializeField]
	private Terrain terrain;

	// Use this for initialization
	void Start () {
		cgm =  postProcessingBehaviour.profile.colorGrading;
		cgms = cgm.settings;
		dofm = postProcessingBehaviour.profile.depthOfField;
		dofms = dofm.settings;

		if(QualitySettings.GetQualityLevel() <= 2) postProcessingBehaviour.enabled = false;

		/*TerrainData tData = terrain.terrainData;
		float[,,] alphaData = tData.GetAlphamaps(0, 0, tData.alphamapWidth, tData.alphamapHeight);

		float percentage = 0.2f;
        for(int y=0; y<tData.alphamapHeight; y++){
            for(int x = 0; x < tData.alphamapWidth; x++){
				float swap = alphaData[x, y, 1];
                alphaData[x, y, 1] = alphaData[x, y, 2];
                alphaData[x, y, 2] = swap;
            }
        }

		tData.SetAlphamaps(0, 0, alphaData);*/
	}
	
	// Update is called once per frame
	void Update () {
		int season = GameManager.GetTwoSeason();
		if(season == 0 && !snowParticles.isEmitting)
			snowParticles.Play();
		if(season == 1 && snowParticles.isEmitting)
			snowParticles.Stop();

        int d = GameManager.DayOfYear;
		d = Mathf.Abs(365/2 - d);
		cgms.basic.temperature = -10 + 30*(1f-d/(365f/2f));

		/*// Get distance from camera and target
		float dist = Vector3.Distance(Camera.main.transform.position, CameraController.LookAtTransform().position);
		//float dist = Vector3.Dot(CameraController.LookAtTransform().position-Camera.main.transform.position,transform.forward);
		// Set variables
		//dofms.focusDistance = dist/2;
		//dofms.aperture = dist/5;
		CalculateDistanceToVisibleObject();
		//dofms.focusDistance = Mathf.Lerp(dofms.focusDistance, Mathf.Clamp(Camera.main.GetComponent<CameraController>().cameraDistance/10+1, 1, 10), Time.deltaTime*5);
*/
		float dist = CameraController.Instance.cameraDistance;

		float focDist = 0.0255f*dist + 1.75f;
		float aprt =  0.0145f*dist + 1.35f;

		dofms.focusDistance = focDist;
		dofms.aperture = aprt;

		cgm.settings = cgms;
		postProcessingBehaviour.profile.colorGrading = cgm;
		dofm.settings = dofms;
		postProcessingBehaviour.profile.depthOfField = dofm;
	}

	// not used
	private void CalculateDistanceToVisibleObject()
    {
		var fwd = Camera.main.transform.TransformDirection (Vector3.forward);
		RaycastHit hit;
		if (Physics.Raycast (Camera.main.transform.position, fwd, out hit, 200)) {
			float distanceOfObject = hit.distance;
			float targetFocusRange= distanceOfObject/.4f;
			float targetFocusDistance=distanceOfObject-(distanceOfObject/4);
			if(distanceOfObject<3){
				targetFocusRange=distanceOfObject;
				targetFocusDistance=distanceOfObject-.5f;
					if(distanceOfObject<.7){
						targetFocusRange=.5f;
						targetFocusDistance=.4f;
					}
			}
			dofms.focalLength = targetFocusRange;
			dofms.focusDistance = targetFocusDistance;
		}
    }

}
