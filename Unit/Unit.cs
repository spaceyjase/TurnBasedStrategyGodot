using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Level;
using TurnBasedStrategyCourse_godot.Unit.Actions;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class Unit : Spatial
  {
    [Signal]
    public delegate void OnUnitSelected(Unit selectedUnit);

    [Signal]
    public delegate void OnUnitMoving(Unit selectedUnit, GridPosition oldPosition, GridPosition newPosition);

    [Export] private NodePath levelGridNodePath;

    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationStateMachine;
    private Spatial selectedVisual;
    private bool selected;
    private LevelGrid levelGrid;
    private GridPosition gridPosition;

    public LevelGrid LevelGrid => levelGrid;
    public GridPosition GridPosition => gridPosition;

    private MoveAction moveAction;

    public bool Selected
    {
      get => selected;
      set
      {
        selected = value;
        if (selected)
        {
          selectedVisual.Show();
        }
        else
        {
          selectedVisual.Hide();
        }
      }
    }

    public void SetAnimation(string animationName)
    {
      animationStateMachine.Travel(animationName);
    }

    public override void _Ready()
    {
      animationTree = GetNode<AnimationTree>("AnimationTree");
      animationStateMachine = animationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
      selectedVisual = GetNode<Spatial>("SelectedVisual");
      selectedVisual.Hide();

      levelGrid = GetNode<LevelGrid>(levelGridNodePath);
      gridPosition = levelGrid.GetGridPosition(Translation);

      moveAction = GetNode<MoveAction>(nameof(MoveAction));
    }

    public override void _Process(float delta)
    {
      base._Process(delta);

      Move(delta);
    }

    private void Move(float delta)
    {
      moveAction.Move(delta);

      var newGridPosition = levelGrid.GetGridPosition(Translation);
      if (newGridPosition == gridPosition) return;

      EmitSignal(nameof(OnUnitMoving), this, gridPosition, newGridPosition);
      gridPosition = newGridPosition;
    }

    public void SetMovementDirection(Vector3 direction)
    {
      moveAction.SetMovementTarget(direction);
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_SelectorArea_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal,
      int shape_idx)
    {
      if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed &&
          eventMouseButton.ButtonIndex == 1)
      {
        EmitSignal(nameof(OnUnitSelected), this);
      }
    }
  }
}