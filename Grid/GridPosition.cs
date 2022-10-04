using System;

namespace TurnBasedStrategyCourse_godot.Grid
{
  public class GridPosition : Godot.Object, IEquatable<GridPosition>
  {
    public GridPosition()
    {
      X = 0;
      Z = 0;
    }

    public GridPosition(int x, int z)
    {
      X = x;
      Z = z;
    }

    public int X { get; }
    public int Z { get; }

    public bool Equals(GridPosition other) => other != null && X == other.X && Z == other.Z;

    public static GridPosition operator +(GridPosition a, GridPosition b)
    {
      return new GridPosition(a.X + b.X, a.Z + b.Z);
    }

    public static GridPosition operator -(GridPosition a, GridPosition b)
    {
      return new GridPosition(a.X - b.X, a.Z - b.Z);
    }

    public static bool operator ==(GridPosition left, GridPosition right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(GridPosition left, GridPosition right)
    {
      return !Equals(left, right);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (X * 397) ^ Z;
      }
    }

    public override string ToString() => $"{X}, {Z}";

    public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
  }
}