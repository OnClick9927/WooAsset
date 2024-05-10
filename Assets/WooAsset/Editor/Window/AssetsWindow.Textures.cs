using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class Textures
        {
            private static Texture _folder;

            public static Texture folder
            {
                get
                {
                    if (_folder == null)
                        _folder = EditorGUIUtility.TrIconContent("Folder Icon").image;
                    return _folder;
                }
            }
            private static Texture _err;

            public static Texture err
            {
                get {
                    if (_err == null)
                        _err = EditorGUIUtility.TrIconContent("d_console.erroricon.sml").image;
                    return _err;
                }
            }
            private static Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
            public static Texture GetMiniThumbnail(string path)
            {
                if (AssetsHelper.IsDirectory(path))
                    return folder;
                Texture tx;
                _textures.TryGetValue(path, out tx);
                if (!tx)
                {
                    tx = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<Object>(path));
                    _textures[path] = tx;
                }
                return tx;
            }
        }
    }
}
