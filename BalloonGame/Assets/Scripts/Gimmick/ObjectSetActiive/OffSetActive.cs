using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffSetActive : MonoBehaviour
{
    [Header("��\���ɂ��������I�u�W�F�N�g")]
    [SerializeField] GameObject[] gameObject = default!;

    private void Awake()
    {
        foreach (GameObject obj in gameObject)
        {
            obj.SetActive(true);
        }
    }

    public void OnObjectTrue()
    {
        foreach (GameObject obj in gameObject)
        {
            obj.SetActive(false);
        }
    }
}
