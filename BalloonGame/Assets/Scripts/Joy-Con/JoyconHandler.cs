using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyconHandler : MonoBehaviour
{
    enum JoyconType
    {
        Left,
        Right,
        RingCon,
    }

    enum ActionPhase
    {
        None,
        Performed,
        Canceled,
    }

    [SerializeField] JoyconType joyconType;
    [SerializeField] InputSystemManager.ActionMaps maps;

    List<Joycon> joycons;

    Vector2 _stick = Vector2.zero;

    public InputSystemManager.ActionMaps Maps => maps;

    /// <summary>
    /// ジャイロ
    /// </summary>
    public Vector3 Gyro { get; private set; }
    /// <summary>
    /// 加速度
    /// </summary>
    public Vector3 Accel { get; private set; }
    /// <summary>
    /// 傾き
    /// </summary>
    public Quaternion Orientation { get; private set; }
    /// <summary>
    /// スティック入力値
    /// </summary>
    public Vector2 Stick => _stick;

    //左側のボタン
    public event Action OnLeftButtonPressed;
    public event Action OnLeftButtonReleased;
    public event Action OnLeftButtonHeld;
    //右側のボタン
    public event Action OnRightButtonPressed;
    public event Action OnRightButtonReleased;
    public event Action OnRightButtonHeld;
    //上側のボタン
    public event Action OnUpButtonPressed;
    public event Action OnUpButtonReleased;
    public event Action OnUpButtonHeld;
    //下側のボタン
    public event Action OnDownButtonPressed;
    public event Action OnDownButtonReleased;
    public event Action OnDownButtonHeld;
    //shoulderボタン
    public event Action OnShoulderButtonPressed;
    public event Action OnShoulderButtonReleased;
    public event Action OnShoulderButtonHeld;
    //triggerボタン
    public event Action OnTriggerButtonPressed;
    public event Action OnTriggerButtonReleased;
    public event Action OnTriggerButtonHeld;
    //SLボタン
    public event Action OnSLButtonPressed;
    public event Action OnSLButtonReleased;
    public event Action OnSLButtonHeld;
    //SRボタン
    public event Action OnSRButtonPressed;
    public event Action OnSRButtonReleased;
    public event Action OnSRButtonHeld;
    //マイナスボタン
    public event Action OnMinusButtonPressed;
    public event Action OnMinusButtonReleased;
    public event Action OnMinusButtonHeld;
    //プラスボタン
    public event Action OnPlusButtonPressed;
    public event Action OnPlusButtonReleased;
    public event Action OnPlusButtonHeld;
    //ホームボタン
    public event Action OnHomeButtonPressed;
    public event Action OnHomeButtonReleased;
    public event Action OnHomeButtonHeld;
    //キャプチャーボタン
    public event Action OnCaptureButtonPressed;
    public event Action OnCaptureButtonReleased;
    public event Action OnCaptureButtonHeld;
    //スティックボタン
    public event Action OnStickButtonPressed;
    public event Action OnStickButtonReleased;
    public event Action OnStickButtonHeld;

    //MAPデータで呼び分けする
    readonly Dictionary<Joycon.Button, Action> onButtonPressed = new();
    readonly Dictionary<Joycon.Button, Action> onButtonReleased = new();
    readonly Dictionary<Joycon.Button, Action> onButtonHeld = new();
    readonly ActionPhase[] actionPhase = new ActionPhase[(int)Joycon.Button.SHOULDER_2 + 1];

    uint _keyPressed = 0;

    private void Start()
    {
        joycons = JoyconManager.Instance.j;
        if (joycons.Count < (int)joyconType + 1)
        {
            Destroy(gameObject);
        }

        //Dictionaryにイベントを登録
        onButtonPressed.Add(Joycon.Button.DPAD_UP, OnUpButtonPressed);
        onButtonPressed.Add(Joycon.Button.DPAD_DOWN, OnDownButtonPressed);
        onButtonPressed.Add(Joycon.Button.DPAD_LEFT, OnLeftButtonPressed);
        onButtonPressed.Add(Joycon.Button.DPAD_RIGHT, OnRightButtonPressed);
        onButtonPressed.Add(Joycon.Button.SHOULDER_1, OnShoulderButtonPressed);
        onButtonPressed.Add(Joycon.Button.SHOULDER_2, OnTriggerButtonPressed);
        onButtonPressed.Add(Joycon.Button.SL, OnSLButtonPressed);
        onButtonPressed.Add(Joycon.Button.SR, OnSRButtonPressed);
        onButtonPressed.Add(Joycon.Button.MINUS, OnMinusButtonPressed);
        onButtonPressed.Add(Joycon.Button.PLUS, OnPlusButtonPressed);
        onButtonPressed.Add(Joycon.Button.HOME, OnHomeButtonPressed);
        onButtonPressed.Add(Joycon.Button.CAPTURE, OnCaptureButtonPressed);
        onButtonPressed.Add(Joycon.Button.STICK, OnStickButtonPressed);

        onButtonReleased.Add(Joycon.Button.DPAD_UP, OnUpButtonReleased);
        onButtonReleased.Add(Joycon.Button.DPAD_DOWN, OnDownButtonReleased);
        onButtonReleased.Add(Joycon.Button.DPAD_LEFT, OnLeftButtonReleased);
        onButtonReleased.Add(Joycon.Button.DPAD_RIGHT, OnRightButtonReleased);
        onButtonReleased.Add(Joycon.Button.SHOULDER_1, OnShoulderButtonReleased);
        onButtonReleased.Add(Joycon.Button.SHOULDER_2, OnTriggerButtonReleased);
        onButtonReleased.Add(Joycon.Button.SL, OnSLButtonReleased);
        onButtonReleased.Add(Joycon.Button.SR, OnSRButtonReleased);
        onButtonReleased.Add(Joycon.Button.MINUS, OnMinusButtonReleased);
        onButtonReleased.Add(Joycon.Button.PLUS, OnPlusButtonReleased);
        onButtonReleased.Add(Joycon.Button.HOME, OnHomeButtonReleased);
        onButtonReleased.Add(Joycon.Button.CAPTURE, OnCaptureButtonReleased);
        onButtonReleased.Add(Joycon.Button.STICK, OnStickButtonReleased);

        onButtonHeld.Add(Joycon.Button.DPAD_UP, OnUpButtonHeld);
        onButtonHeld.Add(Joycon.Button.DPAD_DOWN, OnDownButtonHeld);
        onButtonHeld.Add(Joycon.Button.DPAD_LEFT, OnLeftButtonHeld);
        onButtonHeld.Add(Joycon.Button.DPAD_RIGHT, OnRightButtonHeld);
        onButtonHeld.Add(Joycon.Button.SHOULDER_1, OnShoulderButtonHeld);
        onButtonHeld.Add(Joycon.Button.SHOULDER_2, OnTriggerButtonHeld);
        onButtonHeld.Add(Joycon.Button.SL, OnSLButtonHeld);
        onButtonHeld.Add(Joycon.Button.SR, OnSRButtonHeld);
        onButtonHeld.Add(Joycon.Button.MINUS, OnMinusButtonHeld);
        onButtonHeld.Add(Joycon.Button.PLUS, OnPlusButtonHeld);
        onButtonHeld.Add(Joycon.Button.HOME, OnHomeButtonHeld);
        onButtonHeld.Add(Joycon.Button.CAPTURE, OnCaptureButtonHeld);
        onButtonHeld.Add(Joycon.Button.STICK, OnStickButtonHeld);
    }

    private void Update()
    {
        if (joycons.Count <= 0) return;
        Joycon j = joycons[(int)joyconType];
        _keyPressed = 0;

        OnStickInput(j);
        for (Joycon.Button button = 0; button <= Joycon.Button.SHOULDER_2; button++)
        {
            OnButton(j, button);
        }
        GetGyro(j);
        GetAccel(j);
        GetOrientation(j);
    }

    private void OnDisable()
    {
        _stick = Vector2.zero;
        Gyro = Vector3.zero;
        Accel = Vector3.zero;
        Orientation = Quaternion.identity;
    }

    private void OnStickInput(Joycon j)
    {
        float[] stick = j.GetStick();
        _stick.Set(stick[0], stick[1]);
    }

    private void GetGyro(Joycon j)
    {
        Gyro = j.GetGyro();
    }

    private void GetAccel(Joycon j)
    {
        Accel = j.GetAccel();
    }

    private void GetOrientation(Joycon j)
    {
        Orientation = j.GetVector();
    }

    private void OnButton(Joycon j, Joycon.Button button)
    {
        //Dictionaryで呼び分ける
        if (j.GetButtonUp(button))
        {
            if (actionPhase[(int)button] == ActionPhase.Canceled) return;
            actionPhase[(int)button] = ActionPhase.Canceled;

            if (onButtonReleased.TryGetValue(button, out Action action))
            {
                action?.Invoke();
            }
        }
        if (j.GetButtonDown(button))
        {
            if (actionPhase[(int)button] == ActionPhase.Performed) return;
            actionPhase[(int)button] = ActionPhase.Performed;

            _keyPressed |= 1u << (int)button;

            if (onButtonPressed.TryGetValue(button, out Action action))
            {
                action?.Invoke();
            }
        }
        if (j.GetButton(button))
        {
            if (onButtonHeld.TryGetValue(button, out Action action))
            {
                action?.Invoke();
            }
        }
    }

    public bool WasPressedAnyKeyThisFrame => _keyPressed != 0;
}
