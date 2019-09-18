Shader "Custom/Lighten"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Opacity ("Opacity", Float) = 1

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

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

        // Render the object with the texture generated above, and invert the colors
        Pass
        {
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
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = UnityObjectToClipPos(v.vertex);
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                o.grabPos = ComputeGrabScreenPos(o.pos);
                // o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex, _BackgroundTexture;
            float _Opacity;


            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 bgColor = tex2Dproj(_BackgroundTexture, i.grabPos);
                fixed4 spriteColor = tex2D(_MainTex, i.uv);

                return lerp( 
                    bgColor,
                    fixed4(
                        max(bgColor.r, spriteColor.r),
                        max(bgColor.g, spriteColor.g),
                        max(bgColor.b, spriteColor.b),
                        1
                    ),
                    _Opacity
                );
            }
            ENDCG
        }

    }
}