using Godot;

namespace TurnBasedStrategyCourse_godot.Camera
{
  public class CameraController : Spatial
  {
    [Export] private float movementSpeed = 10f;
    [Export] private float rotationSpeed = 1f;

    [Export] private float minZoom = -0.2f;
    [Export] private float maxZoom = 2f;

    private const float cameraSpeed = Mathf.Pi / 2f;

    private Spatial gimbal;

    public override void _Ready()
    {
      base._Ready();

      gimbal = GetNode<Spatial>("GimbalIn");
    }

    public override void _Process(float delta)
    {
      base._Process(delta);

      var leftRight = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
      var forwardBack = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");

      var movement = new Vector3(leftRight, 0f, forwardBack).Normalized();

      var globalTransform = GlobalTransform;
      globalTransform.origin += movement * movementSpeed * delta;
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
  }
}