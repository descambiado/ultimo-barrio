using Sandbox;
using System;

namespace UltimoBarrio.Players
{
    [GameResource("Movement Profile", "movement", "Defines speed and limits for movement states.", Icon = "directions_run")]
    public class MovementProfile : GameResource
    {
        [Category("Speeds")] public float WalkSpeed { get; set; } = 130f;
        [Category("Speeds")] public float RunSpeed { get; set; } = 280f;
        [Category("Speeds")] public float DuckedSpeed { get; set; } = 70f;

        [Category("Dynamics")] public float AccelerationTime { get; set; } = 0.15f;
        [Category("Dynamics")] public float DecelerationTime { get; set; } = 0.1f;

        [Category("Stamina")] public float MaxStamina { get; set; } = 100f;
        [Category("Stamina")] public float StaminaDrainRate { get; set; } = 20f;
        [Category("Stamina")] public float StaminaRegenRate { get; set; } = 15f;
        [Category("Stamina")] public float JumpStaminaCost { get; set; } = 15f;

        [Category("Camera Bob")] public float BobSpeedWalk { get; set; } = 12f;
        [Category("Camera Bob")] public float BobIntensityWalk { get; set; } = 1.5f;
        [Category("Camera Bob")] public float BobSpeedRun { get; set; } = 18f;
        [Category("Camera Bob")] public float BobIntensityRun { get; set; } = 3f;
        [Category("Camera Bob")] public float BobSpeedDucked { get; set; } = 6f;
        [Category("Camera Bob")] public float BobIntensityDucked { get; set; } = 0.8f;

        [Category("Lean")] public float LeanAngle { get; set; } = 15f;
        [Category("Lean")] public float LeanSpeed { get; set; } = 8f;
        [Category("Lean")] public float LeanOffset { get; set; } = 15f;

        [Category("Sway")] public float WeaponSwayAmount { get; set; } = 1.5f;
        [Category("Sway")] public float WeaponSwaySpeed { get; set; } = 4f;

        [Category("Landing")] public float LandingCameraDip { get; set; } = 5f;
        [Category("Landing")] public float LandingRecoverSpeed { get; set; } = 10f;

        [Category("Weight")] public float MaxWeight { get; set; } = 40f;
        [Category("Weight")] public float MaxWeightSpeedPenalty { get; set; } = 0.3f;
    }
}
