using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("��΂��I�u�W�F�N�g")]
    [SerializeField] GameObject flyObj = default!;
    [Header("��΂��X�s�[�h")]
    [SerializeField] float shotSpeed = default!;
    [Header("�^�[�Q�b�g")]
    [SerializeField] GameObject targetPos = default!;
    [SerializeField] Rigidbody rb = null;

    EnemyObjectPoolController objectPool;
    Vector3 moveVec;
    float delayTime = 0f;
    void Start()
    {
        moveVec = targetPos.transform.position - transform.position;

        objectPool = transform.parent.GetComponent<EnemyObjectPoolController>();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(delayTime > 3)
        {
            rb.AddForce(moveVec * shotSpeed, ForceMode.Impulse);
        }

        delayTime += Time.deltaTime;
    }

    private void OnBecameInvisible()
    {
        HideFromStage();
    }

    public void ShowInStage(Vector3 _pos)
    {
        transform.position = _pos;
    }

    public void HideFromStage()
    {
        objectPool.Collect(this);
    }
}
