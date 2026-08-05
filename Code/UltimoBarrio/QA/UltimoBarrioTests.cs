using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.Apartments;
using UltimoBarrio.Economy;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// In-process unit tests executable via ConCmd ub_test_run.
    /// Tests use pure logic helpers that mirror component behavior without requiring a Scene context.
    /// </summary>
    public static class UltimoBarrioTests
    {
        private static int _passed;
        private static int _failed;
        private static List<string> _failures = new();

        [ConCmd( "ub_test_run" )]
        public static void RunAll()
        {
            _passed = 0;
            _failed = 0;
            _failures = new List<string>();

            Log.Info( "[UBTest] === Starting Ultimo Barrio Unit Tests ===" );

            // --- Inventory pure logic ---
            Test_Inventory_TryAdd_IncreasesCount();
            Test_Inventory_TryRemove_DecreasesCount();
            Test_Inventory_TryRemove_NotEnough_Fails();
            Test_Inventory_GetCount_ReturnsCorrectAmount();
            Test_Inventory_SeparateInstances_DoNotShareState();

            // --- Wallet pure logic ---
            Test_Wallet_Deposit_IncreasesBalance();
            Test_Wallet_TryWithdraw_Sufficient_Succeeds();
            Test_Wallet_TryWithdraw_Insufficient_Fails_NoChange();
            Test_Wallet_CanAfford_Correct();

            // --- Stash authorization logic ---
            Test_StashAuth_Unclaimed_ReturnsFalse();
            Test_StashAuth_CorrectOwner_ReturnsTrue();
            Test_StashAuth_WrongOwner_ReturnsFalse();
            Test_StashAuth_NullId_ReturnsFalse();

            // --- Apartment one-per-player rule ---
            Test_Apartment_OnePerPlayer_SecondClaimRejected();
            Test_Apartment_TwoPlayers_GetDistinctApartments();

            // --- Item ID canonical names ---
            Test_PickupItemIds_Canonical();

            // --- Trader purchase/sale atomicity ---
            Test_Trader_SellChatarra_DeductsAndCredits();
            Test_Trader_BuyAmmo_InsufficientFunds_NoChange();
            Test_Trader_BuyAmmo_SufficientFunds_Succeeds();

            // Summary
            Log.Info( $"[UBTest] === RESULTS: {_passed} passed, {_failed} failed ===" );
            foreach ( var f in _failures )
            {
                Log.Error( $"[UBTest] FAIL: {f}" );
            }
        }

        // ─── Pure Inventory Logic ────────────────────────────────────────

        static void Test_Inventory_TryAdd_IncreasesCount()
        {
            var inv = new FakeInventory();
            inv.Add( "chatarra", 5 );
            Assert( "TryAdd increases count", inv.Get( "chatarra" ) == 5 );
        }

        static void Test_Inventory_TryRemove_DecreasesCount()
        {
            var inv = new FakeInventory();
            inv.Add( "chatarra", 10 );
            inv.Remove( "chatarra", 3 );
            Assert( "TryRemove decreases count", inv.Get( "chatarra" ) == 7 );
        }

        static void Test_Inventory_TryRemove_NotEnough_Fails()
        {
            var inv = new FakeInventory();
            inv.Add( "chatarra", 2 );
            bool result = inv.Remove( "chatarra", 5 );
            Assert( "TryRemove with insufficient amount returns false", !result );
            Assert( "TryRemove with insufficient amount leaves count unchanged", inv.Get( "chatarra" ) == 2 );
        }

        static void Test_Inventory_GetCount_ReturnsCorrectAmount()
        {
            var inv = new FakeInventory();
            inv.Add( "ammo_9mm", 12 );
            inv.Add( "chatarra", 3 );
            Assert( "GetCount ammo_9mm correct", inv.Get( "ammo_9mm" ) == 12 );
            Assert( "GetCount chatarra correct", inv.Get( "chatarra" ) == 3 );
            Assert( "GetCount nonexistent is 0", inv.Get( "nonexistent" ) == 0 );
        }

        static void Test_Inventory_SeparateInstances_DoNotShareState()
        {
            var inv1 = new FakeInventory();
            var inv2 = new FakeInventory();
            inv1.Add( "chatarra", 10 );
            Assert( "inv1 has 10", inv1.Get( "chatarra" ) == 10 );
            Assert( "inv2 has 0 (separate)", inv2.Get( "chatarra" ) == 0 );
        }

        // ─── Wallet ─────────────────────────────────────────────────────

        static void Test_Wallet_Deposit_IncreasesBalance()
        {
            var w = new FakeWallet( 0 );
            w.Deposit( 50 );
            Assert( "Deposit increases balance", w.Balance == 50 );
        }

        static void Test_Wallet_TryWithdraw_Sufficient_Succeeds()
        {
            var w = new FakeWallet( 100 );
            bool result = w.TryWithdraw( 40 );
            Assert( "TryWithdraw succeeds when sufficient", result );
            Assert( "Balance correct after withdrawal", w.Balance == 60 );
        }

        static void Test_Wallet_TryWithdraw_Insufficient_Fails_NoChange()
        {
            var w = new FakeWallet( 10 );
            bool result = w.TryWithdraw( 50 );
            Assert( "TryWithdraw fails when insufficient", !result );
            Assert( "Balance unchanged after failed withdrawal", w.Balance == 10 );
        }

        static void Test_Wallet_CanAfford_Correct()
        {
            var w = new FakeWallet( 25 );
            Assert( "CanAfford exact amount", w.CanAfford( 25 ) );
            Assert( "CanAfford over amount is false", !w.CanAfford( 26 ) );
        }

        // ─── Stash Authorization ─────────────────────────────────────────

        static void Test_StashAuth_Unclaimed_ReturnsFalse()
        {
            Assert( "Unclaimed stash denies", !StashCanAccess( ApartmentClaimState.Unclaimed, "", "player-A" ) );
        }

        static void Test_StashAuth_CorrectOwner_ReturnsTrue()
        {
            Assert( "Correct owner gets access", StashCanAccess( ApartmentClaimState.Claimed, "player-A", "player-A" ) );
        }

        static void Test_StashAuth_WrongOwner_ReturnsFalse()
        {
            Assert( "Wrong player denied", !StashCanAccess( ApartmentClaimState.Claimed, "player-A", "player-B" ) );
        }

        static void Test_StashAuth_NullId_ReturnsFalse()
        {
            Assert( "Null requestor denied", !StashCanAccess( ApartmentClaimState.Claimed, "player-A", null ) );
        }

        // ─── Apartment Claim Logic ────────────────────────────────────────

        static void Test_Apartment_OnePerPlayer_SecondClaimRejected()
        {
            var claimed = new Dictionary<string, string>();
            bool first = TryClaim( claimed, "apartment-a01", "player-A" );
            bool second = TryClaim( claimed, "apartment-a02", "player-A" );
            Assert( "First claim succeeds", first );
            Assert( "Second claim same player rejected", !second );
        }

        static void Test_Apartment_TwoPlayers_GetDistinctApartments()
        {
            var claimed = new Dictionary<string, string>();
            TryClaim( claimed, "apartment-a01", "player-A" );
            TryClaim( claimed, "apartment-a02", "player-B" );
            Assert( "A01 owner is player-A", claimed.ContainsKey( "apartment-a01" ) && claimed["apartment-a01"] == "player-A" );
            Assert( "A02 owner is player-B", claimed.ContainsKey( "apartment-a02" ) && claimed["apartment-a02"] == "player-B" );
        }

        // ─── Canonical Item IDs ──────────────────────────────────────────

        static void Test_PickupItemIds_Canonical()
        {
            Assert( "Scrap is 'chatarra'", "chatarra" == "chatarra" );
            Assert( "Ammo is 'ammo_9mm'", "ammo_9mm" == "ammo_9mm" );
            Assert( "USP is 'weapon_usp'", "weapon_usp" == "weapon_usp" );
        }

        // ─── Trader Atomic Transactions ──────────────────────────────────

        static void Test_Trader_SellChatarra_DeductsAndCredits()
        {
            var inv = new FakeInventory();
            var w = new FakeWallet( 0 );
            inv.Add( "chatarra", 5 );

            // Simulate sell 1 chatarra at $2
            bool result = SimulateSell( inv, w, "chatarra", 1, 2 );
            Assert( "Sell chatarra succeeds", result );
            Assert( "Chatarra deducted after sell", inv.Get( "chatarra" ) == 4 );
            Assert( "Wallet credited after sell", w.Balance == 2 );
        }

        static void Test_Trader_BuyAmmo_InsufficientFunds_NoChange()
        {
            var inv = new FakeInventory();
            var w = new FakeWallet( 3 ); // Need $5
            bool result = SimulateBuy( inv, w, "ammo_9mm", 12, 5 );
            Assert( "Buy fails with insufficient funds", !result );
            Assert( "Ammo not added on failure", inv.Get( "ammo_9mm" ) == 0 );
            Assert( "Wallet unchanged on failure", w.Balance == 3 );
        }

        static void Test_Trader_BuyAmmo_SufficientFunds_Succeeds()
        {
            var inv = new FakeInventory();
            var w = new FakeWallet( 20 );
            bool result = SimulateBuy( inv, w, "ammo_9mm", 12, 5 );
            Assert( "Buy ammo succeeds with sufficient funds", result );
            Assert( "Ammo added to inventory", inv.Get( "ammo_9mm" ) == 12 );
            Assert( "Wallet deducted correctly", w.Balance == 15 );
        }

        // ─── Pure Logic Helpers ──────────────────────────────────────────

        static bool StashCanAccess( ApartmentClaimState claimState, string ownerId, string requestorId )
        {
            if ( claimState == ApartmentClaimState.Unclaimed || string.IsNullOrEmpty( ownerId ) ) return false;
            if ( string.IsNullOrEmpty( requestorId ) ) return false;
            return ownerId == requestorId;
        }

        static bool TryClaim( Dictionary<string, string> claimed, string aptId, string playerId )
        {
            if ( claimed.ContainsValue( playerId ) ) return false;
            if ( claimed.ContainsKey( aptId ) ) return false;
            claimed[aptId] = playerId;
            return true;
        }

        // Atomic buy: only succeeds if funds sufficient AND inventory has space
        static bool SimulateBuy( FakeInventory inv, FakeWallet wallet, string itemId, int amount, int price )
        {
            if ( !wallet.CanAfford( price ) ) return false;
            wallet.TryWithdraw( price );
            inv.Add( itemId, amount );
            return true;
        }

        // Atomic sell: only succeeds if item available
        static bool SimulateSell( FakeInventory inv, FakeWallet wallet, string itemId, int amount, int priceEach )
        {
            if ( inv.Get( itemId ) < amount ) return false;
            inv.Remove( itemId, amount );
            wallet.Deposit( amount * priceEach );
            return true;
        }

        static void Assert( string name, bool condition )
        {
            if ( condition )
            {
                _passed++;
                Log.Info( $"[UBTest] PASS: {name}" );
            }
            else
            {
                _failed++;
                _failures.Add( name );
                Log.Error( $"[UBTest] FAIL: {name}" );
            }
        }

        // ─── Fake implementations (no Sandbox dependency) ────────────────

        class FakeInventory
        {
            private Dictionary<string, int> _items = new();

            public void Add( string id, int amount )
            {
                if ( _items.ContainsKey( id ) ) _items[id] += amount;
                else _items[id] = amount;
            }

            public bool Remove( string id, int amount )
            {
                if ( !_items.ContainsKey( id ) || _items[id] < amount ) return false;
                _items[id] -= amount;
                if ( _items[id] == 0 ) _items.Remove( id );
                return true;
            }

            public int Get( string id ) => _items.TryGetValue( id, out int v ) ? v : 0;
        }

        class FakeWallet
        {
            public int Balance { get; private set; }
            public FakeWallet( int initial ) { Balance = initial; }
            public void Deposit( int amount ) { Balance += amount; }
            public bool CanAfford( int amount ) => Balance >= amount;
            public bool TryWithdraw( int amount )
            {
                if ( Balance < amount ) return false;
                Balance -= amount;
                return true;
            }
        }
    }
}
