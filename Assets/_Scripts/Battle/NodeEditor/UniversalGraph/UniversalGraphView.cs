using GraphProcessor;
using UnityEditor;
using UnityEngine.UIElements;

public class UniversalGraphView : BaseGraphView
{
    public UniversalGraphWindow universalGraphWindow;

    // Nothing special to add for now
    public UniversalGraphView(EditorWindow window) : base(window)
    {
        universalGraphWindow = window as UniversalGraphWindow;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        BuildStackNodeContextualMenu(evt);
        base.BuildContextualMenu(evt);
    }

    /// <summary>
    ///     Add the New Stack entry to the context menu
    /// </summary>
    /// <param name="evt"></param>
    protected void BuildStackNodeContextualMenu(ContextualMenuPopulateEvent evt)
    {
        var position =
            (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
        evt.menu.AppendAction("New Stack", e => AddStackNode(new BaseStackNode(position)),
            DropdownMenuAction.AlwaysEnabled);
    }
}