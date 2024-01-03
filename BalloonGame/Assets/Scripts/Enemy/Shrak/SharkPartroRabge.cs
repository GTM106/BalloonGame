using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkPartroRange : MonoBehaviour
{
    [SerializeField] SharkController sharkController = default!;
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Shark")
        {
            sharkController.OnRangeCheckRangeCollsionEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Shark")
        {
            sharkController.OnRangeCheckRangeCollsionExit();
        }
    }
}
