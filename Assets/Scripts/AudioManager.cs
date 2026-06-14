using UnityEngine;

/// <summary>
/// Quản lý tất cả sound effects trong game
/// Attach script này vào một GameObject trong scene
/// </summary>
public class AudioManager : MonoBehaviour
{
    // Singleton pattern
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sound Effects")]
    [Tooltip("Âm thanh khi cầm đĩa bánh lên")]
    [SerializeField] private AudioClip pickupSound;

    [Tooltip("Âm thanh khi đặt đĩa bánh xuống")]
    [SerializeField] private AudioClip placeSound;

    [Tooltip("Âm thanh khi gộp bánh")]
    [SerializeField] private AudioClip mergeSound;

    [Tooltip("Âm thanh khi click button")]
    [SerializeField] private AudioClip buttonClickSound;

    [Header("Settings")]
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tạo AudioSource nếu chưa có
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Cấu hình AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    /// <summary>
    /// Phát âm thanh khi cầm đĩa lên
    /// </summary>
    public void PlayPickupSound()
    {
        PlaySound(pickupSound);
    }

    /// <summary>
    /// Phát âm thanh khi đặt đĩa xuống
    /// </summary>
    public void PlayPlaceSound()
    {
        PlaySound(placeSound);
    }

    /// <summary>
    /// Phát âm thanh khi gộp bánh
    /// </summary>
    public void PlayMergeSound()
    {
        PlaySound(mergeSound);
    }

    /// <summary>
    /// Phát âm thanh khi click button
    /// </summary>
    public void PlayButtonClickSound()
    {
        PlaySound(buttonClickSound);
    }

    /// <summary>
    /// Method chung để phát bất kỳ AudioClip nào
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
        else if (clip == null)
        {
            Debug.LogWarning("[AudioManager] AudioClip is null! Hãy assign sound effect vào Inspector.");
        }
    }

    /// <summary>
    /// Thay đổi volume (0.0 - 1.0)
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Tắt/bật âm thanh
    /// </summary>
    public void SetMuted(bool muted)
    {
        if (audioSource != null)
        {
            audioSource.mute = muted;
        }
    }
}
