// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Persistence;

internal sealed class SaveEnvelope
{
	public const int CurrentVersion = 1;

	public int EnvelopeVersion { get; set; } = CurrentVersion;

	public int SaveVersion { get; set; } = SaveSnapshot.CurrentVersion;

	public long Generation { get; set; }

	public string SlotId { get; set; } = string.Empty;

	public string PayloadJson { get; set; } = string.Empty;

	public ulong ContentCrc64 { get; set; }
}
