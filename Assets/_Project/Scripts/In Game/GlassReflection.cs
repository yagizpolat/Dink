using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Düz bir cam yüzeyini aynaya çevirir: ayna kamerası holü cam düzlemine göre yansıtıp bir RenderTexture'a çizer,
/// cam materyali (Dink/CamYansima) bunu ekran koordinatıyla okur. Giriş kapısı kapanınca açılır.
/// </summary>
public class GlassReflection : MonoBehaviour
{
    [SerializeField] private Renderer[] glassRenderers;
    [SerializeField] private Material reflectiveGlass;
    [SerializeField] private float clipOffset = 0.02f;
    [SerializeField, Range(0.25f, 1f)] private float resolutionScale = 0.6f;

    private Camera mainCam;
    private Camera mirrorCam;
    private RenderTexture rt;
    private Material[][] originalMaterials;

    private static readonly int ReflTexId = Shader.PropertyToID("_ReflTex");

    private void OnEnable()
    {
        mainCam = Camera.main;
        if (mainCam == null) { enabled = false; return; }

        var go = new GameObject("AynaKamerasi");
        go.hideFlags = HideFlags.DontSave;
        mirrorCam = go.AddComponent<Camera>();
        mirrorCam.CopyFrom(mainCam);
        mirrorCam.depth = mainCam.depth - 1;

        rt = new RenderTexture(Mathf.RoundToInt(Screen.width * resolutionScale), Mathf.RoundToInt(Screen.height * resolutionScale), 24);
        mirrorCam.targetTexture = rt;
        reflectiveGlass.SetTexture(ReflTexId, rt);

        originalMaterials = new Material[glassRenderers.Length][];
        for (int i = 0; i < glassRenderers.Length; i++)
        {
            originalMaterials[i] = glassRenderers[i].sharedMaterials;
            var mats = glassRenderers[i].sharedMaterials;
            for (int m = 0; m < mats.Length; m++)
                if (mats[m] != null && mats[m].name.StartsWith("L_DisCam")) mats[m] = reflectiveGlass;
            glassRenderers[i].sharedMaterials = mats;
        }

        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        RenderPipelineManager.endCameraRendering += OnEndCamera;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        RenderPipelineManager.endCameraRendering -= OnEndCamera;

        // Sahne kapanırken camlar bu bileşenden önce yok edilmiş olabilir
        if (originalMaterials != null)
            for (int i = 0; i < glassRenderers.Length; i++)
                if (glassRenderers[i] != null) glassRenderers[i].sharedMaterials = originalMaterials[i];
        originalMaterials = null;

        if (mirrorCam != null)
        {
            mirrorCam.targetTexture = null;
            Destroy(mirrorCam.gameObject);
        }
        if (rt != null)
        {
            reflectiveGlass.SetTexture(ReflTexId, null);
            rt.Release();
            Destroy(rt);
            rt = null;
        }
    }

    private void LateUpdate()
    {
        if (mirrorCam == null || mainCam == null) return;

        Vector3 pos = transform.position;
        Vector3 normal = transform.forward;

        // Cam düzlemine göre yansıma matrisi
        float d = -Vector3.Dot(normal, pos) - clipOffset;
        Vector4 plane = new Vector4(normal.x, normal.y, normal.z, d);
        Matrix4x4 reflection = Reflection(plane);

        mirrorCam.fieldOfView = mainCam.fieldOfView;
        mirrorCam.aspect = mainCam.aspect;
        mirrorCam.worldToCameraMatrix = mainCam.worldToCameraMatrix * reflection;

        // Camın arkasında kalanı (kapı kanatlarını) kes
        Vector4 clip = CameraSpacePlane(mirrorCam, pos, normal);
        mirrorCam.projectionMatrix = mainCam.CalculateObliqueMatrix(clip);
    }

    private void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam == mirrorCam) GL.invertCulling = true;
    }

    private void OnEndCamera(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam == mirrorCam) GL.invertCulling = false;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal)
    {
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(pos + normal * clipOffset);
        Vector3 cnormal = m.MultiplyVector(normal).normalized;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private static Matrix4x4 Reflection(Vector4 p)
    {
        Matrix4x4 r = Matrix4x4.identity;
        r.m00 = 1f - 2f * p.x * p.x; r.m01 = -2f * p.x * p.y; r.m02 = -2f * p.x * p.z; r.m03 = -2f * p.w * p.x;
        r.m10 = -2f * p.y * p.x; r.m11 = 1f - 2f * p.y * p.y; r.m12 = -2f * p.y * p.z; r.m13 = -2f * p.w * p.y;
        r.m20 = -2f * p.z * p.x; r.m21 = -2f * p.z * p.y; r.m22 = 1f - 2f * p.z * p.z; r.m23 = -2f * p.w * p.z;
        return r;
    }
}
