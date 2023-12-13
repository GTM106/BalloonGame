using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeLimitView : MonoBehaviour
{
    [SerializeField, Required] Canvas _timeLimitCanvas = default!;
    [SerializeField, Required] TMP_Text _text;

    public void Enable()
    {
        _timeLimitCanvas.enabled = true;
    }

    public void Disable()
    {
        _timeLimitCanvas.enabled = false;
    }

    public void UpdateText(string text)
    {
        _text.text = text;
    }
}
