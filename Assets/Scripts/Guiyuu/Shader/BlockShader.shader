Shader "Unlit/BlockShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineTex ("Outline Tex", 2D) = "black" {}
        _EdgeWidth ("Edge Width", Int) = 1
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

            sampler2D _MainTex;
            sampler2D _OutlineTex;
            float _EdgeWidth;
            float4 _MainTex_ST;

            CBUFFER_START(UnityPerMaterial)
            float _ShowEdge;
            float _Left;
            float _Right;
            float _Top;
            float _Bottom;
            float _TopLeft;
            float _TopRight;
            float _BotLeft;
            float _BotRight;
            float _Visibility;
            float _IsPickedUp;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float top0 = step(i.uv.y, _EdgeWidth/2.0/64.0);
                float bot0 = step(1.0 - i.uv.y, _EdgeWidth/2.0/64.0);
                float left0 = step(i.uv.x, _EdgeWidth/2.0/64.0);
                float right0 = step(1.0 - i.uv.x, _EdgeWidth/2.0/64.0);
                float topLeft0 = top0 * left0;
                float topRight0 = top0 * right0;
                float botLeft0 = bot0 * left0;
                float botRight0 = bot0 * right0;
                float grid = top0*_Top + bot0*_Bottom + left0*_Left + right0*_Right + topLeft0*_TopLeft +
                    topRight0*_TopRight + botLeft0*_BotLeft + botRight0*_BotRight;
                
                fixed4 main = tex2D(_MainTex, i.uv);
                float4 outline = tex2D(_OutlineTex, i.uv);

                float4 color = lerp(main, outline, saturate(_Visibility));
                color = lerp(color, grid.rrrr, saturate(_Visibility-1));
                color = color * (1-_IsPickedUp) + main * _IsPickedUp;
                
                float top = step(i.uv.y, _EdgeWidth/64.0);
                float bot = step(1.0 - i.uv.y, _EdgeWidth/64.0);
                float left = step(i.uv.x, _EdgeWidth/64.0);
                float right = step(1.0 - i.uv.x, _EdgeWidth/64.0);
                float topLeft = top * left;
                float topRight = top * right;
                float botLeft = bot * left;
                float botRight = bot * right;
                
                return color * (1 - _ShowEdge)
                + (main + top*_Top + bot*_Bottom + left*_Left + right*_Right + topLeft*_TopLeft +
                    topRight*_TopRight + botLeft*_BotLeft + botRight*_BotRight)*_ShowEdge;
            }
            ENDCG
        }
    }
}
