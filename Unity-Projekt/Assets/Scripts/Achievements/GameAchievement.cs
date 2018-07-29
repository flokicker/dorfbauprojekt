using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAchievement
{
    public Achievement achievement
    {
        get { return Achievement.Get(achievementId); }
    }
    [SerializeField]
    private int achievementId;

    public int currentAmount;

    public GameAchievement(Achievement ach)
    {
        achievementId = ach.id;

        currentAmount = 0;
    }

    public int GetLvl()
    {
        for (int i = 0; i < achievement.amountLvl.Length; i++)
            if (currentAmount < achievement.amountLvl[i]) return i;
        return achievement.amountLvl.Length;
    }

    public void UpdateAmount(int add)
    {
        int oldLvl = GetLvl();
        currentAmount += add;
        if (GetLvl() > oldLvl) ChatManager.Msg("Neuen Erfolg freigeschalten: " + achievement.name + " Stufe " + (oldLvl + 1), Color.cyan);
    }
}
