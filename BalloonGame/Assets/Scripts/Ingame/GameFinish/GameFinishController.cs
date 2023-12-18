using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinishController : MonoBehaviour
{
    [SerializeField, Required] GameFinishView _gameFinishView = default!;

    public void EnableUI()
    {
        _gameFinishView.Enable();
    }

    public void DisableUI()
    {
        _gameFinishView.Disable();
    }
}
