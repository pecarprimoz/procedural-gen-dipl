using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonalChange : MonoBehaviour
{
    // Every season will be active 4 seconds
    private float TotalSeasonChangeDuration = 0.0f;
    private float ElapsedTimeTotal = 0.0f;
    private float ElapsedTimePerSeason = 0.0f;

    const float SeasonTimeLength = 4.0f;

    void Start()
    {
        // We have 4 seasons in total
        TotalSeasonChangeDuration = SeasonTimeLength * 4;
    }  


    // hierarchy for trees
    /*
     * ParentPlaceableObject -> Biome -> Tree -> First child
     * 
     */
    private static void UpdateBiomeContent(TerrainInfo info, SeasonType seasonType)
    {
        // go trough all biomes
        foreach (var kvp in info.ContentManager.BiomeParentGameObjects)
        {
            var biomeIdx = kvp.Key;
            var biomeParent = kvp.Value;
            // go trough biome content
            foreach (var contentElement in biomeParent.GetComponentsInChildren<Transform>())
            {
                // take the first element and hope its a tree
                var treeLod0 = contentElement.GetChild(0).GetChild(0);
                treeLod0.gameObject.GetComponent<MeshRenderer>().materials[2].SetColor("_Color", Color.red);
            }
        }
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
                info.CurrentSeason = SeasonType.kSpring;
                AssignSplatMap.DoSplat(info, info.CurrentSeason);
                Debug.Log($"SEASON TRANSITION END {info.CurrentSeason}");
                return;
            }
            if (ElapsedTimePerSeason >= 4.0f)
            {
                Debug.LogFormat("Changing season from {} to {}", (SeasonType)info.CurrentSeason, (SeasonType)info.CurrentSeason + 1);
                info.CurrentSeason = info.CurrentSeason + 1;
                ElapsedTimePerSeason = 0.0f;
            }
            UpdateBiomeContent(info, info.CurrentSeason);
            AssignSplatMap.DoSplat(info, info.CurrentSeason);
        }
    }
}
