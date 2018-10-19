using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System;

[System.Serializable]
public class TechBranch
{
    [XmlAttribute("id")]
    public int id;
    
    public string name, description;
    
    public int costTechPoints;
    
    public int costFaithPoints;

    public List<TechBranch> children = new List<TechBranch>();

    //public List<GameResources> costResource;
}

[XmlRoot("TechTree")]
public class TechTree
{
    [XmlArray("TechBranches")]
    [XmlArrayItem("Techbranch")]
    public List<TechBranch> root;

    [XmlIgnore]
    public List<int> unlockedBranches;

    public TechTree()
    {

    }
    public TechTree(TechTree baseTree)
    {
        root = new List<TechBranch>();
        root.AddRange(baseTree.root);
        unlockedBranches = new List<int>();
        SetInitialUnlockedBranches();
    }

    public void SetInitialUnlockedBranches()
    {
        unlockedBranches.Add(1);
        unlockedBranches.Add(121);
    }
    
    public bool IsUnlocked(int id)
    {
        if (unlockedBranches == null) return false;
        return unlockedBranches.Contains(id);
    }

    public bool IsResearched(string name)
    {
        foreach (TechBranch tbr in root)
            if (CheckResearchChildren(name, tbr)) return true;
        return false;
    }
    public bool CheckResearchChildren(string name, TechBranch parent)
    {
        if (!IsUnlocked(parent.id)) return false;
        if (parent.name == name) return true;
        foreach(TechBranch child in parent.children)
        {
            if (CheckResearchChildren(name, child)) return true;
        }
        return false;
    }

    public bool Research(TechBranch br)
    {
        if (unlockedBranches == null) return false;
        if (!unlockedBranches.Contains(br.id))
        {
            if (GameManager.village.GetFaithPoints() < br.costFaithPoints || GameManager.village.GetTechPoints() < br.costTechPoints) return false;

            // take costs
            GameManager.village.ChangeFaithPoints(-br.costFaithPoints);
            GameManager.village.ChangeTechPoints(-br.costTechPoints);

            unlockedBranches.Add(br.id);
            ChatManager.Msg("Du hast " + br.name + " erforscht!", MessageType.News);

            switch(br.name)
            {
                case "Kornfeld 1":
                    GameManager.UnlockJob(Job.Get("Bauer"));
                    break;
                case "Jäger 1":
                    GameManager.UnlockJob(Job.Get("Jäger"));
                    break;
                case "Holzfäller 1":
                    GameManager.UnlockJob(Job.Get("Holzfäller"));
                    break;
                case "Steinmetz 1":
                    GameManager.UnlockJob(Job.Get("Steinmetz"));
                    break;
                case "Glauben":
                    GameManager.UnlockBuilding(Building.Get("Opferstätte"));
                    break;
            }
            UIManager.Instance.UpdateTechTree();
        }
        return true;
    }

        /*public bool IsResearched(int id)
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
        }*/
    }
