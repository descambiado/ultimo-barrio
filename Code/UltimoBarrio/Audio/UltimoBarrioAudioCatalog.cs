using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.Audio
{
    public enum AudioEvent
    {
        UIClick,
        UIReject,
        PickupScrap,
        PickupWater,
        PickupMedicine,
        PickupAmmo,
        TraderBuy,
        DoorOpen,
        DoorClose,
        DoorLocked,
        StashOpen,
        WeaponPistolShoot,
        WeaponPistolReload,
        NightSiren,
        RaidStart,
        AmbienceDay,
        AmbienceNight
    }

    [Title("Audio Catalog")]
    [Category("Último Barrio — Audio")]
    [Icon("volume_up")]
    public sealed class UltimoBarrioAudioCatalog : Component
    {
        public static UltimoBarrioAudioCatalog Instance { get; private set; }

        private readonly Dictionary<AudioEvent, string> _soundEvents = new()
        {
            { AudioEvent.UIClick, "ui.button.press" },
            { AudioEvent.UIReject, "ui.button.deny" },
            { AudioEvent.PickupScrap, "pickup.scrap" },
            { AudioEvent.PickupWater, "pickup.water" },
            { AudioEvent.PickupMedicine, "pickup.medicine" },
            { AudioEvent.PickupAmmo, "pickup.ammo" },
            { AudioEvent.TraderBuy, "trader.buy" },
            { AudioEvent.DoorOpen, "door.open" },
            { AudioEvent.DoorClose, "door.close" },
            { AudioEvent.DoorLocked, "door.locked" },
            { AudioEvent.StashOpen, "stash.open" },
            { AudioEvent.WeaponPistolShoot, "weapon.usp.shoot" },
            { AudioEvent.WeaponPistolReload, "weapon.usp.reload" },
            { AudioEvent.NightSiren, "raid.siren" },
            { AudioEvent.RaidStart, "raid.start" }
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
