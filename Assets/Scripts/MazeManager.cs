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
                    Vector3 position = new Vector3(x, 0.5f, y);
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
    public void RpcGenerateWall(Vector3 createPos, Vector3 openPos)
    {
        Debug.Log($"RpcOpenWallが呼び出されました: createPos={createPos}, openPos={openPos}");
        // createPosに壁を生成し、openPosの壁を削除する処理を実装
        if (Runner.IsServer)
        {
            // サーバー側で壁を生成
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = createPos;
            wall.transform.localScale = new Vector3(1, 2, 1); // 壁のサイズを調整
            wall.name = "Wall";
            wall.tag = "Wall"; // 壁にタグを設定

            // openPosの壁を削除
            Collider[] hitColliders = Physics.OverlapSphere(openPos, 0.5f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Wall"))
                {
                    Destroy(hitCollider.gameObject);
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcGenerateTrap()
    {
        Debug.Log("RpcGenerateTrapが呼び出されました");
    }
}
