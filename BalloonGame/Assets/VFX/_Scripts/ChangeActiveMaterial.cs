using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ChangeActiveMaterial : MonoBehaviour
{
    //ToDo
    //マテリアルを書き換えてるので実行終了時に戻す処理を追加
    public void ChangeActive(Material material)
    {
        //ShaderGraphのboolは0,1の整数として扱うのでGetIntを用いて変更しなければならないためこのようにしている
        int isActive = material.GetInt("_Active");

        material.SetInt("_Active", isActive == 0 ? 1 : 0);
    }
}
