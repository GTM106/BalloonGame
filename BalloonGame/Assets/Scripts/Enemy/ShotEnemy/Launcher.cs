using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] EnemyObjectPoolController objectPool;
    [SerializeField] float interval;

    void Start()
    {
        //発射用コルーチンスタート
        StartCoroutine(Shot());
    }

    //コルーチン
    IEnumerator Shot()
    {
        //発射ループ
        while (true)
        {
            objectPool.Launch(transform.position);

            yield return new WaitForSeconds(interval);
        }
    }
}
