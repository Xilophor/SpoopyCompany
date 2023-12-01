using System;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpoopyComponents
{
    public class NetworkHandler : NetworkBehaviour
    {
        /*private void Awake()
        {
            if (NetworkHandler.Instance == null)
            {
                NetworkHandler.Instance = this;
                return;
            }
            Object.Destroy(NetworkHandler.Instance.gameObject);
            NetworkHandler.Instance = this;
        }*/

        public override void OnNetworkSpawn()
        {
            // if (!IsServer || !IsHost && IsOwner) {
            // }
            Debug.Log("SpoopyCompany - NetworkHandler created");
            OnEvent = null;
        }

        [ClientRpc]
        public void EventClientRPC(string eventType)
        {
            if(OnEvent == null) return;
            OnEvent(eventType);
            Debug.Log("SpoopyCompany - Fired Event: " + eventType + " with " + OnEvent.GetInvocationList().Length + " listener(s)");
        }

        public static event Action<string> OnEvent;
	    //public static NetworkHandler Instance { get; private set; }
    }
}