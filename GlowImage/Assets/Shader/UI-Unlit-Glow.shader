Shader "UI/Unlit/Glow"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        _BlurColor ("Blur Color", Color) = (1, 1, 1, 1)
        _BlurSize ("Blur Size", float) = 1
        _BlurIntensitive("Blur Intensitive", float) = 1

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

    CGINCLUDE
    struct appdata_t
    {
        float4 vertex   : POSITION;
        float4 color    : COLOR;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex   : SV_POSITION;
        fixed4 color    : COLOR;
        half2 texcoord  : TEXCOORD0;
        float4 worldPosition : TEXCOORD1;
    };

    #pragma multi_compile __ UNITY_UI_ALPHACLIP
    #pragma multi_compile QUALITY_LOW QUALITY_MEDIUM QUALITY_HIGH

    #include "UnityCG.cginc"
    #include "UnityUI.cginc"

    #ifdef QUALITY_LOW
    // guassian kernel 3, 1.4
    static const float2x2 guass = { { 0.09235313, 0.1191903 }, { 0.1191903, 0.1538262 } };
    #endif
    #ifdef QUALITY_MEDIUM
    // guassian kenrel 5, 1.4
    static const float3x3 guass = { { 0.01214613, 0.02610995, 0.03369732 }, { 0.02610995, 0.05612731, 0.07243753 }, { 0.03369732, 0.07243753, 0.09348739 } };
    #endif
    #ifdef QUALITY_HIGH
    // guassian kernel 7, 1.4
    static const float4x4 guass = { { 0.0008407254, 0.003010241, 0.00647097, 0.008351391 }, { 0.003010241, 0.01077825, 0.02316949, 0.02990239 }, { 0.00647097, 0.02316949, 0.04980635, 0.06427974 }, { 0.008351391, 0.02990239, 0.06427974, 0.082959 } };
    #endif

    fixed4 _Color;
    fixed4 _TextureSampleAdd;
    float4 _ClipRect;
    sampler2D _MainTex;
    float4 _MainTex_TexelSize;
    fixed4 _BlurColor;
    float _BlurSize;
    float _BlurIntensitive;

    v2f vert (appdata_t IN)
    {
        v2f OUT;
        OUT.worldPosition = IN.vertex;
        OUT.vertex = mul(UNITY_MATRIX_MVP, OUT.worldPosition);

        OUT.texcoord = IN.texcoord;

        #ifdef UNITY_HALF_TEXEL_OFFSET
        OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
        #endif

        OUT.color = IN.color * _Color;
        return OUT;
    }

    #define ClampSprite(alpha, texcoord) (alpha * step(0, texcoord.x)  * step(0, texcoord.y)  * step(texcoord.x, 1)  * step(texcoord.y, 1))

    fixed4 frag(v2f IN) : SV_Target
    {
        half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

        float needClip = UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

        #ifdef UNITY_UI_ALPHACLIP
        clip (needClip - 0.001);
        #endif

        color.a = ClampSprite(color.a, IN.texcoord);

        return color;
    }

    fixed4 frag_blur (v2f IN) : SV_Target
    {
        float needClip = UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
        #ifdef UNITY_UI_ALPHACLIP
        clip (needClip - 0.001);
        #endif

        #ifdef QUALITY_LOW
        int k = 1;
        #endif
        #ifdef QUALITY_MEDIUM
        int k = 2;
        #endif
        #ifdef QUALITY_HIGH
        int k = 3;
        #endif
        float2 blurSize = _BlurSize * _MainTex_TexelSize.xy;
        float blurAlpha = 0;
        float2 tempCoord;
        float tempAlpha;
        for (int px = -k; px <= k; px++)
        {
            for (int py = -k; py <= k; py++)
            {
                tempCoord.x = px * blurSize.x;
                tempCoord.y = py * blurSize.y;
                tempCoord += IN.texcoord;
                tempAlpha = tex2D(_MainTex, tempCoord).a + _TextureSampleAdd.a;
                tempAlpha *= guass[k - abs(px)][k - abs(py)];
                tempAlpha = ClampSprite(tempAlpha, tempCoord);
                blurAlpha += tempAlpha;
            }
        }
        half4 blurColor = _BlurColor;
        blurColor.a *= blurAlpha * IN.color.a;
        blurColor.a *= step(0.001, blurSize.x);
        blurColor *= _BlurIntensitive;
        return blurColor;
    }
    ENDCG

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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_blur
			ENDCG
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
	}
}
