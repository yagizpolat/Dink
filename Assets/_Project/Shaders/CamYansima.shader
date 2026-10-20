// Giriş kapısı camı: GlassReflection'ın çizdiği ayna görüntüsünü ekran koordinatıyla okur.
// Hafif koyu cam tonu ve kenara doğru artan yansıma (fresnel) ile aynadan çok gece camı gibi durur.
Shader "Dink/CamYansima"
{
    Properties
    {
        _ReflTex ("Yansıma", 2D) = "black" {}
        _Tint ("Cam Tonu", Color) = (0.55, 0.52, 0.47, 1)
        _Strength ("Yansıma Gücü", Range(0, 1.5)) = 0.85
        _Dark ("Cam Koyuluğu", Color) = (0.02, 0.02, 0.025, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ReflTex); SAMPLER(sampler_ReflTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _Tint; float _Strength; float4 _Dark;
            CBUFFER_END

            struct A { float4 pos : POSITION; float3 n : NORMAL; };
            struct V { float4 pos : SV_POSITION; float4 sp : TEXCOORD0; float3 n : TEXCOORD1; float3 v : TEXCOORD2; };

            V vert (A i)
            {
                V o;
                float3 w = TransformObjectToWorld(i.pos.xyz);
                o.pos = TransformWorldToHClip(w);
                o.sp = ComputeScreenPos(o.pos);
                o.n = TransformObjectToWorldNormal(i.n);
                o.v = GetWorldSpaceViewDir(w);
                return o;
            }

            half4 frag (V i) : SV_Target
            {
                float2 uv = i.sp.xy / i.sp.w;
                half3 refl = SAMPLE_TEXTURE2D(_ReflTex, sampler_ReflTex, uv).rgb;
                float f = 1 - saturate(abs(dot(normalize(i.n), normalize(i.v))));
                float k = _Strength * lerp(0.75, 1.0, f);
                return half4(_Dark.rgb + refl * _Tint.rgb * k, 1);
            }
            ENDHLSL
        }
    }
}
