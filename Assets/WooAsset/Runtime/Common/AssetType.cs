namespace WooAsset
{
    [System.Serializable]
    public enum AssetType
    {
        None,
        Ignore,
        Directory,
        Sprite,
        Texture,

        VideoClip,
        Scene,
        Material,
        Mesh,
        GameObject,
        Font,

        Animation,
        AnimationClip,
        AnimatorController,

        ScriptObject,
        TextAsset,
        Raw,
        PhysicMaterial,

        GUISkin,

        AudioMixer,
        AudioClip,


        Shader,
        ShaderVariant,
        ComputeShader,
    }
}
