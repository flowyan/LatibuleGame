// ============================================================
// Skybox.fx  (XNA / FNA Effect)
// Shader Model: fx_3_0
// ============================================================

float4x4 World;
float4x4 View;
float4x4 Projection;

texture SkyCube;

samplerCUBE SkySampler = sampler_state
{
    Texture   = <SkyCube>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;

    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

// ------------------------------------------------------------
// Structures
// ------------------------------------------------------------
struct VSInput
{
    float4 Position : POSITION0;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float3 TexCoord : TEXCOORD0;
};

// ------------------------------------------------------------
// Vertex Shader
// - Removes translation from View so the skybox stays centered.
// - Uses local direction for cubemap lookup.
// ------------------------------------------------------------
VSOutput VSMain(VSInput input)
{
    VSOutput output;

    // Remove camera translation from the view matrix
    float4x4 viewNoTrans = View;
    viewNoTrans._41 = 0;
    viewNoTrans._42 = 0;
    viewNoTrans._43 = 0;

    // Build WVP without camera translation
    float4 worldPos = mul(input.Position, World);
    float4 viewPos  = mul(worldPos, viewNoTrans);
    output.Position = mul(viewPos, Projection);

    // Cubemap lookup direction (use local position direction)
    // If your cube mesh is centered at origin, this is correct.
    output.TexCoord = input.Position.xyz;

    return output;
}

// ------------------------------------------------------------
// Pixel Shader
// ------------------------------------------------------------
float4 PSMain(VSOutput input) : COLOR0
{
    float3 dir = normalize(input.TexCoord);
    return texCUBE(SkySampler, dir);
}

// ------------------------------------------------------------
// Technique
// ------------------------------------------------------------
technique Skybox
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader  = compile ps_3_0 PSMain();
    }
}
