using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer : IAdjustGravity
{
    /// <summary>
    /// �ʏ�_�b�V��
    /// </summary>
    public void Dash(IState.E_State state);

    /// <summary>
    /// �Ԃ���у_�b�V��
    /// </summary>
    public void BoostDash(BoostDashData boostFrame);

    /// <summary>
    /// �W�����v
    /// </summary>
    /// <param name="rb">player�̃��W�b�h�{�f�B</param>
    public void Jump(Rigidbody rb);

    /// <summary>
    /// ���ɐڐG���A��ɍs���鏈��
    /// </summary>
    public void OnWaterStay();

    /// <summary>
    /// �������ɍs���鏈��
    /// </summary>
    public void Fall();
}
