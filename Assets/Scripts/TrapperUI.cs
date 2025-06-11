using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TrapperUI : MonoBehaviour
{
    [SerializeField] private GameObject wallUI;
    [SerializeField] private GameObject roadUI;
    private Transform canvas;
    [SerializeField] private RectTransform redDotUI;   // 赤丸UI
    // private RectTransform redDotUI; // 赤丸UIのインスタンス

    private int tileSize;
    private int width; // 迷路のサイズは何かしらの方法でとってこれるようにしたい
    private int height;
    private Vector2 UIStartPos = new Vector2(20f, 20f); // 左下の開始位置
    private GameObject[,] tileUIs; // UIのタイルを保存する2D配列
    private Vector2Int lastPlayerPos;
    private bool isGenerated = false; // UIが生成されたかどうかのフラグ

    void Update()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Avatar").transform;
        if (playerTransform == null)
        {
            Debug.LogError("敵のアバターが見つかりません。シーンに配置されていることを確認してください。");
            return;
        }
        if (!isGenerated)
        {
            if (!checkSpawnable())
            {
                return; // 壁の数が足りない場合は生成しない
            }
            StartCoroutine(DelayedGenerateUI());
            isGenerated = true; // UIが生成されたフラグを立てる
        }
        Vector3 playerPos = playerTransform.position;
        // Debug.Log($"敵の位置: {playerPos}");

        // UIの位置を敵の位置に合わせる
        Vector2 anchoredPos = new Vector2(
            UIStartPos.x + playerPos.x * tileSize,
            UIStartPos.y + playerPos.z * tileSize
        );
        if (redDotUI == null)
        {
            return;
        }
        redDotUI.anchoredPosition = anchoredPos;

        Vector2Int currentPlayerPos = new Vector2Int(
            Mathf.RoundToInt(playerPos.x),
            Mathf.RoundToInt(playerPos.z)
        );
        if (currentPlayerPos == lastPlayerPos)
        {
            return;
        }
        UpdateButtonInteractable(currentPlayerPos);

    }

    public void GenerateUI()
    {
        canvas = GameObject.Find("Canvas").transform; // Canvasを取得
        GameObject mazeManager = GameObject.Find("MazeManager(Clone)");
        if (mazeManager == null)
        {
            Debug.LogError("MazeManagerが見つかりません。シーンに配置されていることを確認してください。");
            return;
        }
        width = mazeManager.GetComponent<GenerateMaze>().width; // 迷路の幅を取得
        height = mazeManager.GetComponent<GenerateMaze>().height; // 迷路の高さを取得
        tileUIs = new GameObject[width, height]; // UIのタイルを保存する2D配列
        tileSize = (int)wallUI.GetComponent<RectTransform>().sizeDelta.x; // UIのタイルサイズを取得
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                Vector3 worldPos = new Vector3(x, 0.5f, y);
                // シーンの壁のあるなしで壁か通路かを判断
                GameObject tile = IsWallAtPosition(worldPos)
                    ? Instantiate(wallUI, canvas)
                    : Instantiate(roadUI, canvas);
                RectTransform rect = tile.GetComponent<RectTransform>();

                Vector2 anchoredPos = new Vector2(
                    UIStartPos.x + x * tileSize,
                    UIStartPos.y + y * tileSize
                );
                rect.anchoredPosition = anchoredPos;
                tileUIs[x, y] = tile; // UIのタイルを保存
            }
        }
        redDotUI = Instantiate(redDotUI, canvas);
        if (redDotUI == null)
        {
            Debug.LogError("赤丸UIのPrefabが設定されていません。Inspectorで設定してください。");
            return;
        }
        redDotUI.anchoredPosition = new Vector2(
            UIStartPos.x + 1 * tileSize, // 初期位置はスタート地点
            UIStartPos.y + 1 * tileSize
        );
        Debug.Log("赤丸UIを生成しました。位置: " + redDotUI.anchoredPosition);
        redDotUI.SetAsLastSibling();
        // 最初のプレイヤー位置を設定
        lastPlayerPos = new Vector2Int(1, 1); // 初期位置はスタート地点
        UpdateButtonInteractable(lastPlayerPos);
    }

    bool IsWallAtPosition(Vector3 position)
    {
        // 半径0.1程度のBoxを中心にチェック
        Collider[] colliders = Physics.OverlapBox(position, Vector3.one * 0.1f);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Wall"))
            {
                Debug.Log($"位置 {position} に壁が存在します。");
                return true;
            }
        }
        return false;
    }

    private IEnumerator DelayedGenerateUI()
    {
        yield return new WaitForSeconds(0.01f);
        GenerateUI();
    }

    private void UpdateButtonInteractable(Vector2Int position)
    {
        // lastPlayerPosの周囲2マスをアクティブに
        for (int y = lastPlayerPos.y - 2; y <= lastPlayerPos.y + 2; y++)
        {
            for (int x = lastPlayerPos.x - 2; x <= lastPlayerPos.x + 2; x++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue; // 範囲外はスキップ
                }
                GameObject tileUI = tileUIs[x, y];
                if (tileUI != null)
                {
                    Button button = tileUI.GetComponent<Button>();
                    if (button != null)
                    {
                        if (!button.interactable)
                        {
                            Debug.Log($"座標({x}, {y})のボタンをアクティブにします。");
                        }
                        button.interactable = true; // ボタンをアクティブに
                    }
                }
            }
        }
        // 周囲2マスを非アクティブに
        for (int y = position.y - 2; y <= position.y + 2; y++)
        {
            for (int x = position.x - 2; x <= position.x + 2; x++)
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue; // 範囲外はスキップ
                }
                GameObject tileUI = tileUIs[x, y];
                if (tileUI != null)
                {
                    Button button = tileUI.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = false;
                    }
                }
            }
        }
        lastPlayerPos = position; // 最後のプレイヤー位置を更新

    }

    private bool checkSpawnable()
    {
        int leastWalls = (width * height) * 2 - 4; // 最低限必要な壁の数
        // シーンにある壁の数をカウント
        int wallCount = GameObject.FindGameObjectsWithTag("Wall").Length;
        Debug.Log($"現在の壁の数: {wallCount}");
        if (wallCount < leastWalls)
        {
            Debug.LogWarning("壁の数が足りません。生成できません。");
            return false; // 壁の数が足りない場合は生成しない
        }
        else
        {
            Debug.Log("壁の数が十分です。生成できます。");
            return true;
        }
    }

}
