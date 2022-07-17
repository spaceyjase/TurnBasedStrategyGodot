using Godot;

namespace TurnBasedStrategyCourse_godot.Extensions
{
  public static class Vector3Extensions
  {
    private static float Lerp(float firstFloat, float secondFloat, float by)
    {
         return firstFloat * (1 - by) + secondFloat * by;
    }

    public static Vector3 Lerp(Vector3 firstVector, Vector3 secondVector, float by)
    {
        var retX = Lerp(firstVector.x, secondVector.x, by);
        var retY = Lerp(firstVector.y, secondVector.y, by);
        var retZ = Lerp(firstVector.z, secondVector.z, by);
        return new Vector3(retX, retY, retZ);
    }
  }
}