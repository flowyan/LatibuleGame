using Latibule.Core;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Gameplay;
using Latibule.Core.Physics;
using Latibule.Core.Types;
using Latibule.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
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
    public Camera Camera { get; private set; }
    public PlayerPhysics Physics { get; private set; }
    public BoundingBox BoundingBox { get; private set; }

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
    private float _gravity = 50f;
    public Vector3 Velocity = Vector3.Zero;
    internal float MaxVelocity = 50f;
    private float _jumpForce = 10f;

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
        Camera = new Camera(CameraPosition, direction, EyePosition);
    }

    public override void OnLoad()
    {
        base.OnLoad();

        Physics = new PlayerPhysics(this);

        // Initialize position and orientation
        StartingCoords = Transform.Position;

        // Initialize the player's bounding box
        UpdateBoundingBox();

        // Bind inputs
        Input.BindKeyPressed(Keys.R, () =>
        {
            Transform.Position = StartingCoords;
            Velocity = Vector3.Zero;
            UpdateBoundingBox();
        });
        Input.BindKeyPressed(Keys.V, () =>
        {
            IsNoclip = !IsNoclip;
            Velocity = Vector3.Zero;
        });
        Input.BindKeyPressed(Keys.P, () => Punch(Camera.Direction, 15f));
    }

    public void UpdateBoundingBox()
    {
        // Calculate the minimum corner for BoundingBox (not the center)
        // Center the bounding box horizontally around the player's position
        var min = new Vector3(
            Transform.Position.X - (Width / 2f),
            Transform.Position.Y, // Feet at bottom of bounding box
            Transform.Position.Z - (Depth / 2f)
        );

        var height = IsSneaking ? HeightSneak : Height; // Adjust height for sneaking

        var max = new Vector3(
            Transform.Position.X + (Width / 2f),
            Transform.Position.Y + height, // Top of bounding box
            Transform.Position.Z + (Depth / 2f)
        );

        BoundingBox = new BoundingBox(min, max);
    }

    private void UpdateCamera(FrameEventArgs args)
    {
        if (!LookEnabled) return;
        Camera.Position = CameraPosition;
        Camera.Update(args);
    }

    public override void OnUpdateFrame(FrameEventArgs args)
    {
        if (GameStates.CurrentGui is DevConsole) return;

        // if (ks.IsKeyDown(Keys.E) && !Engine.PreviousKState.IsKeyDown(Keys.E)) Engine.SetUiOnScreen(new InventoryGui());

        // If a GUI is currently open, skip player update
        // if (Engine.CurrentGui is not null) return;

        var deltaTime = (float)args.Time;

        // Update the hit result for the current frame
        PushBackIfAtWorldEdge();


        var ms = GameStates.MState;

        if (ms.IsButtonDown(MouseButton.Left) && Controls.Cooldown(200)) LeftClickAction();
        if (ms.WasButtonDown(MouseButton.Left) && ms.IsButtonReleased(MouseButton.Left)) Controls.ResetCooldown();
        if (ms.IsButtonDown(MouseButton.Left) && Controls.Cooldown(200)) RightClickAction();
        if (ms.WasButtonDown(MouseButton.Right) && ms.IsButtonReleased(MouseButton.Right)) Controls.ResetCooldown();
        if (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.LeftControl)) IsSneaking = true;
        else IsSneaking = false;

        if (IsNoclip)
        {
            // Allow flying in all directions, ignore gravity and collisions
            // WASD for horizontal, Space for up, LeftShift for down
            Vector3 flyMove = Vector3.Zero;
            var flySpeed = 12f; // Flying speed
            var forward = Vector3.Normalize(new Vector3(Camera.Direction.X, 0, Camera.Direction.Z));
            var right = Vector3.Normalize(Vector3.Cross(forward, Vector3Direction.Up));
            if (Input.IsKeyDown(Keys.W)) flyMove += forward;
            if (Input.IsKeyDown(Keys.S)) flyMove -= forward;
            if (Input.IsKeyDown(Keys.D)) flyMove += right;
            if (Input.IsKeyDown(Keys.A)) flyMove -= right;
            if (Input.IsKeyDown(Keys.Space)) flyMove += Vector3Direction.Up;
            if (IsSneaking) flyMove += Vector3Direction.Down;
            if (flyMove != Vector3.Zero) flyMove = Vector3.Normalize(flyMove);
            RawPosition += flyMove * flySpeed * deltaTime;
            UpdateBoundingBox();
            UpdateCamera(args);
            return;
        }

        // Apply movement input to horizontal velocity
        ApplyMovementInput(deltaTime);

        // Apply gravity to vertical velocity
        ApplyGravity(deltaTime);

        // Handle jumping input (only sets velocity, doesn't move yet)
        HandleJumpInput(deltaTime);

        // Apply friction/drag to velocity
        ApplyFriction(deltaTime);

        // Clamp horizontal velocity to maximum speed
        ClampVelocity();

        // Calculate the total displacement from velocity
        var displacement = Velocity * deltaTime;

        // Predict and resolve collisions, updating position and physics state
        if (CollisionEnabled) HandleCollisions(displacement, deltaTime);
        else RawPosition += displacement;

        UpdateCamera(args);

        // Update the bounding box after position changes
        UpdateBoundingBox();

        if (Transform.Position.Y < -100)
            Transform.Position = new Vector3(Transform.Position.X, 100, Transform.Position.Z);
    }

    private void PushBackIfAtWorldEdge()
    {
        // if (Engine.WorldLevelManager.Current.GetPlayerChunk(new BlockCoord(Position)) == null)
        // {
        //     Position = Vector3.Lerp(Position, new Vector3(0, Transform.Position.Y, 0), 0.02f);
        // }
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
        var forward = Vector3.Normalize(new Vector3(Camera.Direction.X, 0, Camera.Direction.Z));
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

    private void HandleCollisions(Vector3 intendedDisplacement, float deltaTime)
    {
        // Reset grounded state - will be set to true if we detect a ground collision
        IsGrounded = false;

        // First, try to move horizontally (X and Z)
        var horizontalMove = new Vector3(intendedDisplacement.X, 0, intendedDisplacement.Z);
        if (horizontalMove != Vector3.Zero)
        {
            RawPosition = RawPosition with { X = RawPosition.X + horizontalMove.X };
            RawPosition = RawPosition with { Z = RawPosition.Z + horizontalMove.Z };

            // Update bounding box and check for collisions
            UpdateBoundingBox();
            Physics.ResolveCollisions();
        }

        // Then, try to move vertically (Y)
        RawPosition = RawPosition with { Y = RawPosition.Y + intendedDisplacement.Y };
        UpdateBoundingBox();
        Physics.ResolveCollisions();

        // Check for grounded state by casting a ray slightly below the player
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        const float groundCheckDistance = 0.15f;
        const float inwardOffset = 0.01f;

        var allCorners = BoundingBox.GetCorners();
        var bottomCorners = new Vector3[4];
        bottomCorners[0] = allCorners[2];
        bottomCorners[1] = allCorners[3];
        bottomCorners[2] = allCorners[6];
        bottomCorners[3] = allCorners[7];

        var center = BoundingBox.Center;

        // Move corners slightly inward on X and Z
        for (var i = 0; i < bottomCorners.Length; i++)
        {
            var corner = bottomCorners[i];

            var dirX = MathF.Sign(center.X - corner.X);
            var dirZ = MathF.Sign(center.Z - corner.Z);

            corner.X += dirX * inwardOffset;
            corner.Z += dirZ * inwardOffset;

            bottomCorners[i] = corner;
        }

        var boxes = LatibuleGame.GameWorld.GetBoundingBoxes();

        foreach (var box in boxes)
        {
            foreach (var corner in bottomCorners)
            {
                var rayStart = corner + new Vector3(0, 0.14f, 0);
                var rayEnd = rayStart - new Vector3(0, groundCheckDistance, 0);

                if (!AabbHelper.RayIntersectsAabb(rayStart, rayEnd, box, out var hitPoint, out _))
                    continue;

                IsGrounded = true;

                var distanceToGround = rayStart.Y - hitPoint.Y;
                if (distanceToGround >= 0.0001f)
                    return;

                RawPosition = RawPosition with { Y = hitPoint.Y + 0.0001f };
                UpdateBoundingBox();
                return;
            }
        }
    }

    public void LeftClickAction()
    {
        Console.WriteLine("left click action");
        Asseteer.PlaySound(SoundAsset.missing, volume: 0.5f);
    }

    public void RightClickAction()
    {
        Console.WriteLine("right click action");
        Asseteer.PlaySound(SoundAsset.missing, volume: 0.5f);
    }
}