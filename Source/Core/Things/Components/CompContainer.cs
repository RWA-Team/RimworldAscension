using Verse;

namespace RA
{
    public class CompContainer_Properties : CompProperties
    {
        public int itemsCap = 10;
        public float rotModifier = 1f;

        public CompContainer_Properties()
        {
            compClass = typeof(CompContainer);
        }
    }

    public class CompContainer : ThingComp
    {
        public CompContainer_Properties Properties => (CompContainer_Properties) props;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            // Restrict itemsCap to prevent unsafe behaviour
            if (Properties.itemsCap < 1)
            {
                Properties.itemsCap = 1;
                Log.Error("CompContainer's itemsCap value should be between 1 and 15");
            }
            else if (Properties.itemsCap > 15)
            {
                Properties.itemsCap = 15;
                Log.Error("CompContainer's itemsCap value should be between 1 and 15");
            }
        }
    }
}