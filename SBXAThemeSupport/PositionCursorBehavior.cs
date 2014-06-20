namespace SBXAThemeSupport
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interactivity;

    /// <summary>
    /// Positions the cursor at the end of the text.
    /// </summary>
    /// <example>
    /// <code lang="XAML">
    /// <TextBox>
    ///     <i:Interaction.Behaviors>
    ///        <sbxaThemeSupport:PositionCursorBehavior/>
    ///     </i:Interaction.Behaviors>
    /// </TextBox>
    /// </code>
    /// </example>
    public class PositionCursorBehavior : Behavior<UIElement>
    {
        private TextBox textBox;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.textBox = AssociatedObject as TextBox;

            if (this.textBox == null)
            {
                return;
            }
            this.textBox.GotFocus += TextBoxGotFocus;
        }

        protected override void OnDetaching()
        {
            if (this.textBox == null)
            {
                return;
            }
            this.textBox.GotFocus -= TextBoxGotFocus;

            base.OnDetaching();
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            this.textBox.CaretIndex = this.textBox.Text.Length;
        }
    }
}
