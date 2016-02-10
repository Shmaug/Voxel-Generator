float4x4 W;
float4x4 VP;

bool EnableLighting = true;

float3 LightDirection = float3(-.25, -.75, 0);
float AmbientBrightness;

float3 CameraPosition;

float GrassTimer;

texture BlockTexture;
sampler blocksamp {
	Texture = <BlockTexture>;
	Filter = Point;
};

Texture3D LightTexture;
float3 chunkPos;
float3 lightTexSize;
sampler lightsamp {
	Texture = <LightTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
};
/*
float LightBoxSize = 100;
float4x4 LightWVP;
texture DepthTexture;
sampler depthsamp = sampler_state {
	Texture = <DepthTexture>;
	AddressU = Clamp;
	AddressV = Clamp;
	Filter = Point;
};*/

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float4 TexCoord : TEXCOORD0;
};

struct GrassVertex
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float4 TexCoord : TEXCOORD0;
	float4 Animate : TEXCOORD1;
};

struct VSOUT
{
    float4 Position : POSITION0;
	float4 TexCoord : TEXCOORD0;
	float3 worldPos : TEXCOORD1;
	float3 lp : TEXCOORD2;
	//float4 depthCoord : TEXCOORD3;
};

VSOUT BlockVS(VertexShaderInput input) {
	VSOUT output;

	float4 worldpos = mul(input.Position, W);
	output.worldPos = input.Position;
	output.Position = mul(worldpos, VP);
	output.TexCoord = input.TexCoord;

	output.lp = (worldpos.xyz - chunkPos + float3(1, 0, 1)) / lightTexSize; // add 1 to compensate for the 1 block buffer
	//output.depthCoord = mul(input.Position, mul(W, LightWVP));

    return output;
}

VSOUT GrassVS(GrassVertex input) {
	VSOUT output;

	float4 worldpos = mul(input.Position, W);
	if (input.Animate.w > 0) {
		float t = input.Animate.x + GrassTimer * .5;
		worldpos += float4(sin(2*t), 0, cos(t), 0) * .1;
	}
	output.worldPos = input.Position;
	output.Position = mul(worldpos, VP);
	output.TexCoord = input.TexCoord;

	output.lp = (worldpos.xyz - chunkPos + float3(1, 0, 1)) / lightTexSize; // add 1 to compensate for the 1 block buffer
	//output.depthCoord = mul(input.Position, mul(W, LightWVP));

	return output;
}

float4 DiffusePS(VSOUT input) : COLOR0{
	float4 col = tex2D(blocksamp, input.TexCoord);
	clip(col.a  - .1);
	col.a = 1;
	if (EnableLighting) {
		float4 l = tex3D(lightsamp, input.lp);
		col.rgb *= clamp(AmbientBrightness * l.r + l.g, 0, 1);
	}

	return col;
}

technique Block
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 BlockVS();
		PixelShader = compile ps_3_0 DiffusePS();
	}
}
technique Grass
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 GrassVS();
		PixelShader = compile ps_3_0 DiffusePS();
	}
}