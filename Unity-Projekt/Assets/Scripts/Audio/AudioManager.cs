using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {

    [SerializeField]
    private List<AudioClip> chopAudio;

    // Use this for initialization
    void Start () {

        // make sure Instance has been used once to reduce frame drop when first used in game
        GetRandomChop();
    }
	
	// Update is called once per frame
	void Update () {

    }

    public static AudioClip GetRandomChop()
    {
        return Instance.chopAudio[Random.Range(0, Instance.chopAudio.Count)];
    }
}
