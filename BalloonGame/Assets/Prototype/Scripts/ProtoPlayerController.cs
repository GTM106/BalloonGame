using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Location
{
    Ground,
    Sky,
    Water
}

[RequireComponent(typeof(GroundCheck), typeof(Rigidbody))]
public class ProtoPlayerController : MonoBehaviour, IWater
{
    [SerializeField] float scaleAmountDeflatingPerSecond = 0.2f;
    [SerializeField] GameObject balloonObject;
    [SerializeField] float speed = 5f;
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;
    [SerializeField] GroundCheck groundCheck;
    [SerializeField] CinemachineFreeLook freeLook;
    [SerializeField] CinemachineFreeLook freeLook2;
    [SerializeField] AudioSource windAudioSource;
    [SerializeField] AudioSource dashAudioSource;

    bool isAnimation = false;
    private Rigidbody rb;
    [SerializeField] float jumpPower = 10f;
    [SerializeField] float powerJumpPower = 50f;
    [SerializeField] float dashSpeed = 10f;
    [SerializeField] Balloon balloon;

    Vector2 movementVector;

    Vector3 _offset = Vector3.one / 2f;

    bool isPowerJumped = false;
    private bool isPowerJumping;

    Location _location = Location.Ground;

    private void Awake()
    {
        balloonObject = GameObject.Find("Balloon");
        rb = GetComponent<Rigidbody>();
        move.action.performed += Action_performed;
        move.action.canceled += Action_canceled;
        jump.action.performed += Jump_performed;
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        if (_location is Location.Ground or Location.Sky)
        {
            if (!groundCheck.IsGround(out _)) return;
        }
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

        if (isPowerJumping)
        {
            if (groundCheck.IsGround(out _))
            {
                isPowerJumped = false;
                isPowerJumping = false;
                windAudioSource.Stop();
            }
        }
    }

    void Move()
    {
        if (isPowerJumped) { return; }

        //入力値を代入
        Vector2 axis = movementVector;

        if (axis.magnitude < 0.05f)
        {
            dashAudioSource.Stop();

            return;
        }

        //Yを無視
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)).normalized;

        //カメラの向きを元にベクトルの作成
        Vector3 moveVec = (axis.y * cameraForward + axis.x * Camera.main.transform.right) * speed;
        transform.forward = moveVec;
        float balloonSpeed = balloon.state switch
        {
            //Normal is defalutValue
            BalloonState.Normal => 1f,
            BalloonState.Expands => dashSpeed,
            _ => throw new System.NotImplementedException(),
        };

        if (balloon.state == BalloonState.Expands)
        {
            if (!dashAudioSource.isPlaying)
            {
                dashAudioSource.Play();
            }
        }
        else
        {
            dashAudioSource.Stop();

        }
        Vector3 velocity = moveVec * balloonSpeed;
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;
    }

    async void Update()
    {
        //if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
        //{
        //    if (isPowerJumped) { return; }

        //    //print("push!!");
        //    balloon.state = BalloonState.Expands;
        //    ScaleAnimation(0.1f, _offset).Forget();
        //    return;
        //}
        //if (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame)
        //{
        //    if (isPowerJumped) { return; }

        //    //print("push!!");
        //    balloon.state = BalloonState.Expands;
        //    ScaleAnimation(0.1f, _offset).Forget();
        //    return;
        //}       
        if (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            if (isPowerJumped) { return; }

            //print("push!!");
            balloon.state = BalloonState.Expands;
            ScaleAnimation(0.1f, _offset).Forget();
            return;
        }
        if (Gamepad.current != null && Gamepad.current.leftTrigger.wasPressedThisFrame)
        {
            if (isPowerJumped) { return; }

            //print("push!!");
            balloon.state = BalloonState.Expands;
            ScaleAnimation(0.1f, _offset).Forget();
            return;
        }
        if (!isPowerJumped)
        {
            if (Mathf.Approximately(balloonObject.transform.localScale.x, 0.5f)) { return; }
            if (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                balloonObject.transform.localScale = Vector3.one * 0.5f;

                Vector3 force = (Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)) + Vector3.up) * 50f;
                dashAudioSource.Stop();
                windAudioSource.Play();
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
        //if (!isPowerJumped)
        //{
        //    if (Mathf.Approximately(balloonObject.transform.localScale.x, 0.5f)) { return; }
        //    if (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame)
        //    {
        //        balloonObject.transform.localScale = Vector3.one * 0.5f;

        //        Vector3 force = (Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)) + Vector3.up) * 50f;

        //        rb.velocity = Vector3.zero;
        //        rb.AddForce(force * powerJumpPower, ForceMode.Acceleration);
        //        isPowerJumped = true;

        //        //カメラ調整
        //        //freeLook.gameObject.SetActive(false);

        //        await UniTask.Delay(2000);
        //        isPowerJumping = true;
        //        return;
        //    }
        //}

        if (balloon.state == BalloonState.Expands)
        {
            if (isAnimation) return;
            float scaleDecrease = scaleAmountDeflatingPerSecond * Time.deltaTime;
            var scale = balloonObject.transform.localScale;
            scale.Set(Mathf.Max(balloonObject.transform.localScale.x - scaleDecrease, 0.5f), Mathf.Max(balloonObject.transform.localScale.y - scaleDecrease, 0.5f), Mathf.Max(balloonObject.transform.localScale.z - scaleDecrease, 0.5f));
            balloonObject.transform.localScale = scale;

            if (Mathf.Approximately(balloonObject.transform.localScale.x, 0.5f))
            {
                balloon.state = BalloonState.Normal;
            }
        }
    }

    private async UniTask ScaleAnimation(float duration, Vector3 offset)
    {
        if (isAnimation) { return; }
        float time = 0f;

        var startVec = balloonObject.transform.localScale;

        while (time < duration)
        {
            isAnimation = true;
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / duration);

            var scale = startVec;
            scale += offset * progress;
            balloonObject.transform.localScale = scale;

            await UniTask.Yield();
        }

        isAnimation = false;
    }

    public void Enter()
    {
        _location = Location.Water;
    }

    public void Exit()
    {
        _location = groundCheck.IsGround(out _) ? Location.Ground : Location.Sky;
    }
}
