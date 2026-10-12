Shader "GeometryDash/WaterLiquid"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _SurfaceColor ("Surface Color", Color) = (0.2, 0.8, 1.0, 0.8)
        _DeepColor ("Deep Color", Color) = (0.0, 0.2, 0.6, 0.95)
        _HighlightColor ("Highlight Color", Color) = (0.8, 0.95, 1.0, 1.0)
        
        _FlowSpeed1 ("Flow Speed 1", Vector) = (0.1, 0.0, 0, 0)
        _FlowSpeed2 ("Flow Speed 2", Vector) = (-0.05, 0.05, 0, 0)
        _NoiseScale ("Noise Scale", Float) = 2.0
        
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _WaveHeight ("Wave Height", Float) = 0.04
        _HighlightStrength ("Highlight Strength", Float) = 1.0
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
                float4 color : COLOR;
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
            float4 _SurfaceColor;
            float4 _DeepColor;
            float4 _HighlightColor;
            float2 _FlowSpeed1;
            float2 _FlowSpeed2;
            float _NoiseScale;
            float _WaveSpeed;
            float _WaveHeight;
            float _HighlightStrength;

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
                float worldX = i.worldPos.x;
                
                // Calculate surface wave
                float wave = sin(worldX * 1.5 + _Time.y * _WaveSpeed) * _WaveHeight;
                wave += cos(worldX * 4.0 - _Time.y * (_WaveSpeed * 1.2)) * (_WaveHeight * 0.4);
                
                float surfaceThreshold = 1.0 - (_WaveHeight * 1.5) + wave;
                
                if (i.uv.y > surfaceThreshold)
                    discard;
                
                // Flowing noise
                float2 uvBase = i.worldPos.xy * _NoiseScale * 0.1;
                float2 uv1 = uvBase + _Time.y * _FlowSpeed1;
                float2 uv2 = uvBase + _Time.y * _FlowSpeed2;
                
                float n1 = tex2D(_NoiseTex, uv1).r;
                float n2 = tex2D(_NoiseTex, uv2).r;
                float combinedNoise = (n1 * 0.5) + (n2 * 0.5);
                
                // Depth gradient
                float depth01 = clamp(i.uv.y / surfaceThreshold, 0.0, 1.0);
                
                // Base water color
                half4 col = lerp(_DeepColor, _SurfaceColor, depth01);
                
                // Surface foam/highlight
                float distToSurface = surfaceThreshold - i.uv.y;
                
                // Caustics/highlights inside water
                if (combinedNoise > 0.65)
                {
                    float highlight = (combinedNoise - 0.65) / 0.35;
                    col.rgb += _HighlightColor.rgb * highlight * _HighlightStrength * depth01; // More highlights near surface
                }
                
                // Crisp foam line at the very top
                if (distToSurface < 0.02)
                {
                    col.rgb = lerp(col.rgb, _HighlightColor.rgb, 0.8);
                    col.a = 1.0;
                }
                
                col *= i.color;
                return col;
            }
            ENDHLSL
        }
    }
}
