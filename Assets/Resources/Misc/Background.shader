Shader "Custom/Background"
{
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Textures ("Textures", 2DArray) = "" {}
        _Rows ("Rows", Int) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // No culling or depth
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            float4 _MainTex_TexelSize;

            v2f vert(appdata_base v) {          
                v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = UnityObjectToClipPos(v.vertex);
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                o.grabPos = ComputeGrabScreenPos(o.pos);
                // #if UNITY_UV_STARTS_AT_TOP
                // if (_MainTex_TexelSize.y < 0)
                //         o.grabPos.y = 1-o.grabPos.y;
                // #endif
                return o;
            }

            UNITY_DECLARE_TEX2DARRAY(_Textures);
            float4 _Textures_TexelSize;
            
            int _TextureOrder_Length;
            float _TextureOrder[128];

            float _LineDeformationsHeight[256];
            float _LineDeformationsTime[256];
            float _LineDeformationsCamera[256];

            float _VerticalDeformationCamera;

            uint2 _BaseOffset;
            float2 _AutoscrollSpeed;
            float2 _PosMax;
            float2 _PosMin;

            uint _Rows;
            
            fixed4 frag (v2f i) : SV_Target {
                float2 position = _WorldSpaceCameraPos;
                position += _AutoscrollSpeed * _Time[1] * 60;                
                position += _BaseOffset; // TODO: Rework
                position = max(_PosMin, min(_PosMax, position));

                // Get screen-space position (in pixels) of the pixel currently being rendered
                uint2 screenPixelCoord = uint2(
                    i.grabPos.x * _ScreenParams.x,
                    i.grabPos.y * _ScreenParams.y
                );
                
                screenPixelCoord.y -= position.y * 30 * _VerticalDeformationCamera;  // TODO: Rework

                int hDeformationId;
                uint lineHeightAcc = 0;
                for (uint i = 0; i < 512; i++) {
                    lineHeightAcc += _LineDeformationsHeight[i];
                    if (lineHeightAcc > screenPixelCoord.y) {
                        hDeformationId = i;
                        break;
                    }
                }

                screenPixelCoord.x -= _Time[1] * 60 * _LineDeformationsTime[hDeformationId];
                screenPixelCoord.x -= position.x * 30 * _LineDeformationsCamera[hDeformationId];

                float2 textureCoord = float2(
                    screenPixelCoord.x / _Textures_TexelSize.w,
                    screenPixelCoord.y / _Textures_TexelSize.z
                );

                uint textureOrderRow = textureCoord.y;
                uint columns = _TextureOrder_Length / _Rows;
                uint textureOrderCol = uint(textureCoord.x) % columns;
                uint textureOrderIndex = (textureOrderRow * columns) + textureOrderCol;

                textureCoord %= 1;

                uint textureId = _TextureOrder[textureOrderIndex];

                return UNITY_SAMPLE_TEX2DARRAY(
                    _Textures,
                    float3(
                        textureCoord.x,
                        textureCoord.y,
                        textureId
                    )
                );
            }
            ENDCG
        }
    }
}
