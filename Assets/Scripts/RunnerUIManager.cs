using System.Collections;
using DG.Tweening;
using ExitGames.Client.Photon.StructWrapping;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunnerUIManager : MonoBehaviour
{
    public static RunnerUIManager Instance { get; private set; }

    [SerializeField] private GameObject blindMaskPrefab;

    private GameObject blindMask;
    private Material blindMaskMaterial;
    private int blindRefCount = 0;
    private bool isBlindActive = false;
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
    private GameObject runnerUI;
    private Image[] keyIcons; // 鍵の画像を格納する配列
    private int lastDisplayedSeconds = -1; // 直前に表示した秒数
    private GameObject blindEffectUI;
    private GameObject speedDownEffectUI;
    private GameObject reverseInputEffectUI;
    private bool isDisconnected = false; // 切断状態かどうか


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvas = FindFirstObjectByType<Canvas>().transform;
        runnerUI = Instantiate(runnerUIPrefab, canvas);
        blindEffectUI = runnerUI.transform.Find("BlindEffectUI").gameObject;
        speedDownEffectUI = runnerUI.transform.Find("SpeedDownEffectUI").gameObject;
        reverseInputEffectUI = runnerUI.transform.Find("ReverseInputEffectUI").gameObject;
        blindEffectUI.SetActive(false);
        speedDownEffectUI.SetActive(false);
        reverseInputEffectUI.SetActive(false);
        timerLabel = GameObject.Find("TimerLabel").GetComponent<TextMeshProUGUI>();
        timerLabel.text = "00:00"; // 初期値

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
        runnerUI.SetActive(false);
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
        if (isDisconnected)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        if (gameManager.IsGameStarted())
        {
            countDownText.gameObject.SetActive(false);
            countDownBackground.gameObject.SetActive(false);
            roleText.gameObject.SetActive(false);
            runnerUI.SetActive(true); // タイマー表示を有効化
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
                UpdateTrapEffectUI();
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
        if (seconds != lastDisplayedSeconds)
        {
            lastDisplayedSeconds = seconds;

            // 表示更新
            int minutes = seconds / 60;
            int secondsOnly = seconds % 60;
            timerLabel.text = $"{minutes:D2}:{secondsOnly:D2}";

            // 30秒以下なら脈打ちアニメを再生
            if (seconds <= 30)
            {
                StartCoroutine(PulseOnce(timerLabel));
                timerLabel.color = Color.red;
            }
            else
            {
                timerLabel.color = Color.white;
            }
        }
    }

    private void UpdateTrapEffectUI()
    {
        var avatar = GameObject.FindGameObjectWithTag("Avatar").GetComponent<PlayerAvatar>();
        if (avatar.GetBlindTime() > 0)
        {
            if (!isBlindActive)
            {
                ActivateBlind();
            }
            blindEffectUI.SetActive(true);
            float remainingTime = avatar.GetBlindTime();
            blindEffectUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{remainingTime:F1}秒";
        }
        else
        {
            if (isBlindActive)
            {
                StartCoroutine(AnimateRadius(blindMinRadius, blindMaxRadius, blindTransitionDuration));
                isBlindActive = false;
            }
            blindEffectUI.SetActive(false);
        }
        if (avatar.GetSpeedDownTime() > 0)
        {
            speedDownEffectUI.SetActive(true);
            float remainingTime = avatar.GetSpeedDownTime();
            speedDownEffectUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{remainingTime:F1}秒";
        }
        else
        {
            speedDownEffectUI.SetActive(false);
        }
        if (avatar.GetReverseInputTime() > 0)
        {
            reverseInputEffectUI.SetActive(true);
            float remainingTime = avatar.GetReverseInputTime();
            reverseInputEffectUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{remainingTime:F1}秒";
        }
        else
        {
            reverseInputEffectUI.SetActive(false);
        }
    }

    private IEnumerator PulseOnce(TextMeshProUGUI text)
    {
        Vector3 baseScale = Vector3.one;
        Vector3 maxScale = baseScale * 1.3f;
        float pulseDuration = 0.2f;

        // 膨らむ
        float t = 0f;
        while (t < pulseDuration)
        {
            t += Time.deltaTime;
            float normalized = t / pulseDuration;
            text.transform.localScale = Vector3.Lerp(baseScale, maxScale, normalized);
            yield return null;
        }

        // 戻る
        t = 0f;
        while (t < pulseDuration)
        {
            t += Time.deltaTime;
            float normalized = t / pulseDuration;
            text.transform.localScale = Vector3.Lerp(maxScale, baseScale, normalized);
            yield return null;
        }
    }


    public void ActivateBlind()
    {
        isBlindActive = true;
        blindMask.SetActive(true);
        StartCoroutine(AnimateRadius(blindMaxRadius, blindMinRadius, blindTransitionDuration));
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
        resultRect.anchoredPosition = new Vector2(0, height); // 画面の上に配置
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
            // 色を設定
            if (i < keyCount)
            {
                keyIcons[i].color = new Color(1, 0.8f, 0.2f, 1);
            }
        }
    }

    public void SetDisconnected()
    {
        isDisconnected = true;

    }

}
