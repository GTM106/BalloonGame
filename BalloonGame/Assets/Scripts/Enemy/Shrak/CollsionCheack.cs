using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollsionCheack : MonoBehaviour
{
    [SerializeField] SharkController sharkController = default!;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "NotBreakBlock")
        {
            sharkController.SetHitNotBreakBlock();
        }
        else if (other.gameObject.tag == "BreakBlock")
        {
            sharkController.StartBlockBreak(other);
        }
    }
}
