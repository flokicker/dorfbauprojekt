using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEffect : MonoBehaviour {

    private float time;
    private MeshRenderer meshRenderer;

	// Use this for initialization
	void Start () {
        time = 0;
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.color = Color.white;

    }
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        if(time >= 0.2f)
        {
            Color c = Color.white;
            c.a = Mathf.Max(1f - (time - 0.2f) / 0.2f, 0);
            meshRenderer.sharedMaterial.color = c;
        }
        if (time >= 0.4f) Destroy(gameObject);
	}
}
