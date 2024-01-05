using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffSetActive : MonoBehaviour
{
    [Header("非表示にさせたいオブジェクト")]
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
