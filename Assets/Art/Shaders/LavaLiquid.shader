Shader "GeometryDash/LavaLiquid"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _HotColor ("Hot Color", Color) = (1.0, 0.9, 0.1, 1.0)
        _MidColor ("Mid Color", Color) = (1.0, 0.4, 0.0, 1.0)
        _DarkColor ("Dark Color", Color) = (0.3, 0.0, 0.0, 1.0)
        
        _FlowSpeed1 ("Flow Speed 1", Vector) = (0.2, 0.05, 0, 0)
        _FlowSpeed2 ("Flow Speed 2", Vector) = (-0.1, 0.1, 0, 0)
        _NoiseScale ("Noise Scale", Float) = 3.0
        
        _WaveSpeed ("Wave Speed", Float) = 3.0
        _WaveHeight ("Wave Height", Float) = 0.05
        _SurfaceBrightness ("Surface Brightness", Float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // SpriteRenderer color
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _HotColor;
            float4 _MidColor;
            float4 _DarkColor;
            float2 _FlowSpeed1;
            float2 _FlowSpeed2;
            float _NoiseScale;
            float _WaveSpeed;
            float _WaveHeight;
            float _SurfaceBrightness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color;
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // We use world X for continuous waves across tiled sprites
                float worldX = i.worldPos.x;
                
                // Calculate surface wave
                float wave = sin(worldX * 2.0 + _Time.y * _WaveSpeed) * _WaveHeight;
                // Add secondary wave for irregularity
                wave += cos(worldX * 3.5 - _Time.y * (_WaveSpeed * 0.7)) * (_WaveHeight * 0.5);
                
                // Surface threshold
                float surfaceThreshold = 1.0 - (_WaveHeight * 1.5) + wave;
                
                // Discard pixels above the wave
                if (i.uv.y > surfaceThreshold)
                    discard;
                
                // Calculate flow UVs
                // Use worldPos for seamless tiling
                float2 uvBase = i.worldPos.xy * _NoiseScale * 0.1;
                float2 uv1 = uvBase + _Time.y * _FlowSpeed1;
                float2 uv2 = uvBase + _Time.y * _FlowSpeed2;
                
                // Sample noise
                float n1 = tex2D(_NoiseTex, uv1).r;
                float n2 = tex2D(_NoiseTex, uv2).r;
                
                // Combine noise
                float combinedNoise = (n1 * 0.6) + (n2 * 0.4);
                
                // Base depth gradient (dark at bottom, hot at top)
                // Normalize depth based on surface
                float depth01 = clamp(i.uv.y / surfaceThreshold, 0.0, 1.0);
                
                // Mix colors based on depth and noise
                float heatMap = (depth01 * 0.5) + (combinedNoise * 0.5);
                
                half4 col;
                if (heatMap > 0.7)
                    col = lerp(_MidColor, _HotColor, (heatMap - 0.7) / 0.3);
                else
                    col = lerp(_DarkColor, _MidColor, heatMap / 0.7);
                
                // Surface glow
                float distToSurface = surfaceThreshold - i.uv.y;
                if (distToSurface < 0.05)
                {
                    float glowIntensity = 1.0 - (distToSurface / 0.05);
                    col += _HotColor * glowIntensity * _SurfaceBrightness;
                }
                
                col *= i.color; // Multiply by SpriteRenderer color/alpha
                return col;
            }
            ENDHLSL
        }
    }
}
