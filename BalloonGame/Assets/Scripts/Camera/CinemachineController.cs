using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineFreeLook _freeLook = default!;
    [SerializeField] JoyconHandler _joyconHandler = default!;

    [Header("吹っ飛びダッシュ後の機能")]
    [SerializeField] bool _changeTopRigToggle = true;
    [SerializeField, Min(0f)] float _changeTopRigDuration = 0.2f;

    static readonly float topRigValue = 1f;

    private void Update()
    {
        //Cinemachine側でInputManager,InputSystemが使えますが、
        //今回はJoyconを読み取る形式に独自の形式を利用しているため直接入力値を書き換えます。
        _freeLook.m_XAxis.m_InputAxisValue = _joyconHandler.Stick.x;
        _freeLook.m_YAxis.m_InputAxisValue = _joyconHandler.Stick.y;
    }

    public async void OnAfterBoostDash()
    {
        //開発中の機能としてONOFFができるようにしている。
        //設定画面に昇華するかレベルデザイン確定で削除を推奨。
        if (!_changeTopRigToggle) return;

        var token = this.GetCancellationTokenOnDestroy();
        float time = 0f;

        float offset = topRigValue - _freeLook.m_YAxis.Value;
        float firstValue = _freeLook.m_YAxis.Value;

        while (time <= _changeTopRigDuration)
        {
            await UniTask.Yield(token);
            time += Time.deltaTime;

            float progress = time / _changeTopRigDuration;

            _freeLook.m_YAxis.Value = firstValue + offset * progress;
        }

        _freeLook.m_YAxis.Value = topRigValue;
    }
}
