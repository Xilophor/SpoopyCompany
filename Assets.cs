using System.Reflection;
using UnityEngine;

//Thanks to @upsidedowncatfish (Bunya Pine Tree)

namespace SpoopyCompany
{
    public static class Assets
    {
        public static AssetBundle MainAssetBundle = null;

        public static void PopulateAssets(string streamName)
        {
            if (MainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(streamName))
                {
                    MainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }
        }
    }
}