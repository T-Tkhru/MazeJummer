using System.Collections;
using DG.Tweening;
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
    private TextMeshProUGUI countDownText;
    private TextMeshProUGUI roleText;
    private Image countDownBackground;
    private GameManager gameManager;
    private TextMeshProUGUI timerLabel; // タイマー表示用のTextMeshProUGUIコンポーネント
    private bool isResultUIOpen = false; // 結果UIが開いているかどうか
    [SerializeField] private GameObject resultUIPrefab; // 結果UIのPrefab
    private Transform canvas; // Canvasの参照
    [SerializeField] private GameObject runnerUIPrefab; // RunnerUIのPrefab
    private Image[] keyIcons; // 鍵の画像を格納する配列


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvas = FindFirstObjectByType<Canvas>().transform;
        Instantiate(runnerUIPrefab, canvas);
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
        blindMask.transform.SetAsFirstSibling();
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

        keyIcons = GameObject.Find("KeyIcons").GetComponentsInChildren<Image>();
        if (keyIcons == null || keyIcons.Length == 0)
        {
            Debug.LogError("KeyImageの子オブジェクトが見つかりません。");
        }
        else
        {
            foreach (var keyIcon in keyIcons)
            {
                keyIcon.gameObject.SetActive(false); // 初期状態では非表示
                Debug.Log($"KeyImage: {keyIcon.name} が見つかりました。");
            }
        }

    }
    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        countDownText = GameObject.Find("CountDownText").GetComponent<TextMeshProUGUI>();
        countDownBackground = GameObject.Find("CountDownBackground").GetComponent<Image>();
        roleText = GameObject.Find("RoleText").GetComponent<TextMeshProUGUI>();
        if (countDownText == null || countDownBackground == null)
        {
            Debug.LogError("CountDownTextまたはCountDownBackgroundが見つかりません。");
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (gameManager == null) return;
        if (gameManager.IsGameStarted())
        {
            countDownText.gameObject.SetActive(false);
            countDownBackground.gameObject.SetActive(false);
            roleText.gameObject.SetActive(false);
            timerLabel.gameObject.SetActive(true); // タイマー表示を有効化
            if (gameManager.IsGameFinished())
            {
                timerLabel.gameObject.SetActive(false); // タイマー表示を無効化
                if (!isResultUIOpen)
                {
                    isResultUIOpen = true; // 結果UIが開いている状態にする
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
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
            if (countdown > 3)
            {
                countDownText.text = "マッチングしました!\nまもなくゲームを開始します!";
                countDownText.gameObject.SetActive(true);
                countDownBackground.gameObject.SetActive(true);
                roleText.text = "あなたはランナーです";

            }
            else if (countdown > 0)
            {
                countDownText.text = countdown.ToString();
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        float remaining = gameManager.GetRemainingTime();
        if (remaining <= 0)
        {
            timerLabel.text = "00:00";
            return;
        }

        int seconds = Mathf.CeilToInt(remaining);
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
        var resultUIs = Instantiate(resultUIPrefab, canvas);
        resultUIs.transform.SetAsLastSibling(); // 最前面に表示
        GameObject result = resultUIs.transform.Find("Result").gameObject;
        RectTransform resultRect = result.GetComponent<RectTransform>();
        result.SetActive(false);
        float height = ((RectTransform)resultRect.parent).rect.height;
        Debug.Log("offscreenY: " + height);
        resultRect.anchoredPosition = new Vector2(0, height); // 画面の上に配置
        Debug.Log("$Screen.height: " + Screen.height);
        Transform winLoseTextTransform = result.transform.Find("WinLoseText");

        if (winLoseTextTransform != null)
        {
            TextMeshProUGUI winLoseText = winLoseTextTransform.GetComponent<TextMeshProUGUI>();
            if (winLoseText != null)
            {
                winLoseText.text = isRunnerWin ? "あなたの勝ちです！" : "あなたの負けです…";
            }
            else
            {
                Debug.LogError("WinLoseTextのTextMeshProUGUIコンポーネントが見つかりません");
            }
        }
        else
        {
            Debug.LogError("WinLoseTextが見つかりません");
        }

        Transform timerTextTransform = result.transform.Find("TimerText");
        if (timerTextTransform != null)
        {
            TextMeshProUGUI timerText = timerTextTransform.GetComponent<TextMeshProUGUI>();
            if (timerText != null)
            {
                float remainingTime = gameManager.GetRemainingTime();
                int timeLimit = gameManager.GetTimeLimit();
                int seconds = Mathf.FloorToInt(timeLimit - remainingTime);
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
        StartCoroutine(WaitForResultUI(result, resultRect));

    }

    private IEnumerator WaitForResultUI(GameObject resultUI, RectTransform resultRect)
    {
        yield return new WaitForSeconds(2f); // 2秒待機
        resultUI.SetActive(true);
        resultRect.DOAnchorPos(Vector2.zero, 1.5f).SetEase(Ease.OutBounce);
    }

    public void UpdateKeyDisplay(int keyCount)
    {
        for (int i = 0; i < keyIcons.Length; i++)
        {
            keyIcons[i].gameObject.SetActive(i < keyCount);
        }
    }

}
