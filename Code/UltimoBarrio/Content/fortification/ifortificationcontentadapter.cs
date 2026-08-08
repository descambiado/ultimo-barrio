using System;

namespace UltimoBarrio.Content.Fortification
{
	/// <summary>
	/// Frontera de adaptador para estructuras construibles de contenido (barricadas,
	/// puertas, alijos, estaciones, generadores...). El core nuevo consumirá esta
	/// interfaz; el pack la implementa de forma autocontenida (BuildStructureHost).
	/// </summary>
	public interface IFortificationContentAdapter
	{
		BuildDefinition Definition { get; }
		float Health { get; }
		bool IsDead { get; }

		void TakeDamage( Content.ContentDamageEvent damageEvent );
		bool Repair( float amount, Func<int, bool> tryConsume );
		void Upgrade();
	}
}
