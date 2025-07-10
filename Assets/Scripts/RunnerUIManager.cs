using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunnerUIManager : MonoBehaviour
{
    public static RunnerUIManager Instance { get; private set; }

    [SerializeField] private GameObject blindMaskPrefab;

    public GameObject blindMask { get; private set; }
    private Material blindMaskMaterial;
    private int blindRefCount = 0;
    private float blindTransitionDuration = 0.5f; // 縮小・拡大の時間
    private float blindMinRadius = 0.2f;
    private float blindMaxRadius = 1.2f;
    private TextMeshProUGUI CountDownText;
    private Image countDownBackground;
    private GameManager gameManager;
    private TextMeshProUGUI timerLabel; // タイマー表示用のTextMeshProUGUIコンポーネント
    private bool isResultUIOpen = false; // 結果UIが開いているかどうか
    [SerializeField] private GameObject resultUIPrefab; // 結果UIのPrefab
    private Transform canvas; // Canvasの参照
    [SerializeField] private GameObject runnerUIPrefab; // RunnerUIのPrefab
    private Transform runnerUI; // RunnerUIのインスタンス


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvas = FindFirstObjectByType<Canvas>().transform;
        runnerUI = Instantiate(runnerUIPrefab, canvas).transform;
        timerLabel = GameObject.Find("TimerLabel").GetComponent<TextMeshProUGUI>();
        timerLabel.text = "00:00"; // 初期値
        timerLabel.gameObject.SetActive(false); // 初期状態では非表示

        // BlindMask を生成して Canvas に配置
        if (canvas == null)
        {
            Debug.LogError("Canvasがシーンに存在しません");
            return;
        }

        blindMask = Instantiate(blindMaskPrefab, canvas);
        var img = blindMask.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("BlindMaskPrefabにImageコンポーネントがありません");
            return;
        }
        blindMaskMaterial = Instantiate(img.material);
        img.material = blindMaskMaterial;
        blindMaskMaterial.SetFloat("_Radius", blindMaxRadius);
        blindMask.SetActive(false);

    }
    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CountDownText = GameObject.Find("CountDownText").GetComponent<TextMeshProUGUI>();
        countDownBackground = GameObject.Find("CountDownBackground").GetComponent<Image>();
        if (CountDownText == null || countDownBackground == null)
        {
            Debug.LogError("CountDownTextまたはCountDownBackgroundが見つかりません。");
            return;
        }
    }

    private void Update()
    {
        if (gameManager == null) return;
        if (gameManager.IsGameStarted())
        {
            CountDownText.gameObject.SetActive(false);
            countDownBackground.gameObject.SetActive(false);
            timerLabel.gameObject.SetActive(true); // タイマー表示を有効化
            if (gameManager.IsGameFinished())
            {
                if (!isResultUIOpen)
                {
                    isResultUIOpen = true; // 結果UIが開いている状態にする
                    OpenResultUI(gameManager.IsRunnerWin());
                }
                return;
            }
            else
            {
                UpdateTimerDisplay(); // タイマーの表示を更新
            }
        }
        else
        {
            int countdown = gameManager.GetCountdownSeconds();
            if (countdown > 0)
            {
                CountDownText.text = countdown.ToString();
                CountDownText.gameObject.SetActive(true);
                countDownBackground.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        float remaining = gameManager.GetRemainingTime();
        int seconds = Mathf.FloorToInt(300f - remaining);
        int minutes = seconds / 60;
        int secondsOnly = seconds % 60;

        timerLabel.text = $"{minutes:D2}:{secondsOnly:D2}";
    }

    public void ActivateBlind(float duration)
    {
        blindRefCount++;
        blindMask.SetActive(true);
        StartCoroutine(HandleBlindEffect(duration));
    }

    private IEnumerator HandleBlindEffect(float duration)
    {
        if (blindRefCount == 1)
        {
            yield return StartCoroutine(AnimateRadius(blindMaxRadius, blindMinRadius, blindTransitionDuration));
        }
        yield return new WaitForSeconds(duration);
        blindRefCount--;
        if (blindRefCount <= 0)
        {
            blindRefCount = 0;
            yield return StartCoroutine(AnimateRadius(blindMinRadius, blindMaxRadius, blindTransitionDuration));
            blindMask.SetActive(false);
        }
    }


    private IEnumerator AnimateRadius(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = Mathf.Lerp(from, to, t);
            blindMaskMaterial.SetFloat("_Radius", value);
            yield return null;
        }
        blindMaskMaterial.SetFloat("_Radius", to);
    }

    private void OpenResultUI(bool isRunnerWin)
    {
        var resultUI = Instantiate(resultUIPrefab, canvas);
        Transform winLoseTextTransform = resultUI.transform.Find("WinLoseText");
        if (winLoseTextTransform != null)
        {
            TextMeshProUGUI winLoseText = winLoseTextTransform.GetComponent<TextMeshProUGUI>();
            if (winLoseText != null)
            {
                winLoseText.text = isRunnerWin ? "あなたの勝ちです！" : "あなたの負けです…";
            }
            else
            {
                Debug.LogError("WinLoseTextオブジェクトにTextMeshProUGUIコンポーネントが見つかりません。");
            }
        }
        else
        {
            Debug.LogError("ResultUI内にWinLoseTextという名前のオブジェクトが見つかりません。");
        }

        Transform timerTextTransform = resultUI.transform.Find("TimerText");
        if (timerTextTransform != null)
        {
            TextMeshProUGUI timerText = timerTextTransform.GetComponent<TextMeshProUGUI>();
            if (timerText != null)
            {
                float remainingTime = gameManager.GetRemainingTime();
                int seconds = Mathf.FloorToInt(300f - remainingTime);
                int minutes = seconds / 60;
                int secondsOnly = seconds % 60;
                timerText.text = $"かかった時間：{minutes:D2}:{secondsOnly:D2}";
            }
            else
            {
                Debug.LogError("TimerTextオブジェクトにTextMeshProUGUIコンポーネントが見つかりません。");
            }
        }
        else
        {
            Debug.LogError("ResultUI内にTimerTextという名前のオブジェクトが見つかりません。");
        }
    }

}
