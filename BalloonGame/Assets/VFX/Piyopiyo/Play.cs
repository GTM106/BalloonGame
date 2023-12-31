using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Play : MonoBehaviour
{
    [SerializeField]VisualEffect effect;

    void Start()
    {
        StartCoroutine("Effct");
    }

    private IEnumerator Effct()
    {
        Debug.Log("Start");
        effect.Play();

        //1秒待つ
        yield return new WaitForSeconds(1.0f);

        Debug.Log("Stop");
        //1秒待った後の処理
        effect.Stop();

    }
}
