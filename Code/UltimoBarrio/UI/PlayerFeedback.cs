using System;

namespace UltimoBarrio.UI
{
    /// <summary>
    /// Feed de feedback transitorio del HUD (pickup, compra, crafteo...).
    /// Los componentes de dominio emiten mensajes; el HUD los muestra sin
    /// que el dominio conozca la UI.
    /// </summary>
    public static class PlayerFeedback
    {
        public static event Action<string> OnFeedback;

        public static void Push( string message )
        {
            if ( string.IsNullOrEmpty( message ) )
                return;

            OnFeedback?.Invoke( message );
        }
    }
}
