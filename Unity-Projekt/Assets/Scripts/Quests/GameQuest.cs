using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameQuest
{
    public Quest quest
    {
        get { return Quest.Get(questId); }
    }
    [SerializeField]
    private int questId;

    public List<GameResources> collectedResources;
    private bool finished;

    public GameQuest(Quest quest)
    {
        questId = quest.id;

        collectedResources = new List<GameResources>();
        finished = false;
    }

    public float Percentage()
    {
        int total = 0, current = 0;
        List<BuildingQuestInfo> totBqi = GameManager.village.GetTotalBuildingsCount();
        foreach(BuildingQuestInfo bqi in quest.buildings)
        {
            foreach (BuildingQuestInfo b in totBqi)
            {
                if (b.buildingId == bqi.buildingId)
                {
                    current += Mathf.Min(b.count, bqi.count);
                    break;
                }
            }
            total += bqi.count;
        }

        List<GameResources> totRes = collectedResources;
        foreach (GameResources res in quest.collectResources)
        {
            foreach (GameResources r in totRes)
                if (r.Id == res.Id)
                {
                    current += Mathf.Min(r.Amount, res.Amount);
                    break;
                }
            total += res.Amount;
        }

        int[] currentJobs = GameManager.village.JobEmployedCount();
        foreach (BuildingQuestInfo jqi in quest.jobs)
        {
            current += currentJobs[jqi.buildingId];
            total += jqi.count;
        }

        // catch division by zero, if no buildings to build 100% reached
        if (total == 0) return 1f;

        return (float)current / total;
    }
    public void UpdateFinished()
    {
        if (!finished && Percentage() >= 1f)
        {
            ChatManager.Msg("Quest abgeschlossen: " + quest.name, MessageType.Info);
            foreach (Quest q in quest.unlockQuest)
            {
                bool unlockedAlreadyExists = false;
                foreach (GameQuest openQ in GameManager.gameData.openQuests)
                    if (openQ.quest.id == q.id)
                        unlockedAlreadyExists = true;
                if (unlockedAlreadyExists) continue;

                GameManager.gameData.openQuests.Add(new GameQuest(q));
                UIManager.Instance.Blink("PanelTopQuests", true);
                ChatManager.Msg("Neue Quest: " + q.name, MessageType.News);
            }
            finished = true;
        }
    }

    public bool Finished()
    {
        return finished;
    }
}
