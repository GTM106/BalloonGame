using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class GameFinishView : MonoBehaviour
{
    [SerializeField, Required] Canvas _gameFinishCanvas = default!;
    [SerializeField, Required] VideoPlayer _videoPlayer = default!;

    public void Enable()
    {
        _gameFinishCanvas.enabled = true;
        _videoPlayer.Play();
    }

    public void Disable()
    {
        _gameFinishCanvas.enabled = false;
        _videoPlayer.Stop();
    }
}
