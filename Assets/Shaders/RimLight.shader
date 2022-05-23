Shader "Custom/RimLight"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white"{}
        _MainColor("Main Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Float) = 1.0
        _Emiss("_Emiss", Float) = 1.0
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
        }
//        Pass
//        {
//            ZWrite On
//            ColorMask 0
//            CGPROGRAM
//            #pragma vertex vert;
//            #pragma fragment frag;
//            struct appdata
//            {
//                float4 vertex : POSITION;
//            };
//
//            struct v2f
//            {
//                float4 position : SV_POSITION;
//            };
//
//            float4 _color;
//
//            v2f vert(appdata v)
//            {
//                v2f o;
//                o.position = UnityObjectToClipPos(v.vertex);
//                return o;
//            }
//
//            float4 frag(v2f i) : SV_Target
//            {
//                return _color;
//            }
//            ENDCG
//        }
        Pass
        {
            ZWrite Off
            //Blend SrcAlpha OneMinusSrcAlpha
            Blend SrcAlpha One
            HLSLPROGRAM
            #pragma vertex vert;
            #pragma fragment frag;
            #include "../CustomRP/ShaderLibrary/Common.hlsl"

            struct appdata
            {
                float4 position : POSITION;
                half2 uv : TEXCOORD0;
                float3 normal : Normal;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPosition : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float3 _MainColor;
                float _RimPower;
                float _Emiss;
            CBUFFER_END
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.position);
                o.worldNormal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                o.worldPosition = mul(unity_ObjectToWorld, v.position);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 worldNormal = normalize(i.worldNormal);
                float3 worldView = normalize(_WorldSpaceCameraPos - i.worldPosition);
                float fresnel = pow(saturate(1 - dot(worldNormal, worldView)), _RimPower);

                float3 color = _MainColor * _Emiss;
                float alpha = fresnel * _Emiss;
                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
}