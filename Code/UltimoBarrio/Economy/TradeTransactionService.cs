// SPDX-License-Identifier: MPL-2.0
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Economy;

/// <summary>
/// Transaction boundary for NPC trade. Callers must already be executing on the host.
/// It keeps inventory and wallet mutations paired so a rejected trade cannot consume
/// either money or items.
/// </summary>
public static class TradeTransactionService
{
	public readonly struct Result
	{
		public bool Succeeded { get; }
		public string Code { get; }
		public string ItemId { get; }
		public int Amount { get; }
		public int Value { get; }

		private Result( bool succeeded, string code, string itemId, int amount, int value )
		{
			Succeeded = succeeded;
			Code = code;
			ItemId = itemId;
			Amount = amount;
			Value = value;
		}

		public static Result Success( string itemId, int amount, int value ) => new( true, "ok", itemId, amount, value );
		public static Result Reject( string code, string itemId = "", int amount = 0 ) => new( false, code, itemId, amount, 0 );
	}

	public static Result TryBuy( IInventory inventory, IWallet wallet, string itemId, int amount, int unitPrice )
	{
		if ( inventory is null || wallet is null )
			return Result.Reject( "missing-account" );

		if ( string.IsNullOrWhiteSpace( itemId ) || amount <= 0 || unitPrice < 0 )
			return Result.Reject( "invalid-request", itemId, amount );

		if ( amount > int.MaxValue / Math.Max( 1, unitPrice ) )
			return Result.Reject( "price-overflow", itemId, amount );

		var total = amount * unitPrice;
		if ( !wallet.CanAfford( total ) )
			return Result.Reject( "insufficient-funds", itemId, amount );

		if ( !inventory.CanAdd( itemId, amount ) )
			return Result.Reject( "inventory-full", itemId, amount );

		if ( !wallet.TryWithdraw( total ) )
			return Result.Reject( "withdrawal-rejected", itemId, amount );

		if ( inventory.TryAdd( itemId, amount ) )
			return Result.Success( itemId, amount, total );

		wallet.Deposit( total );
		return Result.Reject( "inventory-add-rejected", itemId, amount );
	}

	public static Result TrySell( IInventory inventory, IWallet wallet, string itemId, int amount, int unitPrice )
	{
		if ( inventory is null || wallet is null )
			return Result.Reject( "missing-account" );

		if ( string.IsNullOrWhiteSpace( itemId ) || amount <= 0 || unitPrice < 0 )
			return Result.Reject( "invalid-request", itemId, amount );

		var available = inventory.GetCount( itemId );
		if ( available <= 0 )
			return Result.Reject( "missing-item", itemId, amount );

		var soldAmount = Math.Min( amount, available );
		if ( soldAmount > int.MaxValue / Math.Max( 1, unitPrice ) )
			return Result.Reject( "price-overflow", itemId, soldAmount );

		if ( !inventory.TryRemove( itemId, soldAmount ) )
			return Result.Reject( "inventory-remove-rejected", itemId, soldAmount );

		var total = soldAmount * unitPrice;
		wallet.Deposit( total );
		return Result.Success( itemId, soldAmount, total );
	}
}
