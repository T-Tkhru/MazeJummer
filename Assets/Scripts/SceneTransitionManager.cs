using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField sessionNameInputField;

    public static SceneTransitionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン間で保持
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void JoinGameAuto()
    {
        Debug.Log("自動マッチングでゲーム開始");
        PlayerPrefs.SetString("SessionName", ""); // 空にして自動マッチメイク
        SceneManager.LoadScene("Game");
    }

    public void JoinGameWithID()
    {
        string inputID = sessionNameInputField.text.Trim();
        if (string.IsNullOrEmpty(inputID))
        {
            Debug.LogWarning("IDが入力されていません！");
            return;
        }

        Debug.Log($"ID指定でゲーム開始: {inputID}");
        PlayerPrefs.SetString("SessionName", inputID); // 後でScene側で読み取る
        SceneManager.LoadScene("Game");
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("メインメニューに戻ります");
        SceneManager.LoadScene("Start");
    }
}

