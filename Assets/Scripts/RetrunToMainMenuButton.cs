using UnityEngine;

public class ReturnToMainMenuButton : MonoBehaviour
{
    public void OnClickReturn()
    {
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
