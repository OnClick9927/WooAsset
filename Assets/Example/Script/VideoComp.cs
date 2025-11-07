using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.Video;
using WooAsset;

public class VideoComp : MonoBehaviour
{
    private DisplayUGUI displayUGUI;
    private MediaPlayer mediaPlayer;
    public bool loop;
    public AssetReference<VideoClip> clip = new AssetReference<VideoClip>();
    private void Awake()
    {
        displayUGUI = GetComponent<DisplayUGUI>();
        mediaPlayer = GetComponent<MediaPlayer>();
        mediaPlayer.Loop = loop;
        mediaPlayer.MediaPath.PathType = MediaPathType.AbsolutePathOrURL;
        string _path = clip.path;
        _path = Assets.GetRawAssetUrlOrPath(_path);
        mediaPlayer.MediaPath.Path = _path;
        mediaPlayer.Play();
    }
}
