using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hafıza marketi P2: yerdeki koliden makarna alıp raftaki boş bölmeye dizmek.
/// Koliye bakıp E/tık = iki paket ele alınır (kolinin üst katından biri eksilir, altında daha var).
/// Raf boşluğuna bakıp E/tık = eldekiler arkadan öne, sırayla yerine kayar. Bütün yerler dolunca Completed.
/// Nesneler (Koli_*, Raf_*) HafizaMakarna.fbx'ten gelir, Editor/HafizaMarketKur bağlar.
/// </summary>
public class MarketShelfStocking : MonoBehaviour
{
    [SerializeField] private Collider box;
    [SerializeField] private Collider shelfGap;
    [SerializeField] private List<Transform> boxPackets = new List<Transform>();
    [SerializeField] private List<Transform> shelfSlots = new List<Transform>();
    [SerializeField] private float reach = 2.2f;
    [SerializeField] private int carryCount = 2;
    [SerializeField] private Vector3 heldOffset = new Vector3(0.30f, -0.42f, 0.62f);
    [SerializeField] private float placeSeconds = 0.35f;

    public bool Completed { get; private set; }
    public event Action OnCompleted;

    private readonly List<Transform> held = new List<Transform>();
    private int nextSlot;
    private int boxTaken;
    private Camera cam;
    private bool busy;
    private bool hinted;

    private void Start()
    {
        foreach (var s in shelfSlots) s.gameObject.SetActive(false);
        // Arkadan öne doldur: raf derinliği (z) büyükten küçüğe, sonra soldan sağa
        shelfSlots.Sort((a, b) => Mathf.Abs(a.position.z - b.position.z) > 0.03f ? b.position.z.CompareTo(a.position.z) : a.position.x.CompareTo(b.position.x));
    }

    private void Update()
    {
        // Nişangâh yalnız o an yapılabilecek işi göstersin
        var bt = box.GetComponent<InteractionTarget>(); if (bt != null) bt.enabled = !Completed && !busy && held.Count == 0;
        var st = shelfGap.GetComponent<InteractionTarget>(); if (st != null) st.enabled = !Completed && !busy && held.Count > 0;
        if (Completed || busy || Time.timeScale == 0f || PauseService.IsPaused) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!hinted && box.Raycast(ray, out _, reach + 1.5f))
        {
            hinted = true;
            ScreenHint.Show(DinkLang.T("The shelf is missing pasta.", "Rafta makarna eksik.", "Im Regal fehlen Nudeln.", "Il manque des pâtes en rayon.",
                "Faltan pastas en el estante.", "Falta macarrão na prateleira.", "На полке не хватает макарон.", "货架上缺了意面。", "棚のパスタが足りない。", "선반에 파스타가 비었다."));
        }

        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E)) return;
        if (MektupNotHintUI.instance != null && MektupNotHintUI.instance.IsPanelOpen) return;

        if (held.Count == 0 && box.Raycast(ray, out _, reach)) Take();
        else if (held.Count > 0 && shelfGap.Raycast(ray, out _, reach)) StartCoroutine(Place());
    }

    private void Take()
    {
        int need = Mathf.Min(carryCount, shelfSlots.Count - nextSlot);
        for (int i = 0; i < need; i++)
        {
            var p = Instantiate(shelfSlots[nextSlot + i].gameObject).transform;
            p.gameObject.SetActive(true);
            foreach (var r in p.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            p.SetParent(cam.transform, false);
            p.localPosition = heldOffset + new Vector3(0.03f * i, 0.01f * i, -0.03f * i);
            p.localRotation = Quaternion.Euler(-35f, -30f, 8f);
            held.Add(p);
        }
        // Kolinin üst katı her alışta bir eksilir; son alışta koli boşalır
        int hide = held.Count > 0 && nextSlot + held.Count >= shelfSlots.Count ? boxPackets.Count - boxTaken : 1;
        for (int i = 0; i < hide && boxTaken < boxPackets.Count; i++) boxPackets[boxTaken++].gameObject.SetActive(false);
    }

    private IEnumerator Place()
    {
        busy = true;
        foreach (var p in held)
        {
            var slot = shelfSlots[nextSlot++];
            p.SetParent(null, true);
            Vector3 a = p.position; Quaternion ra = p.rotation;
            for (float t = 0; t < 1f; t += Time.deltaTime / placeSeconds)
            {
                float e = t * t * (3f - 2f * t);
                p.SetPositionAndRotation(Vector3.Lerp(a, slot.position, e) + Vector3.up * Mathf.Sin(e * Mathf.PI) * 0.04f, Quaternion.Slerp(ra, slot.rotation, e));
                yield return null;
            }
            Destroy(p.gameObject);
            slot.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.08f);
        }
        held.Clear();
        busy = false;
        if (nextSlot >= shelfSlots.Count)
        {
            Completed = true;
            OnCompleted?.Invoke();
        }
    }
}
