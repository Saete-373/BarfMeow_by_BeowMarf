using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSetting : MonoBehaviour
{
    [SerializeField] private Slider BGM_Slider;
    [SerializeField] private Slider SFX_Slider;

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private float defaultBGMVolume = 0.1f;
    private float defaultSFXVolume = 0.1f;

    void Start()
    {
        StageManager.Instance.menuSetting = this;
    }

    public void OnChangeBGMVolume()
    {
        AudioManager.instance.UpdateBGMVolume(BGM_Slider.value);
        SaveVolume();
    }

    public void OnChangeSFXVolume()
    {
        AudioManager.instance.UpdateSFXVolume(SFX_Slider.value);
        SaveVolume();
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGM_Slider.value);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SFX_Slider.value);
        PlayerPrefs.Save();
    }

    public void LoadVolume()
    {
        if (!PlayerPrefs.HasKey(BGM_VOLUME_KEY))
        {
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
        }

        if (!PlayerPrefs.HasKey(SFX_VOLUME_KEY))
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
        }

        BGM_Slider.value = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume) / defaultBGMVolume;
        SFX_Slider.value = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume) / defaultSFXVolume;
    }

    public void OpenMenuSetting()
    {
        gameObject.SetActive(true);
    }

    public void CloseMenuSetting()
    {
        gameObject.SetActive(false);
    }
}
