using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class EditorSeasonalChange
{
    public void DrawSeasonalChangeGUI(TerrainInfo info)
    {
        if (GUI.Button(EditorGUILayout.GetControlRect(), "Start Season Transition"))
        {
            if (!info.AreSeasonsChanging)
            {
                info.AreSeasonsChanging = true;
            }
            else
            {
                GUILayout.Label("Current season " + info.CurrentSeason.ToString());
                Debug.LogWarning("Season Transitions already running !");
            }

        }
    }
}
