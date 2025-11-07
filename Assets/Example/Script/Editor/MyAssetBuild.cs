/*********************************************************************************
 *Author:         OnClick
 *Version:        0.0.2.241
 *UnityVersion:   2019.4.36f1c1
 *Date:           2022-03-14
 *Description:    IFramework
 *History:        2018.11--
*********************************************************************************/

namespace WooAsset
{
    public class MyAssetBuild : IAssetsBuild
    {
        protected override AssetType CoverAssetType(string path, AssetType type)
        {
            if (type== AssetType.VideoClip)
            {
                return AssetType.Raw;
            }
            return base.CoverAssetType(path, type);
        }
    }
}
