﻿using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using WinUI.Dock.Converters;
using WinUI.Dock.Enums;
using WinUI.Dock.Helpers;

namespace WinUI.Dock.Controls;

public sealed partial class DocumentTabItem : TabViewItem
{
    private string dragKey = string.Empty;

    public DocumentTabItem(TabPosition tabPosition, Document document)
    {
        InitializeComponent();

        Document = document;

        UpdateTabPosition(tabPosition);

        ActiveIndicator.SetBinding(VisibilityProperty, new Binding
        {
            Source = Document.Root,
            Path = new(nameof(DockManager.ActiveDocument)),
            Converter = new ActiveDocumentToVisibilityConverter(),
            ConverterParameter = Document
        });
    }

    public Document? Document { get; private set; }

    public void UpdateTabPosition(TabPosition tabPosition)
    {
        bool isBottom = tabPosition is TabPosition.Bottom;

        double scale = isBottom ? -1.0 : 1.0;

        HeaderScale.ScaleY = scale;
        ContentScale.ScaleY = scale;

        HeaderOptions.Visibility = isBottom ? Visibility.Collapsed : Visibility.Visible;
        ContentOptions.Visibility = isBottom ? Visibility.Visible : Visibility.Collapsed;
    }

    public void Detach()
    {
        Document = null;

        Bindings.Update();
    }

    private void OnDragStarting(UIElement _, DragStartingEventArgs args)
    {
        args.Data.SetData(DragDropHelpers.FormatId, dragKey = DragDropHelpers.GetDragKey(Document!));
    }

    private void OnDropCompleted(UIElement _, DropCompletedEventArgs args)
    {
        if (args.DropResult is DataPackageOperation.Move)
        {
            // If a move operation was performed but no action was taken,
            // the current document may have been sorted or an invalid operation was performed.
            if (Document is not null)
            {
                DocumentGroup group = (DocumentGroup)Document.Owner!;

                group.TryReorder(this, Document);

                Document.Root!.HideDockTargets();
            }
        }
        else if (DragDropHelpers.GetDocument(dragKey) is Document document)
        {
            DockWindow dockWindow = new(document.Root!, document);

            dockWindow.Activate();
        }

        DragDropHelpers.RemoveDragKey(dragKey);
    }

    private void Header_PointerEntered(object _, PointerRoutedEventArgs __)
    {
        HeaderOptions.Opacity = 1.0;
    }

    private void Header_PointerExited(object _, PointerRoutedEventArgs __)
    {
        HeaderOptions.Opacity = 0.0;
    }

    private void Document_PointerPressed(object _, PointerRoutedEventArgs __)
    {
        Document!.Root!.ActiveDocument = Document;
    }

    private void Pin_Click(object _, RoutedEventArgs __)
    {
        DockManager manager = Document!.Root!;

        Point point = TransformToVisual(manager).TransformPoint(new Point(0, 0));

        double left = point.X;
        double top = point.Y;
        double right = manager.ActualWidth - point.X;
        double bottom = manager.ActualHeight - point.Y;

        if (Document.ActualWidth < Document.ActualHeight)
        {
            if (left < right)
            {
                manager.LeftSide.Add(Document);
            }
            else
            {
                manager.RightSide.Add(Document);
            }
        }
        else if (top < bottom)
        {
            manager.TopSide.Add(Document);
        }
        else
        {
            manager.BottomSide.Add(Document);
        }

        Document.Detach();
    }

    private void Close_Click(object _, RoutedEventArgs __)
    {
        if (Document!.Root!.ActiveDocument == Document)
        {
            Document.Root.ActiveDocument = null;
        }

        Document.Detach();
    }
}
