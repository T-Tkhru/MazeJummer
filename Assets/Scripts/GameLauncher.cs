using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using Mono.Cecil.Cil;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
// プレハブをインスペクターから設定できるようにする
{
    [SerializeField]
    private NetworkRunner networkRunnerPrefab; // NetworkRunnerのプレハブ、これを生成してセッション開始、必須。

    [SerializeField]
    private NetworkPrefabRef playerAvatarPrefab;
    [SerializeField]
    private NetworkPrefabRef mazeManager;
    [SerializeField]
    private NetworkPrefabRef wallPrefab; // 壁のプレハブ、迷路生成に使用する
    private NetworkRunner networkRunner; // NetworkRunnerのインスタンス、セッション開始時に生成される、ここで定義すればどこでも使える
    [Networked]
    public TickTimer Timer { get; set; } // タイマー

    private async void Start()
    {

        // NetworkRunnerを生成する
        networkRunner = Instantiate(networkRunnerPrefab);
        // シーンにあるオブジェクト数を表示
        // GameLauncherを、NetworkRunnerのコールバック対象に追加する
        networkRunner.AddCallbacks(this);
        // セッションに参加する
        var result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "MazeGameSession",
            PlayerCount = 2, // プレイヤー数を2に設定
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
        });

        if (result.Ok)
        {
            Debug.Log("成功！");
        }
        else
        {
            Debug.Log("失敗！");
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
            }
            else
            {
                Debug.LogWarning("Cinemachineカメラが見つかりませんでした");
            }
            // クライアント用俯瞰カメラを生成する
            var cameraObject = new GameObject("Camera");
            // Cameraコンポーネントを追加
            var cameraComponent = cameraObject.AddComponent<Camera>();
            // カメラの位置を設定
            cameraObject.transform.position = new Vector3(10, 20, 10); // 適当な位置に配置
            cameraObject.transform.rotation = Quaternion.Euler(90, 0, 0); // 迷路を見下ろす角度に設定
            // viewport rectを設定
            cameraComponent.rect = new Rect(0.7f, 0.7f, 0.3f, 0.3f); // ビューポートのサイズを設定
            Debug.Log("クライアント用のカメラを生成しました");
        }
    }

    private void Update() { }

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
            var maze = mazeManagerObject.GetComponent<GenerateMaze>();
            maze.GenerateMazeOnServer(runner, wallPrefab);
            PlayerData.NickName = "HostPlayer"; // ホストの名前を設定

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
    }
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // 入力の方向を、ビュー座標系からワールド座標系に変換する
        var cameraRotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        var inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        data.Direction = cameraRotation * inputDirection;
        data.Buttons.Set(NetworkInputButtons.Jump, Input.GetButton("Jump"));
        data.Look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

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
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
}