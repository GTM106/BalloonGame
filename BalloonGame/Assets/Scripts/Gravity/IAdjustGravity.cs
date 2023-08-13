using UnityEngine;

public interface IAdjustGravity
{
    float Multiplier { get; }
    void AdjustingGravity();
}