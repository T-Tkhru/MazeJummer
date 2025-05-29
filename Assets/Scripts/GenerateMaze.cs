using CreateMaze;
using Fusion;
using UnityEngine;

public class GenerateMaze : NetworkBehaviour
{
    [SerializeField]
    private int width = 21; // 迷路の幅
    [SerializeField]
    private int height = 21; // 迷路の高さ

    private const int Wall = 1; // 壁の値
    private const int Path = 0; // 通路の値
    [SerializeField]
    private Vector3 startPosition = new Vector3(1, 0, 1); // 迷路の開始位置
    [SerializeField]
    private Vector3 goalPosition = new Vector3(19, 0, 19); // 迷路の終了位置（ゴール位置）

    // [SerializeField]
    // private NetworkPrefabRef wallPrefab; // 壁のプレハブ

    public override void Spawned()
    {
        // ホストの場合のみ迷路を生成
        if (Runner.IsServer || Object.HasStateAuthority)
        {
            Debug.Log("ホストが入室したため迷路を生成します");
            // GenerateMazeOnServer( );
        }
    }

    public void GenerateMazeOnServer(NetworkRunner runner, NetworkPrefabRef wallPrefab)
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

    // Update is called once per frame
    void Update()
    {

    }
}
