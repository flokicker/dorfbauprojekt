using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lake : MonoBehaviour {
    
    public int currentFish, maxFish;

    private float fishRespawnTimer;

    private List<Transform> lakeFish;
    private int currentShownFish;

    // Use this for initialization
    void Start () {
        fishRespawnTimer = 0;
        currentFish = Random.Range(maxFish/2,maxFish);

        lakeFish = new List<Transform>();
        foreach (Transform trf in transform)
            lakeFish.Add(trf);

        currentShownFish = lakeFish.Count;
    }
	
	// Update is called once per frame
	void Update () {
        fishRespawnTimer += Time.deltaTime;
        if (fishRespawnTimer >= RespawnTime())
        {
            fishRespawnTimer = 0;
            currentFish++;
        }

        int sfoc = ShowFishObjectCount();
        if (currentShownFish != sfoc)
        {
            for(int i = 0; i < lakeFish.Count; i++)
            {
                lakeFish[i].gameObject.SetActive(i < sfoc);
            }
            currentShownFish = sfoc;
        }

        currentFish = Mathf.Clamp(currentFish, 0, maxFish);
    }

    // get time until next fish spawns
    private float RespawnTime()
    {
        float respawnTime = 5;

        /*if (currentFish == 0) respawnTime = 40;
        else if (currentFish < maxFish / 2) respawnTime = 35;
        else if (currentFish < 3*maxFish / 4) respawnTime = 20;*/

        if (GameManager.GetFourSeason() == 0) // in winter reproduction is slower
            respawnTime *= 1.5f;

        return respawnTime;
    }

    // amount of fish that should be rendered
    public int ShowFishObjectCount()
    {
        if (currentFish == 0) return 0;

        float perc = (float)currentFish / maxFish;
        perc *= lakeFish.Count;

        return Mathf.Max(1, (int)(perc+0.5f));
    }

    // take amount of fish out of the lake
    public void TakeFish(int amount)
    {
        currentFish -= amount;
        currentFish = Mathf.Clamp(currentFish, 0, maxFish);
    }
}
