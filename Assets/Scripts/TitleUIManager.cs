using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    [SerializeField] private Button startGameAutoButton;
    [SerializeField] private Button startGameWithIDButton;

    void OnEnable()
    {
        if (startGameAutoButton != null)
        {
            startGameAutoButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("Start Game Auto Button is not assigned in the inspector.");
        }

        if (startGameWithIDButton != null)
        {
            startGameWithIDButton.onClick.AddListener(OnStartWithIDButtonClicked);
        }
        else
        {
            Debug.LogError("Start Game With ID Button is not assigned in the inspector.");
        }
    }

    private void OnStartButtonClicked()
    {
        SceneTransitionManager.Instance.JoinGameAuto();
    }
    private void OnStartWithIDButtonClicked()
    {
        SceneTransitionManager.Instance.JoinGameWithID();
    }
}

