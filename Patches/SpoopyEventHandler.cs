using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using SpoopyCompany;
using SpoopyComponents;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;



namespace SpoopyCompany.Patches
{
    [HarmonyPatch]
    public class SpoopyEventHandler
    {
        public class SpoopyCoroutine : MonoBehaviour { }
        //Variable reference for the class
        private static SpoopyCoroutine evntC;

        private static void Init()
        {
            //If the instance not exit the first time we call the static class
            if (evntC == null)
            {
                //Create an empty object called MyStatic
                GameObject gameObject = new GameObject("SpoopyCoroutine");


                //Add this script to the object
                evntC = gameObject.AddComponent<SpoopyCoroutine>();
            }
        }

        public static void ReceivedEventsFromServer(string eventInfo) {
            Init();

            mls.LogInfo("Starting event " + eventInfo);

            switch (eventInfo)
            {
                case "FlickerLights":
                    evntC.StartCoroutine(SpoopyEvents.FlickerLights(RoundManager.Instance));
                    break;
                case "PowerOutage":
                    evntC.StartCoroutine(SpoopyEvents.PowerOutage(RoundManager.Instance));
                    break;
                case "PowerSurge":
                    evntC.StartCoroutine(SpoopyEvents.PowerSurge(RoundManager.Instance, StartOfRound.Instance.randomMapSeed + 4));
                    break;
                case "BurstPipes":
                    evntC.StartCoroutine(SpoopyEvents.BurstPipes(RoundManager.Instance));
                    break;
            }
        }


        [HarmonyPatch(typeof(RoundManager), "Update")]
        [HarmonyPostfix]
        private static void UpdateEvents(RoundManager __instance)
        {
            if (!NetworkManager.Singleton.IsServer || !NetworkManager.Singleton.IsHost)
                return;
            if (!__instance.dungeonFinishedGeneratingForAllPlayers)
                return;
            if (EventTimes.Count > currentEventIndex && __instance.timeScript.currentDayTime > (float)occuranceTimes[currentEventIndex] && (__instance.IsHost || __instance.IsServer))
            {
                mls.LogInfo("Sending Event to Clients");
                string currentEvent = EventTimes[occuranceTimes[currentEventIndex]];

                if ((currentEvent == "FlickerLights" || currentEvent == "PowerOutage") && (Object.FindObjectOfType<BreakerBox>()?.isPowerOn == false || __instance.powerOffPermanently))
                {
			        currentEventIndex++;
                    return;
                }

                networkHandler.EventClientRPC(currentEvent);

			    currentEventIndex++;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundManager), "GenerateNewLevelClientRpc")]
        static void ResetVariables()
        {
            mls.LogInfo("Resetting Event variables");

            hasFlickered = false;
            surgeEventOccured = false;
            eventRandom = new(StartOfRound.Instance.randomMapSeed + 4);
            eventHandlerRandom = new(StartOfRound.Instance.randomMapSeed + 5);
            occuranceTimes = new List<int>();
            EventTimes = new Dictionary<int,string>();

            if (networkHandler == null) networkHandler = GameObject.FindObjectOfType<NetworkHandler>();

            NetworkHandler.OnEvent += ReceivedEventsFromServer;
            mls.LogInfo("Added listener to NetworkHandler");
        }

        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPostfix]
        static void UnsubscribeFromEvent()
        {
            NetworkHandler.OnEvent -= ReceivedEventsFromServer;
            mls.LogInfo("Unsubscribed from server event");
        }

        /*
            Add Event Checks to Semi-Hourly updates
        */
        [HarmonyPatch(typeof(RoundManager), "AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPostfix]
        private static void PlanEvents(RoundManager __instance, int ___currentHour)
        {
            if (!__instance.IsServer) return;

            if (currentEventIndex < occuranceTimes.Count)
            {
                foreach (var evnt in EventTimes)
                {
                    if (evnt.Key < occuranceTimes[currentEventIndex])
                        EventTimes.Remove(evnt.Key);
                    else
                        break;
                }
                occuranceTimes.RemoveRange(0,currentEventIndex);

                currentEventIndex = 0;
            } else {
                occuranceTimes.Clear();
                EventTimes.Clear();
                currentEventIndex = 0;
            }

            mls.LogInfo("Planning Events");

            float timeOffset = __instance.timeScript.lengthOfHours * (float)___currentHour;

            foreach (var evnt in eventChances)
            {
                if(eventHandlerRandom.NextDouble() < evnt.Value)
                {
                    int minAmount = 1;
                    int maxAmount = 1;
                    string type = "None";

                    switch (evnt.Key)
                    {
                        case "FlickerLights":
                            if(surgeEventOccured) continue; //! Ensure Power Outage doesn't happen multiple times in a row, increase odds of explosion
                            

                            if (eventHandlerRandom.NextDouble() < 0.0117f && hasFlickered)
                            {
                                type = "PowerSurge";
                                surgeEventOccured = true;
                            } else if (eventHandlerRandom.NextDouble() < 0.09f && hasFlickered) {
                                type = "PowerOutage";
                            } else {
                                maxAmount = 3;
                                type = "FlickerLights";
                            }
                            break;
                    }

                    if (type == "None") continue;

                    int i;
                    int attempts = 0;

                    List<int> chosenTimes = new();
                    
                    for (i = 0; i < eventHandlerRandom.Next(minAmount,maxAmount); i++)
                    {
                        if (attempts > 4) break; //* For Compatability - Incase another mod changes lengthOfHours to be significantly shorter
                        int timeToOccur = eventHandlerRandom.Next((int)(5f+timeOffset),(int)(__instance.timeScript.lengthOfHours * (float)__instance.hourTimeBetweenEnemySpawnBatches + timeOffset)-20);
                        foreach (var time in chosenTimes)
                        {
                            if (time - 6 < timeToOccur || timeToOccur < time + 6)
                            {
                                i--;
                                attempts++;
                                continue;
                            }
                        }
                        chosenTimes.Add(timeToOccur);
                        EventTimes.Add(timeToOccur, type);
                        occuranceTimes.Add(timeToOccur);
                        mls.LogInfo("Event " + evnt.Key + " chosen to happen at: " + timeToOccur +", hours are "+__instance.timeScript.lengthOfHours+" long");
                    }
                    occuranceTimes.Sort();

                    mls.LogInfo("Event " + evnt.Key + " chosen to happen " + i + " times");
                }
            }
        }


        static readonly ManualLogSource mls = Plugin.Instance.mls;

        public static Random eventHandlerRandom;
        public static Random eventRandom;
        public static bool hasFlickered = false;
        public static bool surgeEventOccured = false;

        static int currentEventIndex;

        public static List<int> occuranceTimes;
        public static Dictionary<int, string> EventTimes { get; set; }

        static NetworkHandler networkHandler;

        // EventTypes = [ "None", "FlickerLights", "PowerOutage", "BurstPipes" ];
        static readonly Dictionary<string, float> eventChances = new() {
            {"FlickerLights", 0.18f}
            }; // Chance out of 1
    }
} // TODO: add check for lights & add outdoor lights to flicker