namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// BuildingSuite — suite ILabSuite del dominio Building (CONTRATO, Worker C).
	///
	/// Pipeline (ub-spike-factory): BuildDefinition + BuildPlacementRules +
	/// BuildStructureHost + BuildHealth + Repair + Upgrade → prefabs →
	/// BuildingTestRig → logs [BuildingLab] &lt;Label&gt; ... → PASS/FAIL.
	/// Secuencia prevista: preview → invalid rejected → valid accepted → rotation →
	/// spawn → HP → damage (ruta real) → repair → upgrade → destroy.
	///
	/// Anti-falsificación: placement validado por BuildPlacementRules real (preview
	/// inválido rechazado, válido aceptado) y daño por IDamageTarget; NUNCA spawnear
	/// directo ni saltarse la validación para fabricar el PASS.
	///
	/// Estado actual: stub de contrato — Initialize devuelve SKIP honesto hasta que
	/// Worker C implemente la lógica. Registrar en ContentRuntimeSuite desde el rig
	/// del building_lab cuando exista.
	/// </summary>
	public sealed class BuildingSuite : ILabSuite
	{
		public string Domain => "Building";
		public string Name { get; }
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		public BuildingSuite( string label )
		{
			Name = label;
		}

		public void Initialize()
		{
			IsComplete = true;
			Result = LabSuiteResult.Skip( "contrato Worker C: preview→invalid rejected→valid accepted→rotation→spawn→HP→damage→repair→upgrade→destroy; sin lógica aún" );
		}

		public void Step( float dt )
		{
			// Nada: la suite se marca completa en Initialize hasta que Worker C implemente.
		}
	}
}
