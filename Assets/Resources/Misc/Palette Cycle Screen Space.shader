Shader "Custom/Palette Cycle (Screen Space)"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        colorMap ("Color Map", 2D) = "white" {}
        _CycleSpeed ("Cycle Speed", Float) = 200
        [Toggle]
        _Lerp ("Linear Interpolation", Float) = 0
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

        GrabPass { }

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

            v2f vert(appdata_base v) {          
                v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = UnityObjectToClipPos(v.vertex);
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            sampler2D _GrabTexture;
            sampler2D _MainTex, colorMap;
            float _CycleSpeed;
            float4 colorMap_TexelSize;
            float _Lerp;

            fixed4 frag (v2f i) : SV_Target {
                uint maxPaletteLength = 32;
                bool doLerp = _Lerp == 1;

                // Yes, w and z are the correct properties.
                // https://docs.unity3d.com/Manual/SL-PropertiesInPrograms.html
                uint colorMapHeight = colorMap_TexelSize.w;
                uint colorMapWidth = colorMap_TexelSize.z;
                float rowHeight = 1.0 / (float)colorMapHeight; // height of each color map row in fraction of texture height
                uint cycleLength = colorMapHeight - 1;

                float cyclePositionRaw = fmod(_Time[0] * _CycleSpeed, cycleLength); // (used for lerp)
                uint cyclePosition = floor(cyclePositionRaw); // index of current cycle line (index line ignored)
                uint cyclePositionNext = (cyclePosition + 1) % cycleLength; // index of next cycle line (index line ignored)

                fixed4 col = tex2Dproj(_GrabTexture, i.grabPos);

                fixed4 colorResult;
                fixed4 colorResultNext;

                // Search through the index of the color map and try to find a color that matches the current pixel.
                for (uint index = 0; index < maxPaletteLength; index++) {
                    if (index >= colorMapWidth) break;

                    fixed4 colMapIndex = tex2D( // Current color in color map index
                        colorMap,
                        float2(
                            (float)index / colorMapWidth,
                            1
                        )
                    );
                    if (all(col.rgb == colMapIndex.rgb)) { // Did we find a matching color?
                        // If so, grab the replacement color for the cycle
                        colorResult = tex2D(
                            colorMap,
                            float2(
                                (float)index / colorMapWidth,
                                1 - rowHeight - (rowHeight * cyclePosition)
                            )
                        );

                        colorResult = fixed4(
                            colorResult.r,
                            colorResult.g,
                            colorResult.b,
                            col.a
                        );

                        if (!doLerp) return colorResult;

                        // If we aren't interpolating the cycle, we stop here
                        colorResultNext = tex2D(
                            colorMap,
                            float2(
                                (float)index / colorMapWidth,
                                1 - rowHeight - (rowHeight * cyclePositionNext)
                            )
                        );

                        colorResultNext = fixed4(
                            colorResultNext.r,
                            colorResultNext.g,
                            colorResultNext.b,
                            col.a
                        );

                        float lerpAmt = cyclePositionRaw - cyclePosition;

                        return lerp(
                            colorResult,
                            colorResultNext,
                            lerpAmt
                        );
                    }
                }
                return col;
            }
            ENDCG
        }
    }
}
