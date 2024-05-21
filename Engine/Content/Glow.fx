#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;


sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;

};
sampler2D SpriteTextureSampler2 = sampler_state
{
    Texture = <SpriteTexture>;

};





int borderWidth = 2;
int intensity = 2;
float time;

/*ITT nem lehet initialize-lni dolgokat... pfuj*/


struct VertexShaderOutput
{
    float2 Position : SV_POSition;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPSX(VertexShaderOutput input) : SV_TARGET0
{
    float2 Tex = input.TextureCoordinates;
    float4 Color = 0.0f;

    float blurSizeX = 1.0 / 14.0 / 4.0;
    float blurSizeY = 1.0 / 14.0 / 4.0;
    
    Color += tex2D(SpriteTextureSampler, float2(Tex.x - 3.0 * blurSizeX, Tex.y)) * 0.09f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x - 2.0 * blurSizeX, Tex.y)) * 0.11f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x - blurSizeX, Tex.y)) * 0.18f;
    Color += tex2D(SpriteTextureSampler, Tex) * 0.24f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x + blurSizeX, Tex.y)) * 0.18f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x + 2.0 * blurSizeX, Tex.y)) * 0.11f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x + 3.0 * blurSizeX, Tex.y)) * 0.09f;

    return Color;

}

float4 MainPSY(VertexShaderOutput input) : SV_TARGET0
{
    float2 Tex = input.TextureCoordinates;
    float4 Color = 0.0f;

    float blurSizeX = 1.0 / 14.0 / 4.0;
    float blurSizeY = 1.0 / 14.0  ;
    
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y - 3.0 * blurSizeY)) * 0.09f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y - 2.0 * blurSizeY)) * 0.11f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y - blurSizeY )) * 0.18f;
    Color += tex2D(SpriteTextureSampler, Tex) * 0.24f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y + blurSizeY)) * 0.18f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y + 2.0 * blurSizeY)) * 0.11f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y + 3.0 * blurSizeY)) * 0.09f;

    return Color;

}

float4 MainPS(VertexShaderOutput input) : SV_TARGET0
{
    return 1 - ((1 - tex2D(SpriteTextureSampler, input.TextureCoordinates) * (1 - tex2D(SpriteTextureSampler, input.TextureCoordinates))));
    /*if (0 == tex2D(SpriteTextureSampler, input.TextureCoordinates).x && 0 == tex2D(SpriteTextureSampler, input.TextureCoordinates).y && 0 == tex2D(SpriteTextureSampler, input.TextureCoordinates).z)
        return 0;
   else
        return 1;*/
         //input.Color * tex2D(SpriteTextureSampler, input.TextureCoordinates);

}
    
   /* float4 avgValue = float4(0, 0, 0,0);
	
    
    float coefficientSum = 0.0;
    float3 incrementalGaussian;
    float sigma = 5;

    incrementalGaussian.x = 1.0 / (sqrt(2.0 * 3.1415) * sigma);
    incrementalGaussian.y = exp(-0.5 / (sigma * sigma));
    incrementalGaussian.z = incrementalGaussian.y * incrementalGaussian.y;
 
  // Take the central sample first...
    avgValue += tex2D(SpriteTextureSampler, input.TextureCoordinates.xy) * incrementalGaussian.x;
    coefficientSum += incrementalGaussian.x;
    incrementalGaussian.xy *= incrementalGaussian.yz;
    
	
    for (float i = 1.0; i <= 10; i++)
    {
        avgValue += tex2D(SpriteTextureSampler, input.TextureCoordinates.xy - i * float2(32, 32) * float2(0,1.0/32.0 )) * incrementalGaussian.x;
        avgValue += tex2D(SpriteTextureSampler, input.TextureCoordinates.xy + i * float2(32, 32) * float2(0, 1.0/32.0)) * incrementalGaussian.x;
        coefficientSum += 2.0 * incrementalGaussian.x;
        incrementalGaussian.xy *= incrementalGaussian.yz;
    }
    
	*/
	/*float3 lV = float3(0.2125, 0.7154, 0.0721);
    float4 color =  tex2D( SpriteTextureSampler,input.TextureCoordinates) * input.Color;
    float l = dot(lV, color.xyz);
    l = max(0.0, l -0.08);
    color.xyz = float3(color.x + lV.x * l, color.y + lV.y * l, color.z + lV.z * l);

    color.w = 1.0;
*/


    //tex2D(SpriteTextureSampler, input.TextureCoordinates)
//    return float4(1, 1, 1, 1);
//	return tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;


technique SpriteDrawing
{
	pass P0
	{
        PixelShader = compile PS_SHADERMODEL MainPSX();
       // PixelShader = compile PS_SHADERMODEL MainPSY();
        //PixelShader = compile PS_SHADERMODEL MainPS();
    }
    pass P1
    {
        PixelShader = compile PS_SHADERMODEL MainPSY();
    }
    pass P3
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }

};