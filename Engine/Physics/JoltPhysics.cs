using System.Diagnostics;
using Engine.Core;
using Engine.Core.Types;
using Engine.Utilities;
using JoltPhysicsSharp;
using OpenTK.Windowing.Common;
using static Engine.Core.Logger;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Engine.Physics;

public class JoltPhysics : IDisposable
{
    public const int MaxBodies = 65536;
    public const int MaxBodyPairs = 65536;
    public const int MaxContactConstraints = 65536;
    public const int NumBodyMutexes = 0;

    // Keep a strong reference so callbacks remain valid for the lifetime of this physics instance.
    private readonly TestPhysicsStepListener _testStepListener = new();

    // Keep managed wrappers alive while native physics still references them.
    private readonly List<object> _ownedStepListeners = [];

    public static class Layers
    {
        public static readonly ObjectLayer NonMoving = 0;
        public static readonly ObjectLayer Moving = 1;
    }

    protected static class BroadPhaseLayers
    {
        public static readonly BroadPhaseLayer NonMoving = 0;
        public static readonly BroadPhaseLayer Moving = 1;
    }

    private const double FixedPhysicsStep = 1.0 / 120.0;
    private const double MaxPhysicsFrameTime = 0.25;
    private const int MaxPhysicsStepsPerFrame = 8;

    private const int CollisionStepsPerUpdate = 2;
    private const float PushableBoxMass = 70f;
    private const float PushableBoxFriction = 20f;
    private const float PushableBoxLinearDamping = 0.05f;
    private const float PushableBoxAngularDamping = 0.05f;

    private PhysicsSystemSettings _settings;
    public readonly List<BodyID> Bodies = [];
    public readonly List<Character> Characters = [];
    private double _physicsAccumulator;

    public void SetupJoltPhysics()
    {
#if DEBUG
        Foundation.SetAssertFailureHandler((inExpression, inMessage, inFile, inLine) =>
        {
            var message = inMessage ?? inExpression;
            var outMessage = $"[JoltPhysics] Assertion failure at {inFile}:{inLine}: {message}";
            LogError(outMessage);
            throw new Exception(outMessage);
        });
#endif

        if (!Foundation.Init(false))
        {
            throw new Exception("Failed to initialize JoltPhysics Foundation");
        }

        _settings = new PhysicsSystemSettings()
        {
            MaxBodies = MaxBodies,
            MaxBodyPairs = MaxBodyPairs,
            MaxContactConstraints = MaxContactConstraints,
            NumBodyMutexes = NumBodyMutexes,
        };

        JobSystem = new JobSystemThreadPool();
        SetupCollisionFiltering();
        PhysicsSystem = new(_settings);

        PhysicsSystem.AddStepListener(_testStepListener);
        _ownedStepListeners.Add(_testStepListener);

        // ContactListener
        PhysicsSystem.OnContactValidate += OnContactValidate;
        PhysicsSystem.OnContactAdded += OnContactAdded;
        PhysicsSystem.OnContactPersisted += OnContactPersisted;
        PhysicsSystem.OnContactRemoved += OnContactRemoved;
        // BodyActivationListener
        PhysicsSystem.OnBodyActivated += OnBodyActivated;
        PhysicsSystem.OnBodyDeactivated += OnBodyDeactivated;

        PhysicsSystem.Gravity = new Vector3(0.0f, -9.81f, 0.0f);

        // CreateBox(Vector3.One, new Vector3(0, 5f, 0), Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
        // CreateFloor(10, Layers.NonMoving);

        debugRenderer = new JoltPhysicsDebugRenderer();
    }

    private JoltPhysicsDebugRenderer debugRenderer;

    class TestPhysicsStepListener : PhysicsStepListener
    {
        protected override void OnStep(in PhysicsStepListenerContext context)
        {
            // LogDebug($"Test step listener: {context.DeltaTime}");
        }
    }

    public JobSystem JobSystem { get; set; }
    public PhysicsSystem PhysicsSystem { get; private set; }
    public BodyInterface BodyInterface => PhysicsSystem.BodyInterface;
    public BodyLockInterface BodyLockInterface => PhysicsSystem.BodyLockInterface;

    public void OnUpdateFrame(FrameEventArgs args)
    {
        if (DevConsole.IsOpen) return;
        // TODO: Call this during level load when level loading is implemented
        // Optional step: Before starting the physics simulation you can optimize the broad phase. This improves collision detection performance (it's pointless here because we only have 2 bodies).
        // You should definitely not call this every frame or when e.g. streaming in a new level section as it is an expensive operation.
        // Instead insert all new objects in batches instead of 1 at a time to keep the broad phase efficient.
        // PhysicsSystem.OptimizeBroadPhase();

        double frameTime = args.Time;
        if (double.IsNaN(frameTime) || double.IsInfinity(frameTime) || frameTime < 0.0)
        {
            frameTime = 0.0;
        }

        _physicsAccumulator += Math.Min(frameTime, MaxPhysicsFrameTime);

        int physicsSteps = 0;
        while (_physicsAccumulator >= FixedPhysicsStep && physicsSteps < MaxPhysicsStepsPerFrame)
        {
            // Step the world using a fixed Hz time slice.
            var error = PhysicsSystem.Update((float)FixedPhysicsStep, CollisionStepsPerUpdate, JobSystem);
            Debug.Assert(error == PhysicsUpdateError.None);

            foreach (var character in Characters)
            {
                character.PostSimulation(0.05f);
            }

            _physicsAccumulator -= FixedPhysicsStep;
            physicsSteps++;
        }

        if (physicsSteps == MaxPhysicsStepsPerFrame)
        {
            // Avoid a spiral of death if the simulation falls too far behind.
            _physicsAccumulator = 0.0;
        }
    }

    public void OnRenderFrame(FrameEventArgs args)
    {
        foreach (var bodyTuple in LatibuleEngine.Map.GetBodyIDs())
        {
            var transform = BodyInterface.GetTransformedShape(BodyLockInterface, bodyTuple.Item1);
            DrawTransformedShapeWireframe(transform, new JoltColor(bodyTuple.Item2));
        }
        foreach (var bodyID in Bodies)
        {
            var transform = BodyInterface.GetTransformedShape(BodyLockInterface, bodyID);
            DrawTransformedShapeWireframe(transform, new JoltColor(0xFF00FF));
        }
    }
    
    // if (_ignoreDrawBodies.Contains(bodyID)) continue;

    // var renderer = new ShapeRenderer(
    //     Asseteer.GetShader(EngineShaders.EngineShader.mesh),
    //     new Cube(),
    //     new Transform(
    //         transform.CenterOfMassTransform.Translation.ToOpenTK(),
    //         (transform.Shape.LocalBounds.Min + transform.Shape.LocalBounds.Max * 2f).ToOpenTK(),
    //         transform.ShapeRotation.ToOpenTKEulerDegrees()
    //     ),
    //     Asseteer.GetTexture(EngineTextures.Dev.dev_measuregeneric01),
    //     Vector2.One,
    //     0f
    // );
    // renderer.Render();

    private void DrawTransformedShapeWireframe(TransformedShape transformedShape, JoltColor color)
    {
        if (!EngineStates.EnabledDebugOverlays[DebugOverlayType.BoundingBoxes] || transformedShape.Shape == null!) return;
        debugRenderer.SetCameraPosition(LatibuleEngine.Camera.Position.ToNumerics());
        transformedShape.Shape.Draw(debugRenderer, transformedShape.CenterOfMassTransform, Vector3.One, color, false, true);
        debugRenderer.NextFrame();
    }

    public void RegisterBodyId(BodyID bodyId)
    {
        if (Bodies.Contains(bodyId)) return;

        Bodies.Add(bodyId);
    }

    public void UnregisterBody(BodyID bodyId)
    {
        Bodies.Remove(bodyId);
    }

    public void RegisterCharacter(Character character)
    {
        if (Characters.Contains(character)) return;

        Characters.Add(character);
        Bodies.Add(character.BodyID);
    }

    public void UnregisterCharacter(Character character)
    {
        Characters.Remove(character);
        Bodies.Remove(character.BodyID);
    }

    #region Physics

    protected virtual void SetupCollisionFiltering()
    {
        // We use only 2 layers: one for non-moving objects and one for moving objects
        ObjectLayerPairFilterTable objectLayerPairFilter = new(2);
        objectLayerPairFilter.EnableCollision(Layers.NonMoving, Layers.Moving);
        objectLayerPairFilter.EnableCollision(Layers.Moving, Layers.Moving);

        // We use a 1-to-1 mapping between object layers and broadphase layers
        BroadPhaseLayerInterfaceTable broadPhaseLayerInterface = new(2, 2);
        broadPhaseLayerInterface.MapObjectToBroadPhaseLayer(Layers.NonMoving, BroadPhaseLayers.NonMoving);
        broadPhaseLayerInterface.MapObjectToBroadPhaseLayer(Layers.Moving, BroadPhaseLayers.Moving);

        ObjectVsBroadPhaseLayerFilterTable objectVsBroadPhaseLayerFilter = new(broadPhaseLayerInterface, 2, objectLayerPairFilter, 2);

        _settings.ObjectLayerPairFilter = objectLayerPairFilter;
        _settings.BroadPhaseLayerInterface = broadPhaseLayerInterface;
        _settings.ObjectVsBroadPhaseLayerFilter = objectVsBroadPhaseLayerFilter;
    }

    // protected BodyID CreateFloor(float size, ObjectLayer layer)
    // {
    //     BoxShape shape = new(new Vector3(size, 1.0f, size));
    //     using BodyCreationSettings creationSettings = new(shape, new Vector3(0, -5.0f, 0.0f), Quaternion.Identity, MotionType.Static, layer);
    //     BodyID body = BodyInterface.CreateAndAddBody(creationSettings, Activation.DontActivate);
    //     _bodies.Add(body);
    //     // _ignoreDrawBodies.Add(body);
    //     return body;
    // }

    // protected BodyID CreateBox(in Vector3 halfExtent,
    //     in Vector3 position,
    //     in Quaternion rotation,
    //     MotionType motionType,
    //     ObjectLayer layer,
    //     Activation activation = Activation.Activate)
    // {
    //     BoxShape shape = new(halfExtent);
    //     using BodyCreationSettings creationSettings = new(shape, position, rotation, motionType, layer);
    //
    //     if (motionType == MotionType.Dynamic)
    //     {
    //         // Keep boxes light and low-friction so the player can push them around easily.
    //         creationSettings.OverrideMassProperties = OverrideMassProperties.CalculateInertia;
    //         var massProperties = creationSettings.MassPropertiesOverride;
    //         massProperties.Mass = PushableBoxMass;
    //         creationSettings.MassPropertiesOverride = massProperties;
    //
    //         creationSettings.Friction = PushableBoxFriction;
    //         creationSettings.LinearDamping = PushableBoxLinearDamping;
    //         creationSettings.AngularDamping = PushableBoxAngularDamping;
    //         creationSettings.MotionQuality = MotionQuality.LinearCast;
    //     }
    //
    //     BodyID body = BodyInterface.CreateAndAddBody(creationSettings, activation);
    //     _bodies.Add(body);
    //     return body;
    // }

    // protected BodyID CreateSphere(float radius,
    //     in Vector3 position,
    //     in Quaternion rotation,
    //     MotionType motionType,
    //     ObjectLayer layer,
    //     Activation activation = Activation.Activate)
    // {
    //     SphereShape shape = new(radius);
    //     using BodyCreationSettings creationSettings = new(shape, position, rotation, motionType, layer);
    //     BodyID body = BodyInterface.CreateAndAddBody(creationSettings, activation);
    //     _bodies.Add(body);
    //     return body;
    // }

    public struct VehicleSettings
    {
        public Vector3 Position = new Vector3(0, 2, 0);
        public bool UseCastSphere = true;
        public float WheelRadius = 0.3f;
        public float WheelWidth = 0.1f;
        public float HalfVehicleLength = 2.0f;
        public float HalfVehicleWidth = 0.9f;
        public float HalfVehicleHeight = 0.2f;
        public float WheelOffsetHorizontal = 1.4f;
        public float WheelOffsetVertical = 0.18f;
        public float SuspensionMinLength = 0.3f;
        public float SuspensionMaxLength = 0.5f;
        public float MaxSteeringAngle = MathUtil.DegreesToRadians(30);
        public bool FourWheelDrive = false;
        public float FrontBackLimitedSlipRatio = 1.4f;
        public float LeftRightLimitedSlipRatio = 1.4f;
        public bool AntiRollbar = true;

        public VehicleSettings()
        {
        }
    }

    protected VehicleConstraint AddVehicle(in VehicleSettings settings)
    {
        const int FL_WHEEL = 0;
        const int FR_WHEEL = 1;
        const int BL_WHEEL = 2;
        const int BR_WHEEL = 3;

        // Create vehicle body
        Shape car_shape = new OffsetCenterOfMassShapeSettings(new Vector3(0, -settings.HalfVehicleHeight, 0), new BoxShape(new Vector3(settings.HalfVehicleWidth, settings.HalfVehicleHeight, settings.HalfVehicleLength))).Create();
        using BodyCreationSettings car_body_settings = new(car_shape, settings.Position, Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
        car_body_settings.OverrideMassProperties = OverrideMassProperties.CalculateInertia;
        var massProperties = car_body_settings.MassPropertiesOverride;
        massProperties.Mass = 1500.0f;
        car_body_settings.MassPropertiesOverride = massProperties;
        Body car_body = BodyInterface.CreateBody(car_body_settings);
        BodyInterface.AddBody(car_body, Activation.Activate);

        // Create vehicle constraint
        VehicleConstraintSettings vehicle = new()
        {
            DrawConstraintSize = 0.1f,
            MaxPitchRollAngle = MathUtil.DegreesToRadians(60.0f)
        };

        // Wheels
        WheelSettingsWV fl = new()
        {
            Position = new Vector3(settings.HalfVehicleWidth, -settings.WheelOffsetVertical, settings.WheelOffsetHorizontal),
            MaxSteerAngle = settings.MaxSteeringAngle,
            MaxHandBrakeTorque = 0.0f // Front wheel doesn't have hand brake
        };


        WheelSettingsWV fr = new()
        {
            Position = new Vector3(-settings.HalfVehicleWidth, -settings.WheelOffsetVertical, settings.WheelOffsetHorizontal),
            MaxSteerAngle = settings.MaxSteeringAngle,
            MaxHandBrakeTorque = 0.0f // Front wheel doesn't have hand brake
        };

        WheelSettingsWV bl = new();
        bl.Position = new Vector3(settings.HalfVehicleWidth, -settings.WheelOffsetVertical, -settings.WheelOffsetHorizontal);
        bl.MaxSteerAngle = 0.0f;

        WheelSettingsWV br = new()
        {
            Position = new Vector3(-settings.HalfVehicleWidth, -settings.WheelOffsetVertical, -settings.WheelOffsetHorizontal),
            MaxSteerAngle = 0.0f
        };

        vehicle.Wheels = new WheelSettings[4];
        vehicle.Wheels[FL_WHEEL] = fl;
        vehicle.Wheels[FR_WHEEL] = fr;
        vehicle.Wheels[BL_WHEEL] = bl;
        vehicle.Wheels[BR_WHEEL] = br;

        foreach (WheelSettings w in vehicle.Wheels)
        {
            w.Radius = settings.WheelRadius;
            w.Width = settings.WheelWidth;
            w.SuspensionMinLength = settings.SuspensionMinLength;
            w.SuspensionMaxLength = settings.SuspensionMaxLength;
        }

        WheeledVehicleControllerSettings controller = new();
        vehicle.Controller = controller;

        // Differential
        controller.DifferentialsCount = settings.FourWheelDrive ? 2 : 1;

        controller.SetDifferential(0, new VehicleDifferentialSettings()
        {
            LeftWheel = FL_WHEEL,
            RightWheel = FR_WHEEL,
            LimitedSlipRatio = settings.LeftRightLimitedSlipRatio,
            EngineTorqueRatio = settings.FourWheelDrive ? 0.5f : 1.0f
        });

        controller.DifferentialLimitedSlipRatio = settings.FrontBackLimitedSlipRatio;
        if (settings.FourWheelDrive)
        {
            controller.SetDifferential(1, new VehicleDifferentialSettings()
            {
                LeftWheel = BL_WHEEL,
                RightWheel = BR_WHEEL,
                LimitedSlipRatio = settings.LeftRightLimitedSlipRatio,
                EngineTorqueRatio = 0.5f
            });
        }

        // Anti rollbars
        if (settings.AntiRollbar)
        {
            vehicle.AntiRollBars = new VehicleAntiRollBar[2];
            vehicle.AntiRollBars[0].LeftWheel = FL_WHEEL;
            vehicle.AntiRollBars[0].RightWheel = FR_WHEEL;
            vehicle.AntiRollBars[1].LeftWheel = BL_WHEEL;
            vehicle.AntiRollBars[1].RightWheel = BR_WHEEL;
        }

        // Create the constraint
        VehicleConstraint constraint = new(car_body, vehicle);

        // Create collision tester
        VehicleCollisionTester tester;
        if (settings.UseCastSphere)
            tester = new VehicleCollisionTesterCastSphere(Layers.Moving, 0.5f * settings.WheelWidth);
        else
            tester = new VehicleCollisionTesterRay(Layers.Moving);
        constraint.SetVehicleCollisionTester(tester);

        // Add to the world
        PhysicsSystem.AddConstraint(constraint);
        PhysicsSystem.AddStepListener(constraint);
        _ownedStepListeners.Add(constraint);

        return constraint;
    }

    protected virtual ValidateResult OnContactValidate(PhysicsSystem system, in Body body1, in Body body2, RVector3 baseOffset, in CollideShapeResult collisionResult)
    {
        // LogDebug("Contact validate callback");

        // Allows you to ignore a contact before it is created (using layers to not make objects collide is cheaper!)
        return ValidateResult.AcceptAllContactsForThisBodyPair;
    }

    protected virtual void OnContactAdded(PhysicsSystem system, in Body body1, in Body body2, in ContactManifold manifold, ref ContactSettings settings)
    {
        LogDebug($"A contact was added between body {body1.ID} and body {body2.ID}");
    }

    protected virtual void OnContactPersisted(PhysicsSystem system, in Body body1, in Body body2, in ContactManifold manifold, ref ContactSettings settings)
    {
        // Override the restitution to 0.5
        settings.CombinedRestitution = 0.5f;
        // LogDebug("A contact was persisted");
    }

    protected virtual void OnContactRemoved(PhysicsSystem system, ref SubShapeIDPair subShapePair)
    {
        LogDebug($"A contact was removed between body {subShapePair.Body1ID} and body {subShapePair.Body2ID}");
    }

    protected virtual void OnBodyActivated(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData)
    {
        LogDebug($"A body woke up with ID {bodyID}");
    }

    protected virtual void OnBodyDeactivated(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData)
    {
        LogDebug($"A body went to sleep with ID {bodyID}");
    }

    #endregion

    public void Dispose()
    {
        foreach (BodyID bodyID in Bodies)
        {
            BodyInterface.RemoveAndDestroyBody(bodyID);
        }

        Bodies.Clear();

        JobSystem.Dispose();
        PhysicsSystem.Dispose();
        _ownedStepListeners.Clear();

        Foundation.Shutdown();
    }
}