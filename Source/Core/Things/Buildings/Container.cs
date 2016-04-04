using RimWorld;
using Verse;

namespace RA
{
    public class Container : Building_Storage
    {
        // draw open\closed textures
        public override Graphic Graphic
        {
            get
            {
                var compContainer = this.TryGetComp<CompContainer>();
                if (compContainer != null && compContainer.ItemsCount >= compContainer.Properties.itemsCap)
                {
                    return GraphicDatabase.Get<Graphic_Single>(def.graphic.path + "_full", def.graphic.Shader,
                        def.graphic.drawSize, def.graphic.Color);
                }
                return def.graphic;
            }
        }
    }
}
