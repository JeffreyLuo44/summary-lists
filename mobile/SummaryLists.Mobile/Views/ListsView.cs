using Microsoft.Maui.Controls.Shapes;
using SummaryLists.Mobile.Extensions;
using SummaryLists.Mobile.Models;
using SummaryLists.Mobile.ViewModels;

namespace SummaryLists.Mobile.Views;

public sealed class ListsView : ContentPage
{
    private readonly ListsViewModel _viewModel;

    public ListsView(ListsViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;

        Title = "Lists";

        var emailLabel = new Label()
            .Bold()
            .CenterVertical()
            .Bind(Label.TextProperty, nameof(ListsViewModel.SessionEmail));

        var logoutButton = new Button()
            .Text("Log Out")
            .Bind(Button.CommandProperty, nameof(ListsViewModel.LogoutCommand))
            .Bind(IsEnabledProperty, nameof(ListsViewModel.IsNotBusy));

        var sessionRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        sessionRow.Children.Add(emailLabel);
        sessionRow.Children.Add(logoutButton.Column(1));

        var newListEntry = new Entry()
            .Placeholder("Create new list...")
            .Bind(Entry.TextProperty, nameof(ListsViewModel.NewListTitle), BindingMode.TwoWay);

        var addListButton = new Button()
            .Text("Add")
            .Bind(Button.CommandProperty, nameof(ListsViewModel.AddListCommand))
            .Bind(IsEnabledProperty, nameof(ListsViewModel.IsNotBusy));

        var newListRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        }.ColumnSpacing(8);
        newListRow.Children.Add(newListEntry);
        newListRow.Children.Add(addListButton.Column(1));

        var listTabs = new CollectionView
        {
            HeightRequest = 70,
            SelectionMode = SelectionMode.None,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
            ItemTemplate = new DataTemplate(() =>
            {
                var tabButton = new Button
                {
                    CornerRadius = 12,
                    Margin = new Thickness(0, 0, 8, 0),
                };
                tabButton
                    .Bind(Button.TextProperty, nameof(ListModel.Title))
                    .BackgroundColor(Color.FromArgb("#E9EFFE"))
                    .TextColor(Color.FromArgb("#1F3A73"));

                tabButton.Clicked += (_, _) =>
                {
                    if (tabButton.BindingContext is ListModel tapped)
                    {
                        _viewModel.SelectListCommand.Execute(tapped);
                    }
                };

                tabButton.Triggers.Add(new DataTrigger(typeof(Button))
                {
                    Binding = new Binding(nameof(ListModel.IsSelected)),
                    Value = true,
                    Setters =
                    {
                        new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromArgb("#163B85") },
                        new Setter { Property = Button.TextColorProperty, Value = Colors.White },
                    },
                });

                return tabButton;
            }),
        };
        listTabs.SetBinding(ItemsView.ItemsSourceProperty, nameof(ListsViewModel.Lists));

        var summaryHeading = new Label()
            .Text("Summary")
            .Bold()
            .FontSize(18);

        var summaryTitleLabel = new Label()
            .Bold()
            .FontSize(16)
            .Bind(Label.TextProperty, nameof(ListsViewModel.SummaryTitle));

        var summaryTextLabel = new Label()
            .Bind(Label.TextProperty, nameof(ListsViewModel.SummaryText));

        var regenerateSummaryButton = new Button()
            .Text("Regenerate Summary")
            .Bind(Button.CommandProperty, nameof(ListsViewModel.RegenerateSummaryCommand))
            .Bind(IsEnabledProperty, nameof(ListsViewModel.HasSelectedList));

        var summaryBorder = new Border
        {
            Stroke = Color.FromArgb("#BFD0F0"),
            StrokeThickness = 1,
            BackgroundColor = Color.FromArgb("#F5F8FF"),
            Padding = 12,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    summaryHeading,
                    summaryTitleLabel,
                    summaryTextLabel,
                    regenerateSummaryButton,
                },
            },
        };

        var renameListEntry = new Entry()
            .Placeholder("Rename selected list...")
            .Bind(Entry.TextProperty, nameof(ListsViewModel.RenameListTitle), BindingMode.TwoWay)
            .Bind(IsEnabledProperty, nameof(ListsViewModel.HasSelectedList));

        var renameButton = new Button()
            .Text("Rename")
            .Bind(Button.CommandProperty, nameof(ListsViewModel.RenameListCommand))
            .Bind(IsEnabledProperty, nameof(ListsViewModel.HasSelectedList));

        var deleteListButton = new Button()
            .Text("Delete")
            .BackgroundColor(Color.FromArgb("#B00020"))
            .Bind(Button.CommandProperty, nameof(ListsViewModel.DeleteListCommand))
            .Bind(IsEnabledProperty, nameof(ListsViewModel.HasSelectedList));

        var renameRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
        }.ColumnSpacing(8);
        renameRow.Children.Add(renameListEntry);
        renameRow.Children.Add(renameButton.Column(1));
        renameRow.Children.Add(deleteListButton.Column(2));

        var newItemTextEntry = new Entry()
            .Placeholder("New item text...")
            .Bind(Entry.TextProperty, nameof(ListsViewModel.NewItemText), BindingMode.TwoWay)
            .Bind(IsEnabledProperty, nameof(ListsViewModel.HasSelectedList));

        var newItemNoteEntry = new Entry()
            .Placeholder("Optional note...")
            .Bind(Entry.TextProperty, nameof(ListsViewModel.NewItemNote), BindingMode.TwoWay)
            .Bind(IsEnabledProperty, nameof(ListsViewModel.HasSelectedList));

        var newItemRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
        }.ColumnSpacing(8);
        newItemRow.Children.Add(newItemTextEntry);
        newItemRow.Children.Add(newItemNoteEntry.Column(1));

        var addItemButton = new Button()
            .Text("Add Item")
            .Bind(Button.CommandProperty, nameof(ListsViewModel.AddItemCommand))
            .Bind(IsEnabledProperty, nameof(ListsViewModel.HasSelectedList));

        var itemList = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemTemplate = new DataTemplate(() =>
            {
                var itemTextLabel = new Label()
                    .Bold()
                    .Bind(Label.TextProperty, nameof(ListItemModel.Text));

                var itemNoteLabel = new Label()
                    .Bind(Label.TextProperty, nameof(ListItemModel.Note));

                var editItemButton = new Button()
                    .Text("Edit")
                    .BackgroundColor(Color.FromArgb("#DCE7FF"))
                    .TextColor(Color.FromArgb("#123A79"))
                    .Bind(Button.CommandProperty, "BindingContext.EditItemCommand", source: this)
                    .Bind(Button.CommandParameterProperty, ".");

                var deleteItemButton = new Button()
                    .Text("Delete")
                    .BackgroundColor(Color.FromArgb("#B00020"))
                    .Bind(Button.CommandProperty, "BindingContext.DeleteItemCommand", source: this)
                    .Bind(Button.CommandParameterProperty, ".");

                var itemButtonRow = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                    },
                }.ColumnSpacing(8);
                itemButtonRow.Children.Add(editItemButton);
                itemButtonRow.Children.Add(deleteItemButton.Column(1));

                return new Border
                {
                    Stroke = Color.FromArgb("#D3DDED"),
                    StrokeThickness = 1,
                    Padding = 10,
                    Margin = new Thickness(0, 0, 0, 8),
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                    Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            itemTextLabel,
                            itemNoteLabel,
                            itemButtonRow,
                        },
                    },
                };
            }),
        };
        itemList.SetBinding(ItemsView.ItemsSourceProperty, nameof(ListsViewModel.Items));

        var busyIndicator = new ActivityIndicator()
            .Bind(IsVisibleProperty, nameof(ListsViewModel.IsBusy))
            .Bind(ActivityIndicator.IsRunningProperty, nameof(ListsViewModel.IsBusy));

        var errorLabel = new Label()
            .TextColor(Color.FromArgb("#B00020"))
            .Bind(Label.TextProperty, nameof(ListsViewModel.ErrorMessage));

        var bodyLayout = new VerticalStackLayout
        {
            Children =
            {
                sessionRow,
                newListRow,
                listTabs,
                summaryBorder,
                renameRow,
                newItemRow,
                addItemButton,
                itemList,
                busyIndicator,
                errorLabel,
            },
        }
        .Padding(16)
        .Spacing(12);

        var scrollView = new ScrollView
        {
            Content = bodyLayout,
        };

        var refreshView = new RefreshView()
            .Bind(RefreshView.CommandProperty, nameof(ListsViewModel.RefreshCommand))
            .Bind(RefreshView.IsRefreshingProperty, nameof(ListsViewModel.IsRefreshing), BindingMode.TwoWay);
        refreshView.Content = scrollView;

        Content = refreshView;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}

