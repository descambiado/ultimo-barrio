using System;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Runner maestro de validación estática: ejecuta todos los validadores
    /// (items, melee, loot, movimiento, IA, fortificación, persistencia,
    /// gameplay y los unit tests existentes de UltimoBarrioTests).
    /// Solo consulta estados y lógica pura.
    /// </summary>
    public static class TestRunner
    {
        [ConCmd( "ub_test_all" )]
        public static void RunAll()
        {
            Log.Info( "[UBTest] === Ejecutando suite de validación completa ===" );

            ItemRegistryTests.Run();
            MeleeLogicTests.Run();
            LootTests.Run();
            MovementTests.Run();
            AITests.Run();
            FortificationTests.Run();
            PersistenceTests.Run();
            GameplayTests.Run();
            UltimoBarrioTests.RunAll();

            Log.Info( "[UBTest] === Suite completada ===" );
        }
    }
}
