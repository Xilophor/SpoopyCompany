using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpoopyCompany.Patches;

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

            mls.LogInfo("Loaded Patches");
        }

        private const string pluginGUID = "SpoopyCompany.Events";
        private const string pluginNAME = "SpoopyCompany Events";
        private const string pluginVERSION = "0.1.0";

        private static readonly Harmony harmony = new(pluginGUID);

        public static Plugin Instance;

        public ManualLogSource mls;
    }
}
