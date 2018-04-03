using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideableObject : MonoBehaviour {

    public HashSet<int> personIDs = new HashSet<int>();
    public bool inBuildRadius, isHidden;

	// Use this for initialization
	public virtual void Start () {
        inBuildRadius = Mathf.Abs(transform.position.x) < 13 && Mathf.Abs(transform.position.z) < 13;

		float minDist = float.MaxValue;
		foreach(PersonScript ps in PersonScript.allPeople)
		{
			float dist = Mathf.Abs(ps.transform.position.x - transform.position.x) + Mathf.Abs(ps.transform.position.z - transform.position.z);
			if(dist < minDist) minDist = dist;
		}

		isHidden = inBuildRadius || minDist < 10;
		gameObject.SetActive(isHidden);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
