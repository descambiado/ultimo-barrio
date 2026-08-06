using Sandbox;
using System.Linq;

namespace UltimoBarrio.Building
{
    /// <summary>
    /// Región donde el propietario de un apartamento puede colocar estructuras
    /// libremente (no solo en anchors fijos). El límite real es el
    /// BoxCollider del propio GameObject (marcado trigger, IsTrigger=true) --
    /// esto reutiliza la misma comprobación de físicas del motor en vez de
    /// reimplementar una prueba de punto-en-caja a mano.
    /// </summary>
    [Title( "Build Volume" )]
    [Category( "Último Barrio — Building" )]
    [Icon( "check_box_outline_blank" )]
    public sealed class BuildVolume : Component
    {
        [Property] public string ApartmentId { get; set; } = string.Empty;

        private BoxCollider _bounds;

        protected override void OnStart()
        {
            _bounds = Components.Get<BoxCollider>();
        }

        /// <summary>Comprueba si un punto del mundo cae dentro del volumen.</summary>
        public bool Contains( Vector3 worldPos )
        {
            _bounds ??= Components.Get<BoxCollider>();
            if ( _bounds is null )
                return false;

            var localPos = GameObject.WorldTransform.PointToLocal( worldPos );
            var half = _bounds.Scale * 0.5f;
            var center = _bounds.Center;

            return localPos.x >= center.x - half.x && localPos.x <= center.x + half.x
                && localPos.y >= center.y - half.y && localPos.y <= center.y + half.y
                && localPos.z >= center.z - half.z && localPos.z <= center.z + half.z;
        }

        public static BuildVolume FindForApartment( Scene scene, string apartmentId )
        {
            return scene.GetAllComponents<BuildVolume>().FirstOrDefault( v => v.ApartmentId == apartmentId );
        }

        /// <summary>Encuentra el BuildVolume (si alguno) que contiene el punto dado.</summary>
        public static BuildVolume FindContaining( Scene scene, Vector3 worldPos )
        {
            return scene.GetAllComponents<BuildVolume>().FirstOrDefault( v => v.Contains( worldPos ) );
        }
    }
}
