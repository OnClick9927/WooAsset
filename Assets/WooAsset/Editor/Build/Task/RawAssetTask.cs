using System.IO;
using System.Linq;
using UnityEditor;

namespace WooAsset
{
    public class RawAssetTask : AssetTask
    {
        protected override void OnExecute(AssetTaskContext context)
        {
            var builds = context.buildGroups;
            if (builds.Count == 0)
            {
                InvokeComplete();
                return;
            }
            var rawPaths = AssetDatabase.FindAssets("t:WooAsset.RawObject", builds.ConvertAll(x => x.path).ToArray())
                       .ToList()
                       .ConvertAll(x => AssetDatabase.GUIDToAssetPath(x));
            for (int i = 0; i < rawPaths.Count; i++)
            {
                var raw = AssetsEditorTool.Load<RawObject>(rawPaths[i]);
                if (raw == null) continue;
                if (!File.Exists(raw.rawPath))
                {
                    AssetDatabase.DeleteAsset(rawPaths[i]);
                    AssetDatabase.Refresh();
                }

            }
            var paths = AssetDatabase.FindAssets("t:DefaultAsset", builds.ConvertAll(x => x.path).ToArray())
                       .ToList()
                       .ConvertAll(x => AssetDatabase.GUIDToAssetPath(x));
            for (int i = 0; i < paths.Count; i++)
            {
                string path = paths[i];
                var type = context.assetBuild.GetAssetType(path);
                if (type != AssetType.Raw) continue;
                string savePath = AssetsInternal.RawToRawObjectPath(path);
                var source = File.ReadAllBytes(path);
                bool create = false;
                if (File.Exists(savePath))
                {
                    var raw = AssetsEditorTool.Load<RawObject>(savePath);
                    if (raw != null && source.Length == raw.bytes.Length)
                    {
                        for (int j = 0; j < source.Length; j++)
                        {
                            if (source[j] != raw.bytes[j])
                            {
                                create = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        create = true;
                    }
                    if (create)
                    {
                        AssetDatabase.DeleteAsset(savePath);
                        AssetDatabase.Refresh();
                    }
                }
                else
                {
                    create = true;
                }
                if (create)
                {
                    RawObject sto = AssetsEditorTool.CreateScriptableObject<RawObject>(savePath);
                    sto.bytes = source;
                    sto.rawPath = path;
                    AssetsEditorTool.Update(sto);
                }
            }
            InvokeComplete();
        }
    }
}
