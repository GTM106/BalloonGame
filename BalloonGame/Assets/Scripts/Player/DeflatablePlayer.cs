using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflatablePlayer : IPlayer
{
    readonly PlayerParameter _playerParameter;

    public DeflatablePlayer(PlayerParameter playerParameter)
    {
        _playerParameter = playerParameter;
    }

    public void BoostDash()
    {
        throw new System.NotImplementedException();
    }

    public void Dash()
    {
        Vector2 axis = _playerParameter.JoyconHandler.Stick;

        //Todo:Axisのmagunitudeによってreturnさせる
        //Yを無視
        Vector3 cameraForward = Vector3.Scale(_playerParameter.CameraTransform.forward, new Vector3(1f, 0f, 1f)).normalized;

        Vector3 moveVec = (axis.y * cameraForward + axis.x * _playerParameter.CameraTransform.right) * _playerParameter.MoveSpeed;

        //TODO:ジャンプ実装時に正式な移動処理に変更
        _playerParameter.Rb.velocity = moveVec;
    }

    public void Jump(Rigidbody rb)
    {
        throw new System.NotImplementedException();
    }
}
