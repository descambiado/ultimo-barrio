using Sandbox;
using System.Linq;
using UltimoBarrio.Apartments;

public static class QaAptDump
{
    [ConCmd("ub_qa_dump_apts")]
    public static void DumpApts()
    {
        var apts = Game.ActiveScene.GetAllComponents<ApartmentComponent>().ToList();
        var steamId = Sandbox.Connection.Local?.SteamId.ToString() ?? "unknown";
        Log.Info($"=== APARTMENT DUMP ===");
        Log.Info($"Local Player Persistent ID: steam:{steamId}");
        foreach(var a in apts)
        {
            Log.Info($"ID: {a.ApartmentId} | Claim: {a.ClaimState} | Owner: {a.OwnerId} | Stash: {a.StashReference?.Name} | Spawn: {a.SpawnReference?.Name}");
        }
        Log.Info($"======================");
    }
}
