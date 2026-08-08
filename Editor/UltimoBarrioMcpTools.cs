// SPDX-License-Identifier: MPL-2.0

using System;

namespace Editor.Mcp;

[McpToolset( "ultimo_barrio", "Safe project-specific editor operations for Ultimo Barrio." )]
public static class UltimoBarrioSceneTools
{
	private const string MainScenePath = "scenes/main.scene";

	/// <summary>
	/// Open a scene asset in the editor (activates its tab). Refuses to run during Play Mode.
	/// </summary>
	/// <param name="path">Scene resource path, e.g. "scenes/spikes/building_lab.scene".</param>
	[McpTool( "open_scene" )]
	public static object OpenScene( string path )
	{
		if ( SceneEditorSession.Active?.IsPlaying == true )
			throw new InvalidOperationException( "Stop Play Mode before opening a scene." );

		var asset = AssetSystem.FindByPath( path )
			?? throw new InvalidOperationException( $"Asset '{path}' was not found." );

		asset.OpenInEditor();

		return new
		{
			Opened = true,
			Asset = path
		};
	}

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

		var discardedChanges = session.HasUnsavedChanges;
		var asset = AssetSystem.FindByPath( MainScenePath )
			?? throw new InvalidOperationException( $"Asset '{MainScenePath}' was not found." );

		asset.ClearInMemoryReplacement();
		session.Reload();

		return new
		{
			Reloaded = true,
			DiscardedChanges = discardedChanges,
			Asset = MainScenePath
		};
	}
}
