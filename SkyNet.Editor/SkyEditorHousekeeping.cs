using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SkyNet.Unity.Editor
{
    [InitializeOnLoad]
    internal static class SkyEditorHousekeeping
    {
        private static volatile Queue<Action> invokeQueue = new Queue<Action>();
        private static DateTime saveSceneTime = DateTime.MaxValue;

        public static void Invoke(Action action)
        {
            lock (invokeQueue)
                invokeQueue.Enqueue(action);
        }

        public static void AskToSaveSceneAt(DateTime time)
        {
            saveSceneTime = time;
        }

        static SkyEditorHousekeeping()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            SaveScene();
            InvokeCallbacks();
        }

        private static void SaveScene()
        {
            if (!(saveSceneTime < DateTime.Now))
                return;
            saveSceneTime = DateTime.MaxValue;
            //EditorApplication.SaveCurrentSceneIfUserWantsTo();
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        private static void InvokeCallbacks()
        {
            lock (invokeQueue)
            {
                while (invokeQueue.Count > 0)
                    invokeQueue.Dequeue()();
            }
        }
    }
}
