using Sandbox;

namespace UltimoBarrio.Core
{
    public static class FeatureFlags
    {
        public static bool EnableEconomy { get; set; } = false;
        public static bool EnableCombat { get; set; } = false;
        public static bool EnableRaids { get; set; } = false;
        public static bool EnableWorldClock { get; set; } = false;
        public static bool EnableAI { get; set; } = false;
    }
}
