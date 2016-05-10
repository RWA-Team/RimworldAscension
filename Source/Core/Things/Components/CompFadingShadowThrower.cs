using UnityEngine;
using Verse;

namespace RA
{
    class CompFadingShadowThrower : ThingComp
    {
        public static readonly Color32 LowVertexColor = new Color32(0, 0, 0, 0);

        public override void PostPrintOnto(SectionLayer layer)
        {
            if (parent.def.graphicData.shadowData != null)
            {
                // shadow offset is not calculated in PrintShadow by default for some reason
                var shadow = parent.def.graphicData.shadowData;
                var offset = parent.TrueCenter() + parent.def.graphicData.shadowData.offset;

                var subMesh = layer.GetSubMesh(MatBases.SunShadowFade);
                var item = new Color32(255, 0, 0, (byte)(255f * shadow.BaseY));

                float num, num2;
                if (parent.Rotation == Rot4.North || parent.Rotation == Rot4.South)
                {
                    num = shadow.BaseX/2f;
                    num2 = shadow.BaseZ/2f;
                }
                else
                {
                    num = shadow.BaseZ/2f;
                    num2 = shadow.BaseX/2f;
                }

                var x = offset.x;
                var z = offset.z;
                var count = subMesh.verts.Count;
                subMesh.verts.Add(new Vector3(x - num, 1f, z - num2));
                subMesh.verts.Add(new Vector3(x - num, 1f, z + num2));
                subMesh.verts.Add(new Vector3(x + num, 1f, z + num2));
                subMesh.verts.Add(new Vector3(x + num, 1f, z - num2));
                subMesh.colors.Add(LowVertexColor);
                subMesh.colors.Add(LowVertexColor);
                subMesh.colors.Add(LowVertexColor);
                subMesh.colors.Add(LowVertexColor);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 1);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count + 3);
                var count2 = subMesh.verts.Count;
                subMesh.verts.Add(new Vector3(x - num, 1f, z - num2));
                subMesh.verts.Add(new Vector3(x - num, 1f, z + num2));
                subMesh.colors.Add(item);
                subMesh.colors.Add(item);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count2);
                subMesh.tris.Add(count2 + 1);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count2 + 1);
                subMesh.tris.Add(count + 1);
                var count3 = subMesh.verts.Count;
                subMesh.verts.Add(new Vector3(x + num, 1f, z + num2));
                subMesh.verts.Add(new Vector3(x + num, 1f, z - num2));
                subMesh.colors.Add(item);
                subMesh.colors.Add(item);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count3);
                subMesh.tris.Add(count3 + 1);
                subMesh.tris.Add(count3 + 1);
                subMesh.tris.Add(count + 3);
                subMesh.tris.Add(count + 2);
                var count4 = subMesh.verts.Count;
                subMesh.verts.Add(new Vector3(x - num, 1f, z - num2));
                subMesh.verts.Add(new Vector3(x + num, 1f, z - num2));
                subMesh.colors.Add(item);
                subMesh.colors.Add(item);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 3);
                subMesh.tris.Add(count4);
                subMesh.tris.Add(count + 3);
                subMesh.tris.Add(count4 + 1);
                subMesh.tris.Add(count4);
            }
        }
    }
}
