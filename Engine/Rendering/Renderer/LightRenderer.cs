using Engine.Core;
using OpenTK.Mathematics;

namespace Engine.Rendering.Renderer;

public static class LightRenderer
{
    public const int MAX_POINT_LIGHTS = 32; // MAKE SURE VALUE IS THE SAME AS IN FRAGMENT SHADER

    public static void Render(Shader shader, Matrix4? lightSpaceTransform = null)
    {
        if (EngineWindow.IS_EDITOR) return;
        // Directional light (sun)
        var sunColor = new Vector3(0.1f, 0.1f, 0.1f);
        var sunDirection = new Vector3(0, 90, 0);
        if (lightSpaceTransform.HasValue)
            sunDirection = Vector3.TransformNormal(sunDirection, lightSpaceTransform.Value);

        shader.SetUniform("dirLight.direction", sunDirection);
        shader.SetUniform("dirLight.ambient", sunColor * 0.05f);
        shader.SetUniform("dirLight.diffuse", sunColor);
        shader.SetUniform("dirLight.specular", sunColor);

        var lights = LatibuleEngine.Map.Lights;
        shader.SetUniform("pointLightsAmount", lights.Count(l => l is not null));
        for (var i = 0; i < lights.Length; i++)
        {
            var light = lights[i];
            if (light is null) continue;
            var lightColor = light.Color * light.Intensity;

            var lightPosition = light.Position;
            if (lightSpaceTransform.HasValue)
                lightPosition = Vector3.TransformPosition(lightPosition, lightSpaceTransform.Value);

            shader.SetUniform($"pointLights[{i}].position", lightPosition);
            shader.SetUniform($"pointLights[{i}].constant", light.Constant);
            shader.SetUniform($"pointLights[{i}].linear", light.Linear);
            shader.SetUniform($"pointLights[{i}].quadratic", light.Quadratic);
            shader.SetUniform($"pointLights[{i}].ambient", lightColor * 0.05f);
            shader.SetUniform($"pointLights[{i}].diffuse", lightColor);
            shader.SetUniform($"pointLights[{i}].specular", lightColor);
        }
    }
}