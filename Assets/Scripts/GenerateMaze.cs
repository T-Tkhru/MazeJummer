using CreateMaze;
using UnityEngine;

public class GenerateMaze : MonoBehaviour
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

    [SerializeField]
    private GameObject wallPrefab; // 壁のプレハブ

    void Start()
    {
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
                    Instantiate(wallPrefab, position, Quaternion.identity);
                }
            }
        }
        // 開始位置にボールを生成
        // GameObject startObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // startObject.transform.position = startPosition;
        // startObject.name = "StartBall";
        // startObject.GetComponent<Renderer>().material.color = Color.green; // 開始位置のボールを緑色に設定
        // ゴール位置にボールを生成
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
