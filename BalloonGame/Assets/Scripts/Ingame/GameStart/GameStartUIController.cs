using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartUIController : MonoBehaviour
{
    [SerializeField, Required] GameStartUIView _gameStartUIView = default!;

    public async UniTask EnableUI()
    {
       await _gameStartUIView.Enable();
    }

    public void DisableUI()
    {
        _gameStartUIView.Disable();
    }
}
