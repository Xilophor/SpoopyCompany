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

        [HarmonyPatch(typeof(MenuManager), "Start")]
        [HarmonyPostfix]
        public static void Init()
        {
            if (networkPrefab != null) return;
            networkPrefab = new GameObject("SpoopyNetwork");
            Object.DontDestroyOnLoad(networkPrefab);
            networkPrefab.SetActive(false);
            networkPrefab.AddComponent<NetworkObject>();
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
                    var networkObject = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                    networkObject.SetActive(true);
                    networkObject.GetComponent<NetworkObject>().Spawn(false);
                    Plugin.Instance.mls.LogInfo("Spawned network handler");
                }
            }
            catch
            {
                Plugin.Instance.mls.LogError("Failed to spawned network handler");
            }
        }
        
        static GameObject networkPrefab;
    }
}