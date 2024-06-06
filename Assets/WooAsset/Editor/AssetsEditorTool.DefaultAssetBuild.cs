using System;

namespace WooAsset
{

    partial class AssetsEditorTool
    {
        public class DefaultAssetBuild : IAssetBuild
        {
            public override string GetVersion(string settingVersion, AssetTaskContext context)
            {
                return DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            }
      
        }



    }

}
