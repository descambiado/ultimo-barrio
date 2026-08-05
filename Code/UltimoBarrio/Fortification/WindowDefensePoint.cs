using Sandbox;

namespace UltimoBarrio.Fortification
{
    /// <summary>
    /// Punto de defensa de ventana: marca dónde el jugador puede asomarse y
    /// disparar en cobertura. ApartmentFortification detecta hijos cuyo nombre
    /// contiene "Window" y les adjunta DestructibleStructure automáticamente.
    /// </summary>
    [Title( "Window Defense Point" )]
    [Category( "Último Barrio — Fortification" )]
    [Icon( "window" )]
    public sealed class WindowDefensePoint : Component
    {
        [Property] public string ApartmentId { get; set; } = string.Empty;

        /// <summary>True = posición desde la que se puede cubrir la entrada.</summary>
        [Property] public bool IsCoverPosition { get; set; } = true;

        protected override void OnStart()
        {
            // Asegurar el nombre que ApartmentFortification busca.
            if ( !GameObject.Name.Contains( "Window", System.StringComparison.OrdinalIgnoreCase ) )
                GameObject.Name = $"Window {ApartmentId}";
        }
    }
}
