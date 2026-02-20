using Latibule.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Gameplay;

public class Camera
{
    // Constants
    private const float Fov = 90f; // Field of view in degrees
    private const float NearPlaneDistance = 0.001f; // Near plane distance for projection matrix
    private const float FarPlaneDistance = 250f; // Far plane distance for projection matrix

    // Eye height offset for positioning the camera at eye level instead of feet
    public const float EyeHeightOffset = 1.6f;
    public const float EyeHeightOffsetSneak = 1.5f; // Height when sneaking
    public Matrix4 View { get; set; }
    public Matrix4 Projection { get; private set; }
    public Vector3 Direction { get; set; }
    public Vector3 EyePosition { get; private set; }

    /// <summary>
    /// The position of the camera in world space. Would be the Eye position of the player.
    /// </summary>
    public Vector3 Position { get; set; }

    public Vector3 HorizontalDirection;

    private Vector3 _target;
    private float _aspectRatio;

    private float _yaw;
    private float _pitch;

    public Camera(Vector3 position, Vector3 target, Vector3 eyePosition)
    {
        _aspectRatio = GameStates.GameWindow.Bounds.Size.X / (float)GameStates.GameWindow.Bounds.Size.Y;
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
    }


    public void Update(FrameEventArgs args)
    {
        HandleMouseLook();

        HorizontalDirection.X = Direction.X;
        HorizontalDirection.Z = Direction.Z;

        if (HorizontalDirection.LengthSquared > 0.000001f) HorizontalDirection = Vector3.Normalize(HorizontalDirection);
        else HorizontalDirection = Vector3Direction.Forward; // or keep previous value


        _target = Direction + Position;

        View = Matrix4.LookAt(Position, _target, Vector3Direction.Up);

        // Frustum.Matrix = View * Projection;
    }

    private void UpdateViewMatrix()
    {
        // Use eye position (at eye level) instead of feet position for the camera
        View = Matrix4.LookAt(EyePosition, EyePosition + Direction, Vector3Direction.Up);
    }

    private void UpdateProjectionMatrix()
    {
        Projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(Fov),
            _aspectRatio,
            NearPlaneDistance,
            FarPlaneDistance);
    }

    private void HandleMouseLook()
    {
        if (GameStates.MouseLookLocked) return;

        var deltaX = GameStates.MState.Delta.X;
        var deltaY = GameStates.MState.Delta.Y;

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