using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayEffectCharacterWalk : MonoBehaviour
{
    [SerializeField] VisualEffect dashVFX; // VFX�A�Z�b�g�ւ̎Q��
    [SerializeField] float dashThreshold = 5.0f; // �_�b�V�����x��臒l
    private bool isDashing = false; // �_�b�V�������ǂ����̃t���O

    private void Update()
    {
        float currentSpeed = GetComponent<Rigidbody>().velocity.magnitude;

        if (currentSpeed >= dashThreshold && !isDashing)
        {
            // �_�b�V�����n�܂����u��
            isDashing = true;
            StartDashEffect();
        }
        else if (currentSpeed < dashThreshold && isDashing)
        {
            // �_�b�V�����I������u��
            isDashing = false;
            StopDashEffect();
        }
    }

    private void StartDashEffect()
    {
        // VFX�ɃC�x���g�𑗐M���ăG�t�F�N�g���Đ�
        dashVFX.SendEvent("OnPlay");
    }

    private void StopDashEffect()
    {
        // VFX�ɃC�x���g�𑗐M���ăG�t�F�N�g���~
        dashVFX.SendEvent("StopPlay");
    }
}
