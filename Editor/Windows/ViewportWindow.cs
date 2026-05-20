using System.Numerics;
using Editor.Core;
using Editor.Core.Types;
using Editor.Objects;
using Engine;
using Engine.Core;
using Engine.Core.Types;
using Engine.Rendering;
using FontStashSharp;
using ImGuiNET;
using Latibule.Core.Types;
using Latibule.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Editor.Windows;

public sealed class ViewportWindow : IEditorWindow, IDisposable
{
    private int _framebuffer;
    private int _colorTexture;
    private int _depthStencilRenderbuffer;
    private int _viewportWidth;
    private int _viewportHeight;
    private System.Numerics.Vector2 _viewportImageMin;
    private System.Numerics.Vector2 _viewportImageSize;
    private bool _viewportImageHovered;

    public string Title => "Viewport";
    public EditorWindowSlot Slot => EditorWindowSlot.TopCenter;

    public ViewportWindow()
    {
        Input.BindMousePressed(MouseButton.Left, ClickAction);
    }

    public void Render(FrameEventArgs e)
    {
        var availableSize = ImGui.GetContentRegionAvail();
        var targetWidth = Math.Max(1, (int)availableSize.X);
        var targetHeight = Math.Max(1, (int)availableSize.Y);

        EnsureFramebuffer(targetWidth, targetHeight);
        RenderToFramebuffer(e, targetWidth, targetHeight);

        ImGui.Image((nint)_colorTexture, availableSize, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));

        // Cache the last viewport image bounds so click handling can map from window to viewport-local coordinates.
        _viewportImageMin = ImGui.GetItemRectMin();
        _viewportImageSize = ImGui.GetItemRectSize();
        _viewportImageHovered = ImGui.IsItemHovered();
    }

    public void Update(FrameEventArgs e)
    {
        // potential draw mode or smthing?
        // if (Input.IsMouseDown(MouseButton.Left) && Controls.Cooldown(50)) ClickAction();
    }

    private void ClickAction()
    {
        if (LatibuleEngine.Camera is null || !_viewportImageHovered || _viewportWidth <= 0 || _viewportHeight <= 0)
            return;

        var mouseX = Input.MouseState.X;
        var mouseY = Input.MouseState.Y;

        var localX = mouseX - _viewportImageMin.X;
        var localY = mouseY - _viewportImageMin.Y;

        if (localX < 0 || localY < 0 || localX > _viewportImageSize.X || localY > _viewportImageSize.Y)
            return;

        var ray = MousePicking.CreateRayFromMouse(
            new Vector2(localX, localY),
            new Vector2(_viewportWidth, _viewportHeight),
            LatibuleEngine.Camera.View,
            LatibuleEngine.Camera.Projection);

        if (MousePicking.TryIntersectPhysicsBody(ray, LatibuleEngine.Map.Objects, out var hitObject, out var objectHitPoint))
        {
            Logger.LogDebug($"Clicked viewport ({localX:0.0}, {localY:0.0}), hit physics body: {hitObject} at world position: {objectHitPoint}");
            MalletEditor.SelectObject(hitObject);
        }
        else
        {
            MalletEditor.SelectObject(null);
            Logger.LogDebug($"Clicked viewport ({localX:0.0}, {localY:0.0}), no object hit");
        }

        // LatibuleEngine.Map.AddObject(new EditorObjectText("X", BillboardEnum.Full, FSColor.Red)
        // {
        //     Transform =
        //     {
        //         Position = hitPoint
        //     }
        // }, true);
    }

    public void Dispose()
    {
        if (_depthStencilRenderbuffer != 0)
        {
            GL.DeleteRenderbuffer(_depthStencilRenderbuffer);
            _depthStencilRenderbuffer = 0;
        }

        if (_colorTexture != 0)
        {
            GL.DeleteTexture(_colorTexture);
            _colorTexture = 0;
        }

        if (_framebuffer != 0)
        {
            GL.DeleteFramebuffer(_framebuffer);
            _framebuffer = 0;
        }
    }

    private void RenderToFramebuffer(FrameEventArgs e, int width, int height)
    {
        if (LatibuleEngine.Camera is null) return;

        int[] previousViewport = new int[4];
        GL.GetInteger(GetPName.Viewport, previousViewport);
        GL.GetInteger(GetPName.FramebufferBinding, out var previousFramebuffer);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
        GL.Viewport(0, 0, width, height);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);
        GL.ClearColor(0.08f, 0.08f, 0.1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        LatibuleEngine.Camera.AspectRatio = width / (float)height;
        LatibuleEngine.Camera.UpdateProjectionMatrix();
        RenderQueue.OnFrameRender(e);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebuffer);
        GL.Viewport(previousViewport[0], previousViewport[1], previousViewport[2], previousViewport[3]);
    }

    private void EnsureFramebuffer(int width, int height)
    {
        if (_framebuffer != 0 && width == _viewportWidth && height == _viewportHeight) return;

        Dispose();

        _viewportWidth = width;
        _viewportHeight = height;

        _framebuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

        _colorTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _colorTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _colorTexture, 0);

        _depthStencilRenderbuffer = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthStencilRenderbuffer);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _depthStencilRenderbuffer);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
            throw new InvalidOperationException($"Viewport framebuffer is incomplete: {status}");

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}