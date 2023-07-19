// Definition for custom surface shader
// NOTE: Have to use surface instead of unlit for shadows to work!
Shader "Custom/ClickerSurface" {

	// Define properties that are shown in the inspector
	Properties {

		// Add a property for the color, with the default set to white
		ColorProperty( "Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )

		// Add a property for the texture, with the default set to white
		// NOTE: This is only used for UV mapping to get model depth to render properly
		TextureProperty( "Texture", 2D ) = "white" {}

	}
		
	// The actual shader
	SubShader {

		// Set the type of shader
		Tags {
			"RenderType" = "Opaque"
		}

		// Set the level of detail/depth
		LOD 200

		// Begin CG Code
		CGPROGRAM

			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			// Define global variables for the inspector properties
			sampler2D TextureProperty;
			fixed4 ColorProperty;

			// Create a structure for the incoming data
			struct Input {
				float2 uv_MainTex;
			};

			// Surface shader modifier function
			void surf( Input input, inout SurfaceOutputStandard result ) {

				// Combine the UV mapping for the default texture with the custom color property
				fixed4 color = tex2D( TextureProperty, input.uv_MainTex ) * ColorProperty;

				// Apply the RGB channels to the surface albedo
				result.Albedo = color.rgb;

				// Never go transparent, even if the provided color has alpha
				result.Alpha = 1.0;

				// Never apply any smoothness or metallicness
				result.Metallic = 0.0;
				result.Smoothness = 0.0;

			}

		// End CG Code
		ENDCG
	}

}
