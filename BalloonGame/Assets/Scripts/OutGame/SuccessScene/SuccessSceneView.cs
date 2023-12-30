using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuccessSceneView : MonoBehaviour
{
    [SerializeField, Required] Canvas _canvas = default!;
    [SerializeField] List<Image> _digitImages = default!;
    [Header("0～9の順番で設定してください")]
    [SerializeField] List<Sprite> _numberSprites = default!;
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
        foreach (var image in _digitImages)
        {
            image.gameObject.SetActive(false);
        }

        //桁数の上限を計算する。スコアが0のときは判定できないため例外とする
        int digitCount = score == 0 ? 1 : (int)Mathf.Floor(Mathf.Log10(score) + 1);

        for (int i = 0; i < digitCount; i++)
        {
            int digit = score % 10;
            score /= 10;

            _digitImages[i].sprite = _numberSprites[digit];
            _digitImages[i].gameObject.SetActive(true);
        }
    }
}
