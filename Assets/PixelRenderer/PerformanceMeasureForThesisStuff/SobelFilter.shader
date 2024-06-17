Shader "Custom/SobelFilter"
{
    Properties
    {
        [HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}
        _PixelDensity ("Pixel Density", float) = 10
        _Power ("Power", float) = 50
        _PosterizationCount ("Count", int) = 8
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Edge" 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            float _PixelDensity;
            int _PosterizationCount;
            float _Power;
            sampler2D _MainTex;

            sampler2D _CameraDepthNormalsTexture;

            float SampleDepth(float2 uv)
            {
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthNormalsTexture, uv);
            }

            float2 sobel(float2 uv)
            {
                float2 delta = float2(_PixelDensity / _ScreenParams.x, _PixelDensity / _ScreenParams.y);

                float up = SampleDepth(uv + float2(0.0, 1.0) * delta);
                float down = SampleDepth(uv + float2(0.0, -1.0) * delta);
                float left = SampleDepth(uv + float2(1.0, 0.0) * delta);
                float right = SampleDepth(uv + float2(-1.0, 0.0) * delta);
                float centre = SampleDepth(uv);

                float depth = max(max(up, down), max(left, right));
                return float2(
                    clamp(up - centre, 0, 1) + clamp(down - centre, 0, 1) + clamp(left - centre, 0, 1) + clamp(
                        right - centre, 0, 1), depth);
            }

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : SV_POSITION;
            };

            bool _PerspectiveCorrection;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);

                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float2 sobelData = sobel(i.uv);
                float s = pow(abs(1 - saturate(sobelData.x)), _Power);
                half4 col = tex2D(_MainTex, i.uv);
                half4 d =  tex2D( _CameraDepthNormalsTexture, i.uv);
                float x = ceil(SampleDepth(i.uv) - d.x);

                //col.a = x;
                col = pow(abs(col), 0.4545);
                half4 color = col;
                color.z = round(color.z * _PosterizationCount) / _PosterizationCount;
                col = color;
                col = pow(abs(col), 2.2);


                s = floor(s + 0.2);

                s = lerp(1.0, s, ceil(sobelData.y - d.x));
                float depth = lerp(sobelData.y, SampleDepth(i.uv), s);
                col.rgb *= s;
                col.a += 1 - s;


                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}