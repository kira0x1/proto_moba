namespace Kira.UI;

using System;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Util;

public class GraphNodeUI : Panel
{
    private readonly GraphNode node;
    // private readonly List<GraphNode> neighbours;

    private readonly int gridRows;
    private readonly int gridCols;

    public GraphNodeUI(GraphNode node, Graph graph)
    {
        this.node = node;
        gridRows = graph.GridRows;
        gridCols = graph.GridCols;

        if (node == null) return;

        // neighbours = graph.Neighbours(node);
        AddNodeLabels();
    }

    protected override int BuildHash()
    {
        return HashCode.Combine(node.IsHighlightedPath);
    }

    public override void OnHotloaded()
    {
        base.OnHotloaded();
        LayoutNode();
    }

    public override void FinalLayout(Vector2 offset)
    {
        base.FinalLayout(offset);
        LayoutNode();
    }

    /// <summary>
    /// set graph node positions in screen space
    /// </summary>
    private void LayoutNode()
    {
        const int gap = 50;
        const float offset = 0f;

        var rect = Box.Rect;
        var parent = Parent.Box;

        var height = rect.Height + gap;
        var width = rect.Width + gap;

        var posx = node.x * width;
        var posy = node.y * height;

        // for 0,0 positioning of the grid i.e top left corner:
        // use parent.Rect.Position.x instead
        var centerX = parent.Rect.Center.x;
        var centerY = parent.Rect.Center.y;

        const float halfGap = gap / 2f;
        centerX -= gridRows * (width / 2f) - (offset + halfGap);
        centerY -= gridCols * (height / 2f) - (offset + halfGap);

        posx += centerX;
        posy += centerY;

        rect.Position = new Vector2(posx, posy);

        Box.Rect = rect;

        FinalLayoutChildren(Box.Rect.Position);
    }

    public override void OnLayout(ref Rect layoutRect)
    {
        base.OnLayout(ref layoutRect);
        LayoutNode();
    }

    private void AddNodeLabels()
    {
        Add.Label($"{node.x}, {node.y}");
    }
}