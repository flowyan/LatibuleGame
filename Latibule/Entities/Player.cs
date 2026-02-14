using Latibule.Core;
using Latibule.Core.Gameplay;
using Latibule.Core.Physics;
using Latibule.Models;
using Latibule.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Latibule.Entities;

public class Player
{
    public Vector3 StartingCoords { get; set; }
    public Vector3 RawPosition;
    private Vector3 _up;

    public Vector3 Position
    {
        get => RawPosition;
        set
        {
            RawPosition = value;
            Velocity = Vector3.Zero;
            UpdateBoundingBox();
        }
    }

    private Vector3 CameraPosition => new(RawPosition.X, EyePosition.Y, RawPosition.Z);
    public Camera Camera { get; private set; }
    public PlayerPhysics Physics { get; private set; }
    public BoundingBox BoundingBox { get; private set; }

    // Add player dimensions
    private const float Width = 0.6f;
    private const float Height = 1.8f;
    private const float HeightSneak = 1.6f; // Height when sneaking
    private const float Depth = 0.6f;

    private const float ReachDistance = 5f;


    // Movement properties
    private float _moveSpeed = 4f; // Speed of player movement
    private float _sneakSpeed = 1f; // Speed of player movement when sneaking
    private float _inertia = 0.8f;
    private float _airMultiplier = 0.8f;
    public Vector3 CurrentMovement { get; private set; } = Vector3.Zero;
    public bool IsGrounded { get; set; }
    public static bool IsSneaking;
    private bool _isMovementPressed;

    // Gravity and jump properties
    private float _gravity = 0.5f;
    public Vector3 Velocity = Vector3.Zero;
    private float _velocityLimit = 0.5f;
    private float _jumpForce = 15f;

    // Add a cooldown timer for jump
    private float _jumpCooldown;
    private float _jumpCooldownTime = 0.3f;

    public Vector3 EyePosition =>
        RawPosition + new Vector3(0, IsSneaking ? Camera.EyeHeightOffsetSneak : Camera.EyeHeightOffset, 0);

    public bool CollisionEnabled { get; set; } = true;
    public bool IsNoclip { get; private set; } = true;
    public bool RestrictedAction { get; set; } = false;
    public bool LookEnabled { get; set; } = true;
    public bool CanMove { get; set; } = true;

    public Player(GameWindow gameWindow, Vector3 startingCoords)
    {
        var direction = Vector3Direction.Forward;
        Camera = new Camera(gameWindow.Size.X / (float)gameWindow.Size.Y, CameraPosition, direction, EyePosition);
        Physics = new PlayerPhysics(this);

        // Initialize position and orientation
        StartingCoords = startingCoords;
        Position = startingCoords;
        _up = Vector3Direction.Up;

        // Initialize the player's bounding box
        UpdateBoundingBox();

        // Bind inputs
        Input.BindKeyPressed(Keys.R, () => Position = StartingCoords);
        Input.BindKeyPressed(Keys.V, () => IsNoclip = !IsNoclip);
    }

    public void UpdateBoundingBox()
    {
        // Calculate the minimum corner for BoundingBox (not the center)
        // Center the bounding box horizontally around the player's position
        var min = new Vector3(
            Position.X - (Width / 2f),
            Position.Y, // Feet at bottom of bounding box
            Position.Z - (Depth / 2f)
        );

        var height = IsSneaking ? HeightSneak : Height; // Adjust height for sneaking

        var max = new Vector3(
            Position.X + (Width / 2f),
            Position.Y + height, // Top of bounding box
            Position.Z + (Depth / 2f)
        );

        BoundingBox = new BoundingBox(min, max);
    }

    private void UpdateCamera(FrameEventArgs args)
    {
        if (!LookEnabled) return;
        Camera.Position = CameraPosition;
        Camera.Update(args);
    }

    public void Update(FrameEventArgs args)
    {
        if (GameStates.CurrentGui is DevConsole) return;

        // if (ks.IsKeyDown(Keys.E) && !Engine.PreviousKState.IsKeyDown(Keys.E)) Engine.SetUiOnScreen(new InventoryGui());

        // If a GUI is currently open, skip player update
        // if (Engine.CurrentGui is not null) return;

        var deltaTime = (float)args.Time;

        // Update the hit result for the current frame
        PushBackIfAtWorldEdge();

        if (Velocity.Length >= _velocityLimit || Velocity.LengthSquared <= -_velocityLimit)
        {
            // Limit the velocity to prevent excessive speed
            Velocity = Vector3.Normalize(Velocity) * _velocityLimit;
        }

        var ms = GameStates.MState;

        if (ms.IsButtonDown(MouseButton.Left) && Controls.Cooldown(200)) LeftClickAction();
        if (ms.WasButtonDown(MouseButton.Left) && ms.IsButtonReleased(MouseButton.Left)) Controls.ResetCooldown();
        if (ms.IsButtonDown(MouseButton.Left) && Controls.Cooldown(200)) RightClickAction();
        if (ms.WasButtonDown(MouseButton.Right) && ms.IsButtonReleased(MouseButton.Right)) Controls.ResetCooldown();
        if (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.LeftControl)) IsSneaking = true;
        else IsSneaking = false;

        if (Input.IsKeyPressed(Keys.P))
            Punch(
                Camera.Direction, // Punch in the direction the player is looking
                10f // Strength of the punch
            );

        // Calculate the desired horizontal movement based on input
        var desiredMovement = CalculateMovementInput(deltaTime);

        if (IsNoclip)
        {
            // Allow flying in all directions, ignore gravity and collisions
            // WASD for horizontal, Space for up, LeftShift for down
            Vector3 flyMove = Vector3.Zero;
            var flySpeed = 12f; // Flying speed
            var forward = Vector3.Normalize(new Vector3(Camera.Direction.X, 0, Camera.Direction.Z));
            var right = Vector3.Normalize(Vector3.Cross(forward, _up));
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

        // Apply gravity to velocity
        ApplyGravity(deltaTime);

        // Handle jumping input (only sets velocity, doesn't move yet)
        HandleJumpInput(deltaTime);

        // Calculate the total displacement vector for this frame
        var displacement = CalculateDisplacement(desiredMovement);

        // Predict and resolve collisions, updating position and physics state

        // If no collision objects, just apply the displacement directly
        if (CollisionEnabled) HandleCollisions(displacement, deltaTime);
        else RawPosition += displacement;

        UpdateCamera(args);

        // Update the bounding box after position changes
        UpdateBoundingBox();
    }

    private void PushBackIfAtWorldEdge()
    {
        // if (Engine.WorldLevelManager.Current.GetPlayerChunk(new BlockCoord(Position)) == null)
        // {
        //     Position = Vector3.Lerp(Position, new Vector3(0, Position.Y, 0), 0.02f);
        // }
    }

    private Vector3 CalculateMovementInput(float deltaTime)
    {
        if (CanMove == false)
        {
            // If movement is disabled, reset current movement
            CurrentMovement = Vector3.Zero;
            return Vector3.Zero;
        }

        // Calculate forward and right vectors for movement
        var forward =
            Vector3.Normalize(new Vector3(Camera.Direction.X, 0, Camera.Direction.Z)); // Ground-aligned forward
        var right = Vector3.Normalize(Vector3.Cross(forward, _up));

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

        // Create target movement vector
        var targetMovement = Vector3.Zero;

        if (_isMovementPressed)
        {
            // Create movement vector from input direction
            targetMovement = right * analogMove.X + forward * analogMove.Y;

            // Normalize and scale by move speed if the vector is non-zero
            if (targetMovement != Vector3.Zero)
            {
                var speed = IsSneaking ? _sneakSpeed : _moveSpeed;
                targetMovement = Vector3.Normalize(targetMovement) * speed;

                // Apply air control multiplier if not grounded
                if (!IsGrounded)
                {
                    targetMovement *= _airMultiplier;
                }
            }
        }

        // Blend current movement with target movement (apply inertia)
        if (IsGrounded)
        {
            // Apply frame-rate independent inertia on the ground
            //var frameInertia = (float)Math.Pow(_inertia, deltaTime);
            //_currentMovement = Vector3.Lerp(targetMovement, _currentMovement, frameInertia);
            CurrentMovement = Vector3.Lerp(targetMovement, CurrentMovement, _inertia);

            // Stop completely if movement is very small
            if (CurrentMovement.LengthSquared < 0.0001f) CurrentMovement = Vector3.Zero;
        }
        else
        {
            // In air, maintain more control but still have some inertia
            //var airInertia = (float)Math.Pow(_inertia * 0.8f, deltaTime);
            // _currentMovement = Vector3.Lerp(targetMovement, _currentMovement, airInertia);
            CurrentMovement = Vector3.Lerp(targetMovement, CurrentMovement, _inertia * 0.8f);
        }

        // Ensure we have a valid movement vector (prevents potential NaN issues)
        if (float.IsNaN(CurrentMovement.X) || float.IsNaN(CurrentMovement.Z))
        {
            CurrentMovement = Vector3.Zero;
        }

        return CurrentMovement * deltaTime;
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
            Velocity.Y = _jumpForce * deltaTime;
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
        // Limit the velocity to prevent excessive speed
    }

    private Vector3 CalculateDisplacement(Vector3 horizontalMovement)
    {
        // Combine horizontal movement with vertical velocity for total displacement
        Vector3 horizontalDisplacement = horizontalMovement;
        Vector3 verticalDisplacement = new Vector3(0, Velocity.Y, 0);

        return new Vector3(horizontalDisplacement.X, verticalDisplacement.Y, horizontalDisplacement.Z);
    }

    private void HandleCollisions(Vector3 intendedDisplacement, float deltaTime)
    {
        // Reset grounded state - will be set to true if we detect a ground collision
        IsGrounded = false;

        // First, try to move horizontally (X and Z)
        var horizontalMove = new Vector3(intendedDisplacement.X, 0, intendedDisplacement.Z);
        if (horizontalMove != Vector3.Zero)
        {
            RawPosition.X += horizontalMove.X;
            RawPosition.Z += horizontalMove.Z;

            // Update bounding box and check for collisions
            UpdateBoundingBox();
            Physics.ResolveCollisions();
        }

        // Then, try to move vertically (Y)
        RawPosition.Y += intendedDisplacement.Y;
        UpdateBoundingBox();
        Physics.ResolveCollisions();

        // Check for grounded state by casting a ray slightly below the player
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        const float groundCheckDistance = 0.15f;

        var allCorners = BoundingBox.GetCorners();
        var bottomCorners = new Vector3[4];
        bottomCorners[0] = allCorners[2];
        bottomCorners[1] = allCorners[3];
        bottomCorners[2] = allCorners[6];
        bottomCorners[3] = allCorners[7];

        var boxes = LatibuleGame.GameWorld.GetBoundingBoxes();

        foreach (var box in boxes)
        {
            foreach (var corner in bottomCorners)
            {
                var rayStart = corner + new Vector3(0, 0.05f, 0);
                var rayEnd = rayStart - new Vector3(0, groundCheckDistance, 0);

                if (!AabbHelper.RayIntersectsAabb(rayStart, rayEnd, box, out var hitPoint, out _)) continue;
                IsGrounded = true;

                var distanceToGround = rayStart.Y - hitPoint.Y;
                if (!(distanceToGround < 0.0001f)) return;
                RawPosition.Y = hitPoint.Y + 0.0001f;
                UpdateBoundingBox();

                return;
            }
        }
    }

    public void LeftClickAction()
    {
        Console.WriteLine("left click action");
        // AssetManager.PlaySound(SoundAsset.missing, volume: 0.5f);
    }

    public void RightClickAction()
    {
        Console.WriteLine("right click action");
        // AssetManager.PlaySound(SoundAsset.missing, volume: 0.5f);
    }
}