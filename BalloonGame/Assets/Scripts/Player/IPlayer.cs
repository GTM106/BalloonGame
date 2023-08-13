using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer : IAdjustGravity
{
    /// <summary>
    /// �ʏ�_�b�V��
    /// </summary>
    public void Dash();

    /// <summary>
    /// �Ԃ���у_�b�V��
    /// </summary>
    public void BoostDash();

    /// <summary>
    /// �W�����v
    /// </summary>
    /// <param name="rb">player�̃��W�b�h�{�f�B</param>
    public void Jump(Rigidbody rb);
}
