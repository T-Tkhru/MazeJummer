using UnityEngine;

[System.Serializable]
public class EnvironmentConfigData
{
    public string environment;
}

public static class EnvironmentConfig
{
    private static string _environment = null;
    private static bool _isLoaded = false;

    public static string GetEnvironment()
    {
        if (_isLoaded) return _environment;

        TextAsset jsonAsset = Resources.Load<TextAsset>("environment_config");

        if (jsonAsset != null)
        {
            var config = JsonUtility.FromJson<EnvironmentConfigData>(jsonAsset.text);
            _environment = config.environment;
            Debug.Log($"Environment Config Loaded: {_environment}");
        }
        else
        {
            _environment = null;
            Debug.LogWarning("Environment config not found. Using default environment.");
        }

        _isLoaded = true;
        return _environment;
    }

    public static bool IsDevelopMaster => GetEnvironment() == "develop_master";
}
