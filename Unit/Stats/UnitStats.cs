using Godot;

namespace TurnBasedStrategyCourse_godot.Unit.Stats
{
  public class UnitStats : Resource
  {
    [Export] private float movementSpeed = 4f;
    [Export] private float rotateSpeed = 15f;
    [Export] private float stoppingDistance = .1f;
    [Export] private int maxMoveDistance = 4;

    public float MovementSpeed { get; private set; }
    public float RotateSpeed { get; private set; }
    public float StoppingDistance { get; private set; }
    public int MaxMoveDistance { get; private set; }
    
    public void Initialise()
    {
      MovementSpeed = (float)Get(nameof(movementSpeed));
      RotateSpeed = (float)Get(nameof(rotateSpeed));
      StoppingDistance = (float)Get(nameof(stoppingDistance));
      MaxMoveDistance = (int)Get(nameof(maxMoveDistance));
    }
  }
}