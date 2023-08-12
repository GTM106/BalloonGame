using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BalloonState
{
    Normal,
    Expands
}

public class Balloon : MonoBehaviour
{
    public BalloonState state;
}
