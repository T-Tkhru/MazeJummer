using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Mono.Cecil.Cil;
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
    private GenerateMaze mazeGenerator;
    [SerializeField]
    private NetworkPrefabRef wallPrefab; // 壁のプレハブ、迷路生成に使用する
    private NetworkRunner networkRunner; // NetworkRunnerのインスタンス、セッション開始時に生成される、ここで定義すればどこでも使える

    [Networked]
    public TickTimer Timer { get; set; } // タイマー

    private async void Start()
    {

        // ランダムなプレイヤー名を設定する
        // PlayerData.NickName = $"Player{UnityEngine.Random.Range(0, 10000)}";
        // Debug.Log($"プレイヤー名を {PlayerData.NickName} に設定しました。");
        // NetworkRunnerを生成する（プレハブなので）
        networkRunner = Instantiate(networkRunnerPrefab);
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
    }

    private void Update()
    {
        // Debug.Log(networkRunner.SessionInfo.ToString());
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // ホスト（サーバー兼クライアント）かどうかはIsServerで判定できる
        if (!runner.IsServer)
        {
            return;
        }
        // プレイヤーのアバターを生成する位置を決める
        Vector3 spawnPosition = Vector3.zero;

        // ホストとクライアントでスポーン位置を変える
        // プレイヤーIDを使って、最初のプレイヤーと2人目以降で分岐する
        if (player.PlayerId == 1)
        {
            // 最初のプレイヤー（ホスト）
            spawnPosition = new Vector3(1, 1, 1);
            // 迷路の生成
            mazeGenerator.GenerateMazeOnServer(runner, wallPrefab);
            PlayerData.NickName = "HostPlayer"; // ホストの名前を設定
        }
        else
        {
            // 2人目以降（クライアント）
            spawnPosition = new Vector3(1, 1, -1);
            PlayerData.NickName = "ClientPlayer"; // クライアントの名前を設定
        }
        var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
        Debug.Log($"プレイヤー {player.PlayerId} が参加しました。アバターを{spawnPosition} で生成しました。");
        // プレイヤー（PlayerRef）とアバター（NetworkObject）を関連付ける
        runner.SetPlayerObject(player, avatar);
    }
    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
}