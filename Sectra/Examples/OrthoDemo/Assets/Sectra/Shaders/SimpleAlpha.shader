Shader "SimpleAlpha" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 400
		Blend One One

	CGPROGRAM
		#pragma surface surf Standard alpha
		#pragma target 3.0

		struct Input {
			half4 color : COLOR0;
		};
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Color
			o.Albedo = _Color.rgb;
			o.Alpha = 0.25;
			// Shiny
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
	ENDCG
	}
	FallBack "Diffuse"
}
