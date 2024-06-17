Shader "PixPipe/SimplePixelization"
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

            sampler2D _MainTex;
            float _PixelSize;
           
            float2 pixelateCoord(float2 coord)
            {
                float2 c = floor(coord / float(_PixelSize)) * float(_PixelSize) / _ScreenParams.xy;
                return c;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                
                int texelX = floor(i.uv.x * _ScreenParams.x);
                int texelY = floor(i.uv.y * _ScreenParams.y);
                float2 cord = pixelateCoord(float2(texelX, texelY));
                return tex2D(_MainTex, cord);
            }
            ENDCG
        }
    }
}