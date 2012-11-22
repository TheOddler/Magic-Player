Shader "Custom/Diffuse"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" { }
	}
	
	SubShader
	{
		Pass
		{
			Material
			{
				Diffuse (1,1,1,1)
				Ambient (1,1,1,1)
			}
			Lighting On
			SetTexture [_MainTex] { Combine texture * primary DOUBLE, texture * constant }
		}
	} 
}
