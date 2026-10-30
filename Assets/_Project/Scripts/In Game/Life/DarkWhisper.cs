using UnityEngine;

/// <summary>
/// Zifiri karanlıkta kulağın dibinde kısa bir fısıltı: "Don't turn back", "Run" ya da anlaşılmaz bir mırıltı.
/// Her karanlıkta değil (şans), aynısı üst üste gelmez; ses oyuncunun arkasından, bir omuz yanından gelir.
/// Bütün sesler karanlıkta durduğu için bu kaynak AudioListener duraklamasını yok sayar.
/// </summary>
public class DarkWhisper : MonoBehaviour
{
    [SerializeField] private HallwayLightTimer timer;
    [SerializeField] private AudioClip[] whispers;
    [SerializeField, Range(0f, 1f)] private float chance = 0.6f;
    [SerializeField] private Vector2 delay = new Vector2(0.25f, 0.6f);  // karanlık çöktükten sonra
    [SerializeField] private float volume = 0.55f;

    private AudioSource source;
    private Transform player;
    private bool wasLit = true;
    private int last = -1;

    private void Awake()
    {
        var go = new GameObject("FisiltiKaynagi");
        go.transform.SetParent(transform, false);
        source = go.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
        source.minDistance = 0.3f;
        source.maxDistance = 4f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.ignoreListenerPause = true;
        var walker = FindFirstObjectByType<PlayerWalker>();
        if (walker != null) player = walker.transform;
    }

    private void Update()
    {
        if (PauseService.IsPaused || player == null || whispers.Length == 0) return;
        bool lit = timer.IsLit;
        if (wasLit && !lit && Random.value < chance) Invoke(nameof(Whisper), Random.Range(delay.x, delay.y));
        wasLit = lit;
    }

    private void Whisper()
    {
        if (timer.IsLit) return;
        int i = Random.Range(0, whispers.Length);
        if (i == last && whispers.Length > 1) i = (i + 1) % whispers.Length;
        last = i;
        // Arkada, bir omuz yanında, kulak hizasında
        float side = Random.value < 0.5f ? -1f : 1f;
        source.transform.position = player.position + player.TransformDirection(new Vector3(0.35f * side, 1.6f, -0.45f));
        source.pitch = Random.Range(0.92f, 1.04f);
        source.PlayOneShot(whispers[i], volume);
    }
}
