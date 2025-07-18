using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
// プレハブをインスペクターから設定できるようにする
{
    [SerializeField] private NetworkRunner networkRunnerPrefab; // NetworkRunnerのプレハブ、これを生成してセッション開始、必須。
    [SerializeField] private NetworkPrefabRef playerAvatarPrefab;
    [SerializeField] private NetworkPrefabRef mazeManager;
    private NetworkRunner networkRunner; // NetworkRunnerのインスタンス、セッション開始時に生成される、ここで定義すればどこでも使える
    [Networked] public TickTimer Timer { get; set; } // タイマー
    [SerializeField] private GameObject trapperUIManager;
    [SerializeField] private GameObject runnerUIManager;
    [SerializeField] private string sessionName; // セッション名デバッグ用
    [SerializeField] private GameObject sceneTransitionManagerPrefab; // シーン遷移マネージャーのプレハブ
    [SerializeField] private GameObject disconnectedUIPrefab; // MazeManagerのプレハブ

    private async void Start()
    {
#if UNITY_EDITOR
        string sessionPrefix;
        if (EnvironmentConfig.IsDevelopMaster)
        {
            sessionPrefix = "master_dev_";
            Debug.Log("開発マスター環境で実行中です。セッション名のプレフィックスを 'master_dev_' に設定します。");
        }
        else
        {
            sessionPrefix = "dev_";
            Debug.Log("開発環境で実行中です。セッション名のプレフィックスを 'dev_' に設定します。");
        }
        if (SceneTransitionManager.Instance == null) // Gameシーンから開始したときに呼ばれる、インスペクタで指定したsessionNameを使用する
        {
            Instantiate(sceneTransitionManagerPrefab);
            Debug.Log("SceneTransitionManager を自動生成しました。エディターで実行中です。");
            sessionName = sessionPrefix + sessionName;
        }
        else // Startシーンから開始したときに呼ばれる、GetSessionName()を使用する
        {
            Debug.Log("SceneTransitionManager はすでに存在します。エディターで実行中です。");
            sessionName = $"{sessionPrefix}{GetSessionName()}";
        }
#else
            Debug.Log("Unity Editor以外の環境では、プレフィックスなしのセッション名を使用します。");
            sessionName = GetSessionName();
#endif
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.AddCallbacks(this);


        // セッションに参加する
        var result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = sessionName,
            PlayerCount = 2, // プレイヤー数を2に設定
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
        });
        Debug.Log($"セッション開始結果: {result}");

        if (result.Ok)
        {
            Debug.Log("成功！");
            Debug.Log($"セッション: {networkRunner.SessionInfo.Name}");
        }
        else if (result.ShutdownReason == ShutdownReason.GameIsFull)
        {
            Debug.Log("セッションが満員です。");
            TextMeshProUGUI countDownText = GameObject.Find("CountDownText").GetComponent<TextMeshProUGUI>();
            countDownText.text = "このIDはすでに別のルームで使用されています。";
            StartCoroutine(WaitAndLoadStartScene(3f)); // 3秒後にスタートシーンに戻る
        }
        if (networkRunner.IsClient)
        {
            // cinemachineのカメラを無効化する
            var cinemachineCamera = FindFirstObjectByType<Camera>().GetComponent<CinemachineBrain>();
            if (cinemachineCamera != null)
            {
                // cinemachineのカメラを無効化する
                cinemachineCamera.enabled = false;
                Debug.Log("Cinemachineカメラを無効化しました");
                var camera = Camera.main;
                camera.transform.position = new Vector3(13, 25, 13);
                camera.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
            Instantiate(trapperUIManager); // クライアント用のUIを生成
        }
        else if (networkRunner.IsServer)
        {
            Instantiate(runnerUIManager);
        }
    }

    private IEnumerator WaitAndLoadStartScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("Start");
    }

    private String GetSessionName()
    {
        // PlayerPrefsからセッション名を取得する
        string sessionName = PlayerPrefs.GetString("SessionName", "");
        if (string.IsNullOrEmpty(sessionName))
        {
            Debug.LogWarning("セッション名が設定されていません。自動マッチングを使用します。");
            return null;
        }
        else
        {
            Debug.Log($"セッション名: {sessionName}");
            return sessionName;
        }
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
        {
            return;
        }
        // プレイヤーIDを使って、最初のプレイヤーと2人目以降で分岐する
        if (player.PlayerId == 1)
        {
            // 最初のプレイヤー（ホスト）
            Vector3 spawnPosition = new Vector3(1, 5, 1);
            // MazeManagerを生成して、迷路を生成する
            var mazeManagerObject = runner.Spawn(mazeManager, spawnPosition, Quaternion.identity);
            var maze = mazeManagerObject.GetComponent<MazeManager>();
            maze.GenerateMazeOnServer(runner);


            var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
            Debug.Log($"プレイヤー {player.PlayerId} が参加しました。アバターを{spawnPosition} で生成しました。");
            // プレイヤー（PlayerRef）とアバター（NetworkObject）を関連付ける
            runner.SetPlayerObject(player, avatar);
        }
        else
        {
            // 2人目以降（クライアント）
            Debug.Log($"プレイヤー {player.PlayerId} が参加しました。アバターを生成しません");
        }

    }
    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"プレイヤー {player.PlayerId} が退出しました。");
        Transform canvas = FindFirstObjectByType<Canvas>().transform;
        Instantiate(disconnectedUIPrefab, canvas);
        FindAnyObjectByType<RunnerUIManager>().SetDisconnected();
        StartCoroutine(ReturnToTitleAfterDelay(5f)); // 5秒後にタイトルへ戻る

    }
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // 入力の方向を、ビュー座標系からワールド座標系に変換する
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        data.Direction = cameraRotation * inputDirection;
        data.Buttons.Set(NetworkInputButtons.Jump, Input.GetButton("Jump"));

        input.Set(data);
    }
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        // ホストマイグレーションが発生した場合の処理（タイトルに戻るよう促す）
        // リザルトが表示されていない場合のみ、タイトルへ戻る
        if (!TrapperUIManager.Instance.GetIsResultUiOpen())
        {
            Transform canvas = FindFirstObjectByType<Canvas>().transform;
            Instantiate(disconnectedUIPrefab, canvas);
            FindAnyObjectByType<TrapperUIManager>().SetDisconnected();
            StartCoroutine(ReturnToTitleAfterDelay(5f)); // 5秒後にタイトルへ戻る
        }

    }
    private IEnumerator ReturnToTitleAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        // タイトルへ遷移
        SceneTransitionManager.Instance.ReturnToMainMenu();
    }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
}