using Sandbox;
using System;
using UltimoBarrio.Core;
using UltimoBarrio.Inventory;

namespace UltimoBarrio
{
    public class HeldConsumable : Component
    {
        [Property] public string ConsumableType { get; set; } = ""agua""; // agua, medicina, chatarra
        [Property] public int HealAmount { get; set; } = 25;
        [Property] public string SoundEffect { get; set; } = ""sounds/drink.sound"";
        
        [Property] public float Cooldown { get; set; } = 1.0f;
        private TimeSince timeSinceLastUse;

        protected override void OnUpdate()
        {
            if (IsProxy) return; // Only process input on owner/host

            if (Input.Pressed(""attack1"") && timeSinceLastUse > Cooldown)
            {
                UseConsumable();
                timeSinceLastUse = 0;
            }
            else if (Input.Pressed(""attack2"") && timeSinceLastUse > Cooldown)
            {
                DropConsumable();
                timeSinceLastUse = 0;
            }
        }

        private void UseConsumable()
        {
            var inv = GameObject.Root.Components.GetInDescendantsOrSelf<UltimoBarrioPlayerInventory>();
            if (inv != null)
            {
                if (inv.TryRemove(ConsumableType, 1))
                {
                    if (ConsumableType == ""agua"")
                    {
                        Log.Info(""Beber agua!"");
                    }
                    else if (ConsumableType == ""medicina"")
                    {
                        Log.Info($""Curarse {HealAmount} HP!"");
                    }
                    else if (ConsumableType == ""chatarra"")
                    {
                        Log.Info(""No puedes consumir chatarra. Solo soltarla."");
                        inv.TryAdd(ConsumableType, 1);
                        return;
                    }
                    
                    Sound.Play(SoundEffect, GameObject.WorldPosition);
                }
            }
        }

        private void DropConsumable()
        {
            var inv = GameObject.Root.Components.GetInDescendantsOrSelf<InventoryComponent>();
            if (inv != null)
            {
                inv.RequestDrop(ConsumableType, 1);
            }
        }
    }
}
