using Latibule.Core.Data;
using Latibule.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Latibule.Core.Rendering.Objects;

public class Plane(Game game) : GameObject(game)
{
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private BasicEffect _effect;

    private readonly RasterizerState _rasterizerState = new()
    {
        CullMode = CullMode.CullCounterClockwiseFace,
        MultiSampleAntiAlias = true,
    };

    private readonly Game _game = game;

    public override void Initialize()
    {
        var _graphicsDevice = _game.GraphicsDevice;
        var planeVertices = new VertexPositionTexture[4];

        planeVertices[0] = new VertexPositionTexture(
            new Vector3(-1, 0, -1), new Vector2(0, UVScale.Y)); // bottom-left
        planeVertices[1] = new VertexPositionTexture(
            new Vector3(1, 0, -1), new Vector2(UVScale.X, UVScale.Y)); // bottom-right
        planeVertices[2] = new VertexPositionTexture(
            new Vector3(-1, 0, 1), new Vector2(0, 0)); // top-left
        planeVertices[3] = new VertexPositionTexture(
            new Vector3(1, 0, 1), new Vector2(UVScale.X, 0)); // top-right

        var planeIndices = new short[]
        {
            0, 1, 2,
            1, 3, 2
        };

        _vertexBuffer = new VertexBuffer(
            _graphicsDevice,
            typeof(VertexPositionTexture),
            4,
            BufferUsage.WriteOnly
        );
        _vertexBuffer.SetData(planeVertices);

        _indexBuffer = new IndexBuffer(
            _graphicsDevice,
            IndexElementSize.SixteenBits,
            6,
            BufferUsage.WriteOnly
        );
        _indexBuffer.SetData(planeIndices);

        _effect = new BasicEffect(_graphicsDevice)
        {
            TextureEnabled = true,
            Texture = AssetManager.LoadedTextures["material/stone"],
        };
    }

    public override void Draw(GameTime gameTime)
    {
        var graphicsDevice = _game.GraphicsDevice;
        var oldBlendState = graphicsDevice.BlendState;
        var oldDepthStencilState = graphicsDevice.DepthStencilState;
        var oldRasterizerState = graphicsDevice.RasterizerState;

        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        graphicsDevice.RasterizerState = _rasterizerState;

        var camera = LatibuleGame.Player.Camera;
        _effect.World = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position);
        _effect.View = camera.View;
        _effect.Projection = camera.Projection;

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                4,
                0,
                2 // Number of triangles
            );
        }

        graphicsDevice.BlendState = oldBlendState;
        graphicsDevice.DepthStencilState = oldDepthStencilState;
        graphicsDevice.RasterizerState = oldRasterizerState;
    }
}