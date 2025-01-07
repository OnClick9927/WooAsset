using UnityEditor.IMGUI.Controls;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private class TreeColumns
        {
            public static MultiColumnHeaderState.Column emptyTitle = new MultiColumnHeaderState.Column()
            {
                width = 300,
                minWidth = 300,
                allowToggleVisibility = false,

            };

            public static MultiColumnHeaderState.Column usage = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Usage"),
                width = 300,

                minWidth = 300,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column dependence = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Dependence"),
                width = 300,

                minWidth = 300,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column bundle = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Bundle"),
                maxWidth = 280,
                minWidth = 280,
                allowToggleVisibility = false,
            };
            public static MultiColumnHeaderState.Column bundleSize = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Bundle Size"),
                maxWidth = 100,
                minWidth = 100,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column size = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Size"),
                maxWidth = 100,
                minWidth = 100,
                allowToggleVisibility = false,

            };

            public static MultiColumnHeaderState.Column tag = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Tag"),
                width = 200,
                minWidth = 150,
                allowToggleVisibility = false,
                autoResize = true,
            };
            public static MultiColumnHeaderState.Column type = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Type"),
                maxWidth = 100,
                minWidth = 100,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column reference = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Ref"),
                minWidth = 100,
                maxWidth = 100,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column loadTime = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Load Time"),
                maxWidth = 150,
                minWidth = 150,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column record = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Record"),
                minWidth = 50,
                maxWidth = 50,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column loopDependence = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Loop"),
                minWidth = 40,
                maxWidth = 40,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column Raw = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Raw"),
                minWidth = 40,
                maxWidth = 40,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column usageCount = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Usage"),
                minWidth = 40,
                maxWidth = 40,
                allowToggleVisibility = false,

            };
            public static MultiColumnHeaderState.Column dependenceCount = new MultiColumnHeaderState.Column()
            {
                headerContent = new UnityEngine.GUIContent("Dependence"),
                minWidth = 60,
                maxWidth = 60,
                allowToggleVisibility = false,

            };
        }

    }
}
