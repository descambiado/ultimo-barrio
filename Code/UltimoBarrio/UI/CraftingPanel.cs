using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace UltimoBarrio.UI
{
    /// <summary>
    /// Panel de crafteo: lista recetas, muestra ingredientes y botón Fabricar.
    /// Cada fabricación es una intención [Rpc.Host] hacia la estación.
    /// </summary>
    public class CraftingPanel : PanelComponent
    {
        [Property] public Crafting.CraftingStation TargetStation { get; set; }
        [Property] public InventoryComponent PlayerInventory { get; set; }

        private Panel _container;
        private Label _titleLabel;

        protected override void OnStart()
        {
            if ( Panel != null )
            {
                Panel.Style.Display = DisplayMode.None;
                Panel.Style.Position = PositionMode.Absolute;
                Panel.Style.Left = Length.Percent( 25 );
                Panel.Style.Top = Length.Percent( 20 );
                Panel.Style.Width = Length.Pixels( 520 );
                Panel.Style.BackgroundColor = new Color( 0.12f, 0.12f, 0.12f, 0.92f );
                Panel.Style.Padding = 16;
                Panel.Style.FlexDirection = FlexDirection.Column;
                Panel.Style.PointerEvents = PointerEvents.All;

                _titleLabel = Panel.AddChild<Label>();
                _titleLabel.Text = "ESTACIÓN DE CRAFTEO";
                _titleLabel.Style.FontSize = 22;
                _titleLabel.Style.FontWeight = 800;
                _titleLabel.Style.FontColor = new Color( 0.97f, 0.97f, 1f );
                _titleLabel.Style.MarginBottom = 12;

                _container = Panel.AddChild<Panel>();
                _container.Style.FlexDirection = FlexDirection.Column;
            }
        }

        public void Open( Crafting.CraftingStation station, InventoryComponent playerInventory )
        {
            TargetStation = station;
            PlayerInventory = playerInventory;
            if ( Panel != null )
                Panel.Style.Display = DisplayMode.Flex;
        }

        public void Close()
        {
            TargetStation = null;
            PlayerInventory = null;
            if ( Panel != null )
                Panel.Style.Display = DisplayMode.None;
        }

        private int _lastHash = -1;

        protected override void OnUpdate()
        {
            if ( TargetStation == null || PlayerInventory == null || _container == null )
                return;

            var recipes = TargetStation.AvailableRecipes;
            int hash = recipes.Count;
            foreach ( var recipe in recipes )
            {
                hash = System.HashCode.Combine( hash, recipe.RecipeId );
                foreach ( var ingredient in recipe.Ingredients )
                    hash = System.HashCode.Combine( hash, ingredient.ItemId, ingredient.Amount, PlayerInventory.GetCount( ingredient.ItemId ) );
            }

            if ( hash == _lastHash && _container.ChildrenCount > 0 )
                return;

            _lastHash = hash;
            _container.DeleteChildren( true );

            foreach ( var recipe in recipes )
            {
                bool canCraft = CanCraft( recipe );

                var row = _container.AddChild<Panel>();
                row.Style.FlexDirection = FlexDirection.Row;
                row.Style.AlignItems = Align.Center;
                row.Style.Padding = 6;
                row.Style.BackgroundColor = new Color( 0.16f, 0.16f, 0.2f, 0.9f );

                var info = row.AddChild<Panel>();
                info.Style.FlexDirection = FlexDirection.Column;
                info.Style.Width = Length.Pixels( 300 );

                var nameLbl = info.AddChild<Label>();
                nameLbl.Text = recipe.DisplayName;
                nameLbl.Style.FontSize = 16;
                nameLbl.Style.FontWeight = 700;
                nameLbl.Style.FontColor = Color.White;

                var ingredientsText = string.Join( "  ", recipe.Ingredients.Select( i =>
                {
                    var def = ItemRegistry.GetDefinition( i.ItemId );
                    var name = def?.DisplayName ?? i.ItemId;
                    int have = PlayerInventory.GetCount( i.ItemId );
                    return $"{( have >= i.Amount ? "" : "FALTA " )}{name} {have}/{i.Amount}";
                } ) );

                var ingLbl = info.AddChild<Label>();
                ingLbl.Text = ingredientsText;
                ingLbl.Style.FontSize = 12;
                ingLbl.Style.FontColor = new Color( 0.8f, 0.8f, 0.8f );

                var resultDef = ItemRegistry.GetDefinition( recipe.Result.ItemId );
                var resultLbl = info.AddChild<Label>();
                resultLbl.Text = $"→ {recipe.Result.Amount}x {resultDef?.DisplayName ?? recipe.Result.ItemId}";
                resultLbl.Style.FontSize = 12;
                resultLbl.Style.FontColor = new Color( 0.9f, 0.8f, 0.4f );

                var craftButton = new ActionButton(
                    canCraft ? "Fabricar" : "Falta material",
                    () =>
                    {
                        if ( TargetStation is not null && PlayerInventory is not null )
                            TargetStation.RequestCraft( PlayerInventory.GameObject.Id, recipe.RecipeId );
                    } );
                row.AddChild( craftButton );
            }
        }

        private bool CanCraft( Crafting.CraftingRecipe recipe )
        {
            foreach ( var ingredient in recipe.Ingredients )
            {
                if ( PlayerInventory.GetCount( ingredient.ItemId ) < ingredient.Amount )
                    return false;
            }

            return true;
        }
    }
}
