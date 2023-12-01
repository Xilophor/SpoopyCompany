using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpoopyCompany.Patches;
using UnityEngine;

namespace SpoopyCompany
{
    [BepInPlugin(pluginGUID, pluginNAME, pluginVERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake() {
            if (Instance == null)
                Instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(pluginGUID);

            mls.LogInfo("Loading Patches...");

            harmony.PatchAll(typeof(NetworkObjectManager));
            harmony.PatchAll(typeof(SpoopyEventHandler));


            Assets.PopulateAssets("SpoopyCompany.asset");
            NetcodeWeaver(); //Initialize NetworkPatch

            mls.LogInfo("Loaded Patches");
        }

        private void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }


        private const string pluginGUID = "SpoopyCompany";
        private const string pluginNAME = "SpoopyCompany";
        private const string pluginVERSION = "0.1.0";

        private static readonly Harmony harmony = new(pluginGUID);

        public static Plugin Instance;

        public ManualLogSource mls;
    }
}
