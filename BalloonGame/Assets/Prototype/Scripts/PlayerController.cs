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

    [SerializeField] GameObject balloon;
    [SerializeField] float speed = 5f;
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;
    [SerializeField] GroundCheck groundCheck;

    bool isAnimation = false;
    private float movementX;
    private float movementY;
    private Rigidbody rb;
    [SerializeField] float jumpPower = 10f;
    [SerializeField] float multiplier = 3f;
    [SerializeField] float dashSpeed = 10f;

    Vector2 movementVector;

    BalloonState balloonState = BalloonState.Normal;

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
        if (!groundCheck.IsGround(out _)) return;
        rb.AddForce(new(0f, jumpPower, 0f), ForceMode.Impulse);
    }

    private void Action_canceled(InputAction.CallbackContext obj)
    {
        //print("canceled");
        movementX = 0f;
        movementY = 0f;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        // Moveアクションの入力値を取得
        movementVector = obj.ReadValue<Vector2>();
        // print("move axis : " + movementVector);

        // x,y軸方向の入力値を変数に代入
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    private void FixedUpdate()
    {
        Move();

        //重力の調整
        AdjustingGravity();
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
        //入力値を代入
        Vector2 axis = movementVector;

        if (axis.magnitude < 0.05f) { return; }

        //Yを無視
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1f, 0f, 1f)).normalized;

        //カメラの向きを元にベクトルの作成
        Vector3 moveVec = (axis.y * cameraForward + axis.x * Camera.main.transform.right) * speed;

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

        //if (rb.velocity.magnitude < 10f)
        //{
        //    float mag = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
        //    float speedMag = 5f - mag;
        //    rb.AddForce(speedMag * moveVec, ForceMode.Acceleration);
        //}
    }

    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            //print("push!!");
            balloonState = BalloonState.Expands;
            ScaleAnimation(0.1f, _offset).Forget();
            return;
        }

        if (balloon.transform.localScale.x > 0.5f)
        {
            float scaleDecrease = 0.2f * Time.deltaTime;
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
