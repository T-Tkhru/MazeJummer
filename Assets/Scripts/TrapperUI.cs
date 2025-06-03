using UnityEngine;
using TMPro;

public class TrapperUI : MonoBehaviour
{
    [SerializeField] private GameObject wallUI;
    [SerializeField] private GameObject roadUI;
    [SerializeField] private Transform canvas;

    private const int tileSize = 30;
    private const int width = 21;
    private const int height = 21;
    private Vector2 startPos = new Vector2(20f, 20f); // 左下の開始位置


    private void GenerateUI()
    {
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                Vector3 worldPos = new Vector3(x, 0.5f, y);
                GameObject tile = IsWallAtPosition(worldPos)
                    ? Instantiate(wallUI, canvas)
                    : Instantiate(roadUI, canvas);
                RectTransform rect = tile.GetComponent<RectTransform>();

                Vector2 anchoredPos = new Vector2(
                    startPos.x + x * tileSize,
                    startPos.y + y * tileSize
                );
                rect.anchoredPosition = anchoredPos;
            }
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
}
