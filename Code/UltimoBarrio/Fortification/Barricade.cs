using Sandbox;
using System;
using System.Linq;
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

        // Sin override de OnStart a propósito: BarricadeAnchor.ProcessPlace y
        // RestoreBarricade ya fijan MaxHealth/Health explícitamente según el
        // tier colocado (barricade=150, reinforced_barricade_kit=300) justo
        // tras Create<Barricade>(). Un OnStart que hardcodee MaxHealth=150 aquí
        // pisaría ese valor cuando el motor por fin ejecute OnStart (no es
        // síncrono con Create<T>()), bajando de nivel cualquier barricada
        // reforzada en cuanto arrancara.

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
