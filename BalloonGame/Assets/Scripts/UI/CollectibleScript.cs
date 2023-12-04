using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleScript : MonoBehaviour
{
    [Header("”Žš‚ÌImage‚ð“ü‚ê‚Ä‚­‚¾‚³‚¢")]
    [SerializeField] Sprite[] uiNumber = default!;
    [Header("•\Ž¦êŠ")]
    [SerializeField] Image[] displayPosition = default!;

    private int currentNumber = 0;

    public void Add(int value)
    {
        currentNumber += value;
        NumberDisplay();
    }
    private void Awake()
    {
        foreach (var item in displayPosition)
        {
            item.sprite = uiNumber[0];
        }
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
}
