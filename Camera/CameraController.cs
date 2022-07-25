using Godot;

namespace TurnBasedStrategyCourse_godot.Camera
{
  public class CameraController : Spatial
  {
    [Export] private float movementSpeed = 10f;
    [Export] private float rotationSpeed = 1f;
    [Export] private float minZoom = 0.1f;
    [Export] private float maxZoom = 2f;
    [Export] private float zoomSpeed = 0.1f;

    private float zoom = 0.5f;

    public override void _Input(InputEvent @event)
    {
      base._Input(@event);

      if (@event.IsActionPressed("zoom_in"))
      {
        zoom -= zoomSpeed;
      }

      if (@event.IsActionPressed("zoom_out"))
      {
        zoom += zoomSpeed;
      }
    }

    public override void _Process(float delta)
    {
      base._Process(delta);

      zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
      Scale = new Vector3(1, 1, 1) * zoom;

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
    }
  }
}