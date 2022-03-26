using UnityEngine;
using UnityEngine.UI;
using UMP;

public class UMPTextureUpdator : MonoBehaviour
{
    [SerializeField]
    private RawImage _image;

    [SerializeField]
    private UniversalMediaPlayer _player;

    private Texture2D _texture;
    private long _framesCounter;

    void Start () {
        _player.AddPreparedEvent(OnPrepared);
        _player.AddStoppedEvent(OnStop);
	}
	
	void Update () {
        if (_texture != null && _framesCounter != _player.FramesCounter)
        {
            _texture.LoadRawTextureData(_player.FramePixels);
            _texture.Apply();

            _framesCounter = _player.FramesCounter;
        }
    }

    void OnDestroy()
    {
        _player.RemoveStoppedEvent(OnStop);
    }

    void OnPrepared(Texture texture)
    {
        //Video size != Video buffer size (FramePixels has video buffer size), so we will use
        //previously created playback texture size that based on video buffer size
        _texture = MediaPlayerHelper.GenVideoTexture(texture.width, texture.height);
        _image.texture = _texture;
    }

    void OnStop()
    {
        if (_texture != null)
            Destroy(_texture);
        _texture = null;
    }
}
