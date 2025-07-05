using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;
using System.Collections.Generic;
using Fusion;

public class TrapperUIManager : MonoBehaviour
{
    [SerializeField] private GameObject wallUI;
    [SerializeField] private GameObject roadUI;
    private Transform canvas;
    [SerializeField] private RectTransform playerUI;
    [SerializeField] private RectTransform speedDownTrapUI;
    [SerializeField] private RectTransform blindTrapUI;
    [SerializeField] private RectTransform trapUI3;
    [SerializeField] private Camera subCameraPrefab;
    [SerializeField] private RenderTexture subCameraRenderTexture;
    [SerializeField] private RawImage subCameraDisplay; // サブカメラの表示用RawImage
    [SerializeField] private GameObject trapperUIPrefab;
    private Transform trapperUI; // トラッパーUIのPrefab


    private int tileSize;
    private int width; // 迷路のサイズは何かしらの方法でとってこれるようにしたい
    private int height;
    private Vector2 UIStartPos;
    private GameObject[,] tileUIs; // UIのタイルを保存する2D配列
    private GameObject[,] trapUIs; // トラップのUIを保存する2D配列
    private int[,] mazeData;
    private Vector2Int lastPlayerPos;
    private bool isGenerated = false; // UIが生成されたかどうかのフラグ
    private MazeManager mazeManager;
    private float wallOffset = 0.5f; // 壁のオフセット、壁の高さを考慮して0.5fに設定

    private enum BuildMode { None, Wall, SpeedDownTrap, BlindTrap, Trap3 }
    private BuildMode currentBuildMode = BuildMode.None;
    private HashSet<Vector2Int> trapPositions = new HashSet<Vector2Int>();

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

        // UIの位置を敵の位置に合わせる
        Vector2 anchoredPos = new Vector2(
            UIStartPos.x + playerPos.x * tileSize,
            UIStartPos.y + playerPos.z * tileSize
        );
        if (playerUI == null)
        {
            return;
        }
        playerUI.anchoredPosition = anchoredPos;

        Vector2Int currentPlayerPos = new Vector2Int(
            Mathf.RoundToInt(playerPos.x),
            Mathf.RoundToInt(playerPos.z)
        );
        if (currentPlayerPos == lastPlayerPos)
        {
            return;
        }
        UpdateUI(currentPlayerPos);

    }

    public void GenerateUI()
    {
        canvas = GameObject.Find("Canvas").transform;
        trapperUI = Instantiate(trapperUIPrefab, canvas).transform;
        AttachButtonListeners();
        mazeManager = GameObject.Find("MazeManager(Clone)").GetComponent<MazeManager>();
        if (mazeManager == null)
        {
            Debug.LogError("MazeManagerが見つかりません。シーンに配置されていることを確認してください。");
            return;
        }
        width = mazeManager.width; // 迷路の幅を取得
        height = mazeManager.height; // 迷路の高さを取得
        tileUIs = new GameObject[width, height]; // UIのタイルを保存する2D配列
        trapUIs = new GameObject[width, height]; // トラップのUIを保存する2D配列
        mazeData = new int[width, height]; // 迷路のデータを保存する2D配列
        tileSize = (int)wallUI.GetComponent<RectTransform>().sizeDelta.x; // UIのタイルサイズを取得
        int canvasHeight = (int)canvas.GetComponent<RectTransform>().sizeDelta.y;
        UIStartPos = new Vector2(
            (canvasHeight - width * tileSize) / 2 + tileSize / 2,
            (canvasHeight - height * tileSize) / 2 + tileSize / 2
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
        playerUI = Instantiate(playerUI, trapperUI);
        if (playerUI == null)
        {
            Debug.LogError("赤丸UIのPrefabが設定されていません。Inspectorで設定してください。");
            return;
        }
        playerUI.anchoredPosition = new Vector2(
            UIStartPos.x + 1 * tileSize, // 初期位置はスタート地点
            UIStartPos.y + 1 * tileSize
        );
        Debug.Log("赤丸UIを生成しました。位置: " + playerUI.anchoredPosition);
        playerUI.SetAsLastSibling();
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
        var avatarController = GameObject.FindGameObjectWithTag("Avatar").GetComponentInChildren<CinemachineInputAxisController>();
        avatarController.enabled = false; // サブカメラの表示用にCinemachineInputAxisControllerを無効化
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.RPC_ClientReady(); // 準備完了であることを通知
    }

    private void AttachButtonListeners()
    {
        Button createWallButton = trapperUI.Find("CreateWall").GetComponent<Button>();
        if (createWallButton != null)
        {
            createWallButton.onClick.AddListener(SelectMakeWall);
        }
        else
        {
            Debug.LogError("CreateWallボタンが見つかりません。トラッパーUIのPrefabを確認してください。");
        }
        Button createSpeedDownButton = trapperUI.Find("CreateSpeedDownTrap").GetComponent<Button>();
        if (createSpeedDownButton != null)
        {
            createSpeedDownButton.onClick.AddListener(SelectSpeedDownTrap);
        }
        else
        {
            Debug.LogError("CreateSpeedDownTrapボタンが見つかりません。トラッパーUIのPrefabを確認してください。");
        }
        Button createBlindTrapButton = trapperUI.Find("CreateBlindTrap").GetComponent<Button>();
        if (createBlindTrapButton != null)
        {
            createBlindTrapButton.onClick.AddListener(SelectMakeBlindTrap);
        }
        else
        {
            Debug.LogError("CreateBlindTrapボタンが見つかりません。トラッパーUIのPrefabを確認してください。");
        }
        Button createTrap3Button = trapperUI.Find("CreateTrap3").GetComponent<Button>();
        if (createTrap3Button != null)
        {
            createTrap3Button.onClick.AddListener(SelectMakeTrap3);
        }
        else
        {
            Debug.LogError("CreateTrap3ボタンが見つかりません。トラッパーUIのPrefabを確認してください。");
        }
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

    private void UpdateUI(Vector2Int position)
    {
        UpdateButtonInteractable(position);
        RemoveTrapUI(position);
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

    private void RemoveTrapUI(Vector2Int position)
    {
        foreach (var trapPos in trapPositions)
        {
            if (trapPos == position)
            {
                // トラップのUIを削除
                if (trapUIs[trapPos.x, trapPos.y] != null)
                {
                    Destroy(trapUIs[trapPos.x, trapPos.y]);
                }
                trapPositions.Remove(trapPos);
                Debug.Log($"トラップUIを削除しました: {trapPos}");
                // タイルのUIを通路に戻す
                tileUIs[trapPos.x, trapPos.y].GetComponent<Button>().enabled = true;
                break;
            }
        }
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
        switch (currentBuildMode)
        {
            case BuildMode.Wall:
                mazeData[x, y] = 1;
                CreateWall(x, y);
                break;
            case BuildMode.SpeedDownTrap:
                mazeManager.RpcGenerateSpeedDownTrap(x, y);
                CreateTrapUI(x, y, speedDownTrapUI);
                break;
            case BuildMode.BlindTrap:
                mazeManager.RpcGenerateBlindTrap(x, y);
                CreateTrapUI(x, y, blindTrapUI);
                break;
            case BuildMode.Trap3:
                mazeManager.RpcGenerateTrap3(x, y);
                CreateTrapUI(x, y, trapUI3);
                break;
            default:
                Debug.LogWarning("無効なビルドモードです。");
                return;
        }
    }

    private void CreateWall(int x, int y)
    {
        int[,] tempMazeData = (int[,])mazeData.Clone();
        var checks = new List<(string success, (int x, int y) opened)>();
        var result = SearchPath.CheckOpenWall(tempMazeData, (lastPlayerPos.x, lastPlayerPos.y), (width - 2, height - 2), (x, y));
        if (result.success == "Open")
        {
            tempMazeData[result.opened.x, result.opened.y] = 0;
        }
        checks.Add(result);
        Debug.Log($"壁を開ける結果: {result.success}, 開けた位置: {result.opened}");
        var keyPositions = GetKeyPositions();
        foreach (var keyPos in keyPositions)
        {
            var keyResult = SearchPath.CheckOpenWall(tempMazeData, (lastPlayerPos.x, lastPlayerPos.y), keyPos, (x, y));
            checks.Add(keyResult);
            Debug.Log($"鍵の位置: {keyPos}, 結果: {keyResult.success}, 開けた位置: {keyResult.opened}");
            if (result.success == "Open")
            {
                tempMazeData[result.opened.x, result.opened.y] = 0;
            }
        }

        foreach (var check in checks)
        {
            if (check.success == "Cannot")
            {
                Debug.LogWarning("壁を開けることができません。");
                mazeData[x, y] = 0; // 通路のデータを元に戻す
                return;
            }
        }

        // 壁を生成
        mazeManager.RpcGenerateWall(new Vector3(x, wallOffset, y));
        Destroy(tileUIs[x, y]); // クリックされた位置のUIを削除
        CreateWallUI(x, y); // 新しい壁UIを生成

        foreach (var check in checks)
        {
            if (check.success == "Open")
            {
                if (mazeData[check.opened.x, check.opened.y] != 1)
                {
                    Debug.LogWarning($"すでに壁ではありません: {check.opened}");
                    continue;
                }
                mazeManager.RpcOpenWall(new Vector3(check.opened.x, wallOffset, check.opened.y));
                // UIを更新
                if (tileUIs[check.opened.x, check.opened.y] != null)
                {
                    Destroy(tileUIs[check.opened.x, check.opened.y]); // 開けた位置のUIを削除
                    CreateRoadUI(check.opened.x, check.opened.y); // 新しい通路UIを生成
                    Debug.Log($"壁を開けて通路にしました: {check.opened}");
                }
            }
        }
    }

    private void CreateWallUI(int x, int y)
    {
        GameObject wallTile = Instantiate(wallUI, trapperUI);
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
        GameObject roadTile = Instantiate(roadUI, trapperUI);
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

    private void CreateTrapUI(int x, int y, RectTransform trapUI)
    {
        GameObject trapTile = Instantiate(trapUI.gameObject, trapperUI);
        RectTransform rect = trapTile.GetComponent<RectTransform>();
        Vector2 anchoredPos = new Vector2(
            UIStartPos.x + x * tileSize,
            UIStartPos.y + y * tileSize
        );
        rect.anchoredPosition = anchoredPos;
        trapUIs[x, y] = trapTile;
        trapPositions.Add(new Vector2Int(x, y)); // トラップの位置を保存
        tileUIs[x, y].GetComponent<Button>().enabled = false;
    }

    public void SelectMakeWall()
    {
        // 壁を作るボタンが押されたときの処理
        Debug.Log("壁を作るボタンが押されました。");
        currentBuildMode = BuildMode.Wall;

    }

    public void SelectSpeedDownTrap()
    {
        Debug.Log("スピードダウントラップを作るボタンが押されました。");
        currentBuildMode = BuildMode.SpeedDownTrap;
    }

    public void SelectMakeBlindTrap()
    {
        Debug.Log("トラップ2を作るボタンが押されました。");
        currentBuildMode = BuildMode.BlindTrap;
    }

    public void SelectMakeTrap3()
    {
        // トラップ3を作るボタンが押されたときの処理
        Debug.Log("トラップ3を作るボタンが押されました。");
        currentBuildMode = BuildMode.Trap3;
    }

    private List<(int x, int y)> GetKeyPositions()
    {
        // 鍵の位置を取得するメソッド
        // シーンにある鍵の位置を取得
        List<(int x, int y)> keyPositions = new List<(int x, int y)>();
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        foreach (var key in keys)
        {
            Vector3 pos = key.transform.position;
            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.z);
            keyPositions.Add((x, y));
        }
        Debug.Log($"鍵の位置数: {keyPositions.Count}");
        return keyPositions;
    }
}
