using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour {

	[SerializeField]
	private ParticleSystem snowParticles;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		int season = GameManager.GetVillage().GetTwoSeason();
		if(season == 0 && !snowParticles.isEmitting)
			snowParticles.Play();
		if(season == 1 && snowParticles.isEmitting)
			snowParticles.Stop();
	}
}
