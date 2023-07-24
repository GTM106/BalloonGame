# JoyconHandler のマニュアル
## 目次
- [はじめに](#はじめに)
- [使い方](#使い方)
  - [プロパティ](#プロパティ)
  - [イベント](#イベント)
  - [イベント登録方法](#イベント登録方法)
  - [注意点](#注意点)
- [Joycon登録方法](#joycon登録方法)

## はじめに
このマニュアルは **プログラマー向け** のマニュアルです。

Joyconの`Stick` や `Button`の押した・離した といった読み取りの方法を示します。

感覚的には、InputSystemと同じ要領になっていると思います。まぁ、若干の差異もあるので詳しくはこのあと解説します。

## 使い方
さて、使い方を覚えていきましょう！

### プロパティ
publicにして外部に公開しています。ここからStickの値を移動処理に用いるなどしてください。
| プロパティ名 | 型 | 概要 |
|:---|:---|:---|
|Gyro|Vector3 |ジャイロ |
|Accel |Vector3 |加速度 |
|Orientation |Quaternion |方向 |
|Stick |Vector2 |ジョイコンのスティックの入力値 |

### イベント
イベントは大きく分けて3つに分かれています。
- ボタン押した瞬間
  - 〇〇Pressed
- ボタン押しているとき
  - 〇〇Held
- ボタン離したとき
  - 〇〇Released

| イベント名 | 概要 |
|:---|:---|
|OnLeftButton〇〇|Dpadの左ボタンに関する処理 |
|OnRightButton〇〇 |Dpadの右ボタンに関する処理 |
|OnUpButton〇〇 |Dpadの上ボタンに関する処理 |
|OnDownButton〇〇 |Dpadの下ボタンに関する処理 |
|OnShoulderButton〇〇 |shoulderボタン(LやR)に関する処理 |
|OnTriggerButton〇〇 |triggerボタン(ZLやZR)に関する処理 |
|OnSLButton〇〇 |SLボタン(サイドについてるLボタン)に関する処理 |
|OnSRButton〇〇 |SRボタン(サイドについてるRボタン)に関する処理 |
|OnMinusButton〇〇 |マイナスボタンに関する処理 |
|OnPlusButton〇〇 |プラスボタンに関する処理 |
|OnHomeButton〇〇 |ホームボタンに関する処理 |
|OnCaptureButton〇〇 |キャプチャーボタンに関する処理 |
|OnStickButton〇〇 |スティック押し込みに関する処理 |

### イベント登録方法
```cs
//取得したいJoyconをここに入れる。
//基本右と左の2種類がある。
[SerializeField] JoyconHandler _joyconHandler = defalut!;

//必ずAwakeで追加してください。
//理由はJoyconHandlerのStartでイベントのデータを追加しているからです。
void Awake()
{
    //たとえばHomeボタンを押し続けているときの処理を追加するとする。
    _joyconHandler.OnHomeButtonHeld += OnHomeButtonHeld;
}

//名前はわかりやすければ何でもOKです。
void OnHomeButtonHeld()
{
    //TODO:ここに押し込んでいるときに行う処理を記述
    print("Homeボタンを押し込んでいるよ！");
}
```
```cs
//解除方法
_joyconHandler.OnHomeButtonHeld -= OnHomeButtonHeld
```

### 注意点
現状の仕様では、Awake以後でイベントの登録・解除が出来ません。

もしイベントをAwake以後でイベントを登録・解除をしたいときは知らせてください。仕様を変更します。

## Joycon登録方法
概要を示します。のちほど正式なManualへのリンクに変更します

Bluetoothで接続します。  
今は接続順がとても大切です。そのうち変える予定です。  
`左 → 右 → リングコン用`の順に登録してください
