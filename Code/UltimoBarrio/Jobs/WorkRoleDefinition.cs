// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using Sandbox;

namespace UltimoBarrio.Jobs;

/// <summary>
/// Data owned by a work role, rather than by a player pawn. A role deliberately
/// contains no salary, inventory grants or automatic mission completion: those
/// outcomes belong to their respective host-authoritative systems.
/// </summary>
[Serializable]
public sealed class WorkRoleDefinition
{
	[Property] public string RoleId { get; set; } = string.Empty;
	[Property] public string DisplayName { get; set; } = string.Empty;
	[Property, TextArea] public string Description { get; set; } = string.Empty;
	[Property] public bool IsEnabled { get; set; } = true;
	[Property] public List<string> Tags { get; set; } = new();

	public bool IsValid => !string.IsNullOrWhiteSpace( RoleId ) && !string.IsNullOrWhiteSpace( DisplayName );
}

/// <summary>
/// Small, serializable result boundary for work-role assignment requests.
/// Callers can turn its stable code into UI or logs without interpreting a
/// client-side state change as an authoritative result.
/// </summary>
public readonly record struct WorkRoleAssignmentResult( bool Succeeded, string Code, string RoleId )
{
	public static WorkRoleAssignmentResult Success( string roleId, string code = "assigned" ) => new( true, code, roleId );
	public static WorkRoleAssignmentResult Reject( string code, string roleId = "" ) => new( false, code, roleId );
}
