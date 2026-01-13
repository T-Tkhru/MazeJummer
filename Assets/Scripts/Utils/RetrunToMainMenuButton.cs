using UnityEngine;

public class ReturnToMainMenuButton : MonoBehaviour
{
    public void OnClickReturn()
    {
        // 親要素（DisconnectedUI）を破棄
        if (transform.parent != null)
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
}
