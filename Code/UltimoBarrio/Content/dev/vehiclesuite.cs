namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// VehicleSuite — suite ILabSuite del dominio Vehicle (reporte unificado QA).
	///
	/// El motor de la suite es el <see cref="VehicleTestRig"/> (kit externo PRIMARY
	/// fieldguide.vehiclephysics, sin física propia). Este objeto es el reporter:
	/// el rig registra una instancia, la ejecuta el runner unificado
	/// (ContentRuntimeSuiteRunner) y al terminar la secuencia el rig llama a
	/// <see cref="Complete"/> con el resultado real (PASS solo si los 8 pasos
	/// pasaron por deltas reales de posición/yaw/velocidad).
	///
	/// Secuencia (contrato): spawn → enter → throttle → steer → brake → reverse → exit.
	/// </summary>
	public sealed class VehicleSuite : ILabSuite
	{
		public string Domain => "Vehicle";
		public string Name { get; }
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		private LabSuiteResult _pending;
		private bool _hasPending;

		public VehicleSuite( string label )
		{
			Name = label;
		}

		/// <summary>El rig entrega el resultado final (una vez).</summary>
		public void Complete( LabSuiteResult result )
		{
			if ( _hasPending ) return;
			_pending = result;
			_hasPending = true;
		}

		public void Initialize()
		{
			// Nada que preparar: el rig gestiona fixtures/cámara/spawn.
		}

		public void Step( float dt )
		{
			if ( !IsComplete && _hasPending )
			{
				Result = _pending;
				IsComplete = true;
			}
		}
	}
}
