using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SuccessSceneView : MonoBehaviour
{
    [SerializeField, Required] Canvas _canvas = default!;
    [SerializeField, Required] TMP_Text _text = default!;

    public void Enable()
    {
        _canvas.enabled = true;
    }

    public void Disable()
    {
        _canvas.enabled = false;
    }

    public void SetScore(int score)
    {
        _text.text = score.ToString();
    }
}
