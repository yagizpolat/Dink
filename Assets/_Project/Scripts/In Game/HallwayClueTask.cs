using TMPro;
using UnityEngine;

/// <summary>
/// Koridor görevi: ışık yanıkken uyanık daireyi bulan üç ipucunu (zil, dürbün, isimlik) topla.
/// Sol üstte sayaçlı görev yazısı durur. Üçü bulununca lambalar son kez söner ve bir daha yanmaz,
/// uyanık dairenin kapı altından ışık sızar (sonraki adım: kapı seçimi).
/// </summary>
public class HallwayClueTask : MonoBehaviour
{
    [SerializeField] private HallwayLightTimer timer;
    [SerializeField] private int total = 3;
    [SerializeField] private Light doorLeak;       // uyanık dairenin kapı altı ışığı; görev bitince yanar
    [SerializeField] private Light[] switchGlows;  // düğme parıltıları; görev bitince söner

    public bool Done { get; private set; }
    public AudioClip BellClip { get; private set; }
    public AudioClip MuffledClip { get; private set; }

    private int count;
    private TMP_Text label;
    private AudioSource audioSource;
    private AudioClip foundClip;

    private void Awake()
    {
        BellClip = Tone(new[] { 880f, 698f }, 0.32f, 0.5f);
        MuffledClip = Muffled(1.2f);
        foundClip = Tone(new[] { 523f, 784f }, 0.18f, 0.25f);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        if (doorLeak != null) doorLeak.enabled = false;
        label = CreateLabel();
        Refresh();
    }

    public void Found(HallwayClue clue)
    {
        if (Done) return;
        count++;
        audioSource.PlayOneShot(foundClip);
        Refresh();
        if (count >= total) Invoke(nameof(Complete), 2.5f);
    }

    private void Complete()
    {
        Done = true;
        timer.Shutdown();
        foreach (var g in switchGlows) if (g != null) g.enabled = false;
        if (doorLeak != null) doorLeak.enabled = true;
        label.text = DinkLang.T("The lights won't come back on.", "Işıklar bir daha yanmayacak.", "Das Licht geht nicht mehr an.",
            "La lumière ne reviendra pas.", "Las luces no volverán.", "As luzes não vão voltar.", "Свет больше не загорится.",
            "灯再也不会亮了。", "明かりはもう戻らない。", "불은 다시 켜지지 않는다.");
    }

    private void Refresh()
    {
        string head = DinkLang.T("Find the apartment that is still awake", "Hâlâ uyanık olan daireyi bul", "Finde die Wohnung, in der noch jemand wach ist",
            "Trouve l'appartement encore éveillé", "Encuentra el piso que sigue despierto", "Encontre o apartamento que ainda está acordado",
            "Найди квартиру, где ещё не спят", "找出还醒着的那户", "まだ起きている部屋を探せ", "아직 깨어 있는 집을 찾아라");
        label.text = $"{head}  {count}/{total}";
    }

    private TMP_Text CreateLabel()
    {
        var go = new GameObject("[KoridorGorev]");
        go.transform.SetParent(transform, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 800;
        var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var textGo = new GameObject("Yazi");
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.font = NicknameLogbookUI.LoadFont("Fonts/PTMono-Regular");
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.raycastTarget = false;
        tmp.color = new Color(0.88f, 0.86f, 0.8f, 0.85f);
        RectTransform r = tmp.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
        r.anchoredPosition = new Vector2(48f, -40f);
        r.sizeDelta = new Vector2(1100f, 60f);
        return tmp;
    }

    /// <summary>Ardışık sönümlü sinüs notaları (zil "ding-dong", bulundu sesi).</summary>
    private static AudioClip Tone(float[] notes, float noteLength, float gain)
    {
        const int rate = 44100;
        int per = Mathf.CeilToInt(noteLength * rate);
        var data = new float[per * notes.Length];
        for (int k = 0; k < notes.Length; k++)
            for (int i = 0; i < per; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Min(1f, t * 200f) * Mathf.Exp(-t * 6f);
                data[k * per + i] = gain * env * (Mathf.Sin(2 * Mathf.PI * notes[k] * t) + 0.3f * Mathf.Sin(4 * Mathf.PI * notes[k] * t));
            }
        var clip = AudioClip.Create("Tone", data.Length, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Kapı ardından boğuk televizyon: alçak geçiren süzgeçten geçmiş gürültü + konuşma ritmi, sonunda kesilir.</summary>
    private static AudioClip Muffled(float length)
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(length * rate);
        var data = new float[n];
        var rng = new System.Random(3);
        float y = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float noise = (float)(rng.NextDouble() * 2 - 1);
            y += 0.04f * (noise - y);                                  // boğuk: alçak geçiren
            float speech = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 3.3f * t) * Mathf.Sin(2 * Mathf.PI * 0.7f * t);
            float cut = t < length * 0.75f ? 1f : 0f;                  // televizyon birden susar
            data[i] = 2.2f * y * speech * cut;
        }
        var clip = AudioClip.Create("Muffled", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
