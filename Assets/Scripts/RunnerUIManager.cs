using System.Collections;
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
