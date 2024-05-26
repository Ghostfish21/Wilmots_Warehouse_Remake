Shader "Unlit/DateBarShader"
{
    Properties
    {
        _Progress ("Progress", Float) = 0
        _ProgressColor ("Progress Color", Color) = (0,0,0,1) 
        _Color ("Color", Color) = (0,0,0,1)
        _DateBarTex ("Date Bar Tex", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _ProgressColor;
            float _Progress;
            float4 _Color;
            sampler2D _DateBarTex;
            float4 _DateBarTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _DateBarTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = tex2D(_DateBarTex, i.uv);
                if (i.uv.x > _Progress) 
                    return (1-color.a) * _Color;
                return (1-color.a) * _ProgressColor;
            }
            ENDCG
        }
    }
}
