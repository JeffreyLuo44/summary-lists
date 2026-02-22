namespace SummaryLists.Mobile.Extensions;

public static class MauiPropertyExtensions
{
    public static T Bind<T>(
        this T bindable,
        BindableProperty targetProperty,
        string path,
        BindingMode mode = BindingMode.Default,
        object? source = null)
        where T : BindableObject
    {
        bindable.SetBinding(targetProperty, new Binding(path, mode: mode, source: source));
        return bindable;
    }

    public static T Column<T>(this T view, int column)
        where T : View
    {
        Grid.SetColumn(view, column);
        return view;
    }

    public static Label Text(this Label label, string text)
    {
        label.Text = text;
        return label;
    }

    public static Label FontSize(this Label label, double value)
    {
        label.FontSize = value;
        return label;
    }

    public static Label Bold(this Label label)
    {
        label.FontAttributes = FontAttributes.Bold;
        return label;
    }

    public static Label TextColor(this Label label, Color value)
    {
        label.TextColor = value;
        return label;
    }

    public static Label CenterVertical(this Label label)
    {
        label.VerticalOptions = LayoutOptions.Center;
        return label;
    }

    public static Button Text(this Button button, string text)
    {
        button.Text = text;
        return button;
    }

    public static Button BackgroundColor(this Button button, Color color)
    {
        button.BackgroundColor = color;
        return button;
    }

    public static Button TextColor(this Button button, Color color)
    {
        button.TextColor = color;
        return button;
    }

    public static Entry Placeholder(this Entry entry, string placeholder)
    {
        entry.Placeholder = placeholder;
        return entry;
    }

    public static Entry Keyboard(this Entry entry, Keyboard keyboard)
    {
        entry.Keyboard = keyboard;
        return entry;
    }

    public static Entry Password(this Entry entry, bool isPassword = true)
    {
        entry.IsPassword = isPassword;
        return entry;
    }

    public static VerticalStackLayout Padding(this VerticalStackLayout layout, double value)
    {
        layout.Padding = value;
        return layout;
    }

    public static VerticalStackLayout Spacing(this VerticalStackLayout layout, double value)
    {
        layout.Spacing = value;
        return layout;
    }

    public static Grid ColumnSpacing(this Grid grid, double value)
    {
        grid.ColumnSpacing = value;
        return grid;
    }
}
