using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {

    [SerializeField]
    private List<AudioClip> chopAudio;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {

    }

    public static AudioClip GetRandomChop()
    {
        return Instance.chopAudio[Random.Range(0, Instance.chopAudio.Count)];
    }
}
