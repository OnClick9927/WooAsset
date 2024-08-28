using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System.Threading.Tasks;

namespace WooAsset
{
    public partial class AssetsEditorTool
    {
        private class SpriteAtlasTool
        {

            static void BuildAtlas(string directory, List<string> sprites)
            {
                List<Texture> textures = sprites.SelectMany(x => AssetDatabase.LoadAllAssetsAtPath(x)
                .Select(x => x as Texture).ToList()).ToList();
                textures.RemoveAll(x => x == null);
                textures.RemoveAll(x => x is RenderTexture);
                if (textures.Count == 0) return;

                string file_path = $"{directory}.spriteatlas";
                SpriteAtlas asset = new SpriteAtlas();

                asset.SetPlatformSettings(option.PlatformSetting);
                asset.SetTextureSettings(option.GetTextureSetting());
                asset.SetPackingSettings(option.GetPackingSetting());
                AssetDatabase.CreateAsset(asset, file_path);
                asset.Add(textures.ToArray());
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();

            }
            public static async Task Execute()
            {
                if (EditorSettings.spritePackerMode == SpritePackerMode.Disabled)
                {
                    AssetsHelper.LogError("SpritePackerMode is Disabled");
                    return;
                }
                var atlasPaths = option.atlasPaths.ToArray();
                AssetDatabase.FindAssets("t:SpriteAtlas", atlasPaths)
                        .Select(x => AssetDatabase.GUIDToAssetPath(x))
                        .ToList()
                        .ForEach(x =>
                        {
                            AssetDatabase.DeleteAsset(x);
                        });
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                await Task.Delay(1000);
                var data = AssetDatabase.FindAssets("t:Texture", atlasPaths)
                    .Select(x => AssetDatabase.GUIDToAssetPath(x))
                    .Select(x => new { dir = AssetsHelper.GetDirectoryName(x), path = x })
                    .GroupBy(x => x.dir).ToDictionary(x => x.Key, x => x.Select(y => y.path).ToList());
                foreach (var item in data)
                    BuildAtlas(item.Key, item.Value);
                AssetDatabase.Refresh();
                SpriteAtlasUtility.PackAllAtlases(BuildTarget, false);
                await Task.Delay(1000);
                AssetsHelper.Log("build atlas succeed");
            }
        }

    }
}
