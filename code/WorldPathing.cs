namespace Kira;

public sealed class WorldPathing : Component
{
    protected override void OnUpdate()
    {
        DrawGrid();
    }

    private void DrawGrid()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                var pos = Transform.Position;


                // var box = new BBox()
                // Gizmo.Draw.SolidBox();
            }
        }
    }
}