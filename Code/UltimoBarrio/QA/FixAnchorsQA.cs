using Sandbox;
using System.Linq;
using UltimoBarrio.Apartments;

namespace UltimoBarrio.QA
{
    public static class FixAnchorsQA
    {
        [ConCmd("ub_qa_fix_anchors")]
        public static void FixAnchors()
        {
            if (!Networking.IsHost) return;
            Log.Info("[QA] Fixing Anchors");
            
            var apts = Game.ActiveScene.GetAllComponents<ApartmentComponent>();
            foreach (var apt in apts)
            {
                var anchors = apt.GameObject.Children.Where(c => c.Name.Contains("Anchor"));
                var center = apt.WorldPosition;
                
                foreach (var anc in anchors)
                {
                    var tr = Game.ActiveScene.Trace.Ray(center + Vector3.Up * 100f, center + Vector3.Down * 300f).Run();
                    if (tr.Hit)
                    {
                        anc.WorldPosition = tr.HitPosition;
                        Log.Info($"[QA] Relocated {apt.ApartmentId} {anc.Name} to {anc.WorldPosition} on {tr.GameObject?.Name}");
                    }
                }
            }
        }
    }
}
