using System;
using Godot;
using TurnBasedStrategyCourse_godot.Events;

namespace TurnBasedStrategyCourse_godot.Camera
{
  public class CameraShake : Godot.Camera
  {
    // The maximum offset applied to the camera in units.
    [Export] private float maxAmplitude = 0.3f;

    // We made our noise generator in the editor and saved it as a text resource
    // file. This allows us to edit it in the Inspector while the game runs,
    // with live reloading.
    [Export] private Resource noiseResource;
  
    private OpenSimplexNoise noise;
    private float shakeIntensity;

    // We turn processing on and off through this property's setter function.
    private float ShakeIntensity
    {
      get => shakeIntensity;
      set
      {
        shakeIntensity = Mathf.Clamp(value, 0.0f, 1.0f);
        SetProcess(!Mathf.IsZeroApprox(shakeIntensity));
      }
    }

    public override void _Ready()
    {
      base._Ready();
      noise = ResourceLoader.Load(noiseResource.ResourcePath) as OpenSimplexNoise;
      if (noise == null) throw new Exception("Failed to load noise resource");
    
      //noise = noiseResource.Instance<OpenSimplexNoise>();
      noise.Seed = (int)GD.Randi();

      EventBus.Instance.Connect(nameof(EventBus.CameraShake), this, nameof(OnCameraShake));
    }

    private void OnCameraShake(float intensity)
    {
      ShakeIntensity = intensity;
    }

    public override void _Process(float delta)
    {
      base._Process(delta);
      // Every frame, we lower the intensity while the effect is active.
      // When the intensity reaches zero, the setter turns off processing.
      ShakeIntensity -= delta;
      // We use the time value to move along the noise generator's axes.
      // Using time gives us a smooth and continuous effect.
      var timeElapsed = OS.GetTicksMsec();
      // We calculate a direction by getting two values from the noise generator.
      var randomDirection = new Vector2(noise.GetNoise2d(timeElapsed, 0), noise.GetNoise2d(0, timeElapsed)).Normalized();
      // And we apply the shake offset using the current intensity.
      var amplitude = maxAmplitude * Mathf.Pow(ShakeIntensity, 2);
      // Those properties offset the camera's viewport rather than moving the node in the world.
      HOffset = randomDirection.x * amplitude;
      VOffset = randomDirection.y * amplitude;
    }
  }
}