using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPoolController : MonoBehaviour
{
    [Header("BulletController")]
    [SerializeField] BulletController bullet;
    [Header("�ő吶����")]
    [SerializeField] int maxCount;

    Queue<BulletController> bulletQueue;

    //���񐶐����̃|�W�V����
    Vector3 setPos = new(100, 100, 0);

    private void Awake()
    {
        //Queue�̏�����
        bulletQueue = new Queue<BulletController>();

        //�e�𐶐����郋�[�v
        for (int i = 0; i < maxCount; i++)
        {
            //����
            BulletController tmpBullet = Instantiate(bullet, setPos, Quaternion.identity, transform);
            //Queue�ɒǉ�
            bulletQueue.Enqueue(tmpBullet);
        }
    }


    //�e��݂��o������
    public BulletController Launch(Vector3 _pos)
    {
        //Queue����Ȃ�null
        if (bulletQueue.Count <= 0) return null;

        BulletController tmpBullet = bulletQueue.Dequeue();

        tmpBullet.gameObject.SetActive(true);

        tmpBullet.ShowInStage(_pos);

        return tmpBullet;
    }


    public void Collect(BulletController _bullet)
    {
        _bullet.gameObject.SetActive(false);
        bulletQueue.Enqueue(_bullet);
    }
}
