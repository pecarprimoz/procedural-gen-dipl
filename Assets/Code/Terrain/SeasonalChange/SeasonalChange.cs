using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonalChange : MonoBehaviour
{
    // Every season will be active 4 seconds
    private float TotalSeasonChangeDuration = 0.0f;
    private float ElapsedTimeTotal = 0.0f;
    private float ElapsedTimePerSeason = 0.0f;
    private SeasonType CurrentSeason = SeasonType.kSpring;

    const float SeasonTimeLength = 4.0f;

    void Start()
    {
        // We have 4 seasons in total
        TotalSeasonChangeDuration = SeasonTimeLength * 4;
    }

    public void SeasonalChangeUpdate(TerrainInfo info)
    {
        if (info.AreSeasonsChanging)
        {
            // if total elapsed time 
            if (ElapsedTimeTotal < TotalSeasonChangeDuration)
            {
                var dTime = Time.deltaTime;
                ElapsedTimeTotal += dTime;
                ElapsedTimePerSeason += dTime;
            }
            else
            {
                info.AreSeasonsChanging = false;
                ElapsedTimeTotal = 0.0f;
                ElapsedTimePerSeason = 0.0f;
                CurrentSeason = SeasonType.kSpring;
                AssignSplatMap.DoSplat(info, CurrentSeason);
                Debug.Log($"SEASON TRANSITION END {CurrentSeason}");
                return;
            }
            if (ElapsedTimePerSeason >= 4.0f)
            {
                Debug.LogFormat("Changing season from {} to {}", (SeasonType)CurrentSeason, (SeasonType)CurrentSeason + 1);
                CurrentSeason = CurrentSeason + 1;
                ElapsedTimePerSeason = 0.0f;
            }
            AssignSplatMap.DoSplat(info, CurrentSeason);
        }
    }
}
