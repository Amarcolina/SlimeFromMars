Shader "Custom/HealthShader" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Health ("Health Percent", Range(0, 1)) = 0.5
        _Replace ("Replacement Color", Color) = (0,0,0,0)
        _HealthColor ("Health Color", Color) = (0,1,0,1)
        _EmptyColor ("Empty Color", Color) = (1,0,0,1)
    }
    SubShader {
        Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

        Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};

            half4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
            half _Health;
            half4 _Replace;
            half4 _HealthColor;
            half4 _EmptyColor;

			fixed4 frag(v2f IN) : SV_Target{
                half4 c = tex2D (_MainTex, IN.texcoord);

                half anyDiff = any(c.rgb - _Replace.rgb);
                half4 replaceColor = lerp(_HealthColor, _EmptyColor, smoothstep(_Health, _Health + 1.0 / 256.0, c.a));

                return lerp(replaceColor, c, anyDiff) * _Color;
			}
		ENDCG
		}
    } 
    FallBack "Diffuse"
}
