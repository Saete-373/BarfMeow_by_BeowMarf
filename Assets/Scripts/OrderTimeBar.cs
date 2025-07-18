using UnityEngine;
using UnityEngine.UI;

public class OrderTimeBar : MonoBehaviour
{
    public Slider slider;
    [SerializeField] private Image _fillImage;

    public void SetMaxOrderTime(float seconds)
    {
        slider.maxValue = seconds;
        slider.value = seconds;
    }

    public void SetOrderTime(float seconds)
    {
        slider.value = seconds;

        if (seconds > slider.maxValue * 0.6)
        {
            _fillImage.color = new Color(0.635f, 1f, 0.466f);
        }
        else if (seconds > slider.maxValue * 0.4)
        {
            _fillImage.color = new Color(1f, 0.9453858f, 0.4666667f);
        }
        else if (seconds > slider.maxValue * 0.2)
        {
            _fillImage.color = new Color(1f, 0.7063209f, 0.4666667f);
        }
        else
        {
            _fillImage.color = new Color(1f, 0.4871294f, 0.4666667f);
        }
    }

}
