namespace UltimoBarrio.Content.Dev
{
	/// <summary>Estado final de una suite de laboratorio (salida machine-readable [UBSuite]).</summary>
	public enum LabSuiteStatus
	{
		Pass,
		Fail,
		Skip
	}

	/// <summary>
	/// Resultado de una suite. <see cref="Detail"/> es la parte machine-readable que
	/// el runner emite tras el prefijo [UBSuite] &lt;Domain&gt;.&lt;Name&gt;:
	///   time=&lt;segundos&gt;s delta=&lt;magnitud&gt; state=&lt;estado&gt;
	/// </summary>
	public struct LabSuiteResult
	{
		public LabSuiteStatus Status;
		public float TimeSeconds; // duración de la suite
		public float Delta;       // magnitud principal medida (daño, distancia t0→t2, HP, ...)
		public string State;      // estado final ("complete", "skip:&lt;razón&gt;", "error:&lt;...&gt;", "fail:&lt;razón&gt;")
		public string Detail;     // "time=..s delta=.. state=.." (machine-readable)

		public static LabSuiteResult Pass( float time, float delta, string state )
		{
			return new LabSuiteResult
			{
				Status = LabSuiteStatus.Pass,
				TimeSeconds = time,
				Delta = delta,
				State = state,
				Detail = $"time={time:F2}s delta={delta:F1} state={state}"
			};
		}

		public static LabSuiteResult Fail( float time, float delta, string state )
		{
			return new LabSuiteResult
			{
				Status = LabSuiteStatus.Fail,
				TimeSeconds = time,
				Delta = delta,
				State = state,
				Detail = $"time={time:F2}s delta={delta:F1} state={state}"
			};
		}

		public static LabSuiteResult Skip( string reason )
		{
			return new LabSuiteResult
			{
				Status = LabSuiteStatus.Skip,
				TimeSeconds = 0f,
				Delta = 0f,
				State = "skip",
				Detail = $"time=0.00s delta=0.0 state=skip ({reason})"
			};
		}
	}

	/// <summary>
	/// Contrato de una suite de laboratorio ejecutable por <see cref="ContentRuntimeSuiteRunner"/>.
	/// Infra QA, SOLO en UltimoBarrio.Content.Dev (nunca producción).
	///
	/// Anti-falsificación (crítico): la suite NUNCA llama internals que eviten la ruta
	/// real probada (daño, navegación, placement, traces). El rig solo sustituye input
	/// humano y target humano; el PASS se deriva de los logs de la ruta real
	/// (host → trace → IDamageTarget / NavMeshAgent / validación server).
	/// </summary>
	public interface ILabSuite
	{
		/// <summary>Dominio de la suite: "Weapon", "Enemy", "Building", "Vehicle".</summary>
		string Domain { get; }

		/// <summary>Etiqueta del test dentro del dominio ([UBSuite] &lt;Domain&gt;.&lt;Name&gt;).</summary>
		string Name { get; }

		/// <summary>Setup previo (contexto, targets, fixtures). Se llama UNA vez al arrancar la suite.</summary>
		void Initialize();

		/// <summary>Avanza un paso de la suite (cada frame mientras no esté completa).</summary>
		void Step( float dt );

		/// <summary>True cuando la suite terminó; Step deja de llamarse.</summary>
		bool IsComplete { get; }

		/// <summary>Resultado final (Pass/Fail/Skip + Detail). Válido cuando IsComplete.</summary>
		LabSuiteResult Result { get; }
	}
}
