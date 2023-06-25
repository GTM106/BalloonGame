using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GroundCheck), typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    enum BalloonState
    {
        Normal,
        Expands
    }

    [SerializeField] float scaleAmountDeflatingPerSecond = 0.2f;
    [SerializeField] GameObject balloon;
    [SerializeField] float speed = 5f;
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;
    [SerializeField] GroundCheck groundCheck;
    [SerializeField] CinemachineFreeLook freeLook;
    [SerializeField] CinemachineFreeLook freeLook2;

    bool isAnimation = false;
    private Rigidbody rb;
    [SerializeField] float jumpPower = 10f;
    [SerializeField] float powerJumpPower = 50f;
    [SerializeField] float multiplier = 3f;
    [SerializeField] float dashSpeed = 10f;

    Vector2 movementVector;

    BalloonState balloonState = BalloonState.Normal;

    Vector3 _offset = Vector3.one / 2f;

    bool isPowerJumped = false;
    private bool isPowerJumping;

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
        if (!groundCheck.IsGround(out _)) return;
        rb.AddForce(new(0f, jumpPower, 0f), ForceMode.Impulse);
    }

    private void Action_canceled(InputAction.CallbackContext obj)
    {
        //print("canceled");
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        // Moveアクションの入力値を取得
        movementVector = obj.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Move();

        //重力の調整
        AdjustingGravity();

        if (isPowerJumping)
        {
            if (groundCheck.IsGround(out _))
            {
                isPowerJumped = false;
                isPowerJumping = false;
                //freeLook.gameObject.SetActive(true);
            }
        }
    }

    private void AdjustingGravity()
    {
        float multiplierValue = balloonState switch
        {
            BalloonState.Normal => multiplier,
            BalloonState.Expands => 0.5f,
            _ => throw new System.NotImplementedException(),
        };

        rb.AddForce((multiplierValue - 1f) * Physics.gravity, ForceMode.Acceleration);
    }

    void Move()
    {
        if (isPowerJumped) { return; }

        //入力値を代入
        Vector2 axis = movementVector;

        if (axis.magnitude < 0.05f) { return; }

        //Yを無視
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)).normalized;

        //カメラの向きを元にベクトルの作成
        Vector3 moveVec = (axis.y * cameraForward + axis.x * Camera.main.transform.right) * speed;
        transform.forward = moveVec;
        float balloonSpeed = balloonState switch
        {
            //Normal is defalutValue
            BalloonState.Normal => 1f,
            BalloonState.Expands => dashSpeed,
            _ => throw new System.NotImplementedException(),
        };

        Vector3 velocity = moveVec * balloonSpeed;
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;
    }

    async void Update()
    {
        if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            if (isPowerJumped) { return; }

            //print("push!!");
            balloonState = BalloonState.Expands;
            ScaleAnimation(0.1f, _offset).Forget();
            return;
        }
        if (!isPowerJumped)
        {
            if (Mathf.Approximately(balloon.transform.localScale.x, 0.5f)) { return; }
            if (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                balloon.transform.localScale = Vector3.one * 0.5f;

                Vector3 force = (Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)) + Vector3.up) * 50f;

                rb.velocity = Vector3.zero;
                rb.AddForce(force * powerJumpPower, ForceMode.Acceleration);
                isPowerJumped = true;

                //カメラ調整
                //freeLook.gameObject.SetActive(false);
                
                await UniTask.Delay(2000);
                isPowerJumping = true;
                return;
            }
        }

        if (balloon.transform.localScale.x > 0.5f)
        {
            float scaleDecrease = scaleAmountDeflatingPerSecond * Time.deltaTime;
            var scale = balloon.transform.localScale;
            scale.Set(balloon.transform.localScale.x - scaleDecrease, balloon.transform.localScale.y - scaleDecrease, balloon.transform.localScale.z - scaleDecrease);
            balloon.transform.localScale = scale;
        }
        else
        {
            balloonState = BalloonState.Normal;
        }
    }

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
