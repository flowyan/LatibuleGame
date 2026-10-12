#version 330

in vec3 TexCoords; // direction vector representing a 3D texture coordinate
out vec4 FragColor;

uniform samplerCube cubemap; // cubemap texture sampler

void main()
{
    // sample from the cube map texture
    FragColor = texture(cubemap, TexCoords);
}