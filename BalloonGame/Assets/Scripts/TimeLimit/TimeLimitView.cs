using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeLimitView : MonoBehaviour
{
    [SerializeField, Required] Canvas _timeLimitCanvas = default!;
    [SerializeField, Required] Image _timeLimitBar = default!;
    [SerializeField, Required] Image _timeLimitClockHand = default!;

    readonly float positionOffset = -1850f * Screen.width / 3820f;
    const float rotationOffset = 360f;

    Vector3 _firstTimeLimitBarPos;

    private void Awake()
    {
        _firstTimeLimitBarPos = _timeLimitBar.transform.position;
    }

    public void Enable()
    {
        _timeLimitCanvas.enabled = true;
    }

    public void Disable()
    {
        _timeLimitCanvas.enabled = false;
    }

    public void UpdateView(float progress)
    {
        _timeLimitBar.fillAmount = Mathf.Clamp01(progress);
        _timeLimitBar.transform.position = _firstTimeLimitBarPos + positionOffset * (1f - progress) * Vector3.right;
        _timeLimitClockHand.transform.rotation = Quaternion.Euler(0f, 0f, rotationOffset * progress);
    }
}
