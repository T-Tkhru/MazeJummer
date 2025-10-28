using CreateMaze;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class MazeManager : NetworkBehaviour
{
    public int width = 25; // 迷路の幅
    public int height = 25; // 迷路の高さ

    private const int Wall = 1; // 壁の値
    private const int Path = 0; // 通路の値
    private Vector3 goalPosition;
    [SerializeField] private NetworkPrefabRef wallPrefab; // 壁のプレハブ、迷路生成に使用する
    private float wallOffset = 0.5f; // 壁のオフセット、壁の高さを考慮して0.5fに設定
    private float keyOffset = 0.25f; // 鍵のオフセット、鍵の高さを考慮して0.5fに設定
    [SerializeField] private NetworkObject speedDownTrapPrefab;
    [SerializeField] private NetworkObject blindTrapPrefab;
    [SerializeField] private NetworkObject reverseInputTrapPrefab;
    [SerializeField] private NetworkObject keyPrefab;
    [SerializeField] private NetworkObject goalPallPrefab;

    public void GenerateMazeOnServer(NetworkRunner runner)
    {
        Debug.Log("迷路生成を開始します");
        var mazeCreator = new MazeCreator_Extend(width, height);
        var maze = mazeCreator.CreateMaze();
        // 迷路の情報を文字列に変換して表示
        for (int y = 0; y < maze.GetLength(1); y++)
        {
            for (int x = 0; x < maze.GetLength(0); x++)
            {
                if (maze[x, y] == Wall)
                {
                    // 壁の位置に壁のプレハブを生成
                    Vector3 position = new Vector3(x, wallOffset, y);
                    var wall = runner.Spawn(wallPrefab, position, Quaternion.identity);
                }
            }
        }
        // 鍵を迷路の端に生成
        runner.Spawn(keyPrefab, new Vector3(width - 2, keyOffset, 1), Quaternion.Euler(-90, 0, 0));
        runner.Spawn(keyPrefab, new Vector3(1, keyOffset, height - 2), Quaternion.Euler(-90, 0, 0));

        goalPosition.x = width - 2; // ゴール位置のX座標を迷路の幅に合わせる
        goalPosition.z = height - 2; // ゴール位置のZ座標を迷路の高さに合わせる
        goalPosition.y = goalPallPrefab.transform.localScale.y * 0.5f;
        var goalPall = runner.Spawn(goalPallPrefab, goalPosition, Quaternion.identity);
        Debug.Log($"ゴールを生成しました: {goalPall.gameObject.name} at {goalPosition}");
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcGenerateWall(Vector3 createPos)
    {
        if (Runner.IsServer)
        {
            // サーバー側で壁を生成
            var wall = Runner.Spawn(wallPrefab, createPos, Quaternion.identity);
            Debug.Log($"壁を生成しました: {wall.gameObject.name} at {createPos}");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcOpenWall(Vector3 openPos)
    {
        Debug.Log($"RpcDeleteWallが呼び出されました: position={openPos}");
        if (Runner.IsServer)
        {
            Collider[] colliders = Physics.OverlapBox(openPos, Vector3.one * 0.1f);
            if (colliders.Length == 0)
            {
                Debug.LogWarning($"壁が見つかりません: position={openPos}");
                return;
            }

            foreach (var col in colliders)
            {
                if (col.CompareTag("Wall"))
                {
                    Debug.Log($"壁を削除します: {col.gameObject.name} at {openPos}");
                    // 壁を削除
                    Runner.Despawn(col.GetComponent<NetworkObject>());
                    Debug.Log($"壁を削除しました: {col.gameObject.name}");
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcGenerateSpeedDownTrap(int x, int y)
    {
        Debug.Log("RpcGenerateSpeedDownTrapが呼び出されました");
        if (Runner.IsServer)
        {
            // サーバー側でトラップを生成
            Vector3 trapPosition = new Vector3(x, 0.5f, y); // トラップの位置を設定
            var trap = Runner.Spawn(speedDownTrapPrefab, trapPosition, Quaternion.identity);
            Debug.Log($"トラップを生成しました: {trap.gameObject.name} at {trapPosition}");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcGenerateBlindTrap(int x, int y)
    {
        Debug.Log("RpcGenerateBlindTrapが呼び出されました");
        if (Runner.IsServer)
        {
            // サーバー側でトラップを生成
            Vector3 trapPosition = new Vector3(x, 0.5f, y);
            var trap = Runner.Spawn(blindTrapPrefab, trapPosition, Quaternion.identity);
            Debug.Log($"トラップを生成しました: {trap.gameObject.name} at {trapPosition}");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcGenerateReverseInputTrap(int x, int y)
    {
        Debug.Log("RpcGenerateReverseInputTrapが呼び出されました");
        if (Runner.IsServer)
        {
            // サーバー側でトラップを生成
            Vector3 trapPosition = new Vector3(x, 0.5f, y);
            var trap = Runner.Spawn(reverseInputTrapPrefab, trapPosition, Quaternion.identity);
            Debug.Log($"トラップを生成しました: {trap.gameObject.name} at {trapPosition}");
        }
    }

}
