using Sandbox;

namespace UltimoBarrio.Core
{
    /// <summary>
    /// Interruptores de sistemas. Todos activos por defecto en la vertical
    /// slice; pueden apagarse para pruebas aisladas. Los sistemas nuevos
    /// (IA, raids, economía, reloj, combate) deben respetar su flag.
    /// </summary>
    public static class FeatureFlags
    {
        public static bool EnableEconomy { get; set; } = true;
        public static bool EnableCombat { get; set; } = true;
        public static bool EnableRaids { get; set; } = true;
        public static bool EnableWorldClock { get; set; } = true;
        public static bool EnableAI { get; set; } = true;
    }
}
