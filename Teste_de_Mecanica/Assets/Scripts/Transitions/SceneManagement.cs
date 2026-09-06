using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneManagement
{
    private static string sceneTransitionName;

    public static string SceneTransitionName => sceneTransitionName;

    public static void SetTransitionName(string transitionName)
    {
        sceneTransitionName = transitionName;
    }

    public static string ConsumeTransitionName()
    {
        string transition = sceneTransitionName;

        sceneTransitionName = null;

        return transition;
    }
}
