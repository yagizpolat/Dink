using UnityEngine;

/// <summary>
/// Oyuncu yürürken döngülü adım sesini çalar, durunca kısarak duraklatır.
/// Oyuncu nesnesi PlayerWalker ile bulunur; hız konum farkından ölçülür.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FootstepLoop : MonoBehaviour
{
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float moveThreshold = 0.4f;
    [SerializeField] private float fadeSpeed = 6f;

    private AudioSource source;
    private Transform player;
    private Vector3 lastPos;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
    }

    private void Update()
    {
        if (player == null)
        {
            var walker = FindFirstObjectByType<PlayerWalker>();
            if (walker == null) return;
            player = walker.transform;
            lastPos = player.position;
            return;
        }

        Vector3 delta = player.position - lastPos;
        delta.y = 0f;
        lastPos = player.position;
        float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;

        bool moving = speed > moveThreshold && Time.timeScale > 0f;
        float target = moving ? volume : 0f;
        source.volume = Mathf.MoveTowards(source.volume, target, fadeSpeed * Time.unscaledDeltaTime);

        if (moving && !source.isPlaying) source.Play();
        else if (!moving && source.isPlaying && source.volume <= 0.001f) source.Pause();
    }
}
