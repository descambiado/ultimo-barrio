using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio
{
    /// <summary>
    /// Punto de resolución único de definiciones de ítems.
    /// Prioridad: 1) GameResources del editor (data-driven real), 2) catálogo
    /// canónico de ItemCatalog. Nunca devuelve null para ids conocidos.
    /// </summary>
    public static class ItemRegistry
    {
        public static ItemDefinition GetDefinition( string itemId )
        {
            if ( string.IsNullOrEmpty( itemId ) )
                return null;

            // 1) Recursos reales del editor (override del catálogo).
            var asset = ResourceLibrary.GetAll<ItemDefinition>().FirstOrDefault( x => x.ItemId == itemId );
            if ( asset != null )
                return asset;

            // 2) Catálogo canónico.
            if ( ItemCatalog.TryGet( itemId, out var fallback ) )
                return fallback;

            return null;
        }

        public static bool Exists( string itemId ) => GetDefinition( itemId ) is not null;

        /// <summary>Valida el catálogo canónico y reporta errores (ids duplicados, armas sin presentación, consumibles sin efecto).</summary>
        public static List<string> ValidateCatalog()
            => ItemCatalog.Validate();

        /// <summary>
        /// Comprueba que todos los ids referenciados desde listas externas
        /// (recetas, loot, traders) existen en el registro. Devuelve la lista
        /// de referencias rotas (vacía si todo es válido).
        /// </summary>
        public static List<string> ValidateReferences( IEnumerable<string> referencedIds )
        {
            var errors = new List<string>();

            if ( referencedIds is null )
                return errors;

            foreach ( var id in referencedIds.Distinct() )
            {
                if ( string.IsNullOrWhiteSpace( id ) )
                {
                    errors.Add( "Referencia de ítem vacía." );
                    continue;
                }

                if ( !Exists( id ) )
                    errors.Add( $"Ítem referenciado inexistente: '{id}'." );
            }

            return errors;
        }
    }
}
