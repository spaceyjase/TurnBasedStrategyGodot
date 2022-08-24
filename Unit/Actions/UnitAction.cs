using System;
using Godot;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public abstract class UnitAction : Node
  {
    protected Unit unit;
    
    public Action OnEnter { get; set; }
    public Action OnExit { get; set; }

    public override void _Ready()
    {
      base._Ready();

      unit = Owner as Unit;
    }

    public abstract void Execute(float delta);

    public string ActionName => Name;

    public virtual int ActionPointCost => 1;
  }
}