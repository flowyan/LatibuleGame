#version 330 core
// In this tutorial it might seem like a lot is going on, but really we just combine the last tutorials, 3 pieces of source code into one
// and added 3 extra point lights.

// Material properties define how the surface responds to light
struct Material {
    sampler2D diffuse;   // Diffuse texture (base color)
    sampler2D specular;  // Specular map (controls shininess per pixel)
    float shininess; // How shiny the surface is (higher = smaller, brighter highlights)
};

// This is our point light where we need the position as well as the constants defining the attenuation of the light
struct PointLight {
    vec3 position;   // World position of the light

    float constant;  // Constant attenuation factor
    float linear;    // Linear attenuation factor
    float quadratic; // Quadratic attenuation factor (falls off with distance squared)

    vec3 ambient;    // Ambient light contribution
    vec3 diffuse;    // Diffuse light color
    vec3 specular;   // Specular light color
};

#define MAX_POINT_LIGHTS 32 // MAKE SURE VALUE IS THE SAME AS IN LightRenderer
uniform int pointLightsAmount;
uniform PointLight pointLights[MAX_POINT_LIGHTS];

uniform Material material;
uniform vec3 viewPos;  // Camera position in world space
uniform float alphaCutoff; // 0 = disabled, else e.g. 0.5

out vec4 FragColor;

in vec3 Normal;    // Normal vector from vertex shader
in vec3 FragPos;   // Fragment position in world space
in vec2 TexCoords; // Texture coordinates

// Here we have some function prototypes, these are the signatures the GPU will use to know how the
// parameters of each light calculation is laid out.
// We have one function per light, since this makes it so we don't have to take up too much space in the main function.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    vec3 result = vec3(0.0);

    // Phase 1: Directional lighting
    //    vec3 result = CalcDirLight(dirLight, norm, viewDir);
    // Phase 2: Point lights
    for (int i = 0; i < pointLightsAmount; i++) {
        result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);
    }

    // Phase 3: Spot light
    //    result += CalcSpotLight(spotLight, norm, FragPos, viewDir);

    float alpha = texture(material.diffuse, TexCoords).a;
    if (alphaCutoff > 0.0 && alpha < alphaCutoff) discard;
    FragColor = vec4(result, alpha);
}

// Calculate point light contribution (light radiates from a point in all directions)
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec4 albedo = texture(material.diffuse, TexCoords); // RGBA
    vec3 baseColor = albedo.rgb;

    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);

    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);

    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance +
    light.quadratic * (distance * distance));

    vec3 ambient  = light.ambient  * baseColor;
    vec3 diffuse  = light.diffuse  * diff * baseColor;
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords));

    return (ambient + diffuse + specular) * attenuation;
}