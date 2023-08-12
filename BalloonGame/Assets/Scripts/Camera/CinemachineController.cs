using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineFreeLook _freeLook = default!;
    [SerializeField] JoyconHandler _joyconHandler = default!;

    private void Update()
    {
        //Cinemachine側でInputManager,InputSystemが使えますが、
        //今回はJoyconを読み取る形式に独自の形式を利用しているため直接入力値を書き換えます。
        _freeLook.m_XAxis.m_InputAxisValue = _joyconHandler.Stick.x;
        _freeLook.m_YAxis.m_InputAxisValue = _joyconHandler.Stick.y;
    }
}
