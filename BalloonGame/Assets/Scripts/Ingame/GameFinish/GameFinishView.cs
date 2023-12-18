using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinishView : MonoBehaviour
{
    [SerializeField, Required] Canvas _gameFinishCanvas = default!;

    public void Enable()
    {
        _gameFinishCanvas.enabled = true;
    }

    public void Disable()
    {
        _gameFinishCanvas.enabled = false;
    }
}
