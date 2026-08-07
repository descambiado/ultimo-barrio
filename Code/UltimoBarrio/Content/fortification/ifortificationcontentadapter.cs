namespace UltimoBarrio.Content.Fortification
{
	/// <summary>
	/// Frontera de adaptador para fortificaciones de contenido (barricadas, puertas,
	/// alijos, estaciones, generadores...). El core nuevo consumirá esta interfaz;
	/// el pack la implementa de forma autocontenida (FortificationContentHost).
	/// </summary>
	public interface IFortificationContentAdapter
	{
		FortificationContentDefinition Definition { get; }
		float Health { get; }
		bool IsDead { get; }

		void TakeDamage( Content.ContentDamageEvent damageEvent );
		void Repair( float amount );
		void Upgrade();
	}
}
