using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour, IHittable
{
    [SerializeField] CollectibleScript collectibleScript = default!;
    [Header("収集アイテム取得時に上昇する値")]
    [SerializeField, Min(0)] int itemValue = default!;

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        //SE700再生
        collectibleScript.Add(itemValue);
        Destroy(this.gameObject);
    }
    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {

    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }
}
