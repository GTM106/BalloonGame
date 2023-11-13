using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundSettingsData", menuName = "ScriptableObject/Sound Settings Data")]
public class SoundSettingsData : ScriptableObject
{
    [SerializeField] SoundList datas;

    public SoundList Datas => datas;
}
