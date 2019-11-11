Shader "Custom/Palette Swap (BG)"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        colorMap ("Color Map", 2D) = "white" {}
        [Slider]
        _RedScale ("Red Scale (Unknown Colors)", Float) = 1
        [Slider]
        _BlueScale ("Blue Scale (Unknown Colors)", Float) = 1
        [Slider]
        _GreenScale ("Green Scale (Unknown Colors)", Float) = 1
        _Threshold ("Threshold", Float) = 1
    }
    SubShader
    {
        // Draw ourselves after all opaque geometry
        Tags { "Queue" = "Transparent" }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {

        }

        // Render the object with the texture generated above, and invert the colors
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
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                        o.grabPos.y = 1-o.grabPos.y;
                #endif
                return o;
            }

            sampler2D _GrabTexture, colorMap;
            float4 colorMap_TexelSize;
            float _Brightness;
            float _RedScale;
            float _BlueScale;
            float _GreenScale;
            float _Threshold;

            fixed4 frag(v2f i) : SV_Target {
                uint maxPaletteLength = 64;
                
                fixed4 bgcolor = tex2Dproj(_GrabTexture, i.grabPos);
                int3 bgcolor256 = round(bgcolor.rgb * 255);
                uint colorMapWidth = colorMap_TexelSize.z; // In pixels
                
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
                    int3 colMapIndex256 = round(colMapIndex.rgb * 255);

                    if (all(abs(bgcolor256.rgb - colMapIndex256.rgb) <= _Threshold)) { // Did we find a matching color?
                    // if (all(bgcolor256.rgb == colMapIndex256.rgb)) { // Did we find a matching color?
                        // If so, grab the replacement color for the cycle
                        return tex2D(
                            colorMap,
                            float2(
                                (float)index / colorMapWidth,
                                0
                            )
                        );
                    }
                }
                return fixed4(
                    bgcolor.r * _RedScale,
                    bgcolor.g * _GreenScale,
                    bgcolor.b * _BlueScale,
                    bgcolor.a
                );
            }
            ENDCG
        }

    }
}