using UnityEngine;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxTime(float seconds)
    {
        slider.maxValue = seconds;
        slider.value = seconds;
    }

    public void SetTime(float seconds)
    {
        slider.value = seconds;
    }
}
