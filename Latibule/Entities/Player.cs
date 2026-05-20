using Engine;
using Engine.Core;
using Engine.Core.ECS;
using Engine.Physics;
using Engine.Utilities;
using JoltPhysicsSharp;
using Latibule.Components;
using Latibule.Core;
using Latibule.Core.Gameplay;
using Latibule.Core.Types;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Latibule.Entities;

public class Player : GameObject
{
    public Vector3 StartingCoords { get; set; }

    public Vector3 RawPosition
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }

    private Vector3 CameraPosition => new(RawPosition.X, EyePosition.Y, RawPosition.Z);
    public Character Body { get; private set; }

    public static Inventory Inventory { get; set; } = new();

    // Player dimensions
    private const float Width = 0.6f;
    private const float Height = 1.8f;
    private const float HeightSneak = 1.6f; // Height when sneaking
    private const float Depth = 0.6f;

    // Movement properties
    private float _moveSpeed = 6f; // Speed of player movement
    private float _sneakSpeed = 1f; // Speed of player movement when sneaking
    private float _acceleration = 60f; // How fast we accelerate to target speed
    private float _friction = 8f; // Ground friction when no input
    private float _airMultiplier = 0.9f; // Air control multiplier
    public Vector3 CurrentMovementInput { get; private set; } = Vector3.Zero; // Current input direction
    public bool IsGrounded { get; set; }
    public static bool IsSneaking;
    private bool _isMovementPressed;

    // Gravity and jump properties
    private float _gravity = 24f;
    public Vector3 Velocity = Vector3.Zero;
    internal float MaxVelocity = 50f;
    private float _jumpForce = 8f;

    private float _jumpCooldown;
    private float _jumpCooldownTime = 0.4f;

    public Vector3 EyePosition =>
        RawPosition + new Vector3(0, IsSneaking ? Camera.EyeHeightOffsetSneak : Camera.EyeHeightOffset, 0);

    public bool CollisionEnabled { get; set; } = true;
    public bool IsNoclip { get; internal set; }
    public bool RestrictedAction { get; set; } = false;
    public bool LookEnabled { get; set; } = true;
    public bool CanMove { get; set; } = true;

    public Player()
    {
        var direction = Vector3Direction.Forward;
        LatibuleEngine.Camera = new Camera(CameraPosition, direction, EyePosition,
            EngineStates.GameWindow.ClientSize.X / (float)EngineStates.GameWindow.ClientSize.Y);

        var characterSettings = new CharacterSettings
        {
            Shape = new BoxShape(new BoxShapeSettings(
                new Vector3(Width / 2, Height / 2, Depth / 2).ToNumerics(),
                0.01f // the "roundness" of the box shape
            )),
            Layer = JoltPhysics.Layers.Moving,
            EnhancedInternalEdgeRemoval = true,
            Mass = 70f,
        };

        var position = RawPosition.ToNumerics();
        var transformRotation = System.Numerics.Quaternion.Identity;
        Body = new Character(characterSettings, in position, in transformRotation, 0, LatibuleEngine.Physics.PhysicsSystem);
        Body.AddToPhysicsSystem();
        LatibuleEngine.Physics.RegisterCharacter(Body);
    }

    public override void OnLoad()
    {
        base.OnLoad();

        WithComponents([
            new ViewModelComponent()
        ]);

        // Initialize position and orientation
        StartingCoords = Transform.Position;

        // Initialize the player's bounding box
        SyncCharacterToTransform();

        // Bind inputs
        Input.BindKeyPressed(Keys.R, () =>
        {
            Transform.Position = StartingCoords;
            Velocity = Vector3.Zero;

            Body.SetPositionAndRotation(
                StartingCoords.ToNumerics(),
                System.Numerics.Quaternion.Identity
            );

            Body.SetLinearVelocity(System.Numerics.Vector3.Zero);
        });
        Input.BindKeyPressed(Keys.V, ToggleNoclip);
        Input.BindKeyPressed(Keys.P, () => Punch(LatibuleEngine.Camera.Direction, 15f));

        Input.BindKeyPressed(Keys.D1, () => Inventory.SelectedItemIndex = 1);
        Input.BindKeyPressed(Keys.D2, () => Inventory.SelectedItemIndex = 2);
        Input.BindKeyPressed(Keys.D3, () => Inventory.SelectedItemIndex = 3);
        Input.BindKeyPressed(Keys.D4, () => Inventory.SelectedItemIndex = 4);
        Input.BindKeyPressed(Keys.D5, () => Inventory.SelectedItemIndex = 5);
        Input.BindKeyPressed(Keys.D6, () => Inventory.SelectedItemIndex = 6);
        Input.BindKeyPressed(Keys.D7, () => Inventory.SelectedItemIndex = 7);
        Input.BindKeyPressed(Keys.D8, () => Inventory.SelectedItemIndex = 8);
        Input.BindKeyPressed(Keys.D9, () => Inventory.SelectedItemIndex = 9);
        Input.BindKeyPressed(Keys.D0, () => Inventory.SelectedItemIndex = -1);
    }

    public void ToggleNoclip()
    {
        IsNoclip = !IsNoclip;
        Velocity = Vector3.Zero;
    }

    private void SyncCharacterToTransform()
    {
        Body.SetPositionAndRotation(
            Transform.Position.ToNumerics(),
            System.Numerics.Quaternion.Identity
        );

        Body.SetLinearVelocity(Velocity.ToNumerics());
    }

    private void UpdateCamera()
    {
        if (!LookEnabled) return;
        LatibuleEngine.Camera.Position = CameraPosition;
        LatibuleEngine.Camera.Update();
    }

    public override void OnUpdateFrame(FrameEventArgs args)
    {
        if (GameStates.CurrentGui is DevConsoleWindow) return;

        var deltaTime = (float)args.Time;

        var ms = EngineStates.MState;

        if (ms.IsButtonDown(MouseButton.Left) && Controls.Cooldown(200)) LeftClickAction();
        if (ms.WasButtonDown(MouseButton.Left) && ms.IsButtonReleased(MouseButton.Left)) Controls.ResetCooldown();
        if (ms.IsButtonDown(MouseButton.Right) && Controls.Cooldown(200)) RightClickAction();
        if (ms.WasButtonDown(MouseButton.Right) && ms.IsButtonReleased(MouseButton.Right)) Controls.ResetCooldown();

        IsSneaking = Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.LeftControl);

        if (IsNoclip)
        {
            Vector3 flyMove = Vector3.Zero;
            const float flySpeed = 12f;
            var forward = Vector3.Normalize(new Vector3(LatibuleEngine.Camera.Direction.X, 0, LatibuleEngine.Camera.Direction.Z));
            var right = Vector3.Normalize(Vector3.Cross(forward, Vector3Direction.Up));
            if (Input.IsKeyDown(Keys.W)) flyMove += forward;
            if (Input.IsKeyDown(Keys.S)) flyMove -= forward;
            if (Input.IsKeyDown(Keys.D)) flyMove += right;
            if (Input.IsKeyDown(Keys.A)) flyMove -= right;
            if (Input.IsKeyDown(Keys.Space)) flyMove += Vector3Direction.Up;
            if (IsSneaking) flyMove += Vector3Direction.Down;
            if (flyMove != Vector3.Zero) flyMove = Vector3.Normalize(flyMove);
            RawPosition += flyMove * flySpeed * deltaTime;

            Body.SetPositionAndRotation(
                RawPosition.ToNumerics(),
                System.Numerics.Quaternion.Identity
            );

            Body.SetLinearVelocity(System.Numerics.Vector3.Zero);
            Velocity = Vector3.Zero;

            UpdateCamera();
            return;
        }

        IsGrounded = Body.GroundState == GroundState.OnGround;

        ApplyMovementInput(deltaTime);
        ApplyGravity(deltaTime);
        HandleJumpInput(deltaTime);
        ApplyFriction(deltaTime);
        ClampVelocity();

        Body.SetLinearVelocity(Velocity.ToNumerics());

        RawPosition = Body.GetPosition().ToOpenTK();

        UpdateCamera();

        if (Transform.Position.Y < -100)
        {
            Transform.Position = new Vector3(Transform.Position.X, 100, Transform.Position.Z);

            Body.SetPositionAndRotation(
                Transform.Position.ToNumerics(),
                System.Numerics.Quaternion.Identity
            );

            Body.SetLinearVelocity(System.Numerics.Vector3.Zero);
            Velocity = Vector3.Zero;
        }
    }

    private void ApplyMovementInput(float deltaTime)
    {
        if (!CanMove)
        {
            // If movement is disabled, reset current movement input
            CurrentMovementInput = Vector3.Zero;
            return;
        }

        // Calculate forward and right vectors for movement
        var forward = Vector3.Normalize(new Vector3(LatibuleEngine.Camera.Direction.X, 0, LatibuleEngine.Camera.Direction.Z));
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3Direction.Up));

        // Determine if movement keys are pressed
        _isMovementPressed = Input.IsKeyDown(Keys.W) ||
                             Input.IsKeyDown(Keys.A) ||
                             Input.IsKeyDown(Keys.S) ||
                             Input.IsKeyDown(Keys.D);

        // Create a movement vector from key inputs
        var analogMove = Vector2.Zero;
        if (Input.IsKeyDown(Keys.W)) analogMove.Y += 1;
        if (Input.IsKeyDown(Keys.S)) analogMove.Y -= 1;
        if (Input.IsKeyDown(Keys.D)) analogMove.X += 1;
        if (Input.IsKeyDown(Keys.A)) analogMove.X -= 1;

        // Normalize the input if it's non-zero
        if (analogMove != Vector2.Zero) analogMove = Vector2.Normalize(analogMove);

        // Create target movement direction
        Vector3 inputDirection = Vector3.Zero;
        if (_isMovementPressed)
        {
            inputDirection = right * analogMove.X + forward * analogMove.Y;
            if (inputDirection != Vector3.Zero)
                inputDirection = Vector3.Normalize(inputDirection);
        }

        // Store the current movement input for reference
        CurrentMovementInput = inputDirection;

        // Calculate target velocity based on input
        var targetSpeed = IsSneaking ? _sneakSpeed : _moveSpeed;
        var targetVelocity = inputDirection * targetSpeed;

        // Apply acceleration towards target velocity
        var acceleration = _acceleration;
        if (!IsGrounded)
        {
            // Reduced control in air
            acceleration *= _airMultiplier;
        }

        // Get current horizontal velocity
        var currentHorizontalVelocity = new Vector3(Velocity.X, 0, Velocity.Z);

        // Accelerate towards target
        var velocityDiff = targetVelocity - currentHorizontalVelocity;
        var accelerationAmount = acceleration * deltaTime;

        // Limit acceleration to prevent overshooting
        if (velocityDiff.Length > accelerationAmount)
        {
            velocityDiff = Vector3.Normalize(velocityDiff) * accelerationAmount;
        }

        // Apply acceleration to horizontal velocity
        Velocity.X += velocityDiff.X;
        Velocity.Z += velocityDiff.Z;
    }

    private void ApplyFriction(float deltaTime)
    {
        if (!IsGrounded || _isMovementPressed) return;

        // Apply ground friction when grounded and no input
        var horizontalVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
        var speed = horizontalVelocity.Length;

        if (speed > 0.001f)
        {
            var frictionAmount = _friction * deltaTime;
            var newSpeed = MathF.Max(0, speed - frictionAmount);
            var frictionFactor = newSpeed / speed;

            Velocity.X *= frictionFactor;
            Velocity.Z *= frictionFactor;
        }
        else
        {
            // Stop completely if velocity is very small
            Velocity.X = 0;
            Velocity.Z = 0;
        }
    }

    private void ClampVelocity()
    {
        if (Velocity.Length >= MaxVelocity || Velocity.LengthSquared <= -MaxVelocity) Velocity = Vector3.Normalize(Velocity) * MaxVelocity;
    }

    private void ApplyGravity(float deltaTime)
    {
        if (!CollisionEnabled) return;

        // Apply gravity to velocity if not grounded
        if (!IsGrounded)
        {
            Velocity.Y -= _gravity * deltaTime;
        }
        else if (Velocity.Y < 0)
        {
            // Zero out downward velocity when grounded
            Velocity.Y = 0;
        }
    }

    private void HandleJumpInput(float deltaTime)
    {
        // Decrease jump cooldown if it's active
        if (_jumpCooldown > 0)
        {
            _jumpCooldown -= 1 * deltaTime;
        }

        // Handle jumping - allow jumping when grounded and cooldown is complete
        if (Input.IsKeyDown(Keys.Space) && IsGrounded && _jumpCooldown <= 0)
        {
            Velocity.Y = _jumpForce;
            _jumpCooldown = _jumpCooldownTime;
        }
    }

    // Punches the player in a direction
    public void Punch(Vector3 direction, float strength)
    {
        // Apply a force in the specified direction
        if (IsNoclip || RestrictedAction) return;
        if (direction == Vector3.Zero) return;
        var normalizedDirection = Vector3.Normalize(direction);
        // Apply the punch force to the player's velocity
        Velocity += normalizedDirection * strength;
    }

    // private void CheckGrounded()
    // {
    //     const float groundCheckDistance = 0.15f;
    //     const float inwardOffset = 0.01f;
    //
    //     var allCorners = BoundingBox.GetCorners();
    //     var bottomCorners = new Vector3[4];
    //     bottomCorners[0] = allCorners[2];
    //     bottomCorners[1] = allCorners[3];
    //     bottomCorners[2] = allCorners[6];
    //     bottomCorners[3] = allCorners[7];
    //
    //     var center = BoundingBox.Center;
    //
    //     // Move corners slightly inward on X and Z
    //     for (var i = 0; i < bottomCorners.Length; i++)
    //     {
    //         var corner = bottomCorners[i];
    //
    //         var dirX = MathF.Sign(center.X - corner.X);
    //         var dirZ = MathF.Sign(center.Z - corner.Z);
    //
    //         corner.X += dirX * inwardOffset;
    //         corner.Z += dirZ * inwardOffset;
    //
    //         bottomCorners[i] = corner;
    //     }
    //
    //     var boxes = LatibuleEngine.Map.GetBoundingBoxes();
    //
    //     foreach (var box in boxes)
    //     {
    //         foreach (var corner in bottomCorners)
    //         {
    //             var rayStart = corner + new Vector3(0, 0.14f, 0);
    //             var rayEnd = rayStart - new Vector3(0, groundCheckDistance, 0);
    //
    //             if (!AabbHelper.RayIntersectsAabb(rayStart, rayEnd, box, out var hitPoint, out _))
    //                 continue;
    //
    //             IsGrounded = true;
    //
    //             var distanceToGround = rayStart.Y - hitPoint.Y;
    //             if (distanceToGround >= 0.0001f)
    //                 return;
    //
    //             RawPosition = RawPosition with { Y = hitPoint.Y + 0.0001f };
    //             UpdateBoundingBox();
    //             return;
    //         }
    //     }
    // }

    private static void LeftClickAction() => Inventory.SelectedItem()?.Use();
    private static void RightClickAction() => Inventory.SelectedItem()?.SecondaryUse();
}