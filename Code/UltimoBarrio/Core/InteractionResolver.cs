using Sandbox;

namespace UltimoBarrio.Core;

/// <summary>
/// Shared interaction boundary for player traces. It resolves an interactable
/// from a hit child or parent, then validates immediately before execution.
/// This is the project equivalent of DarkRP's shared object-access layer.
/// </summary>
public static class InteractionResolver
{
	public static IWorldInteractable Find( GameObject hitObject )
	{
		if ( hitObject == null || !hitObject.IsValid() ) return null;

		return hitObject.Components.GetInAncestorsOrSelf<IWorldInteractable>();
	}

	public static bool CanUse( IWorldInteractable interactable, InteractionRequest request )
	{
		if ( interactable == null || request.InteractorObject == null || !request.InteractorObject.IsValid() ) return false;
		return interactable.CanInteract( request );
	}

	public static bool TryUse( IWorldInteractable interactable, InteractionRequest request )
	{
		if ( !CanUse( interactable, request ) ) return false;
		interactable.OnInteract( request );
		return true;
	}
}
