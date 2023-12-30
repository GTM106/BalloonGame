using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarmHole : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] GameObject targetObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetObject)
        {
            other.gameObject.transform.position = target.position;
        }
    }
}
