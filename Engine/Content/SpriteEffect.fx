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

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float blurSizeX = 1;
float blurSizeY = 1;

float4 PS_BlurHorizontal(VertexShaderOutput input ) : COLOR0
{
    float2 Tex = input.TextureCoordinates;
    float Color = 0.0f;

    Color += tex2D(SpriteTextureSampler, float2(Tex.x - 3.0 * blurSizeX, Tex.y)) * 0.09f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x - 2.0 * blurSizeX, Tex.y)) * 0.11f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x - blurSizeX, Tex.y)) * 0.18f;
   Color += tex2D(SpriteTextureSampler,Tex) * 0.24f;
   Color += tex2D(SpriteTextureSampler, float2(Tex.x + blurSizeX, Tex.y)) * 0.18f;
   Color += tex2D(SpriteTextureSampler, float2(Tex.x + 2.0 * blurSizeX, Tex.y)) * 0.11f;
   Color += tex2D(SpriteTextureSampler, float2(Tex.x + 3.0 * blurSizeX, Tex.y)) * 0.09f;

    return Color;
}

float4 PS_BlurVertical(float2 Tex : TEXCOORD0) : COLOR0
{
    float Color = 0.0f;

    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y - 3.0 * blurSizeY)) * 0.09f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y - 2.0 * blurSizeY)) * 0.11f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y - blurSizeY)) * 0.18f;
    Color += tex2D(SpriteTextureSampler, Tex) * 0.24f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y + blurSizeY)) * 0.18f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y + 2.0 * blurSizeY)) * 0.11f;
    Color += tex2D(SpriteTextureSampler, float2(Tex.x, Tex.y + 3.0 * blurSizeY)) * 0.09f;

    return Color;
}


float4 MainPS(VertexShaderOutput input) : COLOR
{
    return input.Color;
   /* if (tex2D(SpriteTextureSampler, input.TextureCoordinates).r == 0 && tex2D(SpriteTextureSampler, input.TextureCoordinates).g == 0 && tex2D(SpriteTextureSampler, input.TextureCoordinates).b == 0)
        return tex2D(SpriteTextureSampler, input.TextureCoordinates);
	else
        return float4(1, 0.5, 1, 0);*/

}

technique SpriteDrawing
{
	pass P0
	{
        PixelShader = compile PS_SHADERMODEL MainPS();
		PixelShader = compile PS_SHADERMODEL PS_BlurHorizontal();
        PixelShader = compile PS_SHADERMODEL PS_BlurVertical();
    }
};