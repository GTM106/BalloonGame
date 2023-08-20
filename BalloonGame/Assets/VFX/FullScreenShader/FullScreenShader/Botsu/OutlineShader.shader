Shader "Custom/OutlineShader"
{
    Properties{
           _MainTex("Texture", 2D) = "white" {}
           _OutlineColor("Outline Color", Color) = (1,1,1,1)
           _OutlineWidth("Outline Width", Range(0.0, 0.1)) = 0.01
    }
    SubShader
    {
        Pass {
            Tags {"Queue" = "Transparent" "RenderType" = "Opaque"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform float4 _OutlineColor;
            uniform float _OutlineWidth;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float4 texColor = tex2D(_MainTex, i.uv);
                float4 outlineColor = _OutlineColor;

                // Calculate outline
                float4 outline = tex2D(_MainTex, i.uv + float2(_OutlineWidth, 0));
                outline += tex2D(_MainTex, i.uv + float2(-_OutlineWidth, 0));
                outline += tex2D(_MainTex, i.uv + float2(0, _OutlineWidth));
                outline += tex2D(_MainTex, i.uv + float2(0, -_OutlineWidth));
                outline /= 4.0;

                // Set color based on outline
                if (texColor.a < 0.1) {
                    return outlineColor;
                }
                else {
                     return texColor;
                }
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
