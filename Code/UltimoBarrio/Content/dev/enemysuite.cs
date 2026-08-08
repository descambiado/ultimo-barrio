namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// EnemySuite — suite ILabSuite del dominio Enemy (CONTRATO, Worker A).
	///
	/// Pipeline (ub-spike-factory): EnemyContentDefinition/Host/Perception/Attack/
	/// LootDefinition → prefabs → EnemyTestRig (NavMesh REAL, sin teleport) →
	/// logs [EnemyLab] &lt;Label&gt; ... → PASS/FAIL. Escalera de evidencia:
	/// COMPILA → CARGA → VISIBLE → INTERACTÚA → FUNCIONA (navegación real
	/// t0/t1/t2 sobre NavMeshAgent, ataque con daño before/after/delta).
	///
	/// Anti-falsificación: medir la navegación con NavMeshAgent real y ataque por
	/// la ruta real (host → percepción → ataque → IDamageTarget); NUNCA teletransportar
	/// ni llamar internals para fabricar t0/t1/t2.
	///
	/// Estado actual: stub de contrato — Initialize devuelve SKIP honesto hasta que
	/// Worker A implemente la lógica. Registrar en ContentRuntimeSuite desde el rig
	/// del enemy_lab cuando exista.
	/// </summary>
	public sealed class EnemySuite : ILabSuite
	{
		public string Domain => "Enemy";
		public string Name { get; }
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		public EnemySuite( string label )
		{
			Name = label;
		}

		public void Initialize()
		{
			IsComplete = true;
			Result = LabSuiteResult.Skip( "contrato Worker A: navegación NavMesh real (t0/t1/t2) + ataque IDamageTarget; sin lógica aún" );
		}

		public void Step( float dt )
		{
			// Nada: la suite se marca completa en Initialize hasta que Worker A implemente.
		}
	}
}
