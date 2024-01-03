using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkPatrolRange : MonoBehaviour
{
    [SerializeField] SharkController sharkController = default!;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Shark"))
        {
            sharkController.OnRangeCheckRangeCollsionEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Shark"))
        {
            sharkController.OnRangeCheckRangeCollsionExit();
        }
    }
}
