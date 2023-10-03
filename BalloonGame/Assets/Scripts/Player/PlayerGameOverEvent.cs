using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameOverEvent : MonoBehaviour
{
    public event Action OnGameOver = default!;

    public void GameOver()
    {
        OnGameOver?.Invoke();
    }
}
