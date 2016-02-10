float4x4 W;
float4x4 VP;

float4 col;
Texture tex;
sampler texSamp = sampler_state {
	Texture = <tex>;
};

struct VSOUT
{
    float4 Position : POSITION0;
	float2 texCoord : TEXCOORD0;
};

VSOUT skyVS(float4 Position : POSITION0, float2 tex : TEXCOORD0) {
	VSOUT output;

	float4 worldpos = mul(Position, W);
	output.Position = mul(worldpos, VP);
	output.texCoord = tex;

    return output;
}

float4 skyPS(VSOUT input) : COLOR0{
	float4 c = tex2D(texSamp, input.texCoord);
	c *= col;
	clip(c.a <= 0 ? -1 : 1);
	return c;
}

technique Skybox
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 skyVS();
		PixelShader = compile ps_3_0 skyPS();
	}
}