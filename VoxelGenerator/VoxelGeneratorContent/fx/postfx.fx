sampler screenSamp : register(s0);

////////////////////// OCCULD //////////////////////
float4 PSocculd(float2 coord : TEXCOORD0) : COLOR0
{
	if (tex2D(screenSamp, coord).a == 1)
		return float4(0, 0, 0, 0);
	clip(-1);
	return float4(0, 0, 0, 0);
}

technique Occuld
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PSocculd();
	}
}

////////////////////// BLUR //////////////////////
#define KERNEL_SIZE (7 * 2 + 1)
float weights[KERNEL_SIZE];
float2 offsets[KERNEL_SIZE];

float4 PSblur(float2 coord : TEXCOORD0) : COLOR0
{
	float4 color = float4(0, 0, 0, 0);
	for (int i = 0; i < KERNEL_SIZE; ++i)
		color += tex2D(screenSamp, coord + offsets[i]) * weights[i];

	return color;
}

technique Blur
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PSblur();
    }
}

////////////////////// VOLUMETRIC SCATTER //////////////////////
#define SCATTER_SAMPLES 8
float Exposure = 1.f;
float Density = .5f;
float Decay = .75f;
float Weight = 1.f;

float2 lightPosition;
float2 pixel;

float4 PSscatter(float2 Coord : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(screenSamp, Coord);

	float t = 1;
	float dT = Density / SCATTER_SAMPLES;
	float decay = 1;
	for (int i = 0; i < SCATTER_SAMPLES; i++) {
		float2 p = lerp(lightPosition, Coord, t);
		t -= dT;
		color += tex2D(screenSamp, p) * decay * Weight;
		decay *= Decay;
	}
	color *= Exposure;

	return color;
}

technique Scatter
{
	pass scatter
	{
		PixelShader = compile ps_2_0 PSscatter();
	}
}

