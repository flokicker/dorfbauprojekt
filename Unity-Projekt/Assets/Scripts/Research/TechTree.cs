using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TechBranch
{
    public string name;
    public bool unlocked = false, researched = false;
    public TechBranch parent = null;
    public List<TechBranch> children;
    public int unlockBuilding, unlockJob, unlockResource;

    public TechBranch(string nm, GameResources[] costRes, int costFai, int resTime, int unlb)
    {
        name = nm;

        children = new List<TechBranch>();

        costResource = new List<GameResources>();
        costResource.AddRange(costRes);
        costFaith = costFai;
        researchTime = resTime;

        unlockBuilding = unlb;
    }

    public List<GameResources> costResource;
    public int costFaith, researchTime;
}
[System.Serializable]
public class TechTree {
    public List<TechBranch> tree;

    public TechTree()
    {
        tree = new List<TechBranch>();

        // branch 1
        TechBranch branchOrigin = new TechBranch("Opferstätte 1", new GameResources[] { new GameResources(0,150), new GameResources(1, 40), new GameResources(GameResources.ANIMAL_DUCK, 10) }, 0, 10, Building.SACRIFICIALALTAR);


        TechBranch branch1 = new TechBranch("Steinaxt", new GameResources[] { new GameResources(0, 1) }, 5, 10, -1);
        TechBranch branch2 = new TechBranch("Test3", new GameResources[] { new GameResources(0, 1) }, 5, 10, -1);
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

    public bool Research(int id)
    {
        TechBranch br = tree[id];
        if (GameManager.village.GetFaithPoints() - br.costFaith < -100) return false;

        GameManager.village.TakeFaithPoints(br.costFaith);

        if (br.unlockBuilding != -1 && !Building.IsUnlocked(br.unlockBuilding))
            Village.UnlockBuilding(br.unlockBuilding);

        br.researched = true;
        foreach (TechBranch child in br.children)
        {
            child.unlocked = true;
        }
        return true;
    }
}
