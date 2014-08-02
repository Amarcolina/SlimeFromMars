Shader "Custom/BrambleShader" {
	Properties {
		_MainTex ("Base (RGBA)", 2D) = "white" {}
        _Cutoff  ("Cutoff", Range(0.004, 0.996)) = 0.5
        _Spread  ("Alpha Spread", Range(0, 0.5)) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha

		sampler2D _MainTex;
        float _Cutoff;
        float _Spread;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = 1.0 - smoothstep(c.a - _Spread, c.a + _Spread, 1.0 - _Cutoff);
            o.Alpha *= 1.0 - step(c.a, 0.004);
            o.Alpha *= step(c.a, 0.996);
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
