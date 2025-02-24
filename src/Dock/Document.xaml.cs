﻿using Dock.Abstracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Windows.ApplicationModel.DataTransfer;

namespace Dock;

[ContentProperty(Name = nameof(Content))]
[TemplatePart(Name = "PART_DragIndicator", Type = typeof(Grid))]
public partial class Document : Component
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title),
                                                                                          typeof(string),
                                                                                          typeof(Document),
                                                                                          new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(nameof(CanClose),
                                                                                             typeof(bool),
                                                                                             typeof(Document),
                                                                                             new PropertyMetadata(true));

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content),
                                                                                            typeof(object),
                                                                                            typeof(Document),
                                                                                            new PropertyMetadata(null));

    private Grid dragIndicator = null!;

    public Document()
    {
        DefaultStyleKey = typeof(Document);

        DragOver += Document_DragOver;
        DragLeave += Document_DragLeave;
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool CanClose
    {
        get => (bool)GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        dragIndicator = (Grid)GetTemplateChild("PART_DragIndicator");
    }

    private void Document_DragOver(object sender, DragEventArgs e)
    {
        dragIndicator.Visibility = Visibility.Visible;

        e.AcceptedOperation = DataPackageOperation.Link;
        e.Handled = true;
    }

    private void Document_DragLeave(object sender, DragEventArgs e)
    {
        dragIndicator.Visibility = Visibility.Collapsed;

        e.AcceptedOperation = DataPackageOperation.None;
        e.Handled = true;
    }
}
