using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSetActive : MonoBehaviour
{
    [Header("表示させたいオブジェクト")]
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
