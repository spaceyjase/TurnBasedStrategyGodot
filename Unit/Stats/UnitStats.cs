using Godot;
using TurnBasedStrategyCourse_godot.Unit.Health;

namespace TurnBasedStrategyCourse_godot.Unit.Stats
{
  public class UnitStats : Resource
  {
    [Export] private UnitHealth health;
    [Export] private int maxMoveDistance = 4;
    [Export] private int maxShootDistance = 7;
    [Export] private float movementSpeed = 4f;
    [Export] private float rotateSpeed = 15f;
    [Export] private float stoppingDistance = .1f;
    [Export] private int totalActionPoints = 2;

    public float MovementSpeed { get; private set; }
    public float RotateSpeed { get; private set; }
    public float StoppingDistance { get; private set; }
    public int MaxMoveDistance { get; private set; }
    public int MaxShootDistance { get; private set; }
    public int TotalActionPoints { get; private set; }

    public int Health => health.CurrentHealth;
    public int MaxHealth => health.MaxHealth;

    public void TakeDamage(int damage) => health.TakeDamage(damage);

    public void Initialise()
    {
      MovementSpeed = (float)Get(nameof(movementSpeed));
      RotateSpeed = (float)Get(nameof(rotateSpeed));
      StoppingDistance = (float)Get(nameof(stoppingDistance));
      MaxMoveDistance = (int)Get(nameof(maxMoveDistance));
      MaxShootDistance = (int)Get(nameof(maxShootDistance));
      TotalActionPoints = (int)Get(nameof(totalActionPoints));

      health = (UnitHealth)health.Duplicate();
      health.Initialise();
    }
  }
}