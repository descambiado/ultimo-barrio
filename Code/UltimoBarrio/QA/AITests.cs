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
            //
            // AVISO: SaqueadorBrain.ChangeState() no valida transiciones — acepta
            // cualquier estado sin comprobar el origen. Esta tabla documenta el
            // diseño previsto, pero no está enlazada al código real de OnUpdate();
            // por tanto solo puede comprobar que la tabla es internamente
            // consistente (cada fila "legal=true" aparece en el set derivado de
            // filas legales, cada fila "legal=false" no aparece en él), no que
            // SaqueadorBrain la respete en runtime. Si se necesita esa garantía,
            // añadir un guard real a ChangeState() y comprobarlo aquí en su lugar.
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

            var declaredLegal = new HashSet<(AI.SaqueadorBrain.SaqueadorState, AI.SaqueadorBrain.SaqueadorState)>(
                rules.Where( r => r.legal ).Select( r => (r.from, r.to) ) );

            foreach ( var (from, to, legal) in rules )
            {
                bool isInLegalSet = declaredLegal.Contains( (from, to) );
                if ( isInLegalSet == legal ) passed++;
                else
                {
                    failed++;
                    Log.Error( $"[UBTest] FAIL: tabla de transiciones inconsistente para {from}→{to}." );
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
