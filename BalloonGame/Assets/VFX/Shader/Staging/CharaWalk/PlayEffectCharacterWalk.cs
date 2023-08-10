using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayEffectCharacterWalk : MonoBehaviour
{
    [SerializeField] VisualEffect effect;
    [SerializeField] Rigidbody CharaRb;

    void Update()
    {
        if (CharaRb.velocity.magnitude <= 1.0f)
        {
            effect.SendEvent("StopPlay");
            return;
        }

        effect.SendEvent("OnPlay");
    }
}
