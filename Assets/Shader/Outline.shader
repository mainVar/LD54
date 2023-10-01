Shader "Unlit/Outline"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }
    SubShader
    { 
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 texcol = tex2D(_MainTex, i.uv);
                half4 outcol = texcol;
                half2 pixelSize = 1.0 / _ScreenParams.xy;
                half2 offset[8] = {half2(-1,-1), half2(0,-1), half2(1,-1), half2(-1,0), half2(1,0), half2(-1,1), half2(0,1), half2(1,1)};
                for (int j = 0; j < 8; j++)
                {
                    half4 sample = tex2D(_MainTex, i.uv + offset[j] * _OutlineWidth * pixelSize);
                    outcol += _OutlineColor * (sample - texcol);
                }
                return outcol;
            }
            ENDCG
        }
    }
}