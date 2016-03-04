
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RA
{
    public static class TradeSliders
    {
        public const int DragTimeGrowMinimumOffset = 300;

        public static Tradeable dragTrad;

        public static int dragBaseAmount;

        public static bool dragLimitWarningGiven;

        public static Sustainer dragSustainer;

        public static float lastDragRealTime = -10000f;

        public static readonly SoundDef DragStartSound = SoundDef.Named("TradeSlider_DragStart");

        public static readonly SoundDef DragAmountChangedSound = SoundDef.Named("Drag_TradeSlider");

        public static readonly SoundDef DragEndSound = SoundDef.Named("TradeSlider_DragEnd");

        public static void TradeSliderDraggingStarted(float mouseOffX, float rateFactor)
        {
            DragStartSound.PlayOneShotOnCamera();
        }

        public static void TradeSliderDraggingUpdate(float mouseOffX, float rateFactor)
        {
            var num = dragBaseAmount;
            if (Mathf.Abs(mouseOffX) > 300f)
            {
                if (mouseOffX > 0f)
                {
                    dragBaseAmount -= GenMath.RoundRandom(rateFactor);
                }
                else
                {
                    dragBaseAmount += GenMath.RoundRandom(rateFactor);
                }
            }
            var num2 = dragBaseAmount - (int)(mouseOffX / 4f);
            var num3 = dragTrad.offerCount;
            AcceptanceReport acceptanceReport = null;
            while (num3 != num2)
            {
                if (num2 > num3)
                {
                    acceptanceReport = dragTrad.TrySetToDropOneMore();
                }
                if (num2 < num3)
                {
                    acceptanceReport = dragTrad.TrySetToLaunchOneMore();
                }
                if (!acceptanceReport.Accepted)
                {
                    if (!dragLimitWarningGiven)
                    {
                        if (dragTrad.CountHeldBy(Transactor.Colony) + dragTrad.CountHeldBy(Transactor.Trader) > 1)
                        {
                            Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
                        }
                        dragLimitWarningGiven = true;
                    }
                    dragBaseAmount = num;
                    break;
                }
                if (num2 > num3)
                {
                    num3++;
                }
                else
                {
                    num3--;
                }
                dragLimitWarningGiven = false;
            }
            if (acceptanceReport.Accepted)
            {
                if (dragSustainer == null)
                {
                    DragAmountChangedSound.PlayOneShotOnCamera();
                }
                else
                {
                    var num4 = -mouseOffX;
                    if (num4 > 300f)
                    {
                        num4 = 300f;
                    }
                    if (num4 < -300f)
                    {
                        num4 = -300f;
                    }
                    dragSustainer.externalParams["DragX"] = num4;
                }
                lastDragRealTime = Time.realtimeSinceStartup;
            }
            if (dragSustainer != null)
            {
                dragSustainer.Maintain();
                dragSustainer.externalParams["TimeSinceDrag"] = Time.realtimeSinceStartup - lastDragRealTime;
            }
        }

        public static void TradeSliderDraggingCompleted(float mouseOffX, float rateFactor)
        {
            dragSustainer = null;
            DragEndSound.PlayOneShotOnCamera();
        }
    }
}
