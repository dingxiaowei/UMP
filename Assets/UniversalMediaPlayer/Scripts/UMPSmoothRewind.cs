using UnityEngine;
using UnityEngine.UI;
using UMP;

public class UMPSmoothRewind : MonoBehaviour
{
    [SerializeField]
    private UniversalMediaPlayer _mediaPlayer;

    [SerializeField]
    private Slider _rewindSlider;

    private long _framesConterCahce;

    private void Update()
    {
        if (_mediaPlayer.PlatformPlayer is MediaPlayerStandalone)
        {
            if (_mediaPlayer.IsPlaying && _framesConterCahce != _mediaPlayer.FramesCounter)
            {
                _framesConterCahce = _mediaPlayer.FramesCounter;
                var frameAmount = (_mediaPlayer.PlatformPlayer as MediaPlayerStandalone).FramesAmount;

                if (frameAmount > 0)
                    _rewindSlider.value = (float)_framesConterCahce / frameAmount;
            }
        }
    }

    public void OnPositionChanged()
    {
        _mediaPlayer.Position = _rewindSlider.value;
    }
}
