using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechBranch
{
    public string name;
    public bool unlocked = false, researched = false;
    public TechBranch parent = null;
    public List<TechBranch> children;

    public TechBranch(string nm, GameResources[] costRes, int costFai, int resTime)
    {
        name = nm;

        children = new List<TechBranch>();

        costResource = new List<GameResources>();
        costResource.AddRange(costRes);
        costFaith = costFai;
        researchTime = resTime;
    }

    public List<GameResources> costResource;
    public int costFaith, researchTime;
}
public class TechTree {
    public List<TechBranch> tree;

    public TechTree()
    {
        tree = new List<TechBranch>();

        // branch 1
        TechBranch branchOrigin = new TechBranch("Opferstätte 1", new GameResources[] { new GameResources(0,150), new GameResources(1, 40), new GameResources(GameResources.ANIMAL_DUCK, 10) }, 0, 10);


        TechBranch branch1 = new TechBranch("Steinaxt", new GameResources[] { new GameResources(0, 1) }, 5, 10);
        TechBranch branch2 = new TechBranch("Test3", new GameResources[] { new GameResources(0, 1) }, 5, 10);
        branchOrigin.children.Add(branch1);
        branchOrigin.children.Add(branch2);
        branch1.parent = branchOrigin;
        branch2.parent = branchOrigin;

        tree.Add(branchOrigin);
        tree.Add(branch1);
        tree.Add(branch2);

        branchOrigin.unlocked = true;
    }

    public bool IsResearched(int id)
    {
        if (id >= tree.Count) return false;
        return tree[id].researched;
    }

    public bool IsUnlocked(int id)
    {
        if (id >= tree.Count) return false;
        return tree[id].unlocked;
    }

    public void Research(int id)
    {
        GameManager.village.TakeFaithPoints(tree[id].costFaith);

        tree[id].researched = true;
        foreach (TechBranch br in tree[id].children)
        {
            br.unlocked = true;
        }
    }
}
