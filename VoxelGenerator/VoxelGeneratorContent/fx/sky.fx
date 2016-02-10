float4x4 W;
float4x4 VP;

float3 CameraPosition;

float SkyAlpha;

Texture SkyTexture;
samplerCUBE skySamp = sampler_state {
	Texture = <SkyTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VSOUT
{
    float4 Position : POSITION0;
	float3 texCoord : TEXCOORD0;
};

VSOUT skyVS(float4 Position : POSITION0) {
	VSOUT output;

	float4 worldpos = mul(Position, W);
	output.Position = mul(worldpos, VP);
	output.texCoord = worldpos - CameraPosition;

    return output;
}

float4 skyPS(VSOUT input) : COLOR0{
	return texCUBE(skySamp, normalize(input.texCoord)) * SkyAlpha;
}

technique Skybox
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 skyVS();
		PixelShader = compile ps_3_0 skyPS();
	}
}