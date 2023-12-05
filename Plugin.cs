using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
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

            LoadConfig();

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


        #region Config
        private void LoadConfig()
        {
            FlickerLightsChance = Config.Bind("Event Chances",
                                                "FlickerLights",
                                                87f,
                                                "The % chance the lights will flicker at some point in the day. Values between 100 and 600 will increase the likeliness that this occurs multiple times a day. Only host settings apply. Set to 0 to disable.");
            PowerOutageChance = Config.Bind("Event Chances",
                                                "PowerOutage",
                                                7.8f,
                                                "The % chance there will be a power outage at some point in the day. Values between 100 and 600 will increase the likeliness that this occurs multiple times a day. Lights flickering must occur first. Only host settings apply. Set to 0 to disable.");
            PowerSurgeChance = Config.Bind("Event Chances",
                                                "PowerSurge",
                                                0.9f,
                                                "The % chance there will be a power surge at some point in the day. Lights flickering must occur first. Only host settings apply. Set to 0 to disable.");
            /*PipeBurstChance = Config.Bind("Event Chances",
                                                "PipeBurst",
                                                1.7f,
                                                "The % chance the pipes burst at some point in the day. Only host settings apply. Set to 0 to disable.");*/
        }

        public ConfigEntry<float> FlickerLightsChance;
        public ConfigEntry<float> PowerOutageChance;
        public ConfigEntry<float> PowerSurgeChance;
        public ConfigEntry<float> PipeBurstChance;
        #endregion Config


        private const string pluginGUID = "SpoopyCompany";
        private const string pluginNAME = "SpoopyCompany";
        private const string pluginVERSION = "0.1.0";

        private static readonly Harmony harmony = new(pluginGUID);

        public static Plugin Instance;

        public ManualLogSource mls;
    }
}
