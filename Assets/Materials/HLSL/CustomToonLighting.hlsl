#ifndef CUSTOM_TOON_LIGHTING_INCLUDED
#define CUSTOM_TOON_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#if VERSION_GREATER_EQUAL(9, 0)
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#if (SHADERPASS != SHADERPASS_FORWARD)
#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
#endif
#else
#ifndef SHADERPASS_FORWARD
#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
#endif
#endif
#endif

struct CustomLightingData {
    // Position and orientation
    float3 positionWS;
    float3 normalWS;
    float3 viewDirectionWS;
    float4 shadowCoords;

    // Surface attributes
    float3 albedo;
    float smoothness;
};

float GetSmoothnessPower(float rawSmoothness)
{
    return exp2(10 * rawSmoothness + 1);
}

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData d, Light light)
{
    float3 radiance = light.color * light.shadowAttenuation;

    float diffuse = saturate(dot(d.normalWS, light.direction));
    float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
    float specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse;

    float3 color = d.albedo * radiance * (diffuse + specular);

    return color;
}
#endif

float3 CalculateCustomLighting(CustomLightingData d) {
#ifdef SHADERGRAPH_PREVIEW
    float3 lightDir = float3(0.5, 0.5, 0);
    float intensity = saturate(dot(d.normalWS, lightDir)) +
        pow(saturate(dot(d.normalWS, normalize(d.viewDirectionWS + lightDir))), GetSmoothnessPower(d.smoothness));
    return d.albedo * intensity;
#else
    Light mainLight = GetMainLight();//d.shadowCoord, d.positionWS, 1);
    float3 color = 0;
    color += CustomLightHandling(d, mainLight);
    return color;
#endif
}

void CalculateCustomLighting_float(float3 Position, float3 Normal, float3 ViewDirection, float3 Albedo, float Smoothness,
    out float3 Color) {

    CustomLightingData d;
    d.normalWS = Normal;
    d.viewDirectionWS = ViewDirection;
    d.albedo = Albedo;
    d.smoothness = Smoothness;

#ifdef SHADERGRAPH_PREVIEW
    d.shadowCoords = 0;
#else
    float4 positionCS = TransformWorldToHClip(Position);
	#if SHADOWS_SCREEN
        d.shadowCoords = ComputeScreenPos(positionCS);
	#else
        d.shadowCoords = TransformWorldToShadowCoord(Position);
	#endif

#endif

    Color = CalculateCustomLighting(d);
}

#endif