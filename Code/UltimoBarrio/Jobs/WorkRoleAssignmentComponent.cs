// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using Sandbox;
using UltimoBarrio.Core;

namespace UltimoBarrio.Jobs;

/// <summary>
/// Host-authoritative assignment state for one player pawn. Attach it to the
/// networked player prefab and configure <see cref="AvailableRoles"/> from the
/// scene or prefab. It is intentionally not a payroll, mission or inventory
/// system: a role only records a validated identity-to-role decision.
/// </summary>
[Title( "Work Role Assignment" )]
[Category( "Último Barrio — Jobs" )]
[Icon( "work" )]
public sealed class WorkRoleAssignmentComponent : Component
{
	[Property] public List<WorkRoleDefinition> AvailableRoles { get; set; } = new();

	[Sync( SyncFlags.FromHost )]
	public string AssignedRoleId { get; private set; } = string.Empty;

	[Sync( SyncFlags.FromHost )]
	public string AssignedRoleName { get; private set; } = string.Empty;

	public bool HasAssignment => !string.IsNullOrWhiteSpace( AssignedRoleId );

	/// <summary>
	/// Emitted by the host after a role state transition. Persistence, UI and
	/// mission adapters can subscribe without duplicating authority checks.
	/// </summary>
	public event Action<WorkRoleAssignmentResult> AssignmentChanged;

	/// <summary>
	/// Player-facing request boundary. A remote caller can only request a role
	/// for the pawn it owns; the host resolves and validates the actual identity.
	/// </summary>
	public void RequestAssignment( string roleId )
	{
		if ( Networking.IsHost )
		{
			var ownerIdentity = PlayerIdentity.FromGameObject( GameObject );
			TryAssignForHost( ownerIdentity, roleId );
			return;
		}

		RpcRequestAssignment( roleId );
	}

	/// <summary>
	/// Host entry point for trusted world interactions. The supplied identity is
	/// matched against this pawn's network owner before any replicated state is
	/// changed, so one player cannot assign another player's role.
	/// </summary>
	public WorkRoleAssignmentResult TryAssignForHost( PlayerIdentity requester, string roleId )
	{
		if ( !Networking.IsHost )
			return WorkRoleAssignmentResult.Reject( "not-host", roleId );

		var ownerIdentity = PlayerIdentity.FromGameObject( GameObject );
		if ( !ownerIdentity.IsValid )
			return WorkRoleAssignmentResult.Reject( "missing-player-owner", roleId );

		if ( !requester.IsValid || requester != ownerIdentity )
			return WorkRoleAssignmentResult.Reject( "requester-not-owner", roleId );

		var role = FindRole( roleId );
		if ( role is null )
			return WorkRoleAssignmentResult.Reject( "unknown-role", roleId );

		if ( !role.IsEnabled )
			return WorkRoleAssignmentResult.Reject( "role-disabled", role.RoleId );

		if ( string.Equals( AssignedRoleId, role.RoleId, StringComparison.OrdinalIgnoreCase ) )
			return WorkRoleAssignmentResult.Success( role.RoleId, "unchanged" );

		AssignedRoleId = role.RoleId;
		AssignedRoleName = role.DisplayName;

		var result = WorkRoleAssignmentResult.Success( AssignedRoleId );
		AssignmentChanged?.Invoke( result );
		Log.Info( $"UB.Jobs Assigned player={ownerIdentity.CanonicalId} role={AssignedRoleId}" );
		return result;
	}

	/// <summary>
	/// Clears the role without introducing a client-side mutation path. This is
	/// intended for a host-owned role desk or session cleanup flow.
	/// </summary>
	public WorkRoleAssignmentResult TryClearForHost( PlayerIdentity requester )
	{
		if ( !Networking.IsHost )
			return WorkRoleAssignmentResult.Reject( "not-host" );

		var ownerIdentity = PlayerIdentity.FromGameObject( GameObject );
		if ( !ownerIdentity.IsValid )
			return WorkRoleAssignmentResult.Reject( "missing-player-owner" );

		if ( !requester.IsValid || requester != ownerIdentity )
			return WorkRoleAssignmentResult.Reject( "requester-not-owner" );

		if ( !HasAssignment )
			return WorkRoleAssignmentResult.Success( string.Empty, "unchanged" );

		AssignedRoleId = string.Empty;
		AssignedRoleName = string.Empty;
		var result = WorkRoleAssignmentResult.Success( string.Empty, "cleared" );
		AssignmentChanged?.Invoke( result );
		Log.Info( $"UB.Jobs Cleared player={ownerIdentity.CanonicalId}" );
		return result;
	}

	[Rpc.Host]
	private void RpcRequestAssignment( string roleId )
	{
		TryAssignForHost( PlayerIdentity.FromConnection( Rpc.Caller ), roleId );
	}

	private WorkRoleDefinition FindRole( string roleId )
	{
		if ( string.IsNullOrWhiteSpace( roleId ) )
			return null;

		foreach ( var role in AvailableRoles )
		{
			if ( role is not null && role.IsValid && string.Equals( role.RoleId, roleId.Trim(), StringComparison.OrdinalIgnoreCase ) )
				return role;
		}

		return null;
	}
}
