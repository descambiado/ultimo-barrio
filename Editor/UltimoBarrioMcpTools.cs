// SPDX-License-Identifier: MPL-2.0

using System;

namespace Editor.Mcp;

[McpToolset( "ultimo_barrio", "Safe project-specific editor operations for Ultimo Barrio." )]
public static class UltimoBarrioSceneTools
{
	/// <summary>
	/// Reload the active scene from its source file. Refuses to run during Play Mode or when the
	/// editor has unsaved scene changes unless discarding them is requested explicitly.
	/// </summary>
	/// <param name="discardUnsavedChanges">Set true only when the file on disk is authoritative.</param>
	[McpTool( "reload_active_scene_from_disk" )]
	public static object ReloadActiveSceneFromDisk( bool discardUnsavedChanges = false )
	{
		var session = SceneEditorSession.Active
			?? throw new InvalidOperationException( "No scene editor session is active." );

		if ( session.IsPlaying )
			throw new InvalidOperationException( "Stop Play Mode before reloading the scene." );

		if ( session.HasUnsavedChanges && !discardUnsavedChanges )
		{
			throw new InvalidOperationException(
				"The active scene has unsaved changes. Save or discard them explicitly before reloading." );
		}

		// Se recarga la escena que está realmente activa, no una ruta fija: el proyecto
		// arranca en scenes/ultimo_barrio_alpha.scene y la constante anterior apuntaba a
		// scenes/main.scene, así que limpiaba la copia en memoria equivocada.
		var scenePath = session.Scene?.Source?.ResourcePath
			?? throw new InvalidOperationException( "The active scene has no source asset on disk." );

		var discardedChanges = session.HasUnsavedChanges;
		var asset = AssetSystem.FindByPath( scenePath )
			?? throw new InvalidOperationException( $"Asset '{scenePath}' was not found." );

		asset.ClearInMemoryReplacement();
		session.Reload();

		return new
		{
			Reloaded = true,
			DiscardedChanges = discardedChanges,
			Asset = scenePath
		};
	}
}
