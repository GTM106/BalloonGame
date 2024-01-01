using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleScript : MonoBehaviour
{
    [Header("������Image�����Ă�������")]
    [SerializeField] Sprite[] uiNumber = default!;
    [Header("�\���ꏊ")]
    [SerializeField] Image[] displayPosition = default!;
    [Header("���W�A�C�e���擾�l��BGM_B�J�ڂ���l")]
    [SerializeField] int bgmTrackB_ChangeCount = default!;
    [Header("���W�A�C�e���擾����BGM_C�J�ڂ���l")]
    [SerializeField] int bgmTrackC_ChangeCount = default!;
    [SerializeField, Required] ScoreManager scoreManager = default!;
    [SerializeField, Required] Canvas _canvas = default!;
    [SerializeField, Required] BGMTrackChanger _bgmTrackChanger = default!;

    List<int> bgmTrackChange = new List<int>();

    private int currentNumber = 0;
    private int itemCount = 0;
    public void Add(int value)
    {
        currentNumber += value;
        scoreManager.SetScore(currentNumber);
        NumberDisplay();
    }

    private void Awake()
    {
        foreach (var item in displayPosition)
        {
            item.sprite = uiNumber[0];
        }

        bgmTrackChange.Add(bgmTrackB_ChangeCount);
        bgmTrackChange.Add(bgmTrackC_ChangeCount);

        Disable();
    }

    public void Enable()
    {
        _canvas.enabled = true;
    }

    public void Disable()
    {
        _canvas.enabled = false;
    }

    void NumberDisplay()
    {
        int hundredsDigit = currentNumber / 100;
        int tensAndOnes = currentNumber % 100;
        int tensDigit = tensAndOnes / 10;
        int onesDigit = tensAndOnes % 10;

        int total = hundredsDigit * 100 + tensDigit * 10 + onesDigit;

        if (total > 999)
        {
            hundredsDigit = 9;
            tensDigit = 9;
            onesDigit = 9;
        }
        displayPosition[0].sprite = uiNumber[onesDigit];
        displayPosition[1].sprite = uiNumber[tensDigit];
        displayPosition[2].sprite = uiNumber[hundredsDigit];
    }

    public void SetItemCount()
    {
        itemCount++;
        BgmTrackChange();
    }

    private void BgmTrackChange()
    {
        if(itemCount == bgmTrackC_ChangeCount || itemCount == bgmTrackB_ChangeCount)
        {
            PlayNextTrack();
        }
    }

    void PlayNextTrack()
    {
        _bgmTrackChanger.PlayNextTrack();
    }
}
