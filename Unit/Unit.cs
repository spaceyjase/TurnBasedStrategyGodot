using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Level;
using TurnBasedStrategyCourse_godot.Unit.Actions;
using TurnBasedStrategyCourse_godot.Unit.Stats;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class Unit : Spatial
  {
    [Signal]
    public delegate void OnUnitSelected(Unit selectedUnit);

    [Signal]
    public delegate void OnUnitMoving(Unit selectedUnit, GridPosition oldPosition, GridPosition newPosition);

    [Export] private UnitStats unitStats;

    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationStateMachine;
    private Spatial selectedVisual;
    private bool selected;
    
    public UnitAction CurrentAction { get; set; }
    private UnitAction InitialAction { get; set; }

    public LevelGrid LevelGrid { get; private set; }

    public GridPosition GridPosition { get; private set; }

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

    public float MovementSpeed => unitStats.MovementSpeed;
    public double StoppingDistance => unitStats.StoppingDistance;
    public float RotateSpeed => unitStats.RotateSpeed;
    private int MaxMoveDistance => unitStats.MaxMoveDistance;

    private readonly Dictionary<string, UnitAction> actions = new Dictionary<string, UnitAction>();

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

      foreach (var child in GetChildren())
      {
        if (!(child is UnitAction action)) continue;

        if (actions.Count == 0)
        {
          InitialAction = action;
        }

        actions[action.Name] = action;
      }
      
      CurrentAction = InitialAction;

      TargetPosition = Translation;
    }

    public void Initialise(LevelGrid levelGrid)
    {
      LevelGrid = levelGrid;
      GridPosition = LevelGrid.GetGridPosition(Translation);
      
      unitStats.Initialise();
    }

    public override void _Process(float delta)
    {
      base._Process(delta);
      
      ProcessAction(delta);
    }

    private void ProcessAction(float delta)
    {
      CurrentAction.Execute(delta);

      var newGridPosition = LevelGrid.GetGridPosition(Translation);
      if (newGridPosition == GridPosition) return;

      var oldPosition = GridPosition;
      GridPosition = newGridPosition;
      EmitSignal(nameof(OnUnitMoving), this, oldPosition, GridPosition);
    }

    public void MoveTo(Vector3 direction)
    {
      if (CurrentAction.Name != nameof(IdleAction)) return;
      
      var gridPosition = LevelGrid.GetGridPosition(direction);
      if (!IsValidGridPosition(gridPosition)) return;

      TargetPosition = LevelGrid.GetWorldPosition(gridPosition);
      
      ChangeAction(nameof(MoveAction));
    }

    public void Spin()
    {
      if (CurrentAction.Name != nameof(IdleAction)) return;
      
      ChangeAction(nameof(SpinAction));
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
    
    public IEnumerable<GridPosition> ValidPositions => GetValidGridPosition();
    public Vector3 TargetPosition { get; private set; } = Vector3.Zero;
    public IEnumerable<UnitAction> Actions => actions.Values.Where(x => x != InitialAction);

    public void ChangeAction(string name)
    {
      if (actions.ContainsKey(name) == false)
      {
        GD.PrintErr($"State {name} does not exist!");
        return;
      }
      
      ChangeAction(actions[name]);
    }

    private void ChangeAction(UnitAction newAction)
    {
      if (newAction == null)
      {
        GD.PrintErr("Cannot transition to a null state!");
        return;
      }

      CurrentAction?.OnExit?.Invoke();

      CurrentAction = newAction;
      
      newAction.OnEnter?.Invoke();
    }
    
    private bool IsValidGridPosition(GridPosition position)
    {
      return GetValidGridPosition().Contains(position);
    }

    private IEnumerable<GridPosition> GetValidGridPosition()
    {
      for (var x = -MaxMoveDistance; x <= MaxMoveDistance; ++x)
      {
        for (var z = -MaxMoveDistance; z <= MaxMoveDistance; ++z)
        {
          var offset = new GridPosition(x, z);
          var testPosition = GridPosition + offset;

          if (!LevelGrid.IsValidPosition(testPosition)) continue;
          if (GridPosition == testPosition) continue;
          if (LevelGrid.IsOccupied(testPosition)) continue;

          yield return testPosition;
        }
      }
    }

    public void DoAction(string actionName)
    {
      GD.Print(actionName);
    }
  }
}