using CreateMaze;
using Fusion;
using UnityEngine;

public class MazeManager : NetworkBehaviour
{
    public int width = 21; // 迷路の幅
    public int height = 21; // 迷路の高さ

    private const int Wall = 1; // 壁の値
    private const int Path = 0; // 通路の値
    [SerializeField]
    private Vector3 startPosition = new Vector3(1, 0, 1); // 迷路の開始位置
    [SerializeField]
    private Vector3 goalPosition = new Vector3(19, 0, 19); // 迷路の終了位置（ゴール位置）
    [SerializeField]
    private NetworkPrefabRef wallPrefab; // 壁のプレハブ、迷路生成に使用する
    private float wallOffset = 0.5f; // 壁のオフセット、壁の高さを考慮して0.5fに設定

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

        // ゴール位置にボールを生成(これはホストでしか表示されない)
        GameObject goalObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goalPosition.x = width - 2; // ゴール位置のX座標を迷路の幅に合わせる
        goalPosition.z = height - 2; // ゴール位置のZ座標を迷路の高さに合わせる
        goalObject.transform.position = goalPosition;
        goalObject.name = "GoalBall";
        goalObject.GetComponent<Renderer>().material.color = Color.red; // ゴール位置のボールを赤色に設定
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
    public void RpcGenerateTrap1(int x, int y)
    {
        Debug.Log("RpcGenerateTrap1が呼び出されました");
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcGenerateTrap2(int x, int y)
    {
        Debug.Log("RpcGenerateTrap2が呼び出されました");
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcGenerateTrap3(int x, int y)
    {
        Debug.Log("RpcGenerateTrap3が呼び出されました");
    }
}
