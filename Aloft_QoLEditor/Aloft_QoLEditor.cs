using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace Aloft_QoLEditor
{
    [BepInPlugin("com.sp00ktober.Aloft_QoLEditor", "Aloft_QoLEditor", "0.0.1")]
    public class Aloft_QoLEditor : BaseUnityPlugin
    {
        private void Awake()
        {
            InitPatches();
        }

        private static void InitPatches()
        {
            Debug.Log("Patching Aloft...");

            try
            {
                Debug.Log("Applying patches from Aloft_QoLEditor 0.0.1");

                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.sp00ktober.de");

                Debug.Log("Patching completed successfully");
            }
            catch(Exception e)
            {
                Debug.Log("Unhandled exception occurred while patching the game: " + e);
            }
        }
    }
}
