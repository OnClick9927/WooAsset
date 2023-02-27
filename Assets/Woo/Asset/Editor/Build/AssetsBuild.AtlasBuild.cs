using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Linq;
using System.Threading.Tasks;

namespace WooAsset
{

    public partial class AssetsBuild
    {
        public static class AtlasBuild
        {
            public async static Task Run()
            {
                var _paths = tool.atlasPaths;
                List<string> paths = new List<string>();
                foreach (var path in _paths)
                {
                    paths.Add(path);
                    var sub = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                    paths.AddRange(sub);
                }
                paths = paths.Distinct().ToList();
                foreach (var path in paths)
                {
                    Delete(path);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                await Task.Delay(1000);
                foreach (var path in paths)
                {
                    BuildAtlas(path);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            static List<Texture> FindTex(string directory)
            {
                var files = Directory.GetFiles(directory);
                List<Texture> texs = new List<Texture>();
                foreach (var item in files)
                {
                    var load = AssetDatabase.LoadAssetAtPath<Texture>(item);
                    if (load)
                    {
                        texs.Add(load);
                    }
                }
                return texs;
            }
            static void Delete(string directory)
            {
                string file_path = $"{directory}.spriteatlas";
                if (File.Exists(file_path))
                    AssetDatabase.DeleteAsset(file_path);
            }
            static void BuildAtlas(string directory)
            {
                List<Texture> texs = FindTex(directory);
                string file_path = $"{directory}.spriteatlas";
                if (texs.Count <= 0) return;
                SpriteAtlas atlas = new SpriteAtlas();
                atlas.SetPlatformSettings(tool.PlatformSetting);
                atlas.SetTextureSettings(tool.GetTextureSetting());
                atlas.SetPackingSettings(tool.GetPackingSetting());
                AssetDatabase.CreateAsset(atlas, file_path);
                atlas.Add(texs.ToArray());
                EditorUtility.SetDirty(atlas);

            }
        }
    }
}
