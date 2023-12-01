using System;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpoopyComponents
{
    public class NetworkHandler : NetworkBehaviour
    {

        public override void OnNetworkSpawn()
        {
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
    }
}