using System.Collections;
using System.Threading;
using BepInEx.Logging;
using SpoopyCompany.Patches;
using UnityEngine;

namespace SpoopyCompany
{
    public class SpoopyEvents
    {
        public static IEnumerator FlickerLights(RoundManager __instance)
        {
            mls.LogInfo("Flickering Lights at "+ __instance.timeScript.currentDayTime);
            __instance.FlickerLights();
            SpoopyEventHandler.hasFlickered = true;
            yield break;
        }

        public static IEnumerator PowerSurge(RoundManager __instance, int eventSeed)
        {
            System.Random eventRandom = new(eventSeed); // Requires matchup of host and clients, to ensure (due to coroutines) I have to create a new Random
            mls.LogInfo("Damaging Apparatice");
            
            __instance.FlickerLights(false, false);
            yield return new WaitForSeconds((float)eventRandom.Next(10,30)/10);
            __instance.FlickerLights(false, false);
            yield return new WaitForSeconds((float)eventRandom.Next(24,56)/10);
            __instance.FlickerLights(false, false);
            yield return new WaitForSeconds((float)eventRandom.Next(15,25)/10);
            __instance.FlickerLights(false, false);
            yield return new WaitForSeconds(2.5f);
            __instance.SwitchPower(false);
            __instance.powerOffPermanently = true;

            LungProp apparatice = null;

            foreach (var prop in Object.FindObjectsOfType<LungProp>())
            {
                if (prop.isLungDocked)
                {
                    apparatice = prop;
                    break;
                }
            }

            if (apparatice == null)
                yield break;

            Landmine landmine = Resources.FindObjectsOfTypeAll<Landmine>()[0];
            AudioSource explosionAudio = apparatice.gameObject.GetComponent<AudioSource>();
            explosionAudio.Stop();
            explosionAudio.PlayOneShot(landmine.mineDetonate, 1.1f); //? Plays a little louder than max, is it too much?
            Landmine.SpawnExplosion(apparatice.transform.position, true, 8.6f, 15.2f);
            // TODO: Increase explosion size, change to custom explosion method

            apparatice.SetScrapValue(SpoopyEventHandler.eventRandom.Next(8,28)); //* Clients should match due to using same Random
            apparatice.gameObject.transform.GetChild(3).gameObject.SetActive(false); // Disable apparatice light
            apparatice.lungDeviceMesh.materials[1].CopyMatchingPropertiesFromMaterial(apparatice.lungDeviceMesh.materials[0]); // don't understand materials, but it just works
            // TODO: Ensure destroy apparatice is still visibly destroyed when save is reloaded - replace with custom model?
        }
        
        public static IEnumerator PowerOutage(RoundManager __instance)
        {
            mls.LogInfo("Shorting out Lights");

            BreakerBox breakerBox = Object.FindObjectOfType<BreakerBox>();
            
            __instance.FlickerLights(false, false);
            yield return new WaitForSeconds((float)SpoopyEventHandler.eventRandom.Next(12,56)/10);
            __instance.FlickerLights(false, false);
            yield return new WaitForSeconds(1.5f);
            breakerBox?.SwitchBreaker(false);
            yield break;
        }
        
        public static IEnumerator BurstPipes(RoundManager __instance)
        {
            mls.LogInfo("Bursting Pipes at "+ __instance.timeScript.currentDayTime);
            yield break;
        }

        static readonly ManualLogSource mls = Plugin.Instance.mls;
    }
}