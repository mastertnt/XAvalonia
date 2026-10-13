using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using XAvalonia.Controls.Editors;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Sample.Views;

/// <summary>
/// Example of a custom <see cref="IPropertyEditor"/>: edits <see cref="Color"/> values with a swatch,
/// a hexadecimal text box and a few preset colors. Registered by <see cref="XAvaloniaSampleViewsPlugin"/>.
/// </summary>
public sealed class ColorPropertyEditor : PropertyEditorBase<ColorPropertyEditor.EditorPanel>
{
    private static readonly Color[] Presets =
    {
        Colors.Transparent,
        Color.Parse("#1E1E1E"),
        Color.Parse("#252526"),
        Color.Parse("#002B36"),
        Color.Parse("#2D2A2E"),
        Color.Parse("#FDF6E3"),
    };

    /// <inheritdoc/>
    protected override EditorPanel CreateControl(IPropertyItem pItem) => new EditorPanel();

    /// <inheritdoc/>
    protected override void UpdateControl(EditorPanel pControl, IPropertyItem pItem)
    {
        Color lColor = pItem.Value is Color lValue ? lValue : Colors.Transparent;
        pControl.Swatch.Background = new SolidColorBrush(lColor);
        pControl.HexBox.Text = lColor.ToString();
    }

    /// <inheritdoc/>
    protected override void ObserveControl(EditorPanel pControl, IPropertyItem pItem, Action<object?> pCommit)
    {
        void CommitText()
        {
            if (Color.TryParse(pControl.HexBox.Text, out Color lColor))
            {
                pCommit(lColor);
            }
            else if (pItem.Value is Color lCurrent)
            {
                pControl.HexBox.Text = lCurrent.ToString();
            }
        }

        pControl.HexBox.LostFocus += (_, _) => CommitText();
        pControl.HexBox.KeyDown += (_, pArgs) =>
        {
            if (pArgs.Key == Key.Enter)
            {
                CommitText();
            }
        };

        foreach (Color lPreset in Presets)
        {
            Button lButton = new Button
            {
                Width = 22,
                Height = 22,
                Padding = new Thickness(0),
                Background = new SolidColorBrush(lPreset),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
            };
            ToolTip.SetTip(lButton, lPreset.ToString());
            lButton.Click += (_, _) => pCommit(lPreset);
            pControl.PresetPanel.Children.Add(lButton);
        }
    }

    /// <summary>Editor layout: swatch, hexadecimal text box and preset buttons.</summary>
    public sealed class EditorPanel : StackPanel
    {
        /// <summary>Initializes the layout.</summary>
        public EditorPanel()
        {
            Orientation = Orientation.Horizontal;
            Spacing = 8;
            Children.Add(Swatch);
            Children.Add(HexBox);
            Children.Add(PresetPanel);
        }

        /// <summary>Shows the current color.</summary>
        public Border Swatch { get; } = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(4),
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
        };

        /// <summary>Hexadecimal value (e.g. <c>#FF1E1E1E</c>) or color name.</summary>
        public TextBox HexBox { get; } = new TextBox { Width = 130, VerticalAlignment = VerticalAlignment.Center };

        /// <summary>Preset color buttons.</summary>
        public StackPanel PresetPanel { get; } = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }
}
