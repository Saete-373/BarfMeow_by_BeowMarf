using UnityEngine;
using UnityEngine.UI;

public class MoneyBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxMoney(int money)
    {
        slider.maxValue = money;
        slider.value = 0;
    }

    public void SetMoney(int money)
    {
        slider.value = money;
    }
}
