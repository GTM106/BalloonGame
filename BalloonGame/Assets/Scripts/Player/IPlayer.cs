using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer : IAdjustGravity
{
    /// <summary>
    /// 通常ダッシュ
    /// </summary>
    public void Dash();

    /// <summary>
    /// ぶっ飛びダッシュ
    /// </summary>
    public void BoostDash();

    /// <summary>
    /// ジャンプ
    /// </summary>
    /// <param name="rb">playerのリジッドボディ</param>
    public void Jump(Rigidbody rb);

    /// <summary>
    /// 水に接触中、常に行われる処理
    /// </summary>
    public void OnWaterStay();
}
