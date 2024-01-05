using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSetActive : MonoBehaviour
{
    [Header("�\�����������I�u�W�F�N�g")]
    [SerializeField] GameObject[] gameObject = default!;

    [Header("�\��SE")]
    [SerializeField, Required] AudioSource _audioSource = default!;

    private void Awake()
    {
        foreach (GameObject obj in gameObject)
        {
            if(obj.activeSelf)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
        }
    }

    public void OnObjectTrue()
    {
        foreach (GameObject obj in gameObject)
        {
            if (obj.activeSelf)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
        }


        SoundManager.Instance.PlaySE(_audioSource, SoundSource.SE069_GimmickAppearance);
    }
}
