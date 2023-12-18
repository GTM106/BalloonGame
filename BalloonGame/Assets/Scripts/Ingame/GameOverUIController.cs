using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameOverUIController : MonoBehaviour
{
    [SerializeField, Required] Canvas _canvas = default;
    [SerializeField, Required] PlayableDirector _timeline = default!;

    private void Awake()
    {
        Disable();
    }

    public void Enable()
    {
        _canvas.enabled = true;
        _timeline.Play();
    }

    public void Disable()
    {
        _canvas.enabled = false;
        _timeline.Stop();
    }
}
