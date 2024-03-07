using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig 
{
    public static int DefaultJarCount = 9;
    public static int MaxRound = 5;
    public static int MaxPlayer = 20;

    public static float PrepareDuration = 5;
    public static float HidingDuration = 20;
    public static float HuntingDuration = 15;

    public static int BreakJarCount(int currentRound, int playerAmount)
    {
        int breakJarCount = 1;

        if (currentRound <= 3)
            return breakJarCount;

        if (playerAmount < 11)
            breakJarCount = 2;

        return breakJarCount;
    }

    public static int GetSeekAmount(int playerAmount)
    {
        int seekCount = 1;
        if(playerAmount >= 11)
        {
            seekCount = 2;
        }

        return seekCount;
    }
}
