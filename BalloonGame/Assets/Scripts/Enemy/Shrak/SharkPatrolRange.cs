using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkPatrolRange : MonoBehaviour
{
    [SerializeField] GameObject patrolObj = default!;
    [SerializeField] SharkController sharkController = default!;

    private void OnTriggerEnter(Collider other)
    {
        if (other == patrolObj)
        {
            sharkController.OnRangeCheckRangeCollisionEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == patrolObj)
        {
            sharkController.OnRangeCheckRangeCollisionExit();
        }
    }
}
