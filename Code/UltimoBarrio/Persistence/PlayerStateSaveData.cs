// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Persistence;

/// <summary>Estado de jugador persistido: selección de hotbar (por clave de inventario canónica).</summary>
public sealed class PlayerStateSaveData
{
    public string PlayerKey { get; set; } = string.Empty;
    public int SelectedHotbarSlot { get; set; } = -1;
    public int SaveVersion { get; set; } = 1;
}
