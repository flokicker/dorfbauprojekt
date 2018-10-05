using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TechBranch
{
    public int id;
    public string name;
    public bool unlocked = false, researched = false;
    public TechBranch parent = null;
    public List<int> children;
    public int unlockBuilding, unlockJob, unlockResource;

    public TechBranch(int id, string nm, GameResources[] costRes, int costFai, int resTime, int unlb)
    {
        this.id = id;

        name = nm;

        children = new List<int>();

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

        /* TODO: implement time to research */

        // branch 1
        /*TechBranch branchOrigin = new TechBranch("Opferstätte 1", new GameResources[] { new GameResources(0,30), new GameResources(1, 20), new GameResources("Fisch", 1) }, 0, 10, Building.Id("Opferstätte"));
        
        TechBranch branch1 = new TechBranch("Steinaxt", new GameResources[] { new GameResources(0, 1) }, 5, 10, -1);
        TechBranch branch2 = new TechBranch("Trampelpfad", new GameResources[] { new GameResources("Holz", 50), new GameResources("Stein", 25) }, 0, 10, -1);
        branchOrigin.children.Add(branch1);
        branchOrigin.children.Add(branch2);
        branch1.parent = branchOrigin;
        branch2.parent = branchOrigin;

        tree.Add(branchOrigin);
        tree.Add(branch1);
        tree.Add(branch2);

        branchOrigin.unlocked = true;*/
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
        if (GameManager.village.GetFaithPoints() - br.costFaith < -100)
        {
            ChatManager.Msg("Nicht genügend Glaubenspunkte zum erforschen!", MessageType.Info);
            return false;
        }
        List<GameResources> totRes = GameManager.village.GetTotalResourceCount();
        foreach(GameResources cost in br.costResource)
        {
            bool exists = false;
            foreach(GameResources r in totRes)
            {
                if (r.Id == cost.Id)
                {
                    exists = true;
                    if (r.Amount < cost.Amount)
                    {
                        ChatManager.Msg("Nicht genügend Ressourcen im Speicher zum erforschen! (" + r.Name + ": " + r.Amount + "/" + cost.Amount + ")", MessageType.Info);
                        return false;
                    }
                }
            }
            if (!exists)
            {
                ChatManager.Msg("Nicht genügend Ressourcen im Speicher zum erforschen! (" + cost.Name + ": none)", MessageType.Info);
                return false;
            }
        }

        GameManager.village.TakeResources(br.costResource);
        GameManager.village.TakeFaithPoints(br.costFaith);

        ChatManager.Msg("Du hast "+br.name+" erforscht!", MessageType.News);

        if (br.unlockBuilding != -1 && !Building.IsUnlocked(br.unlockBuilding))
            Village.UnlockBuilding(Building.Get(br.unlockBuilding));

        br.researched = true;
        foreach (int childId in br.children)
        {
            TechBranch child = Identify(childId);
            if(child != null)
                child.unlocked = true;
        }
        return true;
    }

    public TechBranch Identify(int id)
    {
        foreach(TechBranch tb in tree)
        {
            if (tb.id == id) return tb;
        }
        return null;
    }
}
