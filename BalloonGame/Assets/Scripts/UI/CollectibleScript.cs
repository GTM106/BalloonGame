using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleScript : MonoBehaviour
{ 
    [Header("•\¦ãŒÀŒ…”")]
    [SerializeField] int digitNumber = default!;
    [Header("”š‚ÌImage‚ğ“ü‚ê‚Ä‚­‚¾‚³‚¢")]
    [SerializeField] Sprite[] uiNumber = default!;
    [Header("•\¦êŠ")]
    [SerializeField] Image[] displayPosition = default!;

    public int currentNumber = 0;

    private void Awake()
    {
        displayPosition[0].sprite = uiNumber[0];
    }

    private void Update()
    {
        NumberDisplay();
    }

    void NumberDisplay()
    {
        if (currentNumber < 10)
        {
            displayPosition[0].sprite = uiNumber[currentNumber];
        }
        else if (10 <= currentNumber && currentNumber < 100)
        {
            int two = currentNumber / 10;
            int one = currentNumber % 10;
            displayPosition[0].sprite = uiNumber[one];
            displayPosition[1].sprite = uiNumber[two];
        }
        else if (100 <= currentNumber && currentNumber < 1000)
        {
            int three = currentNumber / 100;
            int surplus = currentNumber % 100;
            int two = surplus / 10;
            int one = surplus % 10;

            displayPosition[0].sprite = uiNumber[one];
            displayPosition[1].sprite = uiNumber[two];
            displayPosition[2].sprite = uiNumber[three];
        }
    }
}
