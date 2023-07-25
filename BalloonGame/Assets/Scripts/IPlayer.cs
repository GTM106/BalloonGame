using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer
{
    public void Dash();

    public void BoostDash();

    public void Jump(Rigidbody rb);
}
