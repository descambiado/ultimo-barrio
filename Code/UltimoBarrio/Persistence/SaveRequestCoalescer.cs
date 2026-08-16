// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace UltimoBarrio.Persistence;

/// <summary>
/// Agrupa solicitudes de guardado host-side que ocurren dentro de una ventana
/// corta y las entrega una sola vez al escritor ya registrado en
/// <see cref="PersistenceBridge"/>.
///
/// No observa <c>PersistenceBridge.OnSaveRequested</c>: ese evento se publica
/// después de que el puente haya intentado guardar. Los emisores que puedan
/// producir ráfagas deben llamar explícitamente a <see cref="Queue"/> en vez
/// de invocar el puente directamente.
/// </summary>
[Title( "Save Request Coalescer" )]
[Category( "Último Barrio — Persistence" )]
[Icon( "save" )]
public sealed class SaveRequestCoalescer : Component
{
	/// <summary>Tiempo de silencio antes de escribir el snapshot agrupado.</summary>
	[Property]
	public float CoalesceWindowSeconds { get; set; } = 0.35f;

	/// <summary>Límite de razones retenidas para no convertir logs en payload.</summary>
	[Property]
	public int MaxReasons { get; set; } = 8;

	private readonly HashSet<string> _pendingReasons = new( StringComparer.Ordinal );
	private TimeSince _timeSinceLastRequest;
	private bool _hasPendingRequest;

	/// <summary>Se emite únicamente en el host tras enviar la solicitud agrupada.</summary>
	public event Action<string> Flushed;

	public bool HasPendingRequest => _hasPendingRequest;
	public int PendingReasonCount => _pendingReasons.Count;

	/// <summary>
	/// Encola una intención de guardado. Es seguro repetir la misma razón: se
	/// conserva una sola vez y la ventana se reinicia para capturar el estado
	/// final de la ráfaga.
	/// </summary>
	public void Queue( string reason = "gameplay" )
	{
		if ( !Networking.IsHost || Scene.IsEditor )
			return;

		var normalizedReason = NormalizeReason( reason );
		if ( _pendingReasons.Count < Math.Max( 1, MaxReasons ) )
			_pendingReasons.Add( normalizedReason );

		_hasPendingRequest = true;
		_timeSinceLastRequest = 0f;
	}

	/// <summary>Entrega inmediatamente la ráfaga pendiente, por ejemplo al cambiar de mapa.</summary>
	public void Flush()
	{
		if ( !Networking.IsHost || !_hasPendingRequest )
			return;

		var reason = BuildReason();
		_pendingReasons.Clear();
		_hasPendingRequest = false;
		PersistenceBridge.RequestSave( reason );
		Flushed?.Invoke( reason );
	}

	protected override void OnUpdate()
	{
		if ( !_hasPendingRequest || !Networking.IsHost )
			return;

		if ( _timeSinceLastRequest >= Math.Max( 0f, CoalesceWindowSeconds ) )
			Flush();
	}

	private string BuildReason()
	{
		if ( _pendingReasons.Count == 0 )
			return "coalesced:gameplay";

		return $"coalesced:{string.Join( ",", _pendingReasons.OrderBy( reason => reason, StringComparer.Ordinal ) )}";
	}

	private static string NormalizeReason( string reason )
	{
		if ( string.IsNullOrWhiteSpace( reason ) )
			return "gameplay";

		return reason.Trim();
	}
}
