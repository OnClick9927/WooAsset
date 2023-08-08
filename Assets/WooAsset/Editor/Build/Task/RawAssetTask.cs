using System.Linq;
using UnityEditor;

namespace WooAsset
{
    public class RawAssetTask : AssetTask
    {
        protected async override void OnExecute(AssetTaskContext context)
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
                if (!AssetsHelper.ExistsFile(raw.rawPath))
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
                if (type != AssetType.Raw && type != AssetType.RawCopyFile) continue;
                string objPath = AssetsInternal.RawToRawObjectPath(path);

                bool create = false;
                string hash = AssetsHelper.GetFileHash(path);
                if (AssetsHelper.ExistsFile(objPath))
                {
                    var raw = AssetsEditorTool.Load<RawObject>(objPath);
                    if (raw == null || raw.hash != hash)
                    {
                        create = true;
                        AssetDatabase.DeleteAsset(objPath);
                        AssetDatabase.Refresh();
                    }
                }
                else
                {
                    create = true;
                }
                if (create)
                {
                    RawObject sto = AssetsEditorTool.CreateScriptableObject<RawObject>(objPath);
                    var reader = await AssetsHelper.ReadFile(path, true);
                    sto.bytes = reader.bytes;
                    sto.rawPath = path;
                    sto.hash = hash;
                    AssetsEditorTool.Update(sto);
                }
            }
            InvokeComplete();
        }
    }
}
