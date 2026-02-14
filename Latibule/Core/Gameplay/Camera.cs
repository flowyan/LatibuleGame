using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Latibule.Core.Gameplay;

public class Camera
{
    private GraphicsDevice _graphicsDevice;

    // Constants
    private const float Fov = 70f; // Field of view in degrees
    private const float NearPlaneDistance = 0.001f; // Near plane distance for projection matrix
    private const float FarPlaneDistance = 250f; // Far plane distance for projection matrix

    // Eye height offset for positioning the camera at eye level instead of feet
    public const float EyeHeightOffset = 1.6f;
    public const float EyeHeightOffsetSneak = 1.5f; // Height when sneaking
    public Matrix View { get; set; }
    public Matrix Projection { get; private set; }
    public Vector3 Direction { get; set; }
    public Vector3 EyePosition { get; private set; }

    /// <summary>
    /// The position of the camera in world space. Would be the Eye position of the player.
    /// </summary>
    public Vector3 Position { get; set; }

    public Vector3 HorizontalDirection;

    public BoundingFrustum Frustum { get; set; }

    private Vector3 _target;

    private float _yaw;
    private float _pitch;

    public Camera(GraphicsDevice graphics, Vector3 position, Vector3 target, Vector3 eyePosition)
    {
        _graphicsDevice = graphics;
        _target = target;

        Position = position;
        Direction = target - position;
        Direction = Vector3.Normalize(Direction);
        EyePosition = eyePosition;

        HorizontalDirection = new Vector3(Direction.X, 0f, Direction.Z);
        HorizontalDirection.Normalize();

        _yaw = 0f;
        _pitch = 0f;

        UpdateViewMatrix();
        UpdateProjectionMatrix();

        Frustum = new BoundingFrustum(View * Projection);
    }


    public void Update(GameTime gameTime)
    {
        HandleMouseLook();

        HorizontalDirection.X = Direction.X;
        HorizontalDirection.Z = Direction.Z;

        if (HorizontalDirection.LengthSquared() > 0.000001f)
            HorizontalDirection.Normalize();
        else
            HorizontalDirection = Vector3.Forward; // or keep previous value


        _target = Direction + Position;

        View = Matrix.CreateLookAt(Position, _target, Vector3.Up);

        Frustum.Matrix = View * Projection;
    }

    private void UpdateViewMatrix()
    {
        // Use eye position (at eye level) instead of feet position for the camera
        View = Matrix.CreateLookAt(EyePosition, EyePosition + Direction, Vector3.Up);
    }

    private void UpdateProjectionMatrix()
    {
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(Fov),
            _graphicsDevice.Viewport.AspectRatio,
            NearPlaneDistance,
            FarPlaneDistance);
    }

    private void HandleMouseLook()
    {
        if (GameStates.MouseLookLocked) return;

        var deltaX = GameStates.PreviousMState.X;
        var deltaY = GameStates.PreviousMState.Y;

        // Only process mouse movement if there actually was movement
        if (deltaX == 0 && deltaY == 0) return;

        // Update yaw (left/right) and pitch (up/down)
        _yaw -= deltaX * GameOptions.MouseSensitivity; // Invert Y-axis for a natural feel
        _pitch += deltaY * GameOptions.MouseSensitivity;

        // Update direction vector from the new pitch and yaw
        // Clamp pitch to avoid gimbal lock
        _pitch = MathHelper.Clamp(_pitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

        // Calculate the new direction vector based on pitch and yaw
        Direction = new Vector3(
            (float)(Math.Cos(_pitch) * Math.Sin(_yaw)),
            (float)-Math.Sin(_pitch),
            (float)(Math.Cos(_pitch) * Math.Cos(_yaw))
        );

        // Ensure direction is normalized
        Direction = Vector3.Normalize(Direction);
    }
}