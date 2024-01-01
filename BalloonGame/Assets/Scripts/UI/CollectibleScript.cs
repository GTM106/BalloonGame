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

    //���݂̃X�R�A
    private int currentNumber = 0;
    
    //���W�����A�C�e���̐�
    private int itemCount = 0;

    private void Awake()
    {
        foreach (var item in displayPosition)
        {
            item.sprite = uiNumber[0];
        }

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

    public void Add(int value)
    {
        AddItemCount();

        currentNumber += value;
        scoreManager.SetScore(currentNumber);
        NumberDisplay();
    }

    private void NumberDisplay()
    {
        //�\���ő吔��999
        int currentNum = Mathf.Min(999, currentNumber);

        int hundredsDigit = currentNum / 100;
        int tensAndOnes = currentNum % 100;
        int tensDigit = tensAndOnes / 10;
        int onesDigit = tensAndOnes % 10;

        displayPosition[0].sprite = uiNumber[onesDigit];
        displayPosition[1].sprite = uiNumber[tensDigit];
        displayPosition[2].sprite = uiNumber[hundredsDigit];
    }

    private void AddItemCount()
    {
        itemCount++;
        BgmTrackChange();
    }

    private void BgmTrackChange()
    {
        if (itemCount == bgmTrackC_ChangeCount || itemCount == bgmTrackB_ChangeCount)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        _bgmTrackChanger.PlayNextTrack();
    }
}
