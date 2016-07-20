using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class RA_Building_Door : Building_Door
    {
        //public const float VisualDoorOffsetStart = 0.0f;
        //public const float VisualDoorOffsetEnd = 0.45f;

        //public override void Draw()
        //{
        //    Rotation = DoorRotationAt(Position);

        //    //Draw the two moving doors
        //    var percentOpen = Mathf.Clamp01(visualTicksOpen / (float)VisualTicksToOpen);
        //    //Needs clamp for after game load		
        //    var offsetDist = VisualDoorOffsetStart + (VisualDoorOffsetEnd - VisualDoorOffsetStart) * percentOpen;

        //    for (var i = 0; i < 2; i++)
        //    {
        //        //Left, then right
        //        Vector3 offsetNormal;
        //        Mesh mesh;
        //        if (i == 0)
        //        {
        //            offsetNormal = Vector3.back;
        //            mesh = MeshPool.plane10;
        //        }
        //        else
        //        {
        //            offsetNormal = Vector3.forward;
        //            mesh = MeshPool.plane10Flip;
        //        }


        //        //Work out move direction
        //        var openDir = Rotation;
        //        openDir.Rotate(RotationDirection.Clockwise);
        //        offsetNormal = openDir.AsQuat * offsetNormal;

        //        //Position the door
        //        var doorPos = DrawPos;
        //        doorPos.y = Altitudes.AltitudeFor(AltitudeLayer.DoorMoveable);
        //        doorPos += offsetNormal * offsetDist;

        //        //Draw!
        //        Graphics.DrawMesh(mesh, doorPos, Rotation.AsQuat, Graphic.MatAt(Rotation), 0);
        //    }

        //    Comps_PostDraw();
        //}

        public override Graphic Graphic
            => Rotation == Rot4.North || Rotation == Rot4.South
                ? def.graphic
                : GraphicDatabase.Get<Graphic_Single>("Things/Buildings/Structure/Doors/Door", def.graphic.Shader,
                    def.graphic.drawSize, def.graphic.Color);

        //public int VisualTicksToOpen => TicksToOpenNow;
    }
}
