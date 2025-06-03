using UnityEngine;
using TMPro;

public class TrapperUI : MonoBehaviour
{
    [SerializeField] private GameObject wallUI;
    [SerializeField] private GameObject roadUI;
    [SerializeField] private Transform canvas;
    [SerializeField] private RectTransform redDotUI;   // 赤丸UI

    private int tileSize;
    private const int width = 21; // 迷路のサイズは何かしらの方法でとってこれるようにしたい
    private const int height = 21;
    private Vector2 UIStartPos = new Vector2(20f, 20f); // 左下の開始位置


    private void GenerateUI()
    {
        tileSize = (int)wallUI.GetComponent<RectTransform>().sizeDelta.x; // UIのタイルサイズを取得
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                Vector3 worldPos = new Vector3(x, 0.5f, y);
                // シーンの壁のあるなしで壁か通路かを判断
                GameObject tile = IsWallAtPosition(worldPos)
                    ? Instantiate(wallUI, canvas)
                    : Instantiate(roadUI, canvas);
                RectTransform rect = tile.GetComponent<RectTransform>();

                Vector2 anchoredPos = new Vector2(
                    UIStartPos.x + x * tileSize,
                    UIStartPos.y + y * tileSize
                );
                rect.anchoredPosition = anchoredPos;
            }
        }
        redDotUI = Instantiate(redDotUI, canvas);
        redDotUI.anchoredPosition = new Vector2(
            UIStartPos.x + 1 * tileSize, // 初期位置はスタート地点
            UIStartPos.y + 1 * tileSize
        );
        redDotUI.SetAsLastSibling();
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

    void Update()
    {
        Transform enemyTransform = GameObject.FindGameObjectWithTag("Avatar").transform;
        Vector3 enemyPos = enemyTransform.position;

        // UIの位置を敵の位置に合わせる
        redDotUI.anchoredPosition = new Vector2(
            UIStartPos.x + enemyPos.x * tileSize,
            UIStartPos.y + enemyPos.z * tileSize
        );

        Debug.Log($"Enemy Position: {enemyPos}, UI Position: {redDotUI.anchoredPosition}");

    }
}
