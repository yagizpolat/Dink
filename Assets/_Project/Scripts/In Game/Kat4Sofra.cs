using System.Collections;
using UnityEngine;

/// <summary>
/// Kat 4, "Sofra": Daire 3'ün içi Hakan'ların oturma odası. Dört kişilik sofra kurulu; Hakan'ın tabağının üstü
/// kapatılmış, kardeşinin yarısı yenmiş, anne ve babanın tabakları dolu ve buharı tütüyor. Hakan'ın tabağının
/// yanındaki kardeş notu okununca ölülerin tabağındaki buhar söner, avizenin anne-baba tarafı kararır,
/// kabinde 5 yanar. Saat tıkırtısı ve sönme sesi burada üretilir.
/// </summary>
public class Kat4Sofra : MonoBehaviour
{
    [SerializeField] private Collider note;
    [SerializeField] private float reach = 1.8f;

    [Header("Sofra")]
    [SerializeField] private ParticleSystem[] parentsSteam;
    [SerializeField] private Light parentsLamp;
    [SerializeField] private Light childrenLamp;
    [SerializeField] private float childrenLampAfter = 0.55f;

    [Header("Ses")]
    [SerializeField] private AudioSource clockSource;
    [SerializeField] private AudioSource clickSource;

    [Header("Asansör")]
    [SerializeField] private GameObject nextFloorLight;
    [SerializeField] private AudioSource nextFloorDing;

    private bool done;
    private Camera cam;

    private void Awake()
    {
        if (nextFloorLight != null) nextFloorLight.SetActive(false);

        // Duvar saati: durmuş görünür ama tıkırdar
        if (clockSource != null)
        {
            clockSource.clip = Synth(1f, t => (t < 0.012f ? 1f : 0f) * Mathf.Sin(2f * Mathf.PI * 2200f * t) * 0.3f * (1f - t / 0.012f));
            clockSource.loop = true;
            clockSource.Play();
        }
        // Lamba sönerken ampulün kısa "tık"ı
        if (clickSource != null)
            clickSource.clip = Synth(0.25f, t => (Mathf.PerlinNoise(t * 5000f, 0.3f) - 0.5f) * Mathf.Exp(-t * 40f) * 0.8f);
        if (nextFloorDing != null && nextFloorDing.clip == null)
            nextFloorDing.clip = Synth(1.4f, t => Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.35f * Mathf.Exp(-t * 2.5f));
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (note == null || !note.Raycast(ray, out _, reach)) return;

        MektupNotHintUI.Instance.OpenText(NoteText(), MektupNotHintUI.LetterForm.ZeynepNote, !done);
        if (!done) StartCoroutine(EmptyChairs());
    }

    private IEnumerator EmptyChairs()
    {
        done = true;
        while (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) yield return null;
        yield return new WaitForSeconds(1.2f);

        // Buhar kesilir: yemek bir anda soğumuş gibi
        foreach (ParticleSystem steam in parentsSteam)
            if (steam != null) steam.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        yield return new WaitForSeconds(2.5f);

        if (clickSource != null) clickSource.Play();
        if (parentsLamp != null) parentsLamp.intensity = 0f;
        if (childrenLamp != null) childrenLamp.intensity *= childrenLampAfter;
        if (clockSource != null) clockSource.Stop();

        yield return new WaitForSeconds(2.5f);
        ElevatorController.DepartureUnlocked = true;
        if (nextFloorLight != null) nextFloorLight.SetActive(true);
        if (nextFloorDing != null) nextFloorDing.Play();
    }

    private static string NoteText()
    {
        return DinkLang.T(
            "Brother, you're late again.\n\nI warmed up your food and put a plate over it.\n\nI set mum and dad's plates again too. Don't be mad.\n\nIt's 23:42. I'm going to sleep.",
            "Abi yine geç kaldın.\n\nYemeğini ısıttım, üstünü kapattım.\n\nAnnemlerin tabağını yine koydum, kızma.\n\nSaat 23:42 oldu, uyuyorum.",
            "Bruder, du bist wieder zu spät.\n\nIch hab dein Essen warm gemacht und einen Teller draufgelegt.\n\nIch hab auch wieder für Mama und Papa gedeckt. Sei nicht böse.\n\nEs ist 23:42. Ich geh schlafen.",
            "Grand frère, tu es encore en retard.\n\nJ'ai réchauffé ton assiette et je l'ai couverte.\n\nJ'ai encore mis les assiettes de maman et papa. Ne te fâche pas.\n\nIl est 23h42. Je vais dormir.",
            "Hermano, otra vez llegas tarde.\n\nTe calenté la comida y la tapé.\n\nTambién puse otra vez los platos de mamá y papá. No te enfades.\n\nSon las 23:42. Me voy a dormir.",
            "Mano, chegaste tarde outra vez.\n\nAqueci-te a comida e tapei-a.\n\nPus outra vez os pratos da mãe e do pai. Não te zangues.\n\nSão 23:42. Vou dormir.",
            "Братик, ты опять опоздал.\n\nЯ разогрела тебе ужин и накрыла тарелкой.\n\nЯ опять поставила тарелки маме и папе. Не ругайся.\n\nУже 23:42. Я иду спать.");
    }

    private static AudioClip Synth(float seconds, System.Func<float, float> f)
    {
        const int rate = 44100;
        int n = Mathf.RoundToInt(seconds * rate);
        float[] data = new float[n];
        for (int i = 0; i < n; i++) data[i] = f(i / (float)rate);
        AudioClip clip = AudioClip.Create("Kat4Ses", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
