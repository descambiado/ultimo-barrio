using Sandbox;
using System.Linq;
using UltimoBarrio.Apartments;
using UltimoBarrio.Core;

namespace UltimoBarrio.Building
{
    /// <summary>
    /// Autorización de construcción: solo el propietario del apartamento puede
    /// colocar/reparar/mejorar/desmontar dentro de su propio BuildVolume.
    /// Centraliza la comprobación en un único sitio en vez de repetirla en
    /// cada anchor -- mismo motivo por el que ApartmentDoorPolicy/StashComponent
    /// ya centralizan la suya.
    /// </summary>
    public static class StructureAuthorization
    {
        public static bool CanBuild( Scene scene, GameObject player, string apartmentId )
        {
            if ( player is null || string.IsNullOrEmpty( apartmentId ) )
                return false;

            var apartment = scene.GetAllComponents<ApartmentComponent>()
                .FirstOrDefault( a => a.ApartmentId == apartmentId );
            if ( apartment is null || apartment.ClaimState == ApartmentClaimState.Unclaimed )
                return false;

            var identity = PlayerIdentity.FromGameObject( player );
            return identity.IsValid && apartment.OwnerId == identity.CanonicalId;
        }
    }
}
