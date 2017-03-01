Shader "SimpleLao" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess("Shininess", Range(0.03, 1)) = 0.078125
	}
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 400
		Cull Off

	CGPROGRAM
		#pragma surface surf BlinnPhong

		fixed4 _Color;
		half _Shininess;

		struct Input {
			half4 color : COLOR0;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			// Color
			o.Albedo = _Color.rgb * (0.9 * IN.color.rgb + 0.1);
			o.Alpha = _Color.a;
			// Shiny
			o.Gloss = 0;
			o.Specular = _Shininess;
		}
	ENDCG
	}

	FallBack "Specular"
}
