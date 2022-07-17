using Godot;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class Unit : Spatial
  {
    [Export] private float movementSpeed = 4f;
    [Export] private float stoppingDistance = .1f;

    private Vector3 targetPosition = Vector3.Zero;
    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationStateMachine;
    
    public override void _Ready()
    {
      animationTree = GetNode<AnimationTree>("AnimationTree");
      animationStateMachine = animationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
    }

    public override void _Process(float delta)
    {
      base._Process(delta);

      Move(delta);
    }

    private void Move(float delta)
    {
      if (Translation.DistanceTo(targetPosition) > stoppingDistance)
      {
        var moveDirection = (targetPosition - Translation).Normalized();
        Translation += moveDirection * (movementSpeed * delta);
        
        animationStateMachine.Travel(UnitAnimations.Running);
      }
      else
      {
        animationStateMachine.Travel(UnitAnimations.Idle);
      }
    }

    private void SetMovementDirection(Vector3 direction)
    {
      targetPosition = direction;
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_Ground_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
      if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed &&
          eventMouseButton.ButtonIndex == 1)
      {
        SetMovementDirection(position);
      }
    }
  }
}