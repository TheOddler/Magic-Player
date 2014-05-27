Shader "GUI/3D Text Shader" { 
	Properties { 
		_MainTex ("Font Texture", 2D) = "white" {} 
		_Color ("Text Color", Color) = (1,1,1,1) 
		_BackgroundColor ("Background Color", Color) = (0,0,0,1)
	}
	
	SubShader {
		Pass {
			Material {
				Diffuse (1,1,1,1)
				Ambient (1,1,1,1)
			}
			Lighting Off
			
			SetTexture [_MainTex] {
				ConstantColor [_BackgroundColor]
				Combine constant
			}
			
			SetTexture [_MainTex] {
				ConstantColor [_Color]
				Combine constant lerp(texture) previous
			}
		}
	} 
}