Shader "UI/CircleMask"
{
    Properties
    {
        _Color("Color", Color) = (0,0,0,0.8)
        _Center("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius("Radius", Float) = 0.3
        _Aspect("Aspect", Float) = 1.7777 // 16:9
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color;
            float2 _Center;
            float _Radius;
            float _Aspect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uvDiff = i.uv - _Center;
                uvDiff.x *= _Aspect; // 横方向を補正
                float dist = length(uvDiff);

                if (dist < _Radius)
                    return fixed4(0,0,0,0); // 穴の中は透明
                return _Color; // 穴の外は黒
            }
            ENDCG
        }
    }
}
