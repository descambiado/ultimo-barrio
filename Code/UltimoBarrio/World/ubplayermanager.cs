using Sandbox;

namespace UltimoBarrio.World;

/// <summary>
/// Diag mínimo: confirma que el player aterriza (tras fix MotionEnabled en prefab).
/// OnUpdate vacío — el log de spawn basta; no spamear cada frame.
/// </summary>
public sealed class UbPlayerFix : Component
{
	protected override void OnStart()
	{
		Log.Info( $"[UB-FIX] PW gravity={Game.ActiveScene.PhysicsWorld.Gravity}" );
	}
}
