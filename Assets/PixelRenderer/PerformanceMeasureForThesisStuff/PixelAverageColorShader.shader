Shader "PixelArtPipeline/AverageColorPixelizeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Header(Pixelization Settings)]  [Space][Space]
        _PixelSize("Pixel Size", float)=2
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

            float2 pixelateCoord(float2 uv)
            {
                return floor(uv * _ScreenParams.xy / _PixelSize) * _PixelSize / _ScreenParams.xy;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pixelUV = pixelateCoord(i.uv);
                float2 blockStart = floor(pixelUV * _ScreenParams.xy);
                float3 averageColor = float3(0, 0, 0);
                int sampleCount = 0;

                // Loop through pixels in the block to compute the average color
                for (int x = 0; x < _PixelSize; x++)
                {
                    for (int y = 0; y < _PixelSize; y++)
                    {
                        float2 sampleUV = (blockStart + float2(x, y) + 0.5) / _ScreenParams.xy;
                        averageColor += tex2D(_MainTex, sampleUV).rgb;
                        sampleCount++;
                    }
                }

                // Calculate the average color
                averageColor /= sampleCount;

                return float4(averageColor, 1.0);
            }
            ENDCG
        }
    }
}
