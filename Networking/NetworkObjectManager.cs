using HarmonyLib;
using UnityEngine;
using SpoopyComponents;
using SpoopyCompany;
using Unity.Netcode;

namespace SpoopyCompany
{
    [HarmonyPatch]
    public class NetworkObjectManager
    {

        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        public static void Init()
        {
            if (networkPrefab != null) return;
            networkPrefab = (GameObject)Assets.MainAssetBundle.LoadAsset("spoopynetworkhandler");
            networkPrefab.AddComponent<NetworkHandler>();

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
            Plugin.Instance.mls.LogInfo("Created NetworkHandler prefab");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            try
            {
                if(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                {
                    Plugin.Instance.mls.LogInfo("Spawning network handler");
                    networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                    networkHandlerHost.GetComponent<NetworkObject>().Spawn(true); // Automatically remove when returning to menu
                }
            }
            catch
            {
                Plugin.Instance.mls.LogError("Failed to spawned network handler");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        static void DestroyNetworkHandler()
        {
            try
            {
                if(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                {
                    Plugin.Instance.mls.LogInfo("Destroying network handler");
                    Object.Destroy(networkHandlerHost);
                    networkHandlerHost = null;
                }
            }
            catch
            {
                Plugin.Instance.mls.LogError("Failed to destroy network handler");
            }
        }
        
        static GameObject networkPrefab;
        static GameObject networkHandlerHost;
    }
}