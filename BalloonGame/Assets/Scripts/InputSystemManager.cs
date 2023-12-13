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

        //InputSystem側のマップを変更。
        _playerInput.SwitchCurrentActionMap(maps.ToString());

        //JoyconHandler側のマップを現在のマップ値から切り替え
        foreach (var joycon in _joyconHandlers)
        {
            if (joycon == null) continue;
            joycon.enabled = joycon.Maps == currentActionMap;
        }
    }
}