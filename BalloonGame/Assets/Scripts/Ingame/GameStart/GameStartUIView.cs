using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartUIView : MonoBehaviour
{
    [SerializeField, Required] Canvas _gameStartUICanvas = default!;

    public async UniTask Enable()
    {
        var token = this.GetCancellationTokenOnDestroy();

        _gameStartUICanvas.enabled = true;

        //TODO: GameStartUIのアニメーションをつける
        Debug.LogWarning("GameStartUIのアニメーションがまだ未実装です");

        int frame = 5;
        while (frame-- > 0)
        {
            await UniTask.Yield(token);
        }
    }

    public void Disable()
    {
        _gameStartUICanvas.enabled = false;
    }
}
