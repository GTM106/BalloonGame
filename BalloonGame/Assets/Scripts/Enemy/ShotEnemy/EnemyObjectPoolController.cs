using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPoolController : MonoBehaviour
{
    [Header("BulletController")]
    [SerializeField] BulletController bullet;
    [Header("最大生成数")]
    [SerializeField] int maxCount;

    Queue<BulletController> bulletQueue;

    //初回生成時のポジション
    Vector3 setPos = new(100, 100, 0);

    private void Awake()
    {
        //Queueの初期化
        bulletQueue = new Queue<BulletController>();

        //弾を生成するループ
        for (int i = 0; i < maxCount; i++)
        {
            //生成
            BulletController tmpBullet = Instantiate(bullet, setPos, Quaternion.identity, transform);
            //Queueに追加
            bulletQueue.Enqueue(tmpBullet);
        }
    }


    //弾を貸し出す処理
    public BulletController Launch(Vector3 _pos)
    {
        //Queueが空ならnull
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
