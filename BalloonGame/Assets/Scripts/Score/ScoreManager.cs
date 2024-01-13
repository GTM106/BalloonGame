using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("���W�A�C�e�����ɏ�Z�����l")]
    [SerializeField, Min(0)] int offset = 1000;

    int _score = 0;

    public void SetScore(int collectItemCount)
    {
        _score = collectItemCount * offset;
    }

    public int GetScore()
    {
        return _score;
    }
}
