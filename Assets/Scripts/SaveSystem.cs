using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem
{
    private const string CURRENT_STAGE_KEY = "CurrentStage";
    private const string UNLOCKED_STAGE_KEY = "UnlockedStage";
    private const string CURRENT_MONEY_KEY = "CurrentMoney";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    public static void Save()
    {
        // StageManager จะเซฟข้อมูลผ่าน PlayerPrefs โดยตรง
        StageManager.Instance.Save();

        Debug.Log("Game Saved");
    }

    public static void Load()
    {
        // ตรวจสอบว่ามีข้อมูลใดๆ ที่เซฟไว้หรือไม่
        if (PlayerPrefs.HasKey(CURRENT_STAGE_KEY) &&
            PlayerPrefs.HasKey(UNLOCKED_STAGE_KEY) &&
            PlayerPrefs.HasKey(CURRENT_MONEY_KEY))
        {

            StageManager.Instance.Load();

            Debug.Log("Game Loaded");
        }
        else
        {
            Debug.Log("No save data found.");
        }
    }

    // ลบข้อมูลเซฟทั้งหมด
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(CURRENT_STAGE_KEY);
        PlayerPrefs.DeleteKey(UNLOCKED_STAGE_KEY);
        PlayerPrefs.DeleteKey(CURRENT_MONEY_KEY);
        PlayerPrefs.DeleteKey(BGM_VOLUME_KEY);
        PlayerPrefs.DeleteKey(SFX_VOLUME_KEY);
    }
}
