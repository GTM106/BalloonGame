using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayEffectCharacterWalk : MonoBehaviour
{
    [SerializeField] VisualEffect dashVFX; // VFXアセットへの参照
    [SerializeField] float dashThreshold = 5.0f; // ダッシュ速度の閾値
    private bool isDashing = false; // ダッシュ中かどうかのフラグ

    private void Update()
    {
        float currentSpeed = GetComponent<Rigidbody>().velocity.magnitude;

        if (currentSpeed >= dashThreshold && !isDashing)
        {
            // ダッシュが始まった瞬間
            isDashing = true;
            StartDashEffect();
        }
        else if (currentSpeed < dashThreshold && isDashing)
        {
            // ダッシュが終わった瞬間
            isDashing = false;
            StopDashEffect();
        }
    }

    private void StartDashEffect()
    {
        // VFXにイベントを送信してエフェクトを再生
        dashVFX.SendEvent("OnPlay");
    }

    private void StopDashEffect()
    {
        // VFXにイベントを送信してエフェクトを停止
        dashVFX.SendEvent("StopPlay");
    }
}
