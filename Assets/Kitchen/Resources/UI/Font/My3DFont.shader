Shader "Custom/My3DFont" {
	Properties {
		// 只需要色彩和贴图属性
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags{ "RenderType" = "Transparent"  // 有透明效果的Shader
		"IgnoreProjector" = "True"  // 将不受Projector（用来模拟投影或之类效果的投射器）影响
		"Queue" = "Transparent" }  //设置渲染顺序为Transparent，默认的渲染顺序依次为Background，Geometry，AlphaTest，Transparent，Overlay

		LOD 200
		
		Pass{
			// 在材质中添加对色彩属性的支持
			Material{
				Diffuse[_Color]
			}
			Blend SrcAlpha OneMinusSrcAlpha //alpha融合方式
			Lighting Off // 不受光源影响
			Cull Off      // 双面显示
			ZTest Always // 总显示在最前面
			ZWrite Off    // 不把像素写到Depth通道中
			Fog{ Mode Off } // 不受雾的影响

			SetTexture[_MainTex]{  //设置贴图
				constantColor[_Color]
				combine  texture * constant // 使贴图和颜色属性融合
			}
		}
	}
	FallBack "Diffuse"
}
