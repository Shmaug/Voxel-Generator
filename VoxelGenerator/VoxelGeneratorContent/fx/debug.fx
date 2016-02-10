float4x4 W;
float4x4 VP;

float4 debugColor = float4(1, 0, 0, 1);

struct dbvsout
{
	float4 Position : POSITION0;
};

dbvsout dbVS(float4 Position : POSITION0) {
	dbvsout output;

	output.Position = mul(Position, mul(W, VP));

	return output;
}

float4 dbPS(dbvsout input) : COLOR0 {
	return debugColor;
}
technique DebugBox
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 dbVS();
		PixelShader = compile ps_2_0 dbPS();
	}
}
