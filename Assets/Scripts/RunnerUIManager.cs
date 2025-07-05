using System.Collections;
using Fusion;
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
    [SerializeField] private TextMeshProUGUI timerLabelPrefab; // タイマー表示用のTextMeshProUGUIコンポーネント
    private TextMeshProUGUI timerLabel; // タイマー表示用のTextMeshProUGUIコンポーネント


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // BlindMask を生成して Canvas に配置
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvasがシーンに存在しません");
            return;
        }

        blindMask = Instantiate(blindMaskPrefab, canvas.transform);
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
        // タイマー表示用のTextMeshProUGUIコンポーネントを生成
        if (timerLabelPrefab != null)
        {
            timerLabel = Instantiate(timerLabelPrefab, canvas.transform);
            timerLabel.text = "00:00"; // 初期値
            timerLabel.gameObject.SetActive(false); // 初期状態では非表示
        }
        else
        {
            Debug.LogError("timerLabelPrefabが設定されていません。");
        }

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
                return; // ゲームが終了している場合は何もしない
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

}
