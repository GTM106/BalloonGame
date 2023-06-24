using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject balloon;
    [SerializeField] float speed = 5f;
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;

    Vector3 _offset = Vector3.one / 2f;

    private void Awake()
    {
        balloon = GameObject.Find("Balloon");
        rb = GetComponent<Rigidbody>();
        move.action.performed += Action_performed;
        move.action.canceled += Action_canceled;
        jump.action.performed += Jump_performed;
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        print("jump");
        rb.AddForce(new(0f, 5f, 0f), ForceMode.Impulse);
    }

    private void Action_canceled(InputAction.CallbackContext obj)
    {
        //print("canceled");
        //movementX = 0f;
        //movementY = 0f;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        // Moveアクションの入力値を取得
        Vector2 movementVector = obj.ReadValue<Vector2>();
        // print("move axis : " + movementVector);

        // x,y軸方向の入力値を変数に代入
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    private void FixedUpdate()
    {
        // 入力値を元に3軸ベクトルを作成
        Vector3 movement = new(movementX, 0f, movementY);

        // rigidbodyのAddForceを使用してプレイヤーを動かす。

        rb.AddForce(movement * speed);
        if (rb.velocity.magnitude > 10f)
        {
            rb.velocity = new Vector3(rb.velocity.x / 1.1f, rb.velocity.y, rb.velocity.z / 1.1f);
        }
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
            float scaleDecrease = 0.001f;
            var scale = balloon.transform.localScale;
            scale.Set(balloon.transform.localScale.x - scaleDecrease, balloon.transform.localScale.y - scaleDecrease, balloon.transform.localScale.z - scaleDecrease);
            balloon.transform.localScale = scale;
        }
    }

    bool isAnimation = false;
    private float movementX;
    private float movementY;
    private Rigidbody rb;

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
