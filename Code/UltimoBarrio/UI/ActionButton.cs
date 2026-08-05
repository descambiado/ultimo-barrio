using System;
using Sandbox.UI;

namespace UltimoBarrio.UI
{
    /// <summary>Botón mínimo de la UI clásica de Sandbox.UI.</summary>
    public sealed class ActionButton : Panel
    {
        private readonly Label _label;
        private Action _action;

        public ActionButton( string text, Action action )
        {
            _action = action;

            Style.BackgroundColor = new Color( 0.2f, 0.2f, 0.24f, 0.95f );
            Style.Padding = 6;
            // Style.BorderRadius = 4; // Not available in current sbox PanelStyle API
            Style.FontSize = 14;
            Style.FontColor = Color.White;
            Style.Cursor = "pointer";
            Style.PointerEvents = PointerEvents.All;

            _label = AddChild<Label>();
            _label.Text = text;
            _label.Style.FontSize = 14;
            _label.Style.FontColor = Color.White;

            AddEventListener( "onclick", () => _action?.Invoke() );
        }

        public void SetText( string text )
        {
            if ( _label is not null )
                _label.Text = text;
        }
    }
}
