using UnityEngine;
using TMPro;

/// <summary>
/// Dev 3D Kapı Etkileşim Bileşeni.
/// Sabit BoxCollider tetikleyici üzerine konur (kapı kanadına değil).
/// Fare üzerine gelince kapı -95° açılır, iç ışık yanar, metin belirir.
/// Fare ayrılınca kapı 0° kapanır.
/// </summary>
public class Menu3DDoor : MonoBehaviour
{
    // ─── Kapı Türleri ───
    public enum DoorType
    {
        NewGame,
        Settings,
        Quit,
        SubGeneral,
        SubGraphics,
        SubAudio,
        SubLanguage,
        BackToMain
    }

    [Header("Kapı Kimliği")]
    public DoorType doorType = DoorType.NewGame;

    [Header("Menteşe (Dönen Kısım)")]
    [Tooltip("Kapı kanadını tutan menteşe objesi. Rotasyonu bu transform üzerinden yapılır.")]
    public Transform hingeTransform;

    [Header("Açı ve Hız")]
    public float openAngle  = -95f;
    public float closeAngle =   0f;
    public float rotationSpeed = 4.5f;

    [Header("3D Metin (TextMeshPro 3D)")]
    public TextMeshPro labelText;

    [Header("İç Işık")]
    public Light innerLight;
    public float lightMaxIntensity = 8.5f;

    [Header("Ses")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip clickClip;

    // ─── İç Durum ───
    private bool _hovered;
    private Quaternion _targetRot;
    private Color _baseTextColor;

    // ════════════════════════════════════════════

    private void Start()
    {
        // Başlangıç: kapı kapalı, metin görünmez, ışık sönük
        _targetRot = Quaternion.Euler(0f, closeAngle, 0f);

        if (hingeTransform != null)
            hingeTransform.localRotation = _targetRot;

        if (labelText != null)
        {
            _baseTextColor = labelText.color;
            SetTextAlpha(0f);
        }

        if (innerLight != null)
            innerLight.intensity = 0f;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Kapı rotasyonunu yumuşakça hedefe yaklaştır
        if (hingeTransform != null)
        {
            hingeTransform.localRotation = Quaternion.Slerp(
                hingeTransform.localRotation,
                _targetRot,
                Time.deltaTime * rotationSpeed
            );
        }

        // Metin alpha ve ışık intensity'sini yumuşakça geçir
        float target = _hovered ? 1f : 0f;

        if (labelText != null)
        {
            float a = Mathf.MoveTowards(labelText.color.a, target, Time.deltaTime * 4f);
            SetTextAlpha(a);
        }

        if (innerLight != null)
        {
            float targetI = _hovered ? lightMaxIntensity : 0f;
            innerLight.intensity = Mathf.MoveTowards(innerLight.intensity, targetI, Time.deltaTime * 7f);
        }
    }

    // ─── Fare Olayları (MainMenu3DController tarafından çağrılır) ───

    public void HoverEnter()
    {
        if (_hovered) return;
        _hovered = true;
        _targetRot = Quaternion.Euler(0f, openAngle, 0f);
        PlaySound(openClip);
    }

    public void HoverExit()
    {
        if (!_hovered) return;
        _hovered = false;
        _targetRot = Quaternion.Euler(0f, closeAngle, 0f);
        PlaySound(closeClip);
    }

    public void Click()
    {
        PlaySound(clickClip);
    }

    // ─── Yardımcılar ───

    private void SetTextAlpha(float a)
    {
        labelText.color = new Color(_baseTextColor.r, _baseTextColor.g, _baseTextColor.b, a);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float sfxVol = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : PlayerPrefs.GetFloat("Dink_SFXVolume", 0.8f);
            audioSource.PlayOneShot(clip, sfxVol);
        }
    }
}
