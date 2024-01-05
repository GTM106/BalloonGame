using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSetActive : MonoBehaviour
{
    [Header("表示させたいオブジェクト")]
    [SerializeField] GameObject[] gameObject = default!;

    [Header("表時SE")]
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
