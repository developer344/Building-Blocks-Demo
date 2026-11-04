using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// The imported Meta models hide themselves completely once they get small on screen, which in
/// VR happens only a few metres away. This keeps their lower-detail versions visible at any
/// distance instead, and repairs detail levels that are empty or out of order.
/// </summary>
public static class KeepModelsVisible
{
    // Hooked to every scene load so it still applies after the game is restarted.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var group in Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Include))
            DisableCulling(group);
    }

    public static void DisableCulling(LODGroup group)
    {
        var lods = new List<LOD>();
        foreach (var lod in group.GetLODs())
        {
            if (lod.renderers != null && Array.Exists(lod.renderers, r => r != null))
                lods.Add(lod);
        }
        if (lods.Count == 0)
            return;

        // Each level must switch at a smaller screen size than the one before it,
        // and the last level stays on no matter how small the object gets.
        for (int i = 0; i < lods.Count; i++)
        {
            LOD lod = lods[i];
            if (i == lods.Count - 1)
            {
                lod.screenRelativeTransitionHeight = 0f;
            }
            else
            {
                float previous = i == 0 ? 1f : lods[i - 1].screenRelativeTransitionHeight;
                lod.screenRelativeTransitionHeight = Mathf.Min(lod.screenRelativeTransitionHeight, previous * 0.9f);
            }
            lods[i] = lod;
        }

        group.SetLODs(lods.ToArray());
    }
}
