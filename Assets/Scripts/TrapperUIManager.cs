using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using ExitGames.Client.Photon.StructWrapping;
using Unity.Cinemachine;

public class TrapperUIManager : MonoBehaviour
{
    [SerializeField] private GameObject wallUI;
    [SerializeField] private GameObject roadUI;
    private Transform canvas;
    [SerializeField] private RectTransform redDotUI;
    [SerializeField] private Camera subCameraPrefab;
    [SerializeField] private RenderTexture subCameraRenderTexture;
    [SerializeField] private RawImage subCameraDisplay; // サブカメラの表示用RawImage


    private int tileSize;
    private int width; // 迷路のサイズは何かしらの方法でとってこれるようにしたい
    private int height;
    private Vector2 UIStartPos;
    private GameObject[,] tileUIs; // UIのタイルを保存する2D配列
    private int[,] mazeData;
    private Vector2Int lastPlayerPos;
    private bool isGenerated = false; // UIが生成されたかどうかのフラグ
    private MazeManager mazeManager;
    private float wallOffset = 0.5f; // 壁のオフセット、壁の高さを考慮して0.5fに設定

    private enum BuildMode { None, Wall, Trap1, Trap2, Trap3 }
    private BuildMode currentBuildMode = BuildMode.None;

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
        canvas = GameObject.Find("TrapperUI").transform;
        mazeManager = GameObject.Find("MazeManager(Clone)").GetComponent<MazeManager>();
        if (mazeManager == null)
        {
            Debug.LogError("MazeManagerが見つかりません。シーンに配置されていることを確認してください。");
            return;
        }
        width = mazeManager.width; // 迷路の幅を取得
        height = mazeManager.height; // 迷路の高さを取得
        tileUIs = new GameObject[width, height]; // UIのタイルを保存する2D配列
        mazeData = new int[width, height]; // 迷路のデータを保存する2D配列
        tileSize = (int)wallUI.GetComponent<RectTransform>().sizeDelta.x; // UIのタイルサイズを取得
        int canvasHeight = (int)canvas.GetComponent<RectTransform>().sizeDelta.y;
        UIStartPos = new Vector2(
            (canvasHeight - width * tileSize) / 2 + tileSize / 2,
            (canvasHeight - width * tileSize) / 2 + tileSize / 2
        ); // UIの開始位置を計算
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int px = x;
                int py = y;
                Vector3 worldPos = new Vector3(x, wallOffset, y);
                if (IsWallAtPosition(worldPos))
                {
                    CreateWallUI(px, py);
                }
                else
                {
                    CreateRoadUI(px, py);
                }
                RectTransform rect = tileUIs[x, y].GetComponent<RectTransform>();

                Vector2 anchoredPos = new Vector2(
                    UIStartPos.x + x * tileSize,
                    UIStartPos.y + y * tileSize
                );
                rect.anchoredPosition = anchoredPos;

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

        // サブカメラ生成とRawImage表示のセットアップ
        Camera subCam = Instantiate(subCameraPrefab);
        subCam.targetTexture = subCameraRenderTexture;

        RawImage rawImage = GameObject.Find("SubCameraScreen").GetComponent<RawImage>();
        rawImage.texture = subCameraRenderTexture;
        rawImage.rectTransform.anchoredPosition = new Vector2(-256, -102);
        rawImage.rectTransform.sizeDelta = new Vector2(512, 512);
        var avatarController = GameObject.FindGameObjectWithTag("Avatar").GetComponent<CinemachineInputAxisController>();
        avatarController.enabled = false; // サブカメラの表示用にCinemachineInputAxisControllerを無効化
    }

    bool IsWallAtPosition(Vector3 position)
    {
        // 半径0.1程度のBoxを中心にチェック
        Collider[] colliders = Physics.OverlapBox(position, Vector3.one * 0.1f);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Wall"))
            {
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

    public void OnClickRoadButton(int x, int y)
    {
        mazeData[x, y] = 1;
        switch (currentBuildMode)
        {
            case BuildMode.Wall:
                var result = SearchPath.CheckOpenWall(mazeData, (lastPlayerPos.x, lastPlayerPos.y), (width - 2, height - 2), (x, y));
                if (result.success == "NeedNot")
                {
                    mazeManager.RpcGenerateWall(new Vector3(x, wallOffset, y));
                    Destroy(tileUIs[x, y]); // クリックされた位置のUIを削除
                    tileUIs[x, y] = Instantiate(wallUI, canvas); // 新しい通路UIを生成
                    RectTransform rect = tileUIs[x, y].GetComponent<RectTransform>();
                    Vector2 anchoredPos = new Vector2(
                        UIStartPos.x + x * tileSize,
                        UIStartPos.y + y * tileSize
                    );
                    rect.anchoredPosition = anchoredPos;
                    return;
                }
                Debug.Log($"OpenWall Result: {result.success}, Opened Position: {result.opened}");
                if (result.success == "Cannot")
                {
                    Debug.LogWarning("壁を開けることができません。");
                    mazeData[x, y] = 0; // 通路のデータを元に戻す
                    return;
                }
                mazeManager.RpcGenerateWall(new Vector3(x, wallOffset, y));
                mazeManager.RpcOpenWall(new Vector3(result.opened.x, wallOffset, result.opened.y));
                // UIを更新
                if (tileUIs[x, y] != null)
                {
                    Destroy(tileUIs[x, y]); // クリックされた位置のUIを削除
                    CreateWallUI(x, y); // 新しい壁UIを生成

                    Destroy(tileUIs[result.opened.x, result.opened.y]); // 開けた位置のUIを削除
                    CreateRoadUI(result.opened.x, result.opened.y); // 新しい通路UIを生成
                }
                break;
            case BuildMode.Trap1:
                mazeManager.RpcGenerateTrap1(x, y);
                break;
            case BuildMode.Trap2:
                mazeManager.RpcGenerateTrap2(x, y);
                break;
            case BuildMode.Trap3:
                mazeManager.RpcGenerateTrap3(x, y);
                break;
            default:
                Debug.LogWarning("無効なビルドモードです。");
                return;
        }
    }

    private void CreateWallUI(int x, int y)
    {
        GameObject wallTile = Instantiate(wallUI, canvas);
        RectTransform rect = wallTile.GetComponent<RectTransform>();
        Vector2 anchoredPos = new Vector2(
            UIStartPos.x + x * tileSize,
            UIStartPos.y + y * tileSize
        );
        rect.anchoredPosition = anchoredPos;
        tileUIs[x, y] = wallTile;
        mazeData[x, y] = 1; // 壁のデータを更新
    }

    private void CreateRoadUI(int x, int y)
    {
        GameObject roadTile = Instantiate(roadUI, canvas);
        roadTile.GetComponent<Button>().onClick.AddListener(() => OnClickRoadButton(x, y));
        RectTransform rect = roadTile.GetComponent<RectTransform>();
        Vector2 anchoredPos = new Vector2(
            UIStartPos.x + x * tileSize,
            UIStartPos.y + y * tileSize
        );
        rect.anchoredPosition = anchoredPos;
        tileUIs[x, y] = roadTile;
        mazeData[x, y] = 0; // 通路のデータを更新
    }

    public void SelectMakeWall()
    {
        // 壁を作るボタンが押されたときの処理
        Debug.Log("壁を作るボタンが押されました。");
        currentBuildMode = BuildMode.Wall;

    }

    public void SelectMakeTrap1()
    {
        // トラップ1を作るボタンが押されたときの処理
        Debug.Log("トラップ1を作るボタンが押されました。");
        currentBuildMode = BuildMode.Trap1;
    }

    public void SelectMakeTrap2()
    {
        // トラップ2を作るボタンが押されたときの処理
        Debug.Log("トラップ2を作るボタンが押されました。");
        currentBuildMode = BuildMode.Trap2;
    }

    public void SelectMakeTrap3()
    {
        // トラップ3を作るボタンが押されたときの処理
        Debug.Log("トラップ3を作るボタンが押されました。");
        currentBuildMode = BuildMode.Trap3;
    }
}
