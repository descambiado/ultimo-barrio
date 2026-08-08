namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// VehicleSuite — suite ILabSuite del dominio Vehicle (CONTRATO, Worker E).
	///
	/// Pipeline (ub-spike-factory): kit de vehículos EXTERNO (PRIMARY, sin física
	/// propia) → VehicleTestRig → logs [VehicleLab] &lt;Label&gt; ... → PASS/FAIL.
	/// Secuencia prevista: spawn → enter → throttle → steer → brake → reverse → exit.
	///
	/// Anti-falsificación: el movimiento se mide por la ruta real (throttle/steer/
	/// brake del kit, estado del vehículo tras cada input); NUNCA fijar posiciones
	/// ni llamar internals para fabricar el resultado.
	///
	/// Estado actual: stub de contrato — Initialize devuelve SKIP honesto hasta que
	/// Worker E implemente la lógica (depende del research del paquete de vehículos,
	/// manifest bloque H). Registrar en ContentRuntimeSuite desde el rig del
	/// vehicle_lab cuando exista.
	/// </summary>
	public sealed class VehicleSuite : ILabSuite
	{
		public string Domain => "Vehicle";
		public string Name { get; }
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		public VehicleSuite( string label )
		{
			Name = label;
		}

		public void Initialize()
		{
			IsComplete = true;
			Result = LabSuiteResult.Skip( "contrato Worker E: kit externo PRIMARY; spawn→enter→throttle→steer→brake→reverse→exit; sin lógica aún" );
		}

		public void Step( float dt )
		{
			// Nada: la suite se marca completa en Initialize hasta que Worker E implemente.
		}
	}
}
