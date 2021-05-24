#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

//About precision: float for positions and texture coordinates only and half for everything else

#include "../ShaderLibrary/Common.hlsl"
float4 _BaseColor;

// homogeneous clip space position
float4 UnlitPassVertex (float3 positionOS : POSITION) : SV_POSITION 
{
    float3 positionWS = TransformObjectToWorld(positionOS.xyz);
    return TransformWorldToHClip(positionWS);
}

float4 UnlitPassFragment () : SV_TARGET
{
    return _BaseColor;
}

#endif