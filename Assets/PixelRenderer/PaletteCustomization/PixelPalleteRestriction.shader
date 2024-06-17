Shader "PixPipe/ColorAndDither"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DitherTex ("Dither Texture", 2D) = "white" {}
        _DitherAmount("Dither Strength", Range (0, 0.08)) = 0.005
        _PixelSize("Pixel size", int) = 2
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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : SV_POSITION;
                float2 uv2 : TEXCOORD2;
            };

            int _PixelPalletCount;
            half4 _Pallet[1000];
            v2f vert(appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.uv2 = v.texcoord2;
                o.normal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            sampler2D _MainTex;
            sampler2D _DitherTex;
            int _PixelSize;
            float _DitherAmount;

            fixed4 frag(v2f i) : SV_Target
            {
                int texelX = floor(i.uv.x * _ScreenParams.x);
                int texelY = floor(i.uv.y * _ScreenParams.y);
                float2 cord = float2(texelX, texelY);


                float2 ditherCoord = frac((cord / float(_PixelSize)) / 8.0);
                float4 inColor = tex2D(_MainTex, i.uv);

                float4 nearestColor = _Pallet[0];
                float nearestDistance = 100.0;


                for (int j = 0; j < _PixelPalletCount; j++)
                {
                    float dist = length(inColor - _Pallet[j]);
                    if (dist < nearestDistance + tex2D(_DitherTex, ditherCoord).x * _DitherAmount)
                    {
                        nearestDistance = dist;
                        nearestColor = _Pallet[j];
                    }
                }
                return nearestColor;
            }
            ENDCG
        }
    }
}
