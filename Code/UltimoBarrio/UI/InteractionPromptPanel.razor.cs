using Sandbox;
using Sandbox.UI;
using System;

namespace UltimoBarrio.UI
{
    public partial class InteractionPromptPanel : Panel
    {
        public string TitleText { get; set; } = "";
        public string SubTitleText { get; set; } = "";
        
        public bool IsVisible { get; set; } = false;

        public void Show(string title, string subtitle = "")
        {
            TitleText = title;
            SubTitleText = subtitle;
            IsVisible = true;
            StateHasChanged();
        }

        public void Hide()
        {
            IsVisible = false;
            StateHasChanged();
        }

        protected override int BuildHash()
        {
            return HashCode.Combine(TitleText, SubTitleText, IsVisible);
        }
    }
}
