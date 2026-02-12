using Latibule.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Latibule.Gui;

public class DebugUi3D
{
    public bool ShowBoundingBoxes { get; set; } = true;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BasicEffect _basicEffect;
    public Dictionary<Tuple<BoundingBox, Color>, bool> DrawBox { get; set; } = new();

    public DebugUi3D(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _basicEffect = new BasicEffect(_graphicsDevice)
        {
            Alpha = 0.75f,
            VertexColorEnabled = true,
        };
    }

    public void Draw(Player player)
    {
        if (!ShowBoundingBoxes) return;
        // Draw the main bounding box outline
        DrawBoundingBoxOutline(player.Box, Color.White);

        foreach (var boundingBox in LatibuleGame.GameWorld.GetBoundingBoxes())
        {
            DrawBoundingBoxOutline(boundingBox, Color.Yellow);
        }

        foreach (var box in DrawBox.Where(box => box.Value))
        {
            DrawBoundingBoxOutline(box.Key.Item1, box.Key.Item2);
        }

        DrawBox.Clear();
    }

    // Draws a 3D bounding box wireframe outline
    public void DrawBoundingBoxOutline(BoundingBox box, Color color)
    {
        if (box == null) return;

        // Create vertices for the 8 corners of the box
        Vector3[] corners =
        [
            new(box.Min.X, box.Min.Y, box.Min.Z), // 0: bottom-left-front
            new(box.Max.X, box.Min.Y, box.Min.Z), // 1: bottom-right-front
            new(box.Max.X, box.Min.Y, box.Max.Z), // 2: bottom-right-back
            new(box.Min.X, box.Min.Y, box.Max.Z), // 3: bottom-left-back
            new(box.Min.X, box.Max.Y, box.Min.Z), // 4: top-left-front
            new(box.Max.X, box.Max.Y, box.Min.Z), // 5: top-right-front
            new(box.Max.X, box.Max.Y, box.Max.Z), // 6: top-right-back
            new(box.Min.X, box.Max.Y, box.Max.Z) // 7: top-left-back
        ];

        // Create colored vertices
        VertexPositionColor[] vertices = new VertexPositionColor[24]; // 12 lines × 2 vertices each

        // Bottom face
        vertices[0] = new VertexPositionColor(corners[0], color);
        vertices[1] = new VertexPositionColor(corners[1], color);
        vertices[2] = new VertexPositionColor(corners[1], color);
        vertices[3] = new VertexPositionColor(corners[2], color);
        vertices[4] = new VertexPositionColor(corners[2], color);
        vertices[5] = new VertexPositionColor(corners[3], color);
        vertices[6] = new VertexPositionColor(corners[3], color);
        vertices[7] = new VertexPositionColor(corners[0], color);

        // Top face
        vertices[8] = new VertexPositionColor(corners[4], color);
        vertices[9] = new VertexPositionColor(corners[5], color);
        vertices[10] = new VertexPositionColor(corners[5], color);
        vertices[11] = new VertexPositionColor(corners[6], color);
        vertices[12] = new VertexPositionColor(corners[6], color);
        vertices[13] = new VertexPositionColor(corners[7], color);
        vertices[14] = new VertexPositionColor(corners[7], color);
        vertices[15] = new VertexPositionColor(corners[4], color);

        // Vertical edges
        vertices[16] = new VertexPositionColor(corners[0], color);
        vertices[17] = new VertexPositionColor(corners[4], color);
        vertices[18] = new VertexPositionColor(corners[1], color);
        vertices[19] = new VertexPositionColor(corners[5], color);
        vertices[20] = new VertexPositionColor(corners[2], color);
        vertices[21] = new VertexPositionColor(corners[6], color);
        vertices[22] = new VertexPositionColor(corners[3], color);
        vertices[23] = new VertexPositionColor(corners[7], color);

        // Configure the effect for drawing lines
        _basicEffect.World = Matrix.Identity;
        _basicEffect.View = LatibuleGame.Player.Camera.View;
        _basicEffect.Projection = LatibuleGame.Player.Camera.Projection;

        // Draw the lines
        foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(
                PrimitiveType.LineList,
                vertices,
                0,
                12 // 12 lines
            );
        }
    }

    // Clean up resources when done
    public void Dispose()
    {
        _basicEffect?.Dispose();
        _graphicsDevice?.Dispose();
        DrawBox.Clear();
        GC.SuppressFinalize(this);
    }
}