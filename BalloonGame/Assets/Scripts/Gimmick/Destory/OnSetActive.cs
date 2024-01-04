using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSetActive : MonoBehaviour
{
    [Header("�\�����������I�u�W�F�N�g")]
    [SerializeField] GameObject[] gameObject = default!;

    private void Awake()
    {
        foreach (GameObject obj in gameObject) 
        {
            obj.SetActive(false);
        }
    }

    public void OnObjectTrue()
    {
        foreach (GameObject obj in gameObject)
        {
            obj.SetActive(true);
        }
    }
}
