using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleScript : MonoBehaviour
{
    [Header("数字のImageを入れてください")]
    [SerializeField] Sprite[] uiNumber = default!;
    [Header("表示場所")]
    [SerializeField] Image[] displayPosition = default!;

    //[Header("収集アイテム取得値がBGM_B遷移する値")]
    //[SerializeField] int bgmTrackB_ChangeCount = default!;
    //[Header("収集アイテム取得時にBGM_C遷移する値")]
    //[SerializeField] int bgmTrackC_ChangeCount = default!;

    [SerializeField, Required] ScoreManager scoreManager = default!;
    [SerializeField, Required] Canvas _canvas = default!;
    [SerializeField, Required] BGMTrackChanger _bgmTrackChanger = default!;

    //段階が増えるかもとのことなので一時的に変更、良い感じに修正お願いします by.OTOGIYA
    [Header("収集アイテムの遷移する値")]
    [SerializeField] int[] bgmTrack_ChangeCount = default!;

    private int trackNumber = 0;

    //現在のスコア
    private int currentNumber = 0;
    
    //収集したアイテムの数
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
        //表示最大数は999
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
        //OTOGIYAが勝手に書き換えた
        try
        {
            if (bgmTrack_ChangeCount[trackNumber] <= itemCount)
            {
                PlayNextTrack();
                trackNumber++;
            }
        }
        catch(IndexOutOfRangeException) {
            //遷移先がない
        }
    }

    private void PlayNextTrack()
    {
        _bgmTrackChanger.PlayNextTrack();
    }
}
