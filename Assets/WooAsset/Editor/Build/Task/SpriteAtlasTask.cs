using System.Collections.Generic;
using System.IO;
using UnityEditor.U2D;
using UnityEditor;
using UnityEngine.U2D;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using File = System.IO.File;

namespace WooAsset
{
    public class SpriteAtlasTask : AssetTask
    {
        static void BuildAtlas(AssetTaskContext context, string directory, List<string> sprites)
        {
            List<Texture> textures = sprites.SelectMany(x => AssetDatabase.LoadAllAssetsAtPath(x).Select(x => x as Texture).ToList()).ToList();
            textures.RemoveAll(x => x == null);


            string file_path = $"{directory}.spriteatlas";
            SpriteAtlas asset = new SpriteAtlas();

            asset.SetPlatformSettings(context.PlatformSetting);
            asset.SetTextureSettings(context.TextureSetting);
            asset.SetPackingSettings(context.PackingSetting);
            AssetDatabase.CreateAsset(asset, file_path);
            asset.Add(textures.ToArray());
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

        }
        protected async override void OnExecute(AssetTaskContext context)
        {
            if (EditorSettings.spritePackerMode == SpritePackerMode.Disabled)
            {
                SetErr("SpritePackerMode is Disabled");
                InvokeComplete();
                return;
            }

            var data = AssetDatabase.FindAssets("t:Texture", context.atlasPaths)
                .ToList()
                .ConvertAll(x => AssetDatabase.GUIDToAssetPath(x))
                .ConvertAll(x => new { dir = Path.GetDirectoryName(x), path = x })
                .GroupBy(x => x.dir).ToDictionary(x => x.Key, x => x.Select(y => y.path).ToList());
            foreach (var item in data)
            {
                string file_path = $"{item.Key}.spriteatlas";
                if (File.Exists(file_path))
                    AssetDatabase.DeleteAsset(file_path);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            await Task.Delay(1000);
            foreach (var item in data)
            {
                BuildAtlas(context, item.Key, item.Value);
            }
            AssetDatabase.Refresh();
            SpriteAtlasUtility.PackAllAtlases(context.buildTarget, false);
            await Task.Delay(1000);
            InvokeComplete();
        }
    }
}
