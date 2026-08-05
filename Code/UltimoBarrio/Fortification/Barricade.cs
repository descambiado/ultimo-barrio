using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Fortification
{
    /// <summary>
    /// Barricada colocada sobre un BarricadeAnchor. Bloquea el paso y tiene
    /// salud propia; al destruirse desaparece (los materiales no se devuelven).
    /// </summary>
    [Title( "Barricade" )]
    [Category( "Último Barrio — Fortification" )]
    [Icon( "construction" )]
    public sealed class Barricade : DestructibleStructure
    {
        [Property] public string ApartmentId { get; set; } = string.Empty;
        [Property] public string AnchorId { get; set; } = string.Empty;

        protected override void OnStart()
        {
            MaxHealth = 150f;
            base.OnStart();
        }

        protected override void OnStructureDestroyed()
        {
            // Notificar al anchor para que quede libre.
            var anchor = Scene.GetAllComponents<BarricadeAnchor>()
                .FirstOrDefault( a => a.AnchorId == AnchorId && a.ApartmentId == ApartmentId );
            if ( anchor is not null )
                anchor.OnBarricadeDestroyed();

            GameObject.Destroy();
        }
    }
}
