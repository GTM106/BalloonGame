using UnityEngine;

/// <summary>
/// �v���C���[�ɓ�������̂̃C���^�[�t�F�[�X
/// </summary>
public interface IHittable
{
    /// <summary>
    /// �v���C���[�ƐڐG�����u�Ԃ̏���
    /// </summary>
    void OnEnter(Collider playerCollider, BalloonState balloonState);

    /// <summary>
    /// �v���C���[�ƐڐG���Ă���Ƃ���ɍs���鏈��
    /// </summary>
    void OnStay(Collider playerCollider, BalloonState balloonState);

    /// <summary>
    /// �v���C���[�Ɨ��ꂽ�u�Ԃ̏���
    /// </summary>
    void OnExit(Collider playerCollider, BalloonState balloonState);
}