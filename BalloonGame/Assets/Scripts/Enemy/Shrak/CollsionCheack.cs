using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollsionCheack : MonoBehaviour
{
    [SerializeField] SharkController sharkController = default!;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NotBreakBlock"))
        {
            sharkController.SetHitNotBreakBlock();
        }
        else if (other.gameObject.CompareTag("BreakBlock"))
        {
            sharkController.StartBlockBreak(other.gameObject);
        }
    }
}
