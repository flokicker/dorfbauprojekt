using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEffect : MonoBehaviour {

    private float time;

	// Use this for initialization
	void Start () {
        time = 0;
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        if(time >= 0.2f)
        {
            Material mat = GetComponent<MeshRenderer>().sharedMaterial;
            Color c = mat.color;
            c.a = 1f - (time - 0.2f) / 0.2f;
            mat.color = c;
            GetComponent<MeshRenderer>().sharedMaterial = mat;
        }
        if (time >= 0.4f) Destroy(gameObject);
	}
}
