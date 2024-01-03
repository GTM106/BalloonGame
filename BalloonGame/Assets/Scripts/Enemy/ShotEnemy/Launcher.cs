using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [SerializeField] EnemyObjectPoolController objectPool;
    [SerializeField] float interval;

    void Start()
    {
        //���˗p�R���[�`���X�^�[�g
        StartCoroutine(Shot());
    }

    //�R���[�`��
    IEnumerator Shot()
    {
        //���˃��[�v
        while (true)
        {
            objectPool.Launch(transform.position);

            yield return new WaitForSeconds(interval);
        }
    }
}
