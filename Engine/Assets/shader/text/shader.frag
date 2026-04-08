#version 330 core

in vec4 Color;
in vec2 TexCoords;

uniform sampler2D textureSampler;

out vec4 FragColor;

void main()
{
	FragColor = Color * texture(textureSampler, TexCoords);
}