#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform int flipGlyphY;

out vec4 Color;
out vec2 TexCoords;

void main()
{
    Color = aColor;
    TexCoords = aTexCoords;

    vec3 p = aPos;
    if (flipGlyphY == 1)
    p.y = -p.y;

    gl_Position = projection * view * model * vec4(p, 1.0);
}