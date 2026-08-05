// SPDX-License-Identifier: MPL-2.0

using Sandbox;
using System.Linq;

namespace UltimoBarrio.Persistence;

/// <summary>
/// Guardado automático del host: snapshot periódico + guardado inmediato ante
/// eventos del PersistenceBridge (claim, crafteo, transferencia, barricada,
/// mejora). Escritor único host (el propio ApartmentClaimService serializa).
/// </summary>
[Title( "Auto Save Manager" )]
[Category( "Último Barrio — Persistence" )]
[Icon( "save" )]
public sealed class AutoSaveManager : Component
{
    [Property] public float IntervalSeconds { get; set; } = 90f;
    [Property] public bool Enabled { get; set; } = true;

    private TimeSince _timeSinceSave;

    protected override void OnStart()
    {
        if ( !Networking.IsHost )
            return;

        PersistenceBridge.OnSaveRequested += HandleSaveRequested;
        _timeSinceSave = 0f;
    }

    protected override void OnDestroy()
    {
        PersistenceBridge.OnSaveRequested -= HandleSaveRequested;
    }

    protected override void OnUpdate()
    {
        if ( !Networking.IsHost || !Enabled )
            return;

        if ( _timeSinceSave > IntervalSeconds )
        {
            _timeSinceSave = 0f;
            TrySaveNow( "autosave" );
        }
    }

    private void HandleSaveRequested( string reason )
    {
        if ( !Networking.IsHost || !Enabled )
            return;

        _timeSinceSave = 0f;
        TrySaveNow( reason );
    }

    private void TrySaveNow( string reason )
    {
        var service = Scene.GetAllComponents<Apartments.ApartmentClaimService>().FirstOrDefault();
        if ( service is null || !service.IsReady )
            return;

        var result = service.TrySaveNow();
        if ( !result.Succeeded )
            Log.Warning( $"UB.Save AutosaveFallido reason={reason} error={result.Error}" );
    }
}
