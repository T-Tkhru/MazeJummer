using UnityEngine;

public class TitleSoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip buttonSE;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayButtonSound()
    {
        audioSource.PlayOneShot(buttonSE);
    }
}
