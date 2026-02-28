namespace Latibule.Core.Rendering.Renderer;

public static class LightRenderer
{
    public const int MAX_POINT_LIGHTS = 32; // MAKE SURE VALUE IS THE SAME AS IN FRAGMENT SHADER

    public static void Render(Shader shader)
    {
        var lights = LatibuleGame.GameWorld.Lights;
        shader.SetUniform("pointLightsAmount", lights.Count(l => l is not null));
        for (var i = 0; i < lights.Length; i++)
        {
            var light = lights[i];
            if (light is null) continue;
            var lightColor = light.Color * light.Intensity;

            shader.SetUniform($"pointLights[{i}].position", light.Position);
            shader.SetUniform($"pointLights[{i}].constant", light.Constant);
            shader.SetUniform($"pointLights[{i}].linear", light.Linear);
            shader.SetUniform($"pointLights[{i}].quadratic", light.Quadratic);
            shader.SetUniform($"pointLights[{i}].ambient", lightColor * 0.05f);
            shader.SetUniform($"pointLights[{i}].diffuse", lightColor);
            shader.SetUniform($"pointLights[{i}].specular", lightColor);
        }
    }
}