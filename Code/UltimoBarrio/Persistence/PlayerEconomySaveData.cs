// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Persistence;

public sealed class PlayerEconomySaveData
{
	public string PlayerId { get; set; } = string.Empty;
	public int Balance { get; set; }
	public int SaveVersion { get; set; } = 1;
}
