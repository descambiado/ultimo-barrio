using System;

namespace UltimoBarrio.Persistence
{
    /// <summary>
    /// Puente entre los sistemas de juego y la persistencia: cualquier cambio
    /// persistible (claim, crafteo, transferencia, barricada, mejora) pide
    /// guardado sin conocer el proveedor. El servicio de persistencia registra
    /// su SaveNow al inicializarse.
    /// </summary>
    public static class PersistenceBridge
    {
        public static Func<bool> SaveNow { get; private set; }

        public static event Action<string> OnSaveRequested;

        public static void Register( Func<bool> saveNow )
        {
            SaveNow = saveNow;
        }

        public static void Unregister()
        {
            SaveNow = null;
        }

        public static void RequestSave( string reason = "gameplay" )
        {
            OnSaveRequested?.Invoke( reason );

            try
            {
                SaveNow?.Invoke();
            }
            catch ( Exception ex )
            {
                Log.Error( $"[PersistenceBridge] Fallo al guardar ({reason}): {ex}" );
            }
        }
    }
}
