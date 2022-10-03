using System;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Unit.Actions;

namespace TurnBasedStrategyCourse_godot.Camera
{
  public class CameraController : Spatial
  {
    [Export] private float movementSpeed = 10f;
    [Export] private float rotationSpeed = 1f;

    [Export] private float minZoom = -0.2f;
    [Export] private float maxZoom = 2f;

    [Export] private float actionSpeed = 2.0f;
    [Export] private float actionStoppingDistance = 0.4f;


    private enum CameraState
    {
      World,
      Action
    }

    private Spatial gimbal;
    private Godot.Camera camera;
    private Position3D mount;
    private const float cameraSpeed = Mathf.Pi / 2f;
    private CameraState state;
    private Position3D unitMount;

    public override void _Ready()
    {
      base._Ready();

      gimbal = GetNode<Spatial>("GimbalIn");
      mount = GetNode<Position3D>("GimbalIn/Mount");
      camera = GetNode<Godot.Camera>("GimbalIn/Mount/Camera");

      EventBus.Instance.Connect(nameof(EventBus.ActionStarted), this, nameof(OnActionStarted));
      EventBus.Instance.Connect(nameof(EventBus.ActionCompleted), this, nameof(OnActionCompleted));
    }

    public override void _Process(float delta)
    {
      base._Process(delta);

      switch (state)
      {
        case CameraState.World:
          ProcessMovement(delta);
          break;
        case CameraState.Action:
          ProcessAction(delta);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void ProcessAction(float delta)
    {
      if (camera.Translation.DistanceTo(unitMount.GlobalTransform.origin) < actionStoppingDistance)
      {
        camera.Translation = unitMount.GlobalTransform.origin;
        camera.Rotation = unitMount.GlobalRotation;
      }
      else
      {
        var moveDirection = camera.Translation.DirectionTo(unitMount.GlobalTransform.origin);
        camera.Translation += moveDirection * (movementSpeed * actionSpeed * delta);

        var newTransform =
          camera.Transform.LookingAt(camera.GlobalTransform.origin - (moveDirection + Vector3.Forward), Vector3.Up);
        camera.Transform = camera.Transform.InterpolateWith(newTransform, rotationSpeed * actionSpeed * delta);
      }
    }

    private void ProcessMovement(float delta)
    {
      var leftRight = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
      var forwardBack = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");

      var globalTransform = GlobalTransform;
      globalTransform.origin += (globalTransform.basis.z * forwardBack * (movementSpeed * delta));
      globalTransform.origin += (globalTransform.basis.x * leftRight * (movementSpeed * delta));
      GlobalTransform = globalTransform;

      if (Input.IsActionPressed("rotate_left"))
      {
        RotateY(rotationSpeed * delta);
      }

      if (Input.IsActionPressed("rotate_right"))
      {
        RotateY(-rotationSpeed * delta);
      }

      if (Input.IsActionPressed("zoom_in"))
      {
        gimbal.RotateX(-cameraSpeed * delta);
      }

      if (Input.IsActionPressed("zoom_out"))
      {
        gimbal.RotateX(cameraSpeed * delta);
      }

      var rotation = gimbal.Rotation;
      rotation.x = Mathf.Clamp(gimbal.Rotation.x, -Mathf.Pi / maxZoom, minZoom);
      gimbal.Rotation = rotation;
    }

    private void OnActionStarted(UnitAction action)
    {
      if (!(action is ShootAction shootAction)) return;

      camera.SetAsToplevel(true);

      unitMount = shootAction.Unit.CameraPosition;

      state = CameraState.Action;
    }

    private void OnActionCompleted(UnitAction action)
    {
      camera.SetAsToplevel(false);
      camera.GlobalTransform = mount.GlobalTransform;
      camera.GlobalRotation = mount.GlobalRotation;
      state = CameraState.World;
    }
  }
}