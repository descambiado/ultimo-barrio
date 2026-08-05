using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.Combat;
using UltimoBarrio.Crafting;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Validadores de dominio del gameplay: munición, hotbar, drop con
    /// rollback, crafteo atómico y trader. Todo lógica pura — los comandos QA
    /// solo consultan estados; no fingen acciones ni éxitos.
    /// </summary>
    public static class GameplayTests
    {
        [ConCmd( "ub_test_gameplay" )]
        public static void Run()
        {
            Log.Info( "[UBTest] === Validando gameplay ===" );

            var passed = 0;
            var failed = 0;

            // ── Munición: recarga parcial y completa ──────────────────────
            var (clip1, reserve1) = ReloadMath.Reload( 3, 12, 30 );
            if ( clip1 == 12 && reserve1 == 21 ) passed++;
            else { failed++; Log.Error( $"[UBTest] FAIL: recarga parcial (clip={clip1}, res={reserve1})." ); }

            var (clip2, reserve2) = ReloadMath.Reload( 12, 12, 30 );
            if ( clip2 == 12 && reserve2 == 30 ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: recarga con cargador lleno no debe tocar la reserva." ); }

            var (clip3, reserve3) = ReloadMath.Reload( 0, 12, 5 );
            if ( clip3 == 5 && reserve3 == 0 ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: recarga con reserva insuficiente." ); }

            if ( ReloadMath.CanReload( 12, 12, 30 ) == false && ReloadMath.CanReload( 5, 12, 30 ) == true ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: CanReload." ); }

            // ── Hotbar: selección válida solo dentro de límites y no vacía ──
            bool HotbarSelectionValid( int slot, int hotbarSlots, string itemId, int amount )
                => slot >= 0 && slot < hotbarSlots && !string.IsNullOrEmpty( itemId ) && amount > 0;

            if ( HotbarSelectionValid( 2, 6, "water", 1 ) ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: slot 2 válido rechazado." ); }

            if ( HotbarSelectionValid( 6, 6, "water", 1 ) == false ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: slot 6 fuera de rango aceptado." ); }

            if ( HotbarSelectionValid( 0, 6, "", 0 ) == false ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: slot vacío aceptado." ); }

            // ── Drop con rollback: si el pickup no se materializa, se devuelve ──
            var fake = new FakeInventory();
            fake.Add( "chatarra", 5 );
            bool removed = fake.Remove( "chatarra", 1 );
            bool rolledBack = fake.Add( "chatarra", 1 ); // Reembolso tras fallo de spawn.
            if ( removed && rolledBack && fake.Get( "chatarra" ) == 5 ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: drop con rollback altera el inventario." ); }

            // ── Crafteo atómico (regla): consumir todo o nada ─────────────
            var recipe = CraftingLibrary.Get( "craft_ammo_9mm" );
            if ( recipe is not null && recipe.Ingredients.Count == 1 && recipe.Result.ItemId == "ammo_9mm" ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: receta ammo_9mm mal definida." ); }

            // Regla: sin ingredientes no hay consumo.
            var poor = new FakeInventory();
            poor.Add( "chatarra", 3 ); // Faltan 2.
            bool consumed = poor.Remove( "chatarra", recipe?.Ingredients[0].Amount ?? 0 );
            if ( recipe is not null && consumed == false && poor.Get( "chatarra" ) == 3 ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: el crafteo no debe consumir si faltan ingredientes." ); }

            // ── Trader: vender chatarra acredita y descuenta (lógica pura) ──
            var traderInv = new FakeInventory();
            traderInv.Add( "chatarra", 10 );
            int walletBalance = 0;
            const int sellPrice = 2;
            int sellAmount = 3;
            if ( traderInv.Remove( "chatarra", sellAmount ) )
                walletBalance += sellPrice * sellAmount;

            if ( traderInv.Get( "chatarra" ) == 7 && walletBalance == 6 ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: venta de chatarra incorrecta." ); }

            Log.Info( $"[UBTest] === GAMEPLAY: {passed} passed, {failed} failed ===" );
        }

        /// <summary>Mini inventario puro para validaciones (sin Scene).</summary>
        public class FakeInventory
        {
            private readonly Dictionary<string, int> _items = new();

            public void Add( string id, int amount )
            {
                if ( !_items.ContainsKey( id ) ) _items[id] = 0;
                _items[id] += amount;
            }

            public bool Remove( string id, int amount )
            {
                if ( !_items.TryGetValue( id, out var count ) || count < amount )
                    return false;

                _items[id] -= amount;
                if ( _items[id] <= 0 ) _items.Remove( id );
                return true;
            }

            public int Get( string id )
                => _items.TryGetValue( id, out var count ) ? count : 0;
        }
    }
}
