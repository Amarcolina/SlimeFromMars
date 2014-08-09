Shader "Custom/SlimeWallShader" {
	Properties {
		[PerRendererData] _MainTex ("Ramp (RGB)", 2D) = "white" {}
        _Smear ("Smear Texture", 2D)  = "white" {}
        _weights0 ("Weights0", Vector) = (0, 0, 0, 0)
        _weights1 ("Weights1", Vector) = (0, 0, 0, 0)
        _center   ("Center", Float) = 0
	}
	SubShader{
		Tags{ 
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

    Pass{
		CGPROGRAM
		#pragma vertex vert
        #pragma fragment frag
        #pragma target 3.0
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _Smear;
        float4 _weights0;
        float4 _weights1;
        float _center;

        struct v2f {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        struct appdata_t{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

        v2f vert(appdata_t IN){
			v2f OUT;
			OUT.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
			OUT.uv = IN.texcoord;
			return OUT;
		}

        float smoothMin(float a, float b){
            float k = 0.22;
            float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
            return lerp(b, a, h) - k * h * (1.0 - h);
        }

        float distToSeg(float minDist, float2 seg0, float weight0, float2 seg1, float weight1, float2 uv){
            float2 delta = seg1 - seg0;
            float mag2 = dot(delta, delta);
            float t = clamp(dot(uv - seg0, seg1 - seg0) / mag2, 0.0, 1.0);
            return smoothMin(minDist, lerp(1.0, distance(delta * t + seg0, uv), weight0 * weight1));
        }

        float getMinDist(float2 uv){
            float minDist = lerp(0.5, min(0.5, length(uv)), _center);

            minDist = distToSeg(minDist, float2(0, 0), _center, float2(-1, -1), _weights0.x, uv);
            minDist = distToSeg(minDist, float2(0, 0), _center, float2(0, -1), _weights0.y, uv);
            minDist = distToSeg(minDist, float2(0, 0), _center, float2(1, -1), _weights0.z, uv);
            minDist = distToSeg(minDist, float2(0, 0), _center, float2(1, 0), _weights0.w, uv);
            minDist = distToSeg(minDist, float2(0, 0), _center, float2(1, 1), _weights1.x, uv);
            minDist = distToSeg(minDist, float2(0, 0), _center, float2(0, 1), _weights1.y, uv);
            minDist = distToSeg(minDist, float2(0, 0), _center, float2(-1, 1), _weights1.z, uv);
            minDist = distToSeg(minDist, float2(0, 0), _center, float2(-1, 0), _weights1.w, uv);

            minDist = distToSeg(minDist, float2(1, 0), _weights0.w, float2(0, 1), _weights1.y, uv);
            minDist = distToSeg(minDist, float2(1, 0), _weights0.w, float2(0, -1), _weights0.y, uv);
            minDist = distToSeg(minDist, float2(-1, 0), _weights1.w, float2(0, 1), _weights1.y, uv);
            minDist = distToSeg(minDist, float2(-1, 0), _weights1.w, float2(0, -1), _weights0.y, uv);

            return minDist;
        }

        half4 frag (v2f i) : COLOR {
            float minDist = 1.0 - getMinDist(float2(i.uv.x, 0) - float2(0.5, 0.5)) * 2.0;
            float ramp = pow(minDist * 0.85, 4.0) * 8.0;
            half4 c = tex2D(_MainTex, float2(0, ramp));
            c.a = c.a * tex2D(_Smear, i.uv).r;
            return c;
        }
		ENDCG

	} 
    }
	FallBack "Diffuse"
}
