using UnityEngine;

public class RunnerUIManager : MonoBehaviour
{
    public static RunnerUIManager Instance { get; private set; }

    [SerializeField] private GameObject blindMaskPrefab;

    public GameObject BlindMask { get; private set; }

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

        BlindMask = Instantiate(blindMaskPrefab, canvas.transform);
        BlindMask.SetActive(false);
    }
}
