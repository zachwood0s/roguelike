#ifndef EDGE_DETECTION_INCLUDED
#define EDGE_DETECTION_INCLUDED


#include "DecodeDepthNormals.hlsl"

TEXTURE2D(_DepthNormalsTexture); SAMPLER(sampler_DepthNormalsTexture);
TEXTURE2D(_NormalsTexture); SAMPLER(sampler_NormalsTexture);

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

/*
static float2 sobelSamplePoints[4] = {
    float2(0, 1), 
    float2(-1, 0), float2(1, 0),
    float2(0, -1),
};
*/


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

float3 GetNormal(float2 uv) {
    float3 normal;
    float depth;
    float4 coded = SAMPLE_TEXTURE2D(_NormalsTexture, sampler_NormalsTexture, uv);
    DecodeDepthNormal(coded, depth, normal);
    return normal;
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

float3 Sobel(float2 UV, float Thickness)
{
    float2 normalX = 0;
    float2 normalY = 0;
    float2 normalZ = 0;
    [unroll] for (int i = 0; i < 9; i++)
    {
        float newDepth = SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV + sobelSamplePoints[i] * _MainTex_TexelSize.xy * Thickness);
		float3 normalIndicator = GetNormal(UV + sobelSamplePoints[i] * _MainTex_TexelSize.xy * Thickness);
        float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);

        normalX += normalIndicator.x * kernel;
        normalY += normalIndicator.y * kernel;
        normalZ += normalIndicator.z * kernel;
    }
    float3 Out;
	Out.x = length(normalX);
	Out.y = length(normalY);
	Out.z = length(normalZ);
    return Out;
}

// This function runs the sobel algorithm over the opaque texture
void NormalsSobel_float(float2 UV, float Thickness, float normalThreshold, float normalTighten, out float3 Out, out float NormalDir) {
    float3 normalEdgeBias = float3(1, 1, 1);
    float2 normalX = 0;
    float2 normalY = 0;
    float2 normalZ = 0;
    float3 ridge = 0;
	float depth = SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV);
    float normal = GetNormal(UV);

    float indicator = 0.0;

    [unroll] for (int i = 0; i < 9; i++)
    {
        float newDepth = SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV + sobelSamplePoints[i] * _MainTex_TexelSize.xy * Thickness);
		float3 normalIndicator = GetNormal(UV + sobelSamplePoints[i] * _MainTex_TexelSize.xy * Thickness);
        /*
		float depthIndicator;
		GetDepthAndNormal(UV + sobelSamplePoints[i] * _MainTex_TexelSize.xy * Thickness, depthIndicator, normalIndicator);
*/
		//float normalDiff = dot(normal - normalIndicator, normalEdgeBias);
        //float weight = clamp(smoothstep(-normalTighten, normalTighten, normalDiff), 0.0, 1.0);

        // Only the shallower pixel should detect the normal edge.
        //float depthIndicator = clamp(sign((newDepth - depth) * .25 + .0025), 0.0, 1.0);

        //float weight = 1;
        //weight *= step(0, dot(float3(1, 1, 1), cross(normal, normalIndicator)));
       // weight = clamp(weight, 0.0, 1.0);
        float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);

        normalX += normalIndicator.x * kernel;
        normalY += normalIndicator.y * kernel;
        normalZ += normalIndicator.z * kernel;
        //normalX += (1.0 - dot(normal, normalIndicator)) * depthIndicator * weight;
    }

    float3 xneg = GetNormal(UV + float2(1, 0) * _MainTex_TexelSize.xy * Thickness);
    float3 xpos = GetNormal(UV + float2(-1, 0) * _MainTex_TexelSize.xy * Thickness);
	float3 yneg = GetNormal(UV + float2(0, -1) * _MainTex_TexelSize.xy * Thickness);
	float3 ypos = GetNormal(UV + float2(0, 1) * _MainTex_TexelSize.xy * Thickness);
    float keep = dot(normal - xneg, normalEdgeBias);
    keep = min(keep, dot(normal - xpos, normalEdgeBias));
    keep = min(keep, dot(normal - yneg, normalEdgeBias));
    keep = min(keep, dot(normal - ypos, normalEdgeBias));

    float curvature = (cross(xneg, xpos).y - cross(yneg, ypos).x) * 4 / depth;
    NormalDir = -curvature;

    indicator = max(length(normalX), max(length(normalY), length(normalZ)));
	Out.x = length(normalX);
	Out.y = length(normalY);
	Out.z = length(normalZ);
}

float brightness(float3 color)
{
    return color.r + color.g + color.b;
}

static float3x3 X_COMPONENT_MATRIX = float3x3(
    1., 0., -1.,
    2., 0., -2.,
    1., 0., -1.
);
static float3x3 Y_COMPONENT_MATRIX = float3x3(
    1., 2., 1.,
    0., 0., 0.,
    -1., -2., -1.
);

float2 rotate2D(float2 v, float rad) {
    float s = sin(rad);
    float c = cos(rad);
    float2x2 rotMatrix = float2x2(c, s, -s, c);
    return mul(rotMatrix, v);
}
float convoluteMatrices(float3x3 A, float3x3 B) {
    return dot(A[0], B[0]) + dot(A[1], B[1]) + dot(A[2], B[2]);
}

float4 blur(float2 uv, float2 resolution, float2 direction) {
    float4 color = 0;
    float2 off1 = float2(1.3333333333333333, 1.333333333333333) * direction;
    color += SAMPLE_TEXTURE2D(_NormalsTexture, sampler_NormalsTexture, uv) * 0.29411764705882354;
    color += SAMPLE_TEXTURE2D(_NormalsTexture, sampler_NormalsTexture, uv + (off1 * resolution)) * 0.35294117647058826;
    color += SAMPLE_TEXTURE2D(_NormalsTexture, sampler_NormalsTexture, uv - (off1 * resolution)) * 0.35294117647058826;
    return color;
}

float3 getBlurredTextureColor(
    float2 textureCoord,
    float2 resolution
) {
    //return saturate(GetNormal(textureCoord));
    return blur(
        textureCoord,
        resolution,
        normalize(textureCoord - float2(0.5, 0.5))).xyz;
}
float3 getTextureIntensity(
    float2 textureCoord,
    float2 resolution
) {
    float3 color = getBlurredTextureColor(textureCoord, resolution);
    //float3 color = GetNormal(textureCoord);
    return color;//GetNormal(textureCoord);
    //return pow(length(clamp(color, 0, 1)), 2.) / 3.;
}

float2 getTextureIntensityGradientChannel(float2 textureCoord, float2 resolution, int channel)
{
    float3x3 imgMatX = 0;

    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            float2 ds = -resolution + float2(i, j) * resolution; 
            float3 color = getTextureIntensity(textureCoord + ds, resolution);
            imgMatX[i][j] = color[channel];
        }
    }

    float gradXX = convoluteMatrices(X_COMPONENT_MATRIX, imgMatX);
    float gradXY = convoluteMatrices(Y_COMPONENT_MATRIX, imgMatX);

    return float2(gradXX, gradXY);

}

float2 getTextureIntensityGradient(
    float2 textureCoord,
    float2 resolution
) {
    float3x3 imgMatX = 0;
    float3x3 imgMatY = 0;
    float3x3 imgMatZ = 0;

    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            float2 ds = -resolution + float2(i, j) * resolution; 
            float3 color = getTextureIntensity(textureCoord + ds, resolution);
            imgMatX[i][j] = color.x;
            imgMatY[i][j] = color.y;
            imgMatZ[i][j] = color.z;
        }
    }

    float gradXX = convoluteMatrices(X_COMPONENT_MATRIX, imgMatX);
    float gradXY = convoluteMatrices(Y_COMPONENT_MATRIX, imgMatX);

    float gradYX = convoluteMatrices(X_COMPONENT_MATRIX, imgMatY);
    float gradYY = convoluteMatrices(Y_COMPONENT_MATRIX, imgMatY);

    float gradZX = convoluteMatrices(X_COMPONENT_MATRIX, imgMatZ);
    float gradZY = convoluteMatrices(Y_COMPONENT_MATRIX, imgMatZ);

    float lenX = length(float2(gradXX, gradXY));
    float lenY = length(float2(gradYX, gradYY));
    float lenZ = length(float2(gradZX, gradZY));

    if (lenX > lenY && lenX > lenZ)
        return float2(gradXX, gradXY);
    if (lenY > lenZ)
        return float2(gradYX, gradYY);
    return float2(gradZX, gradZY);
    /*
    return float3x2(gradXX, gradXY, 
                    gradYX, gradYY,
                    gradZX, gradZY);
                    */
}


float2 round2DVectorAngle(float2 v) {
    float len = length(v);
    float2 n = normalize(v);
    float maximum = -1.;
    float bestAngle;
    for (int i = 0; i < 8; i++) {
        float theta = (float(i) * 2. * PI) / 8.;
        float2 u = rotate2D(float2(1., 0.), theta);
        float scalarProduct = dot(u, n);
        if (scalarProduct > maximum) {
            bestAngle = theta;
            maximum = scalarProduct;
        }
    }
    return len * rotate2D(float2(1., 0.), bestAngle);
}

float2 suppressGradiant(float2 gradient, float2 plusStep, float2 minusStep)
{
    gradient = round2DVectorAngle(gradient);
    float gradientLength = length(gradient);
    if (length(plusStep) >= gradientLength) return 0;
    if (length(minusStep) >= gradientLength) return 0;
    return gradient;
}

float2 getSuppressedTextureIntensityGradient(float2 UV)
{
    float2 gradient = getTextureIntensityGradient(UV, _MainTex_TexelSize.xy);
    gradient.xy = gradient.yx;

    float2 gradientStep = normalize(gradient) * _MainTex_TexelSize.xy;

    float2 gradientOut = suppressGradiant(gradient,
        getTextureIntensityGradient(UV + gradientStep, _MainTex_TexelSize.xy),
        getTextureIntensityGradient(UV - gradientStep, _MainTex_TexelSize.xy));
    /*

    float2 gradientStep = normalize(gradient) * _MainTex_TexelSize.xy;
    float gradientLength = length(gradient);

    float2 gradientPlusStep = getTextureIntensityGradient(UV + gradientStep, _MainTex_TexelSize.xy);
    if (length(gradientPlusStep) >= gradientLength) return 0;

    float2 gradientMinusStep = getTextureIntensityGradient(UV - gradientStep, _MainTex_TexelSize.xy);
    if (length(gradientMinusStep) >= gradientLength) return 0;
    */

    return gradientOut;
}

float applyDoubleThreshold(
    float2 gradient,
    float weakThreshold,
    float strongThreshold
) {
    float gradientLength = length(gradient);
    if (gradientLength < weakThreshold) return 0.;
    if (gradientLength < strongThreshold) return .5;
    return 1.;
}

float applyHysteresis(
    float2 textureCoord,
    float weakThreshold,
    float strongThreshold
) {
    float dx = _MainTex_TexelSize.x;
    float dy = _MainTex_TexelSize.y;
    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            float2 ds = -_MainTex_TexelSize.xy + float2(i, j) * _MainTex_TexelSize.xy;
            float2 gradient = getSuppressedTextureIntensityGradient(textureCoord + ds);
            float edge = applyDoubleThreshold(gradient, weakThreshold, strongThreshold);
            if (edge == 1.) return 1.;
        }
    }
    return 0.;
}

void SupressSobel_float(float2 UV, float weakThreshold, float strongThreshold, out float3 Out) 
{
    float2 gradient = getSuppressedTextureIntensityGradient(UV);
    //float2 gradient = getTextureIntensityGradient(UV, _MainTex_TexelSize.xy);
    float edge = applyDoubleThreshold(gradient, weakThreshold, strongThreshold);
    if (edge == .5) {
        edge = applyHysteresis(UV, weakThreshold, strongThreshold);
    }
    //Out = saturate(getTextureIntensity(UV, _MainTex_TexelSize.xy));
    //Out = getTextureIntensity(UV, _MainTex_TexelSize);
    //Out.xy = abs(round2DVectorAngle(gradient));
    //Out.z = 0;
    //Out = saturate(Out);
    //Out = length(gradient);
    //Out = getTextureIntensityGradient(UV, _MainTex_TexelSize.xy).xy;
    Out = edge;


    /*
    float2 gradient = getTextureIntensityGradient(UV, _MainTex_TexelSize.xy).xy;
    gradient = round2DVectorAngle(gradient);
    float2 gradientStep = normalize(gradient) * _MainTex_TexelSize.xy;
    float gradientLength = length(gradient);

    Out = gradient;

    float gradientPlusStep = getTextureIntensityGradient(UV + gradientStep, _MainTex_TexelSize.xy);
    if (length(gradientPlusStep) >= gradientLength) Out = 0;

    float gradientMinusStep = getTextureIntensityGradient(UV - gradientStep, _MainTex_TexelSize.xy);
    if (length(gradientMinusStep) >= gradientLength) Out = 0;
    /*
    float gradientPlusStep = getTextureIntensityGradient(
        textureSampler, textureCoord + gradientStep, resolution);
    if (length(gradientPlusStep) >= gradientLength) return vec2(0.);

    float2 gradientMinusStep = getTextureIntensityGradient(
        textureSampler, textureCoord - gradientStep, resolution);
    if (length(gradientMinusStep) >= gradientLength) return vec2(0.);
    */
    //Out = clamp(brightness(sobel), 0, 1);
}

/*
void DepthAndNormalsSobel_float(float2 UV, float Thickness, out float OutDepth, out float OutNormal) {
    // This function calculates the normal and depth sobels at the same time
    // using the depth encoded into the depth normals texture
    float sobelNormal = 0;
    float sobelDepth = 0;
    // We can unroll this loop to make it more efficient
    // The compiler is also smart enough to remove the i=4 iteration, which is always zero
    [unroll] for (int i = 0; i < 4; i++) {
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
*/
#endif