using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderScreenshot : MonoBehaviour {

    public bool capture;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (capture)
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png");
            capture = false;
        }
	}
}
