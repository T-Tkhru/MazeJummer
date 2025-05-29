using UnityEngine;

public static class PlayerData
{
    public static string NickName
    {
        get => PlayerPrefs.GetString("NickName", "No Name");
        set => PlayerPrefs.SetString("NickName", value);
    }
}
