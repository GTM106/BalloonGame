using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoBehaviour
{
    public enum ActionMaps
    {
        Player,
        UI,
    }

    public ActionMaps currentActionMap = ActionMaps.Player;

    [SerializeField] List<JoyconHandler> _joyconHandlers;

    [SerializeField, Required] PlayerInput _playerInput = default!;

    public void ChangeMaps(ActionMaps maps)
    {
        currentActionMap = maps;

        //InputSystem���̃}�b�v��ύX�B
        _playerInput.SwitchCurrentActionMap(maps.ToString());

        //JoyconHandler���̃}�b�v�����݂̃}�b�v�l����؂�ւ�
        foreach (var joycon in _joyconHandlers)
        {
            if (joycon == null) continue;
            joycon.enabled = joycon.Maps == currentActionMap;
        }
    }
}