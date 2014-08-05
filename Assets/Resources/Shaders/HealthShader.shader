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
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert
        //
		sampler2D _MainTex;
        half _Health;
        half4 _Replace;
        half4 _Color;
        half4 _HealthColor;
        half4 _EmptyColor;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);

            half anyDiff = any(c.rgb - _Replace.rgb);
            half4 replaceColor = lerp(_HealthColor, _EmptyColor, step(_Health, c.a));

            c = lerp(replaceColor, c, anyDiff) * _Color;

			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
