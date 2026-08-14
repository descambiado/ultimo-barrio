using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.Audio
{
    /// <summary>
    /// Eventos de audio del banco VERIFICADO del proyecto (Assets/sounds/content/**).
    /// Cada evento mapea a un .sound real; reproducir un evento con sonido inexistente
    /// es imposible por construcción (los paths salen del banco, no inventados).
    /// </summary>
    public enum AudioEvent
    {
        EnemyAlert,
        EnemyAttack,
        EnemyDeath,
        EnemyHurt,
        BarricadeImpact,
        DoorImpact,
        Repair,
        BulletImpactFlesh,
        MeleeImpactFlesh,
        CrowbarImpact,
        CrowbarSwing,
        KnifeSwing,
        ShotgunFire,
        ShotgunReload,
        ShotgunReloadStart,
        UspDeploy,
        UspDry,
        UspFire,
        UspReloadMagIn,
        UspReloadMagOut
    }

    [Title("Audio Catalog")]
    [Category("Último Barrio — Audio")]
    [Icon("volume_up")]
    public sealed class UltimoBarrioAudioCatalog : Component
    {
        public static UltimoBarrioAudioCatalog Instance { get; private set; }

        private readonly Dictionary<AudioEvent, string> _soundEvents = new()
        {
            { AudioEvent.EnemyAlert, "sounds/content/enemies/enemy_alert.sound" },
            { AudioEvent.EnemyAttack, "sounds/content/enemies/enemy_attack.sound" },
            { AudioEvent.EnemyDeath, "sounds/content/enemies/enemy_death.sound" },
            { AudioEvent.EnemyHurt, "sounds/content/enemies/enemy_hurt.sound" },
            { AudioEvent.BarricadeImpact, "sounds/content/fortification/barricade_impact.sound" },
            { AudioEvent.DoorImpact, "sounds/content/fortification/door_impact.sound" },
            { AudioEvent.Repair, "sounds/content/fortification/repair.sound" },
            { AudioEvent.BulletImpactFlesh, "sounds/content/impacts/bullet_impact_flesh.sound" },
            { AudioEvent.MeleeImpactFlesh, "sounds/content/impacts/melee_impact_flesh.sound" },
            { AudioEvent.CrowbarImpact, "sounds/content/weapons/crowbar_impact.sound" },
            { AudioEvent.CrowbarSwing, "sounds/content/weapons/crowbar_swing.sound" },
            { AudioEvent.KnifeSwing, "sounds/content/weapons/knife_swing.sound" },
            { AudioEvent.ShotgunFire, "sounds/content/weapons/shotgun_fire.sound" },
            { AudioEvent.ShotgunReload, "sounds/content/weapons/shotgun_reload.sound" },
            { AudioEvent.ShotgunReloadStart, "sounds/content/weapons/shotgun_reload_start.sound" },
            { AudioEvent.UspDeploy, "sounds/content/weapons/usp_deploy.sound" },
            { AudioEvent.UspDry, "sounds/content/weapons/usp_dry.sound" },
            { AudioEvent.UspFire, "sounds/content/weapons/usp_fire.sound" },
            { AudioEvent.UspReloadMagIn, "sounds/content/weapons/usp_reload_magin.sound" },
            { AudioEvent.UspReloadMagOut, "sounds/content/weapons/usp_reload_magout.sound" }
        };

        protected override void OnStart()
        {
            Instance = this;
        }

        public void PlayEvent(AudioEvent evt, Vector3? position = null)
        {
            if (_soundEvents.TryGetValue(evt, out var soundName))
            {
                if (position.HasValue)
                {
                    Sound.Play(soundName, position.Value);
                }
                else
                {
                    Sound.Play(soundName);
                }
            }
        }
    }
}
