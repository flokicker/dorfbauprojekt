using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideableObject : MonoBehaviour {

    public HashSet<int> personIDs = new HashSet<int>();
    public bool inBuildRadius;

	// Use this for initialization
	public virtual void Start () {
        float d = Vector3.Distance(transform.position, Vector3.zero);
        inBuildRadius = d < 10;
		gameObject.SetActive(inBuildRadius);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
