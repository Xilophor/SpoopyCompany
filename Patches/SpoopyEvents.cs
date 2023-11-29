using System.Collections;
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
        
        public static IEnumerator PowerOutage(RoundManager __instance)
        {
            mls.LogInfo("Shorting out Lights");

            if (SpoopyEventHandler.eventRandom.NextDouble() < 1)//0.08)
            {
                mls.LogInfo("Damaging Apparatice");
                __instance.FlickerLights(false, false);
                yield return new WaitForSeconds((float)SpoopyEventHandler.eventRandom.Next(10,30)/10);
                __instance.FlickerLights(false, false);
                yield return new WaitForSeconds((float)SpoopyEventHandler.eventRandom.Next(24,89)/10);
                __instance.FlickerLights(false, false);
                yield return new WaitForSeconds((float)SpoopyEventHandler.eventRandom.Next(15,25)/10);
                __instance.FlickerLights(false, false);
		        yield return new WaitForSeconds(2.5f);
                __instance.SwitchPower(false);
                __instance.powerOffPermanently = true;
                SpoopyEventHandler.outageEvent = true;

		        LungProp apparatice = Object.FindObjectOfType<LungProp>();
		        Landmine landmine = Resources.FindObjectsOfTypeAll<Landmine>()[0];
                if (apparatice == null)
                    yield break;
                AudioSource explosionAudio = apparatice.gameObject.GetComponent<AudioSource>();
                explosionAudio.Stop();
                explosionAudio.PlayOneShot(landmine.mineDetonate, 1.1f); //? Plays a little louder than max, tis it too much?
                Landmine.SpawnExplosion(apparatice.transform.position, true, 4.6f, 8.4f);
                apparatice.SetScrapValue(SpoopyEventHandler.eventRandom.Next(8,28)); //* Clients should match due to using same Random
            }
            else
            {
		        BreakerBox breakerBox = Object.FindObjectOfType<BreakerBox>();
                if (breakerBox?.isPowerOn == false)
                    yield break;
                
                __instance.FlickerLights(false, false);
                yield return new WaitForSeconds((float)SpoopyEventHandler.eventRandom.Next(12,88)/10);
                __instance.FlickerLights(false, false);
                yield return new WaitForSeconds(1.5f);
                breakerBox.SwitchBreaker(false);
            }
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