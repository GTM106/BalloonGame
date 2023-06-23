using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject balloon;

    Vector3 _offset = Vector3.one / 2f;

    private void Awake()
    {
        balloon = GameObject.Find("Balloon");
    }

    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            print("push!!");
            ScaleAnimation(0.1f, _offset).Forget();
        }

        if (balloon.transform.localScale.x > 0.5f)
        {
            float d = 0.001f;
            var scale = balloon.transform.localScale;
            scale.Set(balloon.transform.localScale.x - d, balloon.transform.localScale.y - d, balloon.transform.localScale.z - d);
            balloon.transform.localScale = scale;
        }
    }

    bool isAnimation = false;

    private async UniTask ScaleAnimation(float duration, Vector3 offset)
    {
        if (isAnimation) { return; }
        float time = 0f;

        var startVec = balloon.transform.localScale;

        while (time < duration)
        {
            isAnimation = true;
            await UniTask.Yield();
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / duration);

            var scale = startVec;
            scale += offset * progress;
            balloon.transform.localScale = scale;
        }

        isAnimation = false;
    }
}
