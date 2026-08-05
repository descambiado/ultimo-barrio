using Sandbox;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace UltimoBarrio.Tools
{
    public static class SurfacePlacementTool
    {
        [ConCmd("ub_tool_ground_overlay")]
        public static void RunPlacement()
        {
            var scene = Game.ActiveScene;
            if (scene == null) return;
            
            int grounded = 0;
            var results = new Dictionary<string, Vector3>();

            var list = new List<Component>();
            list.AddRange(scene.GetAllComponents<UltimoBarrio.Apartments.ApartmentClaimInteractable>());
            list.AddRange(scene.GetAllComponents<UltimoBarrio.StashComponent>());
            list.AddRange(scene.GetAllComponents<UltimoBarrio.Trading.Trader>());
            list.AddRange(scene.GetAllComponents<UltimoBarrio.WorldItemPickup>());

            foreach (var comp in list)
            {
                var go = comp.GameObject;
                var startPos = go.WorldPosition.WithZ(go.WorldPosition.z + 500f);
                var tr = scene.Trace.Ray(startPos, startPos + Vector3.Down * 2000f)
                    .IgnoreGameObjectHierarchy(go)
                    .WithoutTags("trigger", "player")
                    .Run();
                    
                if (tr.Hit)
                {
                    float offsetZ = 0;
                    var box = go.Components.Get<BoxCollider>(FindMode.EnabledInSelfAndDescendants);
                    if (box != null) 
                    {
                        offsetZ = box.Scale.z / 2f;
                    }
                    
                    var newPos = tr.HitPosition + Vector3.Up * (offsetZ + 0.5f);
                    go.WorldPosition = newPos;
                    results[go.Id.ToString()] = newPos;
                    grounded++;
                    Log.Info($"[Grounding] {go.Name} grounded to {newPos}");
                }
            }
            
            var json = JsonSerializer.Serialize(results);
            FileSystem.Data.WriteAllText("grounding_results.json", json);
            Log.Info($"[Grounding] Successfully grounded {grounded} objects and saved to Data/grounding_results.json.");
        }
    }
}
