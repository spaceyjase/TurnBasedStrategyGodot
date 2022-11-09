using Godot;

namespace TurnBasedStrategyCourse_godot.Unit.Health;

public class UnitHealth : Resource
{
  [Export] private int maxHealth = 100;

  public int CurrentHealth { get; private set; }
  public int MaxHealth { get; private set; }

  public void Initialise()
  {
    CurrentHealth = (int)Get(nameof(maxHealth));
    MaxHealth = (int)Get(nameof(maxHealth));
  }

  public void TakeDamage(int damage)
  {
    CurrentHealth -= damage;
    if (CurrentHealth < 0) CurrentHealth = 0;
  }
}