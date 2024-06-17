Shader "PixPipe/OutlineObj"
{
    Properties
    {
        _CameraThreshold ("NormalThreshold",float)=0.0001
        _OutlineColor("OutlineColor", Color) = (0.2,0.2,0.9,1)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties
            sampler2D_float _CameraDepthTexture;
            float _CameraThreshold;
            float4 _OutlineColor;
            struct vertexInput
            {
                float4 vertex : POSITION;
                float3 texCoord : TEXCOORD0;
            };

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float3 texCoord : TEXCOORD0;
                float linearDepth : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.texCoord = input.texCoord;

                output.screenPos = ComputeScreenPos(output.pos);
                output.linearDepth = -(UnityObjectToViewPos(input.vertex).z * _ProjectionParams.w);

                return output;
            }

            float4 frag(vertexOutput input) : COLOR
            {
                float4 c = float4(0, 0, 0, 1);

                // decode depth texture info
                float2 uv = input.screenPos.xy / input.screenPos.w; // normalized screen-space pos
                float camDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                camDepth = Linear01Depth(camDepth); // converts z buffer value to depth value from 0..1

                float diff = saturate(input.linearDepth - camDepth);
                if (diff < _CameraThreshold)
                    c = lerp(_OutlineColor,(0,0,0,0), 1-_OutlineColor.w);

                return c;
                //return float4(camDepth, camDepth, camDepth, 1); // test camera depth value
                //return float4(input.linearDepth, input.linearDepth, input.linearDepth, 1); // test our depth
                //return float4(diff, diff, diff, 1);
            }
            ENDCG
        }
    }
}