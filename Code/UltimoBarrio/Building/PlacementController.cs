using Sandbox;
using System.Linq;
using UltimoBarrio.Fortification;
using UltimoBarrio.Inventory;

namespace UltimoBarrio.Building
{
    /// <summary>
    /// Capa de preview visual sobre el flujo de colocación ya probado
    /// (BarricadeAnchor/FurnitureAnchor + su propia validación host-autoritativa
    /// intacta). No decide si la colocación es válida -- solo refleja
    /// visualmente lo que CanInteract() del anchor real ya decidiría, y deja
    /// que el mismo OnInteract() de producción confirme. Rotar solo cambia la
    /// orientación de la previsualización; el anchor sigue colocando en su
    /// propia posición/rotación fija, así que rotar no tiene efecto real
    /// todavía sobre anchors fijos -- queda listo para cuando exista colocación
    /// libre dentro de un BuildVolume.
    /// </summary>
    [Title( "Placement Controller" )]
    [Category( "Último Barrio — Building" )]
    [Icon( "view_in_ar" )]
    public sealed class PlacementController : Component
    {
        [Property] public float TraceRange { get; set; } = 200f;

        private GameObject _ghost;
        private ModelRenderer _ghostRenderer;
        private float _previewYaw;

        private static readonly string[] _placeableKitIds =
        {
            "wooden_barricade_kit", "reinforced_barricade_kit", "storage_crate_kit"
        };

        protected override void OnUpdate()
        {
            if ( IsProxy )
                return;

            var held = Components.Get<HeldItemController>();
            var activeItemId = held?.ActiveItemId;

            if ( string.IsNullOrEmpty( activeItemId ) || !_placeableKitIds.Contains( activeItemId ) )
            {
                HideGhost();
                return;
            }

            if ( Input.Pressed( "Reload" ) ) // reutiliza R como rotar 45° -- no hay acción "Rotate" dedicada en Input.config.
                _previewYaw = ( _previewYaw + 45f ) % 360f;

            var pc = Components.Get<Sandbox.PlayerController>();
            var rayPos = WorldPosition + Vector3.Up * 64f;
            var rayDir = pc?.EyeAngles.Forward ?? WorldRotation.Forward;

            var tr = Scene.Trace.Ray( rayPos, rayPos + rayDir * TraceRange )
                .IgnoreGameObjectHierarchy( GameObject )
                .Run();

            if ( !tr.Hit )
            {
                HideGhost();
                return;
            }

            var barricadeAnchor = tr.GameObject.Components.Get<BarricadeAnchor>();
            var furnitureAnchor = tr.GameObject.Components.Get<FurnitureAnchor>();

            bool valid;
            Vector3 previewPos;
            Rotation previewRot;

            if ( barricadeAnchor is not null && !barricadeAnchor.HasBarricade )
            {
                valid = _placeableKitIds.Contains( activeItemId ) && activeItemId != "storage_crate_kit";
                previewPos = barricadeAnchor.WorldPosition;
                previewRot = barricadeAnchor.WorldRotation;
            }
            else if ( furnitureAnchor is not null && !furnitureAnchor.HasFurniture )
            {
                valid = activeItemId == "storage_crate_kit";
                previewPos = furnitureAnchor.WorldPosition;
                previewRot = furnitureAnchor.WorldRotation;
            }
            else
            {
                HideGhost();
                return;
            }

            ShowGhost( previewPos, previewRot * Rotation.FromYaw( _previewYaw ), valid );
        }

        private void ShowGhost( Vector3 pos, Rotation rot, bool valid )
        {
            if ( _ghost is null )
            {
                _ghost = new GameObject( true, "PlacementGhost" );
                _ghostRenderer = _ghost.Components.Create<ModelRenderer>();
                _ghostRenderer.Model = Model.Load( "models/citizen_props/crate01.vmdl" );
            }

            _ghost.WorldPosition = pos;
            _ghost.WorldRotation = rot;
            _ghostRenderer.Tint = valid
                ? new Color( 0.2f, 1f, 0.2f, 0.45f )
                : new Color( 1f, 0.2f, 0.2f, 0.45f );
        }

        private void HideGhost()
        {
            if ( _ghost is null )
                return;

            _ghost.Destroy();
            _ghost = null;
            _ghostRenderer = null;
        }

        protected override void OnDestroy()
        {
            HideGhost();
        }
    }
}
