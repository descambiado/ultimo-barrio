import re

with open('Code/UltimoBarrio/QA/QaCommands.cs', 'r', encoding='utf-8') as f:
    text = f.read()

new_commands = '''
        [ConCmd("ub_qa_audit_anchors")]
        public static void AuditAnchors()
        {
            if (!Networking.IsHost) return;
            Log.Info("[QA_TEST] Anchors Audit");
            var apts = Game.ActiveScene.GetAllComponents<ApartmentComponent>();
            foreach (var apt in apts)
            {
                var anchors = apt.GameObject.Children.Where(c => c.Name.Contains("Anchor")).ToList();
                foreach (var anc in anchors)
                {
                    var tr = Scene.Trace.Ray(anc.WorldPosition + Vector3.Up * 10f, anc.WorldPosition + Vector3.Down * 1000f)
                        .IgnoreGameObjectHierarchy(anc)
                        .Run();
                    float dist = tr.Hit ? tr.Distance - 10f : -1f;
                    Log.Info($"[QA_TEST] {apt.ApartmentId} - {anc.Name}: Parent={anc.Parent.Name}, WP={anc.WorldPosition}, LP={anc.LocalPosition}, GroundDist={dist}");
                }
            }
        }

        [ConCmd("ub_qa_place_anchors")]
        public static void PlaceAnchors()
        {
            if (!Networking.IsHost) return;
            Log.Info("[QA_TEST] Placing Anchors");
            var apts = Game.ActiveScene.GetAllComponents<ApartmentComponent>();
            foreach (var apt in apts)
            {
                var center = apt.WorldPosition;
                // Move manually with code raycast
                var anchors = apt.GameObject.Children.Where(c => c.Name.Contains("Anchor"));
                foreach (var anc in anchors)
                {
                    var tr = Scene.Trace.Ray(center + Vector3.Up * 50f, center + Vector3.Down * 200f).Run();
                    if (tr.Hit) 
                    {
                        if (anc.Name.Contains("Window"))
                            anc.WorldPosition = tr.HitPosition + Vector3.Up * 50f + Vector3.Right * 100f; // Mock window pos
                        else
                            anc.WorldPosition = tr.HitPosition;
                        Log.Info($"[QA_TEST] Relocated {apt.ApartmentId} {anc.Name} to {anc.WorldPosition}");
                    }
                }
            }
        }

        [ConCmd("ub_qa_audit_player")]
        public static void AuditPlayer()
        {
            if (!Networking.IsHost) return;
            var player = Game.ActiveScene.GetAllComponents<Sandbox.PlayerController>().FirstOrDefault()?.GameObject;
            if (player == null) return;
            
            var hic = player.Components.GetInDescendantsOrSelf<UltimoBarrio.Combat.HeldItemController>();
            var pm = player.Components.GetInDescendantsOrSelf<UltimoBarrio.Players.PlayerMovementModifier>();
            var inv = player.Components.GetInDescendantsOrSelf<UltimoBarrio.InventoryComponent>();
            var hud = player.Components.GetInDescendantsOrSelf<PlayerHud>();
            var wal = player.Components.GetInDescendantsOrSelf<Wallet>();
            var heal = player.Components.GetInDescendantsOrSelf<UltimoBarrio.Combat.HealthComponent>();
            
            Log.Info("[QA_TEST] Player Audit:");
            Log.Info($"[QA_TEST] HeldItemController: {hic != null}");
            Log.Info($"[QA_TEST] PlayerMovementModifier: {pm != null}");
            Log.Info($"[QA_TEST] InventoryComponent: {inv != null}");
            Log.Info($"[QA_TEST] PlayerHud: {hud != null}");
            Log.Info($"[QA_TEST] Wallet: {wal != null}");
            Log.Info($"[QA_TEST] HealthComponent: {heal != null}");
            
            if (hic != null)
            {
                Log.Info($"[QA_TEST] PistolPrefab: {hic.PrimaryPrefab != null}");
                Log.Info($"[QA_TEST] MeleePrefab: {hic.MeleePrefab != null}");
            }
        }
'''

text = text.replace('    }\n}', new_commands + '    }\n}')

with open('Code/UltimoBarrio/QA/QaCommands.cs', 'w', encoding='utf-8') as f:
    f.write(text)
