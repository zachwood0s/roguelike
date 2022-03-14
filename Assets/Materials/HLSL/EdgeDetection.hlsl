#ifndef EDGE_DETECTION_INCLUDED
#define EDGE_DETECTION_INCLUDED

#pragma enable_d3d11_debug_symbols


#include "DecodeDepthNormals.hlsl"

TEXTURE2D(_DepthNormalsTexture); SAMPLER(sampler_DepthNormalsTexture);

// The sobel effect runs by sampling the texture around a point to see
// if there are any large changes. Each sample is multiplied by a convolution
// matrix weight for the x and y components seperately. Each value is then
// added together, and the final sobel value is the length of the resulting float2.
// Higher values mean the algorithm detected more of an edge


// These are points to sample relative to the starting point
/*
static float2 sobelSamplePoints[9] = {
    float2(-1, 1), float2(0, 1), float2(1, 1),
    float2(-1, 0), float2(0, 0), float2(1, 0),
    float2(-1, -1), float2(0, -1), float2(1, -1),
};
*/
static float2 sobelSamplePoints[9] = {
    float2(-1, 1), float2(0, 1), float2(1, 1),
    float2(-1, 0), float2(0, 0), float2(1, 0),
    float2(-1, -1), float2(0, -1), float2(1, -1),
};


// Weights for the x component
static float sobelXMatrix[9] = {
    1, 0, -1,
    2, 0, -2,
    1, 0, -1
};

// Weights for the y component
static float sobelYMatrix[9] = {
    1, 2, 1,
    0, 0, 0,
    -1, -2, -1
};

// This function runs the sobel algorithm over the depth texture
void DepthSobel_float(float2 UV, float Thickness, out float Out) {
    float2 sobel = 0;
    // We can unroll this loop to make it more efficient
    // The compiler is also smart enough to remove the i=4 iteration, which is always zero
    [unroll] for (int i = 0; i < 9; i++) {
        float depth = SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV + sobelSamplePoints[i] * Thickness);
        sobel += depth * float2(sobelXMatrix[i], sobelYMatrix[i]);
    }
    // Get the final sobel value
    Out = length(sobel);
}

void ViewDirectionFromScreenUV_float(float2 In, out float3 Out) {
    // Code by Keijiro Takahashi @_kzr and Ben Golus @bgolus
    // Get the perspective projection
    float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
    // Convert the uvs into view space by "undoing" projection
    Out = -normalize(float3((In * 2 - 1) / p11_22, -1));
}

// This function runs the sobel algorithm over the opaque texture
void ColorSobel_float(float2 UV, float Thickness, out float Out) {
    // We have to run the sobel algorithm over the RGB channels separately
    float2 sobelR = 0;
    float2 sobelG = 0;
    float2 sobelB = 0;
    // We can unroll this loop to make it more efficient
    // The compiler is also smart enough to remove the i=4 iteration, which is always zero
    [unroll] for (int i = 0; i < 9; i++) {
        // Sample the scene color texture
        float3 rgb = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV + sobelSamplePoints[i] * Thickness);
        // Create the kernel for this iteration
        float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);
        // Accumulate samples for each color
        sobelR += rgb.r * kernel;
        sobelG += rgb.g * kernel;
        sobelB += rgb.b * kernel;
    }
    // Get the final sobel value
    // Combine the RGB values by taking the one with the largest sobel value
    Out = max(length(sobelR), max(length(sobelG), length(sobelB)));
    // This is an alternate way to combine the three sobel values by taking the average
    // See which one you like better
    //Out = (length(sobelR) + length(sobelG) + length(sobelB)) / 3.0;
}

// Sample the depth normal map and decode depth and normal from the texture
void GetDepthAndNormal(float2 uv, out float depth, out float3 normal) {
    float4 coded = SAMPLE_TEXTURE2D(_DepthNormalsTexture, sampler_DepthNormalsTexture, uv);
    DecodeDepthNormal(coded, depth, normal);
}

// A wrapper around the above function for use in a custom function node
void CalculateDepthNormal_float(float2 UV, out float Depth, out float3 Normal) {
    GetDepthAndNormal(UV, Depth, Normal);
    // Normals are encoded from 0 to 1 in the texture. Remap them to -1 to 1 for easier use in the graph
    Normal = Normal * 2 - 1;
}

void NeighborNormalEdgeIndicator(float2 UV, float depth, float3 normal, out float normalIndicator, out float depthIndicator) {
    float newDepth;
    float3 newNormal;
    GetDepthAndNormal(UV, newDepth, newNormal);

    float depthDiff = newDepth - depth;

    float3 normalEdgeBias = float3(0, 1, 0);
    float normalDiff = dot(normal - newNormal, normalEdgeBias);
    float normalIndicator_ = clamp(smoothstep(-1, 1, normalDiff), 0.0, 1.0);

    //float depthIndicator_ = clamp(sign(depthDiff * .25 + .0025), 0.0, 1.0);
    float depthIndicator_ = 1;

    normalIndicator = distance(normal, newNormal) * depthIndicator_ * normalIndicator_;

    depthIndicator = depthDiff;
    
}

// This function runs the sobel algorithm over the opaque texture
void NormalsSobel_float(float2 UV, float Thickness, float normalThreshold, float normalTighten, out float Out, out float NormalDir) {
	float depth;
    float3 normalEdgeBias = float3(1, 1, 1);
    float2 normalX = 0;
    float2 normalY = 0;
    float2 normalZ = 0;
    float3 ridge = 0;
    float3 normal;
	GetDepthAndNormal(UV, depth, normal);

    float indicator = 0.0;

    [unroll] for (int i = 0; i < 9; i++)
    {
		float3 normalIndicator;
		float depthIndicator;
		GetDepthAndNormal(UV + sobelSamplePoints[i] * _MainTex_TexelSize.xy * Thickness, depthIndicator, normalIndicator);
		float normalDiff = dot(normal - normalIndicator, normalEdgeBias);
        float weight = clamp(smoothstep(-normalTighten, normalTighten, normalDiff), 0.0, 1.0);
        //weight = 0;
        //weight *= step(0, dot(float3(1, 1, 1), cross(normal, normalIndicator)));
       // weight = clamp(weight, 0.0, 1.0);
        //weight = 1;
        float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);
        normalX += normalIndicator.x * kernel * weight;
        normalY += normalIndicator.y * kernel * weight;
        normalZ += normalIndicator.z * kernel * weight;
    }
    float3 dx = ddx(normal);
    float3 dy = ddy(normal);
    float3 xneg = normal - dx;
    float3 xpos = normal + dx;
    float3 yneg = normal - dy;
    float3 ypos = normal + dy;
    float curvature = (cross(xneg, xpos).y - cross(yneg, ypos).x) * 4.0 / depth;

    // Won't be able to strictly use this as a mask. will cause dotted lines then, will need some sort of threshold
    NormalDir = curvature;
    indicator = max(length(normalX), max(length(normalY), length(normalZ)));
	Out = step(normalThreshold + .1, indicator);

}

void DepthAndNormalsSobel_float(float2 UV, float Thickness, out float OutDepth, out float OutNormal) {
    // This function calculates the normal and depth sobels at the same time
    // using the depth encoded into the depth normals texture
    float sobelNormal = 0;
    float sobelDepth = 0;
    // We can unroll this loop to make it more efficient
    // The compiler is also smart enough to remove the i=4 iteration, which is always zero
    [unroll] for (int i = 0; i < 9; i++) {
        float depth;
        float3 normal;
        float normalIndicator;
        float depthIndicator;
        GetDepthAndNormal(UV, depth, normal);

        NeighborNormalEdgeIndicator(UV + sobelSamplePoints[i] * _ScreenParams.zw * Thickness, depth, normal, normalIndicator, depthIndicator);
        // Create the kernel for this iteration
        float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);
        // Accumulate samples for each channel
        sobelNormal += normalIndicator * kernel;
        sobelDepth += depthIndicator * kernel;

    }
    OutDepth = floor(smoothstep(0.02, 0.03, sobelDepth) * 2.) / 2.;
    OutNormal = step(1, sobelNormal);//max(length(sobelX), max(length(sobelY), length(sobelZ)));



}
#endif