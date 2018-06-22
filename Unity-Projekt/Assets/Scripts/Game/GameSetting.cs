using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetting {

    private List<GameResources> featuredResources = new List<GameResources>();
    private List<int>[] peopleGroups = new List<int>[10];

    public GameSetting(List<GameResources> featuredResources, List<int>[] peopleGroups)
    {
        this.featuredResources = featuredResources;
        this.peopleGroups = peopleGroups;
    }

    public GameSetting(List<GameResources> featuredResources) : this(featuredResources, new List<int>[10]) { }

    public void AddFeaturedResource(GameResources res)
    {
        featuredResources.Add(res);
    }

    public List<GameResources> GetFeaturedResources()
    {
        return featuredResources;
    }

    public List<int> GetPeopleGroup(int num)
    {
        return peopleGroups[num];
    }

    public void SetPeopleGroup(int num, List<int> group)
    {
        peopleGroups[num] = group;
    }
}
