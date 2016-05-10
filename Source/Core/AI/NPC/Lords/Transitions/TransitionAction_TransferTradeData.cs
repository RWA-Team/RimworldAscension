using System.Linq;
using Verse.AI.Group;

namespace RA
{
    public class TransitionAction_TransferTradeData : TransitionAction
    {
        public override void DoAction(Transition trans)
        {
            trans.target.data = (LordToilData_Trade)trans.sources.FirstOrDefault().data;
        }
    }
}
