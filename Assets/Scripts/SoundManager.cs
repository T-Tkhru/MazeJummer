using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    [Header("SE Settings")]
    [SerializeField] private AudioClip buttonSE;

    [Header("BGM Settings")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip gameBGM;
    [SerializeField] private AudioClip resultBGM;

    [Header("AudioMixer Groups")]
    [SerializeField] private AudioMixerGroup bgmOutput; // インスペクタで指定
    [SerializeField] private AudioMixerGroup seOutput;

    private AudioSource bgmSource;
    private AudioSource seSource;

    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // BGM用 AudioSource
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            if (bgmOutput != null)
                bgmSource.outputAudioMixerGroup = bgmOutput;

            // SE用 AudioSource
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.loop = false;
            seSource.playOnAwake = false;
            if (seOutput != null)
                seSource.outputAudioMixerGroup = seOutput;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // シーンごとに BGM 切り替え
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Start":
                PlayBGM(titleBGM);
                break;
            case "Game":
                PlayBGM(gameBGM);
                break;
        }
    }

    // ========== 再生関数 ==========
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySE(AudioClip clip)
    {
        if (clip != null) seSource.PlayOneShot(clip);
    }

    public void PlayButtonSE()
    {
        if (buttonSE != null) seSource.PlayOneShot(buttonSE);
    }
}
