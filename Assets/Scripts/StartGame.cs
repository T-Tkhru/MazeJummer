using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void joinGame()
    {
        // ゲーム開始のロジックをここに記述
        Debug.Log("ゲームを開始します。");
        SceneManager.LoadScene("Game");
    }
}
