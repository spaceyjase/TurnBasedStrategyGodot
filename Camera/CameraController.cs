using Godot;

namespace TurnBasedStrategyCourse_godot.Camera
{
  public class CameraController : Spatial
  {
    [Export] private float movementSpeed = 10f;
    
    public override void _Process(float delta)
    {
      base._Process(delta);
      
      	var leftRight = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
      	var forwardBack = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");

        var movement = new Vector3(leftRight, 0f, forwardBack).Normalized();

        var globalTransform = GlobalTransform;
        globalTransform.origin += movement * movementSpeed * delta;
        GlobalTransform = globalTransform;
    }
  }
}