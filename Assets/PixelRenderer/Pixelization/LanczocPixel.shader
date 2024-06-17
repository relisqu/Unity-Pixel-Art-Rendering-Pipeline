Shader "PixPipe/LanczocPixelization"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Header(Pixelization Settings)]  [Space][Space]
        _PixelSize("Pixel Size", float) = 8
        _LanczosRadius("Lanczos Radius", float) = 2.0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags
        {
            "Queue" = "Transparent-1"
        }
        Pass
        {
            Name "Pixelate"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _PixelSize;
            float _LanczosRadius;

            float lanczosWeight(float x, float radius)
            {
                if (x == 0.0) return 1.0;
                if (x < -radius || x > radius) return 0.0;
                x *= 3.14159265359;
                float sincX = sin(x) / x;
                float sincXOverRadius = sin(x / radius) / (x / radius);
                return sincX * sincXOverRadius;
            }

            float2 pixelateCoord(float2 uv)
            {
                float2 pixelSize = _PixelSize / _ScreenParams.xy;
                return floor(uv / pixelSize) * pixelSize;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pixelUV = pixelateCoord(i.uv);
                float2 blockStart = floor(pixelUV * _ScreenParams.xy);
                float2 centerUV = (blockStart + _PixelSize / 2.0) / _ScreenParams.xy;

                float3 color = float3(0, 0, 0);
                float totalWeight = 0.0;

                for (int x = -int(_LanczosRadius); x <= int(_LanczosRadius); x++)
                {
                    for (int y = -int(_LanczosRadius); y <= int(_LanczosRadius); y++)
                    {
                        float2 sampleOffset = float2(x, y);
                        float2 sampleUV = (centerUV * _ScreenParams.xy + sampleOffset) / _ScreenParams.xy;
                        float weight = lanczosWeight(length(sampleOffset), _LanczosRadius);

                        color += tex2D(_MainTex, sampleUV).rgb * weight;
                        totalWeight += weight;
                    }
                }

                color /= totalWeight;

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
