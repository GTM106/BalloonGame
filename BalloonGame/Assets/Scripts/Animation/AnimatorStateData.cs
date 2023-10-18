﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimatorStateData", menuName = "State Data/Animator State Data")]
public class AnimatorStateData : ScriptableObject
{
    public List<string> stateNames = new();
}
