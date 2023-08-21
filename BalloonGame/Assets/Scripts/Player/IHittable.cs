using UnityEngine;

/// <summary>
/// プレイヤーに当たるもののインターフェース
/// </summary>
public interface IHittable
{
    /// <summary>
    /// プレイヤーと接触した瞬間の処理
    /// </summary>
    void OnEnter(Collider playerCollider, BalloonState balloonState);

    /// <summary>
    /// プレイヤーと接触しているとき常に行われる処理
    /// </summary>
    void OnStay(Collider playerCollider, BalloonState balloonState);

    /// <summary>
    /// プレイヤーと離れた瞬間の処理
    /// </summary>
    void OnExit(Collider playerCollider, BalloonState balloonState);
}