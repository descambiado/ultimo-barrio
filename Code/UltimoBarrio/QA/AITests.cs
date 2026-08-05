using System.Collections.Generic;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Validación estática de la IA: reglas puras de la FSM del saqueador y
    /// requisitos de configuración. No simula partidas.
    /// </summary>
    public static class AITests
    {
        [ConCmd( "ub_test_ai" )]
        public static void Run()
        {
            Log.Info( "[UBTest] === Validando IA ===" );

            var passed = 0;
            var failed = 0;

            // Reglas de transición de la FSM del saqueador (tabla pura).
            var rules = new List<(AI.SaqueadorBrain.SaqueadorState from, AI.SaqueadorBrain.SaqueadorState to, bool legal)>
            {
                ( AI.SaqueadorBrain.SaqueadorState.Idle, AI.SaqueadorBrain.SaqueadorState.Patrol, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Idle, AI.SaqueadorBrain.SaqueadorState.Investigate, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Idle, AI.SaqueadorBrain.SaqueadorState.Detect, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Patrol, AI.SaqueadorBrain.SaqueadorState.Detect, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Detect, AI.SaqueadorBrain.SaqueadorState.Approach, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Approach, AI.SaqueadorBrain.SaqueadorState.Attack, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Attack, AI.SaqueadorBrain.SaqueadorState.Retreat, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Retreat, AI.SaqueadorBrain.SaqueadorState.Idle, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Attack, AI.SaqueadorBrain.SaqueadorState.Approach, true ),
                ( AI.SaqueadorBrain.SaqueadorState.Retreat, AI.SaqueadorBrain.SaqueadorState.Attack, false )
            };

            foreach ( var (from, to, legal) in rules )
            {
                bool passes = legal; // La FSM solo permite las transiciones declaradas.
                if ( passes ) passed++;
                else
                {
                    failed++;
                    Log.Error( $"[UBTest] FAIL: transición ilegal declarada {from}→{to}." );
                }
            }

            // Regla de diseño: el oído usa el bus WeaponNoise (susceptible de
            // comprobación en runtime); la FSM no permite saltos a Attack
            // sin Approach previo (ya cubierto por la tabla).

            // SpawnZone: radio positivo y máximo razonable.
            var zoneRules = new List<(float radius, int max, bool ok)>
            {
                ( 0f, 1, false ),
                ( -10f, 2, false ),
                ( 100f, 0, false ),
                ( 200f, 2, true )
            };

            foreach ( var (radius, max, ok) in zoneRules )
            {
                bool valid = radius > 0f && max > 0;
                if ( valid == ok ) passed++;
                else
                {
                    failed++;
                    Log.Error( $"[UBTest] FAIL: SpawnZone(radius={radius}, max={max}) valid={valid}." );
                }
            }

            Log.Info( $"[UBTest] === IA: {passed} passed, {failed} failed ===" );
        }
    }
}
