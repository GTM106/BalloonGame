using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ChangeActiveMaterial : MonoBehaviour
{
    //ToDo
    //�}�e���A�������������Ă�̂Ŏ��s�I�����ɖ߂�������ǉ�
    public void ChangeActive(Material material)
    {
        //ShaderGraph��bool��0,1�̐����Ƃ��Ĉ����̂�GetInt��p���ĕύX���Ȃ���΂Ȃ�Ȃ����߂��̂悤�ɂ��Ă���
        int isActive = material.GetInt("_Active");

        material.SetInt("_Active", isActive == 0 ? 1 : 0);
    }
}
