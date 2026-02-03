using UnityEngine;

public class ReturnToMainMenuButton : MonoBehaviour
{
    private bool isGameFinished = false;
    public void OnClickReturn()
    {
        // 親要素（DisconnectedUI）を破棄
        if (transform.parent != null && !isGameFinished)
        {
            Destroy(transform.parent.gameObject);
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager が存在しません");
        }
    }

    public void SetGameFinished(bool finished)
    {
        isGameFinished = finished;
    }
}
