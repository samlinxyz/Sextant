Shader "Hidden/CircularFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MaskColor("Mask Color", Vector) = (0,0,0,1)
		_MaskCenter("Mask Center", Vector) = (0,0,0,0)
		_FadeRadius("Fade Radius", float) = 1
		_Softness("Softness", float) = 0.3
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			fixed4 _MaskColor;
			fixed2 _MaskCenter;
			fixed _FadeRadius;
			fixed _Softness;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				
				//	Calculate the pixel coordinates of the current point relative to the mask center.
				fixed2 relative = (i.uv - _MaskCenter) * _ScreenParams.xy;
				//  Normalize to the longest screen dimension.
				relative /= max(_ScreenParams.x, _ScreenParams.y);

				//	The mask is opaque if further than _FadeRadius from the center.
				fixed transparency = clamp((_FadeRadius - length(relative)) / _Softness, 0, 1);
				col.rgb = lerp(_MaskColor.rgb, col.rgb, smoothstep(0, 1, transparency));

                return col;
            }
            ENDCG
        }
    }
}