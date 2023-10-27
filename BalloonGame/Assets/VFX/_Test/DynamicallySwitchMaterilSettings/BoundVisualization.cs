using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BoundVisualization : MonoBehaviour
{
    private static readonly Color BoundsColor = new(1, 0, 0, 0.5f);

    void OnDrawGizmos()
    {
        if (TryGetComponent(out Renderer renderer))
        {
            var bounds = renderer.bounds;

            Gizmos.color = BoundsColor;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }
    }
}
