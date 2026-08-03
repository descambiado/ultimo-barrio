using Sandbox;
using System;

namespace UltimoBarrio.UI
{
    public static class PlayerMessageService
    {
        public static string GetApartmentClaimMessage(Apartments.ApartmentClaimResult result)
        {
            if (result.Succeeded)
            {
                return "Ahora este piso es tuyo";
            }

            switch (result.Failure)
            {
                case Apartments.ApartmentClaimFailure.ApartmentUnavailable:
                    return "Este piso ya tiene dueño";
                case Apartments.ApartmentClaimFailure.PlayerAlreadyOwnsApartment:
                    return "Ya tienes una vivienda";
                case Apartments.ApartmentClaimFailure.OutOfRange:
                    return "No estás lo bastante cerca";
                default:
                    return "Error interno";
            }
        }
        
        public static string GetAvailableApartmentPrompt()
        {
            return "Este piso está disponible\nPulsa E para reclamarlo";
        }
        
        public static string GetLockedStashPrompt()
        {
            return "No puedes abrir este alijo";
        }
    }
}
