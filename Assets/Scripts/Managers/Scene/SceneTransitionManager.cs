using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField sessionNameInputField;
    [SerializeField] private Image fadePanel;

    public static SceneTransitionManager Instance { get; private set; }
    private float fadeDuration = 0.5f;

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
        FadeOutAndLoadScene("Game", fadeDuration);
    }

    public void JoinGameWithID()
    {
        TMP_InputField sessionNameInputField = FindFirstObjectByType<TMP_InputField>();
        string inputID = sessionNameInputField.text.Trim();
        if (string.IsNullOrEmpty(inputID))
        {
            Debug.LogWarning("IDが入力されていません！");
            return;
        }

        Debug.Log($"ID指定でゲーム開始: {inputID}");
        PlayerPrefs.SetString("SessionName", inputID); // 後でScene側で読み取る
        FadeOutAndLoadScene("Game", fadeDuration);
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("メインメニューに戻ります");
        var networkRunner = FindFirstObjectByType<NetworkRunner>();
        if (networkRunner != null)
        {
            networkRunner.Shutdown();
            Destroy(networkRunner.gameObject);
        }
        FadeOutAndLoadScene("Start", fadeDuration);
    }
    /// <summary>
    /// フェードアウト後にシーン遷移し、遷移先で自動的にフェードインする
    /// </summary>
    public void FadeOutAndLoadScene(string sceneName, float duration = 1f)
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(0, 0, 0, 0);
        fadePanel.DOFade(1f, duration).OnComplete(() =>
        {
            Debug.Log($"フェードアウト完了！シーン遷移: {sceneName}");
            SceneManager.sceneLoaded += OnSceneLoadedFadeIn;
            SceneManager.LoadScene(sceneName);
        });
    }

    // シーン遷移後に自動でフェードイン
    private void OnSceneLoadedFadeIn(Scene scene, LoadSceneMode mode)
    {
        fadePanel.color = new Color(0, 0, 0, 1); // 初期は真っ黒

        // アルファを1→0に変化させる
        fadePanel.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            fadePanel.gameObject.SetActive(false);
            Debug.Log("フェードイン完了！");
        });
        SceneManager.sceneLoaded -= OnSceneLoadedFadeIn;
    }
}

