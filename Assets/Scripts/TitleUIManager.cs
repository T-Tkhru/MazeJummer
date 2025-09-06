using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    [SerializeField] private Button startGameAutoButton;
    [SerializeField] private Button startGameWithIDButton;
    [SerializeField] private TitleSoundManager titleSoundManager;

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
        titleSoundManager.PlayButtonSound();
        SceneTransitionManager.Instance.JoinGameAuto();
    }
    private void OnStartWithIDButtonClicked()
    {
        titleSoundManager.PlayButtonSound();
        SceneTransitionManager.Instance.JoinGameWithID();
    }
}

