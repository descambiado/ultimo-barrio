using Sandbox;

namespace UltimoBarrio.Components;

/// <summary>
/// Shared runtime contract for anything that can move between a player's hands
/// and the world. This is the project-side equivalent of DarkRP's
/// BaseCarryable: presentation is optional, ownership is host-authoritative and
/// concrete items stay data-driven.
/// </summary>
[Title( "Último Barrio Carryable" )]
[Category( "Último Barrio — Framework" )]
public class UbCarryableComponent : Component
{
	[Property] public string ItemId { get; set; } = "";
	[Property] public string DisplayName { get; set; } = "Objeto";
	[Property] public GameObject WorldModel { get; set; }
	[Property] public GameObject ViewModel { get; set; }
	[Property] public GameObject MuzzlePoint { get; set; }

	[Sync] public bool IsHeld { get; private set; }
	[Sync] public bool IsDropped { get; private set; }

	public GameObject MuzzleObject => MuzzlePoint.IsValid() ? MuzzlePoint : GameObject;

	public virtual bool CanEquip( GameObject owner ) => Networking.IsHost && owner.IsValid() && !IsHeld;

	public virtual bool TryEquip( GameObject owner )
	{
		if ( !CanEquip( owner ) ) return false;

		IsHeld = true;
		IsDropped = false;
		GameObject.SetParent( owner );
		return true;
	}

	public virtual bool TryDrop( Vector3 position, Rotation rotation )
	{
		if ( !Networking.IsHost || !IsHeld ) return false;

		IsHeld = false;
		IsDropped = true;
		GameObject.SetParent( null );
		WorldPosition = position;
		WorldRotation = rotation;
		return true;
	}

	public virtual void OnEquipped() { }
	public virtual void OnUnequipped() { }
	public virtual void OnOwnerDeath() { }

	protected override void OnDisabled()
	{
		IsHeld = false;
		base.OnDisabled();
	}
}
