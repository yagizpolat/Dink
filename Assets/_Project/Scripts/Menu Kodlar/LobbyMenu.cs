using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Ana menü: lobi giriş kapısının sol camına yapıştırılmış vinil harfler.
/// Kamera sokaktadır; menü öğeleri dünya uzayında TextMeshPro olarak camın önünde durur.
/// Gerilim efektleri (ışık vızıltısı/titreme, asansör göstergesi, boşta açılan asansör kapısı,
/// sokak ambiyansı) tamamen ölçeksiz zamanla çalışır. Bulunamayan nesneler sessizce atlanır.
/// </summary>
public class LobbyMenu : MonoBehaviour
{
    // ─── Dışarıdan ayarlananlar ───
    public Func<bool> IsInputBlocked;
    public AudioClip ambience;
    public float ambienceVolume = 0.35f;

    /// <summary>Menü yazılarının kökü; gizlenince menü girdisi de durur.</summary>
    public GameObject Content { get; private set; }

    // ─── Yerleşim (cam yüzeyi, kamera tarafı) ───
    private const float CamZ = -0.03f;
    private const float LeafCenterX = -0.52f;
    private const float TitleY = 1.30f;
    private const float ItemsTopY = 1.12f;
    private const float ItemStep = 0.1f;
    private const float TitleSize = 0.62f;
    private const float ItemSize = 0.44f;
    private const float UnselectedAlpha = 0.4f;

    private class Item
    {
        public TextMeshPro text;
        public TextMeshPro glow;
        public BoxCollider col;
        public string raw;
    }

    private readonly List<Item> items = new List<Item>();
    private Action[] actions;
    private bool[] opensDoor;
    private int selected;
    private TextMeshPro title;
    private TMP_FontAsset font;
    private Camera cam;
    private bool busy;
    private float dim = 1f;
    private Coroutine dimRoutine;

    // ─── Sahne nesneleri ───
    private Transform lobi;
    private Light frontLight;
    private float frontBase;
    private Transform elevatorHinge;
    private Quaternion elevatorClosed, elevatorOpen;
    private Transform leafLeft, leafRight;
    private TextMeshPro indicator;

    // ─── Ses ───
    private AudioSource humSrc, sfxSrc, ambSrc, groanSrc;
    private AudioClip tickClip, dingClip, groanClip;

    // ─── Zamanlayıcılar ───
    private float nextFlicker, nextIndicator, lastStumble = -10f, lastInput;
    private bool flickering, idleOpened;
    private Coroutine elevatorRoutine;
    private Vector3 lastMouse;
    private static readonly HashSet<string> warned = new HashSet<string>();

    // ═══════════════════════════════════════════

    public void Build(string[] labels, Action[] itemActions, bool[] startsGame)
    {
        actions = itemActions;
        opensDoor = startsGame;
        cam = Camera.main;
        font = NicknameLogbookUI.LoadFont("Fonts/PTMono-Regular");

        GameObject lobiGo = GameObject.Find("[MENU LOBI]");
        lobi = lobiGo != null ? lobiGo.transform : null;
        if (lobi == null) Warn("[MENU LOBI] bulunamadı; menü yalnızca yazılarla çalışır.");

        Content = new GameObject("LobbyMenu_Yazilar");
        if (lobi != null) Content.transform.SetParent(lobi, false);
        Content.transform.position = Vector3.zero;
        Content.transform.rotation = Quaternion.identity;

        title = MakeText("Baslik", "DINK", TitleSize, new Vector3(LeafCenterX, TitleY, CamZ));
        title.fontStyle = FontStyles.Normal;
        title.characterSpacing = 4f;
        title.color = new Color(0.78f, 0.77f, 0.72f, 0.55f);
        title.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-1.2f, 1.2f));

        SetLabels(labels);
        SetupScene();
        SetupAudio();

        float now = Time.unscaledTime;
        nextFlicker = now + UnityEngine.Random.Range(9f, 20f);
        nextIndicator = now + UnityEngine.Random.Range(25f, 45f);
        lastInput = now;
        lastMouse = Input.mousePosition;
    }

    public void Open()
    {
        if (Content != null) Content.SetActive(true);
        Select(0, false);
    }

    /// <summary>Dil değişince yazıları yeniler (öğe sayısı aynı kalmalı).</summary>
    public void SetLabels(string[] labels)
    {
        for (int i = 0; i < labels.Length; i++)
        {
            Item it;
            if (i < items.Count) it = items[i];
            else
            {
                Vector3 p = new Vector3(LeafCenterX, ItemsTopY - i * ItemStep, CamZ);
                it = new Item { text = MakeText("Oge_" + i, "", ItemSize, p) };
                it.text.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-1.6f, 1.6f));
                it.glow = MakeText("Parilti", "", ItemSize, Vector3.zero);
                it.glow.transform.SetParent(it.text.transform, false);
                it.glow.transform.localPosition = new Vector3(0f, 0f, 0.002f);
                it.glow.transform.localScale = Vector3.one * 1.04f;
                it.glow.color = new Color(1f, 1f, 1f, 0f);
                it.col = it.text.gameObject.AddComponent<BoxCollider>();
                items.Add(it);
            }
            it.raw = labels[i];
            it.text.text = Worn(labels[i]);
            it.glow.text = labels[i];
            it.text.ForceMeshUpdate();
            Bounds b = it.text.textBounds;
            it.col.center = new Vector3(b.center.x, b.center.y, 0f);
            it.col.size = new Vector3(Mathf.Max(b.size.x, 0.1f) + 0.04f, ItemStep * 0.9f, 0.02f);
        }
        Select(Mathf.Clamp(selected, 0, items.Count - 1), false);
    }

    // Bir-iki harfi hafifçe soluklaştırır (yıpranmış etiket)
    private static string Worn(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length < 3) return s;
        int idx = UnityEngine.Random.Range(1, s.Length);
        if (char.IsWhiteSpace(s[idx])) return s;
        return s.Substring(0, idx) + "<alpha=#99>" + s[idx] + "<alpha=#FF>" + s.Substring(idx + 1);
    }

    private TextMeshPro MakeText(string name, string txt, float size, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(Content.transform, false);
        go.transform.position = pos;
        var t = go.AddComponent<TextMeshPro>();
        if (font != null) t.font = font;
        t.text = txt;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        t.richText = true;
        t.rectTransform.sizeDelta = new Vector2(0.85f, 0.3f);
        t.color = Color.white;
        return t;
    }

    private void Select(int i, bool fromUser)
    {
        if (items.Count == 0) return;
        bool changed = i != selected;
        selected = i;
        for (int k = 0; k < items.Count; k++)
        {
            bool on = k == selected;
            items[k].text.color = new Color(0.82f, 0.8f, 0.75f, (on ? 0.85f : UnselectedAlpha) * dim);
            items[k].text.fontStyle = FontStyles.Normal;
            items[k].glow.color = new Color(1f, 1f, 1f, 0f);
        }
        if (fromUser && changed) Stumble();
    }

    /// <summary>Menü yazılarını verilen saydamlık çarpanına yumuşakça götürür (ölçeksiz zaman).</summary>
    public void FadeTo(float target, float duration = 0.25f)
    {
        if (dimRoutine != null) StopCoroutine(dimRoutine);
        dimRoutine = StartCoroutine(DimRoutine(target, duration));
    }

    private IEnumerator DimRoutine(float target, float duration)
    {
        float from = dim;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            SetDim(Mathf.Lerp(from, target, t / duration));
            yield return null;
        }
        SetDim(target);
        dimRoutine = null;
    }

    private void SetDim(float value)
    {
        dim = value;
        if (title != null) title.color = new Color(0.78f, 0.77f, 0.72f, 0.55f * dim);
        Select(selected, false);
    }

    private void Activate(int i)
    {
        if (busy || actions == null || i < 0 || i >= actions.Length) return;
        if (opensDoor != null && i < opensDoor.Length && opensDoor[i])
            StartCoroutine(OpenEntranceThen(actions[i]));
        else
            actions[i]?.Invoke();
    }

    // ═══ Girdi ═══

    private bool InputBlocked => (IsInputBlocked != null && IsInputBlocked())
                                 || Time.frameCount == DoorLightSettings.LastClosedFrame
                                 || Content == null || !Content.activeInHierarchy;

    private void Update()
    {
        float now = Time.unscaledTime;
        bool anyInput = Input.anyKeyDown || Input.GetMouseButtonDown(0) || (Input.mousePosition - lastMouse).sqrMagnitude > 4f
                        || Mathf.Abs(Input.mouseScrollDelta.y) > 0.01f;
        lastMouse = Input.mousePosition;
        if (anyInput)
        {
            lastInput = now;
            if (idleOpened) { idleOpened = false; MoveElevator(false, 2.5f); }
        }

        if (!flickering && now >= nextFlicker)
        {
            nextFlicker = now + UnityEngine.Random.Range(9f, 20f);
            StartCoroutine(Flicker(UnityEngine.Random.Range(2, 5), true));
        }
        if (indicator != null && now >= nextIndicator)
        {
            nextIndicator = now + UnityEngine.Random.Range(25f, 45f);
            StartCoroutine(IndicatorJump());
        }
        if (!idleOpened && elevatorHinge != null && now - lastInput > 40f && !busy)
        {
            idleOpened = true;
            MoveElevator(true, 4f);
        }

        if (busy || InputBlocked || items.Count == 0) return;

        if (cam != null && Cursor.visible)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            int hover = -1;
            foreach (RaycastHit h in Physics.RaycastAll(ray, 20f, ~0, QueryTriggerInteraction.Collide))
                for (int k = 0; k < items.Count; k++)
                    if (h.collider == items[k].col) hover = k;
            if (hover >= 0)
            {
                if (anyInput && hover != selected) Select(hover, true);
                if (Input.GetMouseButtonDown(0)) { Activate(hover); return; }
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Select((selected + 1) % items.Count, true);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Select((selected - 1 + items.Count) % items.Count, true);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) Activate(selected);
    }

    // ═══ Sahne kurulumu ═══

    private Transform FindChild(string path)
    {
        if (lobi == null) return null;
        Transform t = lobi.Find(path);
        if (t == null) Warn("Bulunamadı: [MENU LOBI]/" + path);
        return t;
    }

    private void SetupScene()
    {
        Transform l = FindChild("GlobIsik_On");
        frontLight = l != null ? l.GetComponent<Light>() : null;
        if (frontLight != null) frontBase = frontLight.intensity;

        Transform kabin = FindChild("KabinHareket/KabinIsigi");
        if (kabin != null)
        {
            Light kl = kabin.GetComponent<Light>();
            if (kl != null) kl.enabled = false; else kabin.gameObject.SetActive(false);
        }

        elevatorHinge = FindChild("KapiMenteşe");
        if (elevatorHinge != null)
        {
            elevatorClosed = elevatorHinge.localRotation;
            elevatorOpen = OpenRotation(elevatorHinge, 95f, -1f);
        }

        leafLeft = FindChild("GirisKapi/KanatSol");
        leafRight = FindChild("GirisKapi/KanatSag");

        // Asansör kat göstergesi
        var go = new GameObject("AsansorGostergeYazi");
        if (lobi != null) go.transform.SetParent(lobi, false);
        go.transform.position = new Vector3(1.40f, 2.45f, 8.80f);
        go.transform.rotation = Quaternion.identity;
        indicator = go.AddComponent<TextMeshPro>();
        if (font != null) indicator.font = font;
        indicator.text = "0";
        indicator.fontSize = 1.1f;
        indicator.alignment = TextAlignmentOptions.Center;
        indicator.enableWordWrapping = false;
        indicator.rectTransform.sizeDelta = new Vector2(0.4f, 0.2f);
        indicator.color = new Color(1f, 0.12f, 0.08f, 0.9f);
    }

    // Menteşe dönüşü için, kapı merkezini z yönünde istenen tarafa götüren açıyı seçer
    private static Quaternion OpenRotation(Transform hinge, float angle, float zSign)
    {
        Renderer r = hinge.GetComponentInChildren<Renderer>();
        Vector3 center = r != null ? r.bounds.center : hinge.position + hinge.right;
        float best = angle, bestZ = float.MinValue;
        foreach (float a in new[] { angle, -angle })
        {
            Vector3 moved = hinge.position + Quaternion.AngleAxis(a, Vector3.up) * (center - hinge.position);
            float z = moved.z * zSign;
            if (z > bestZ) { bestZ = z; best = a; }
        }
        // Dünya Y ekseni etrafındaki dönüşü yerel dönüşe çevir
        Quaternion world = Quaternion.AngleAxis(best, Vector3.up) * hinge.rotation;
        return hinge.parent != null ? Quaternion.Inverse(hinge.parent.rotation) * world : world;
    }

    // ═══ Efektler ═══

    private IEnumerator Flicker(int drops, bool tick)
    {
        if (frontLight == null) yield break;
        flickering = true;
        if (tick && sfxSrc != null && tickClip != null) sfxSrc.PlayOneShot(tickClip, 0.5f);
        for (int i = 0; i < drops; i++)
        {
            frontLight.intensity = frontBase * UnityEngine.Random.Range(0.05f, 0.35f);
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.03f, 0.08f));
            frontLight.intensity = frontBase;
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.04f, 0.14f));
        }
        frontLight.intensity = frontBase;
        flickering = false;
    }

    /// <summary>Ön ışıkta kısa bir tökezleme (seçim değişince); başka menüler de çağırabilir.</summary>
    public void Stumble()
    {
        float now = Time.unscaledTime;
        if (flickering || now - lastStumble < 1.5f) return;
        lastStumble = now;
        StartCoroutine(Flicker(1, false));
    }

    private IEnumerator IndicatorJump()
    {
        indicator.text = "11";
        if (sfxSrc != null && dingClip != null) sfxSrc.PlayOneShot(dingClip, 0.25f);
        yield return new WaitForSecondsRealtime(0.8f);
        if (indicator != null) indicator.text = "0";
    }

    private void MoveElevator(bool open, float duration)
    {
        if (elevatorHinge == null) return;
        if (elevatorRoutine != null) StopCoroutine(elevatorRoutine);
        elevatorRoutine = StartCoroutine(ElevatorRoutine(open ? elevatorOpen : elevatorClosed, duration, open));
    }

    private IEnumerator ElevatorRoutine(Quaternion target, float duration, bool open)
    {
        if (groanSrc != null && groanClip != null)
        {
            groanSrc.Stop();
            groanSrc.volume = open ? 0.35f : 0.25f;
            groanSrc.Play();
        }
        Quaternion from = elevatorHinge.localRotation;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            elevatorHinge.localRotation = Quaternion.Slerp(from, target, k);
            yield return null;
        }
        elevatorHinge.localRotation = target;
        elevatorRoutine = null;
    }

    // Oyun başlarken: yazılar söner, lobi olduğu gibi kalır (oyuncu az sonra aynı lobiye ışıklar yanarken girer);
    // karartmayı sahne geçişi yapar
    private IEnumerator OpenEntranceThen(Action act)
    {
        busy = true;

        var texts = new List<TextMeshPro>();
        if (Content != null) texts.AddRange(Content.GetComponentsInChildren<TextMeshPro>());
        var startAlpha = new float[texts.Count];
        for (int i = 0; i < texts.Count; i++) startAlpha[i] = texts[i].color.a;
        for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
        {
            float k = 1f - t / 0.35f;
            for (int i = 0; i < texts.Count; i++) { Color c = texts[i].color; c.a = startAlpha[i] * k; texts[i].color = c; }
            yield return null;
        }
        if (Content != null) Content.SetActive(false);

        yield return new WaitForSecondsRealtime(0.25f);
        busy = false;
        act?.Invoke();
    }

    // ═══ Ses ═══

    private AudioSource NewSource(bool loop, float vol)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.volume = vol;
        s.spatialBlend = 0f;
        s.ignoreListenerPause = true;
        return s;
    }

    private void SetupAudio()
    {
        const int rate = 44100;

        // Floresan vızıltısı: 50 Hz + harmonikler, 1 sn döngü (tam periyot, tıkırtısız)
        float[] hum = new float[rate];
        for (int i = 0; i < hum.Length; i++)
        {
            float t = i / (float)rate;
            hum[i] = 0.5f * Mathf.Sin(2f * Mathf.PI * 50f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * 100f * t)
                   + 0.12f * Mathf.Sin(2f * Mathf.PI * 150f * t) + 0.05f * Mathf.Sin(2f * Mathf.PI * 250f * t);
            hum[i] *= 0.35f;
        }
        humSrc = NewSource(true, 0.06f);
        humSrc.clip = MakeClip("LobiVizilti", hum, rate);
        humSrc.Play();

        // Elektrik tıkırtısı: kısa gürültü patlaması
        int n = rate / 12;
        float[] tick = new float[n];
        for (int i = 0; i < n; i++)
        {
            float env = Mathf.Exp(-i / (rate * 0.008f)) + 0.3f * Mathf.Exp(-Mathf.Abs(i - n * 0.4f) / (rate * 0.004f));
            tick[i] = (UnityEngine.Random.value * 2f - 1f) * env * 0.6f;
        }
        tickClip = MakeClip("LobiTik", tick, rate);

        // Uzak yumuşak çan
        n = (int)(rate * 1.4f);
        float[] ding = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float env = Mathf.Exp(-t * 3f) * Mathf.Clamp01(t * 200f);
            ding[i] = env * (0.6f * Mathf.Sin(2f * Mathf.PI * 880f * t) + 0.25f * Mathf.Sin(2f * Mathf.PI * 1318f * t)) * 0.5f;
        }
        dingClip = MakeClip("LobiCan", ding, rate);

        // Alçak mekanik inilti: dalgalanan alçak frekans + hafif gürültü
        n = rate * 4;
        float[] groan = new float[n];
        float phase = 0f, lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float f = 55f + 8f * Mathf.Sin(t * 1.7f) + 4f * Mathf.Sin(t * 5.3f);
            phase += 2f * Mathf.PI * f / rate;
            lp += 0.02f * ((UnityEngine.Random.value * 2f - 1f) - lp);
            float env = Mathf.Clamp01(t * 2f) * Mathf.Clamp01((4f - t) * 1.5f);
            groan[i] = env * (0.5f * Mathf.Sin(phase) + 0.2f * Mathf.Sin(phase * 2.01f) + 0.6f * lp) * 0.6f;
        }
        groanClip = MakeClip("LobiInilti", groan, rate);

        sfxSrc = NewSource(false, 1f);
        groanSrc = NewSource(false, 0.35f);
        groanSrc.clip = groanClip;

        if (ambience != null)
        {
            ambSrc = NewSource(true, ambienceVolume);
            ambSrc.clip = ambience;
            ambSrc.Play();
        }
    }

    /// <summary>Ambiyans Build'den sonra atanırsa çalmaya başlatır.</summary>
    public void SetAmbience(AudioClip clip)
    {
        ambience = clip;
        if (clip == null) return;
        if (ambSrc == null) ambSrc = NewSource(true, ambienceVolume);
        ambSrc.clip = clip;
        ambSrc.Play();
    }

    // Sentez klipleri her menü açılışında yeniden üretilir; menü kapanınca silinmezse birikiyordu
    private void OnDestroy()
    {
        if (humSrc != null && humSrc.clip != null) Destroy(humSrc.clip);
        if (tickClip != null) Destroy(tickClip);
        if (dingClip != null) Destroy(dingClip);
        if (groanClip != null) Destroy(groanClip);
    }

    private static AudioClip MakeClip(string name, float[] data, int rate)
    {
        AudioClip c = AudioClip.Create(name, data.Length, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }

    private static void Warn(string msg)
    {
        if (warned.Add(msg)) Debug.LogWarning("[LobbyMenu] " + msg);
    }
}
