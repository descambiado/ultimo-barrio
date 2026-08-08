using Sandbox;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Fixture de conductor del vehicle_lab (SOLO dev, UltimoBarrio.Content.Dev).
	///
	/// Sustituye al jugador humano en el autotest: un cuerpo simple (crate01) que se
	/// monta/desmonta del vehículo probando el flujo enter/exit por la ruta real de
	/// parenteo (GameObject.Parent / SetParent, API verificada 26.08.05).
	///
	/// NO aporta física ni input: el movimiento lo produce el kit externo PRIMARY
	/// (fieldguide.vehiclephysics) leyendo las input actions que el rig simula
	/// (Input.SetAction). La dirección única es rig → kit; el fixture solo "se sienta".
	/// </summary>
	[Title( "Vehicle Driver Fixture" )]
	[Category( "Último Barrio — Content (Dev)" )]
	public sealed class VehicleDriverFixture : Component
	{
		/// <summary>Vehículo al que está montado el driver (null si está fuera).</summary>
		public GameObject Vehicle { get; private set; }

		/// <summary>True cuando el driver está dentro del vehículo (parenteado a él).</summary>
		public bool IsInside => Vehicle.IsValid() && GameObject.Parent == Vehicle;

		/// <summary>
		/// Enter: monta el driver en el vehículo (attach). Devuelve true si el parenteo
		/// se aplicó (ruta real: GameObject.Parent). El offset de asiento es en espacio local.
		/// </summary>
		public bool Enter( GameObject vehicle, Vector3 seatOffsetLocal )
		{
			if ( vehicle == null || !vehicle.IsValid() )
			{
				Vehicle = null;
				return false;
			}

			Vehicle = vehicle;
			GameObject.Parent = vehicle;
			GameObject.LocalPosition = seatOffsetLocal;
			GameObject.LocalRotation = Rotation.Identity;

			return GameObject.Parent == vehicle;
		}

		/// <summary>
		/// Exit: desmonta el driver (detach). Devuelve true si quedó sin padre.
		/// </summary>
		public bool Exit()
		{
			if ( !Vehicle.IsValid() )
			{
				Vehicle = null;
				return true; // ya estaba fuera
			}

			GameObject.Parent = null;
			Vehicle = null;

			return GameObject.Parent == null;
		}
	}
}
