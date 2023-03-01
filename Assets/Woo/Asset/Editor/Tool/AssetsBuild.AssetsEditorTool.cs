
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WooAsset
{
    public enum AnchorType
    {
        UpperLeft = 0,
        UpperCenter = 1,
        UpperRight = 2,
        MiddleLeft = 3,
        MiddleCenter = 4,
        MiddleRight = 5,
        LowerLeft = 6,
        LowerCenter = 7,
        LowerRight = 8
    }
    public enum SplitType
    {
        Vertical, Horizontal
    }
    static class AssetsEditorTool
    {

        private class Life : AssetsInternal.IAssetLife<Bundle>, AssetsInternal.IAssetLife<Asset>
        {
            public void OnAssetCreate(string path, Bundle asset)
            {
                bundles.Add(path, new AssetLife<Bundle>()
                {
                    asset = asset,
                    time = System.DateTime.Now,
                    path = path,
                    count = 0
                });
                onAssetLifChange?.Invoke();
            }
            public void OnAssetRetain(Bundle asset, int count)
            {
                bundles[asset.path].count = count;
                onAssetLifChange?.Invoke();

            }
            public void OnAssetRelease(Bundle asset, int count)
            {
                bundles[asset.path].count = count;
                onAssetLifChange?.Invoke();

            }
            public void OnAssetUnload(string path, Bundle asset)
            {
                bundles.Remove(path);
                onAssetLifChange?.Invoke();

            }



            public void OnAssetCreate(string path, Asset asset)
            {
                assets.Add(path, new AssetLife<Asset>()
                {
                    asset = asset,
                    time = System.DateTime.Now,
                    path = path,
                    count = 0,
                    tag = AssetsInternal.GetAssetTag(path)
                });
                onAssetLifChange?.Invoke();

            }


            public void OnAssetRelease(Asset asset, int count)
            {
                assets[asset.path].count = count;
                onAssetLifChange?.Invoke();

            }



            public void OnAssetRetain(Asset asset, int count)
            {
                if (assets.ContainsKey(asset.path))
                {
                    assets[asset.path].count = count;
                    onAssetLifChange?.Invoke();
                }

            }



            public void OnAssetUnload(string path, Asset asset)
            {
                assets.Remove(path);
                onAssetLifChange?.Invoke();

            }
        }

        private static Life ins = new Life();

        [InitializeOnLoadMethod]
        public static void ChangeLoadMode()
        {
            if (AssetsToolSetting.Load<AssetsToolSetting>().fastMode)
                AssetsInternal.mode = new AssetsBuild.FastAssetMode();
            AssetsInternal.localSaveDir = AssetsBuild.outputPath;
            AssetsInternal.SetAssetListen(ins, ins);
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }
        public static event Action onAssetLifChange;
        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    bundles.Clear();
                    assets.Clear();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }

        public class AssetLife<T> where T : IAsset
        {
            public T asset;
            public System.DateTime time;
            public string path;
            public int count;
            public string tag;
        }
        public static Dictionary<string, AssetLife<Bundle>> bundles = new Dictionary<string, AssetLife<Bundle>>();
        public static Dictionary<string, AssetLife<Asset>> assets = new Dictionary<string, AssetLife<Asset>>();

        public static T CreateScriptableObject<T>(string savePath) where T : ScriptableObject
        {
            ScriptableObject sto = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(sto, savePath);
            EditorUtility.SetDirty(sto);
            AssetDatabase.ImportAsset(savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<T>(savePath);
        }
        public static T Load<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        public static void Update<T>(T t) where T : Object
        {
            EditorApplication.delayCall += delegate ()
            {
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            };
        }
        public static string ToAbsPath(this string self)
        {
            string assetRootPath = Path.GetFullPath(Application.dataPath);
            assetRootPath = assetRootPath.Substring(0, assetRootPath.Length - 6) + self;
            return AssetsInternal.ToRegularPath(assetRootPath);
        }
        public static string ToAssetsPath(this string self)
        {
            string assetRootPath = Path.GetFullPath(Application.dataPath);
            return "Assets" + Path.GetFullPath(self).Substring(assetRootPath.Length).Replace("\\", "/");
        }
        public static bool IsDirectory(this string path)
        {
            return Directory.Exists(path);
        }
        public static IEnumerable<Type> GetSubTypesInAssemblies(this Type self)
        {
            if (self.IsInterface)
            {
                return from item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((item) => item.GetTypes())
                       where item.GetInterfaces().Contains(self)
                       select item;
            }

            return from item in AppDomain.CurrentDomain.GetAssemblies().SelectMany((item) => item.GetTypes())
                   where item.IsSubclassOf(self)
                   select item;
        }


        public static Rect LocalPosition(this EditorWindow self)
        {
            return new Rect(Vector2.zero, self.position.size);
        }

        public static Rect Zoom(this Rect rect, AnchorType type, float pixel)
        {
            return Zoom(rect, type, new Vector2(pixel, pixel));
        }
        public static Rect Zoom(this Rect rect, AnchorType type, Vector2 pixelOffset)
        {
            float tempW = rect.width + pixelOffset.x;
            float tempH = rect.height + pixelOffset.y;
            switch (type)
            {
                case AnchorType.UpperLeft:
                    break;
                case AnchorType.UpperCenter:
                    rect.x -= (tempW - rect.width) / 2;
                    break;
                case AnchorType.UpperRight:
                    rect.x -= tempW - rect.width;
                    break;
                case AnchorType.MiddleLeft:
                    rect.y -= (tempH - rect.height) / 2;
                    break;
                case AnchorType.MiddleCenter:
                    rect.x -= (tempW - rect.width) / 2;
                    rect.y -= (tempH - rect.height) / 2;
                    break;
                case AnchorType.MiddleRight:
                    rect.y -= (tempH - rect.height) / 2;
                    rect.x -= tempW - rect.width;
                    break;
                case AnchorType.LowerLeft:
                    rect.y -= tempH - rect.height;
                    break;
                case AnchorType.LowerCenter:
                    rect.y -= tempH - rect.height;
                    rect.x -= (tempW - rect.width) / 2;
                    break;
                case AnchorType.LowerRight:
                    rect.y -= tempH - rect.height;
                    rect.x -= tempW - rect.width;
                    break;
            }
            rect.width = tempW;
            rect.height = tempH;
            return rect;
        }



        public static Rect CutBottom(this Rect r, float pixels)
        {
            r.yMax -= pixels;
            return r;
        }
        public static Rect CutTop(this Rect r, float pixels)
        {
            r.yMin += pixels;
            return r;
        }
        public static Rect CutRight(this Rect r, float pixels)
        {
            r.xMax -= pixels;
            return r;
        }
        public static Rect CutLeft(this Rect r, float pixels)
        {
            r.xMin += pixels;
            return r;
        }
        public static Rect Cut(this Rect r, float pixels)
        {
            return r.Margin(-pixels);
        }
        public static Rect Margin(this Rect r, float pixels)
        {
            r.xMax += pixels;
            r.xMin -= pixels;
            r.yMax += pixels;
            r.yMin -= pixels;
            return r;
        }

        public static Rect[] Split(this Rect r, SplitType type, float offset, float padding = 0, bool justMid = true)
        {
            switch (type)
            {
                case SplitType.Vertical:
                    return r.VerticalSplit(offset, padding, justMid);
                case SplitType.Horizontal:
                    return r.HorizontalSplit(offset, padding, justMid);
                default:
                    return default(Rect[]);
            }
        }
        public static Rect SplitRect(this Rect r, SplitType type, float offset, float padding = 0)
        {
            switch (type)
            {
                case SplitType.Vertical:
                    return r.VerticalSplitRect(offset, padding);
                case SplitType.Horizontal:
                    return r.HorizontalSplitRect(offset, padding);
                default:
                    return default(Rect);
            }
        }
        public static Rect[] VerticalSplit(this Rect r, float width, float padding = 0, bool justMid = true)
        {
            if (justMid)
                return new Rect[2]{
                r.CutRight((int)(r.width-width)).CutRight(padding).CutRight(-Mathf.CeilToInt(padding/2f)),
                r.CutLeft(width).CutLeft(padding).CutLeft(-Mathf.FloorToInt(padding/2f))
            };
            return new Rect[2]{
                r.CutRight((int)(r.width-width)).Cut(padding).CutRight(-Mathf.CeilToInt(padding/2f)),
                r.CutLeft(width).Cut(padding).CutLeft(-Mathf.FloorToInt(padding/2f))
            };
        }
        public static Rect[] HorizontalSplit(this Rect r, float height, float padding = 0, bool justMid = true)
        {
            if (justMid)
                return new Rect[2]{
                r.CutBottom((int)(r.height-height)).CutBottom(padding).CutBottom(-Mathf.CeilToInt(padding/2f)),
                r.CutTop(height).CutTop(padding).CutTop(-Mathf.FloorToInt(padding/2f))
                };
            return new Rect[2]{
                r.CutBottom((int)(r.height-height)).Cut(padding).CutBottom(-Mathf.CeilToInt(padding/2f)),
                r.CutTop(height).Cut(padding).CutTop(-Mathf.FloorToInt(padding/2f))
            };
        }
        public static Rect HorizontalSplitRect(this Rect r, float height, float padding = 0)
        {
            Rect rect = r.CutBottom((int)(r.height - height)).Cut(padding).CutBottom(-Mathf.CeilToInt(padding / 2f));
            rect.y += rect.height;
            rect.height = padding;
            return rect;
        }
        public static Rect VerticalSplitRect(this Rect r, float width, float padding = 0)
        {
            Rect rect = r.CutRight((int)(r.width - width)).Cut(padding).CutRight(-Mathf.CeilToInt(padding / 2f));
            rect.x += rect.width;
            rect.width = padding;
            return rect;
        }


        public static string editorPath
        {
            get
            {
                return AssetsInternal.CombinePath(GetFilePath(), "../../").ToAssetsPath();

            }
        }
        private static string GetFilePath([CallerFilePath] string path = "")
        {
            return path;
        }

    }


}
