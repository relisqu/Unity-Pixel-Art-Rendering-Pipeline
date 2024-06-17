Shader "PixPipe/SimpleOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Header(Pixelization Settings)]  [Space][Space]
        _PixelSize("Pixel Size", float)=2
        
        [Header(Outline Strength Settings)] [Space][Space] 
        _DepthThreshold ("Depth Outlines Threshold",float)=0.04
        _NormalThreshold ("Normal Outlines Threshold",float)=0.04
        [Header(Outline Customization Settings)] [Space][Space] 
        _BaseOutlineWidth ("Outline Width", float) = 1.0
        _OutlineColor("Outline Color", Color) = (0.2,0.2,0.2,1)
        _InnerOutlineColor("Inner Outline Color", Color) = (0.8,0.8,0.8,0.8)
        [Toggle] _PerspectiveCorrection ("Use Perspective Correction", Float) = 1.0
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
                float3 viewPos : TEXCOORD1; // Add this line
            };

            bool _PerspectiveCorrection;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewPos = mul(unity_ObjectToWorld, v.vertex).xyz; // Compute the view-space position

                return o;
            }

            float _DepthThreshold;
            float _NormalThreshold;
            sampler2D _CameraDepthNormalsTexture;
            sampler2D _MainTex;
            float2 _MainTex_TexelSize;
            float4 _OutlineColor;
            float4 _InnerOutlineColor;

            float _PixelSize;
            float _PixelYSize;
            float _OutlineWidth;
            float _BaseOutlineWidth;

            float getDepth(float2 uv, float2 offset)
            {
                float4 NormalDepth;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, uv + offset), NormalDepth.w, NormalDepth.xyz);
                return clamp(NormalDepth.w * 100, 0, 1);
            }

            float3 getNormals(float2 uv, float2 offset)
            {
                float4 NormalDepth;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, uv + offset), NormalDepth.w, NormalDepth.xyz);
                return NormalDepth.xyz;
            }

            float2 pixelateCoord(float2 coord)
            {
                float2 c = floor(coord / float(_PixelSize)) * float(_PixelSize) / _ScreenParams.xy;
                return c;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float outlineWidth = _BaseOutlineWidth;

                if (_PerspectiveCorrection)
                {
                    float dep=getDepth(i.uv, 0);
                    outlineWidth = min(_BaseOutlineWidth, _BaseOutlineWidth * (0.5 / dep));
                }

                int texelX = floor(i.uv.x * _ScreenParams.x);
                int texelY = floor(i.uv.y * _ScreenParams.y);
                float2 cord = pixelateCoord(float2(texelX, texelY));
                float depth = getDepth(cord, 0);

                float2 uvs[4];

                float2 offsets[4] = {
                    float2(0.0, outlineWidth * _PixelSize),
                    float2(0.0, -outlineWidth * _PixelSize),
                    float2(outlineWidth * _PixelSize, 0.0),
                    float2(-outlineWidth * _PixelSize, 0.0)
                };

                for (int j = 0; j < 4; ++j)
                {
                    uvs[j] = cord + pixelateCoord(offsets[j]);
                }

                float depths[4];
                float depthDiff = 0.0;

                for (int j = 0; j < 4; ++j)
                {
                    depths[j] = getDepth(uvs[j], 0);
                    depthDiff += depth - depths[j];
                }

                float depthEdge = step(_DepthThreshold, depthDiff);

                float3 normal = getNormals(cord, 0);
                float3 normals[4];
                float3 normalsDiff = 0.0;

                for (int j = 0; j < 4; ++j)
                {
                    normals[j] = getNormals(uvs[j], 0);
                    normalsDiff += normal - normals[j];
                }

                float normalsEdge = step(_NormalThreshold, normalsDiff);

                if (depthEdge > 0)
                {
                    float4 _innerOutlineColor = lerp(_OutlineColor, tex2D(_MainTex, cord), 1 - _OutlineColor.w);
                    
                    return lerp(_innerOutlineColor, tex2D(_MainTex, i.uv),depth*0.02f);
                }
                if (normalsEdge > 0)
                {
                    float4 _innerOutlineColor = _InnerOutlineColor;
                    return lerp(_innerOutlineColor, tex2D(_MainTex, cord), 1 - _InnerOutlineColor.w);
                }
                else
                {
                    return tex2D(_MainTex, i.uv);
                }
            }
            ENDCG
        }
    }
}