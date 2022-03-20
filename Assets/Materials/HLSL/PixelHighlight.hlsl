#ifndef PIXEL_HIGHLIGHT_INCLUDED
#define PIXEL_HIGHLIGHT_INCLUDED


TEXTURE2D(_NormalsTexture); SAMPLER(sampler_NormalsTexture);


void (float2 UV, out float Depth, out float3 Normal) {
    GetDepthAndNormal(UV, Depth, Normal);
    // Normals are encoded from 0 to 1 in the texture. Remap them to -1 to 1 for easier use in the graph
    Normal = Normal * 2 - 1;
}

#endif