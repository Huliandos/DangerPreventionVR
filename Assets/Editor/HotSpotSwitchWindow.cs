using System;
using System.Collections.Generic;
using System.Linq;
using Distractions.Management.EventSystem;
using Distractions.Management.EventSystem.Listener;
using Distractions.Management.EventSystem.Listener.Hotspots;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class HotSpotSwitchWindow : EditorWindow
{
    private List<InvokableHotspot> registeredHotspots = new List<InvokableHotspot>();
    private InvokableHotspot activeHotspot;

    private Texture leftArrow;
    private Texture rightArrow;
    
    private void OnEnable()
    {
        fetchHotspots();
        leftArrow = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/SteamVR/Textures/arrow.png", typeof(Texture));
        rightArrow = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Plugins/SteamVR/Textures/arrow.png", typeof(Texture));
    }

    [MenuItem("Window/Hotspot Switcher")]
    public static void OpenWindow()
    {
        EditorWindow editorWindow = GetWindow<HotSpotSwitchWindow>("Hotspot Switcher");
        
        EditorUtility.SetDirty(editorWindow);

        editorWindow.Show();
    }

    private void OnGUI()
    {
        CheckUserInput();

        Rect rect = EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Active Hotspot: ");
        
        if (activeHotspot == null)
            fetchHotspots();
        
        GUIContent contentLeftArrow = new GUIContent(leftArrow);
        GUIContent contentRightArrow = new GUIContent(rightArrow);

        if(GUILayout.Button(contentLeftArrow, GUILayout.Width(50), GUILayout.Height(50)))
            changeActiveHotspot(false);
        
        GUILayout.Label(activeHotspot.gameObject.name);
        if (GUILayout.Button(contentRightArrow, GUILayout.Width(50), GUILayout.Height(50)))
            changeActiveHotspot();

        EditorGUILayout.EndHorizontal();

        Repaint();
    }

    private void CheckUserInput()
    {
        Event currentEvent = Event.current;
        if (currentEvent != null && currentEvent.type == EventType.KeyDown)
        {
            if (currentEvent.keyCode == KeyCode.RightArrow)
            {
                changeActiveHotspot();
            }

            if (currentEvent.keyCode == KeyCode.LeftArrow)
            {
                changeActiveHotspot(false);
            }
        }
    }

    private void fetchHotspots()
    {
        registeredHotspots = DistractionEventSystem.distractionEventListeners.FilterCast<InvokableHotspot>().ToList();
        activeHotspot = registeredHotspots.Find(hotspot => hotspot.activateKeyboardInputs);
    }

    private void changeActiveHotspot(int index)
    {
        activeHotspot.activateKeyboardInputs = false;
        activeHotspot = registeredHotspots[index];
        activeHotspot.activateKeyboardInputs = true;
    }

    private void changeActiveHotspot(bool forward = true)
    {
        int currentIndex = registeredHotspots.IndexOf(activeHotspot);
        int nextIndex;
        if (forward)
        {
            if (currentIndex + 1 < registeredHotspots.Count)
            {
                nextIndex = currentIndex + 1;
            }
            else
            {
                nextIndex = 0;
            }
        }
        else
        {
            if (currentIndex - 1 >= 0)
            {
                nextIndex = currentIndex - 1;
            }
            else
            {
                nextIndex = registeredHotspots.Count - 1;
            }
        }
        
        changeActiveHotspot(nextIndex);
    }
}
