namespace WooAsset
{
    [System.Serializable]
    public enum AssetType
    {
        None = 0,
        Ignore = 1,
        Directory = 2,

        AnimatorController = 13,
        Raw = 16,

        Sprite = 3,
        Texture = 4,

        VideoClip = 5,
        Scene = 6,
        Material = 7,
        Mesh = 8,
        GameObject = 9,
        Font = 10,

        AnimationClip = 12,

        ScriptObject = 14,
        TextAsset = 15,
        PhysicMaterial = 17,

        GUISkin = 18,

        AudioMixer = 19,
        AudioClip = 20,


        Shader = 21,
        ShaderVariant = 22,
        ComputeShader = 23,
    }
}
