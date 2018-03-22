using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetting {

    private List<GameResources> featuredResources = new List<GameResources>();

    public GameSetting(List<GameResources> featuredResources)
    {
        this.featuredResources = featuredResources;
    }

    public List<GameResources> GetFeaturedResources()
    {
        return featuredResources;
    }
}
