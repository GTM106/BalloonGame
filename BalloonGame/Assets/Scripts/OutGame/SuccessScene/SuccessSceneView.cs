using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuccessSceneView : MonoBehaviour
{
    [SerializeField, Required] Canvas _canvas = default!;
    [SerializeField] List<Image> _digitImages = default!;
    [Header("0�`9�̏��ԂŐݒ肵�Ă�������")]
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

        //�����̏�����v�Z����B�X�R�A��0�̂Ƃ��͔���ł��Ȃ����ߗ�O�Ƃ���
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
