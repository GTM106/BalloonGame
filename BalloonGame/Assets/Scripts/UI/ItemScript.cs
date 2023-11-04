using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour, IHittable
{
    [SerializeField] CollectibleScript collectibleScript = default!;
    [Header("���W�A�C�e���擾���ɏ㏸����l")]
    [SerializeField, Min(0)] int itemValue = default!;

    public void OnEnter(Collider playerCollider, BalloonState balloonState)
    {
        //SE700�Đ�
        collectibleScript.GetComponent<CollectibleScript>().currentNumber = collectibleScript.GetComponent<CollectibleScript>().currentNumber + itemValue;
        Destroy(this.gameObject);
    }
    public void OnExit(Collider playerCollider, BalloonState balloonState)
    {

    }

    public void OnStay(Collider playerCollider, BalloonState balloonState)
    {
    }
}
