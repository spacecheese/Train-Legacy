Shader "Splines/TerrainLeveler"
{
    Properties 
	{
		_MainTex ("Texture", any) = "" {} 
	}
    SubShader
    {
		ZTest Always Cull Off ZWrite Off
		
		CGINCLUDE
			#include "UnityCG.cginc"
            #include "TerrainTool.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			
			float3 _Start;
			float3 _End;
			float4 _BrushParams;
			#define FLATTEN_RADIUS		(_BrushParams[0])
			#define ERODE_RADIUS		(_BrushParams[1])
			#define ERODE_STRENGTH		(_BrushParams[2])
			#define ERODE_MIX_STRENGTH	(_BrushParams[3])
			
			struct appdata_t {
                float4 vertex : POSITION;
                float2 pcUV : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 pcUV : TEXCOORD0;
            };
			
			v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.pcUV = v.pcUV;
                return o;
            }
			
			inline float3 projectOnLineSegment(float3 start, float3 end, float3 other)
			{
				float3 delta = end - start;
				
				float t = dot(delta, other) / dot(delta, delta);
				t = clamp(t, 0, 1);
				
				return lerp(start, end, t);
			}
		ENDCG
	
        Pass
        {
            Name "Leveler"
			
			CGPROGRAM
			
			#pragma vertex vert
            #pragma fragment Leveler
			
			float4 Leveler(v2f i) : SV_Target
			{
				float2 heightmapUV = PaintContextUVToHeightmapUV(i.pcUV);
				float oldHeight = UnpackHeightmap(tex2D(_MainTex, heightmapUV));
				float3 heightCoord = {heightmapUV.x, 0.0f, heightmapUV.y};
				float3 projected = projectOnLineSegment(_Start, _End, heightCoord - _Start);
				
				if (distance(projected.xy, heightmapUV) <= FLATTEN_RADIUS)
					return PackHeightmap(projected.y);
				else
					return PackHeightmap(oldHeight);
			}
			ENDCG
		}
		
		Pass
		{
			Name "Erode"
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment Erode
			
			// See https://github.com/Unity-Technologies/TerrainToolSamples
			// TerrainToolSamples/RidgeErode
			float4 Erode(v2f i) : SV_Target
            {
				float2 brushUV = PaintContextUVToBrushUV(i.pcUV);
				float2 heightmapUV = PaintContextUVToHeightmapUV(i.pcUV);

				const float2 coords [4] = { {-1,0}, { 1,0}, {0, -1}, { 0, 1} };

				float hc = UnpackHeightmap(tex2D(_MainTex, heightmapUV));
				float hl = UnpackHeightmap(tex2D(_MainTex, heightmapUV + coords[0] * _MainTex_TexelSize.xy));
				float hr = UnpackHeightmap(tex2D(_MainTex, heightmapUV + coords[1] * _MainTex_TexelSize.xy));
				float ht = UnpackHeightmap(tex2D(_MainTex, heightmapUV + coords[2] * _MainTex_TexelSize.xy));
				float hb = UnpackHeightmap(tex2D(_MainTex, heightmapUV + coords[3] * _MainTex_TexelSize.xy));

				float l = min(hl, hr);
				float r = max(hl, hr);
				float b = min(hb, ht);
				float t = max(hb, ht);

				float height = hc;
					
				if (height > l && height < r)
				{
					float hlr01 = pow((height - l) / (r - l), ERODE_STRENGTH);
					height = hlr01 * (r - l) + l;
				}	

				if (height > b && height < t)
				{
					float hbt01 = pow((height - b) / (t - b), ERODE_STRENGTH);
					height = hbt01 * (t - b) + b;
				}
				
				height = lerp(0.25f * ( hl + hr + ht + hb ), height, ERODE_MIX_STRENGTH);
				
				float3 heightCoord = {heightmapUV.x, 0.0f, heightmapUV.y};
				float3 projected = projectOnLineSegment(_Start, _End, heightCoord - _Start);
				
				float strength = clamp(lerp(1, 0, distance(projected.xy, heightmapUV) / ERODE_RADIUS), 0, 1);
				
				return clamp(lerp(hc, height, strength), 0, 0.5f);
            }
			
			ENDCG
		}
    }
    FallBack Off
}
