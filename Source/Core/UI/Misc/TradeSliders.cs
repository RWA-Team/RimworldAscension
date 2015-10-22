using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace RA
{
    public static class TradeSliders
    {
        public const int DragTimeGrowMinimumOffset = 300;

        public static Tradeable dragTrad;

        public static int dragBaseAmount;

        public static bool dragLimitWarningGiven = false;

        public static Sustainer dragSustainer;

        public static float lastDragRealTime = -10000f;

        public static readonly SoundDef DragStartSound = SoundDef.Named("TradeSlider_DragStart");

        public static readonly SoundDef DragAmountChangedSound = SoundDef.Named("Drag_TradeSlider");

        public static readonly SoundDef DragEndSound = SoundDef.Named("TradeSlider_DragEnd");

        public static void TradeSliderDraggingStarted(float mouseOffX, float rateFactor)
        {
            TradeSliders.DragStartSound.PlayOneShotOnCamera();
        }

        public static void TradeSliderDraggingUpdate(float mouseOffX, float rateFactor)
        {
            int num = TradeSliders.dragBaseAmount;
            if (Mathf.Abs(mouseOffX) > 300f)
            {
                if (mouseOffX > 0f)
                {
                    TradeSliders.dragBaseAmount -= GenMath.RoundRandom(rateFactor);
                }
                else
                {
                    TradeSliders.dragBaseAmount += GenMath.RoundRandom(rateFactor);
                }
            }
            int num2 = TradeSliders.dragBaseAmount - (int)(mouseOffX / 4f);
            int num3 = TradeSliders.dragTrad.offerCount;
            AcceptanceReport acceptanceReport = null;
            while (num3 != num2)
            {
                if (num2 > num3)
                {
                    acceptanceReport = TradeSliders.dragTrad.TrySetToDropOneMore();
                }
                if (num2 < num3)
                {
                    acceptanceReport = TradeSliders.dragTrad.TrySetToLaunchOneMore();
                }
                if (!acceptanceReport.Accepted)
                {
                    if (!TradeSliders.dragLimitWarningGiven)
                    {
                        if (TradeSliders.dragTrad.CountHeldBy(Transactor.Colony) + TradeSliders.dragTrad.CountHeldBy(Transactor.Trader) > 1)
                        {
                            Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
                        }
                        TradeSliders.dragLimitWarningGiven = true;
                    }
                    TradeSliders.dragBaseAmount = num;
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
                TradeSliders.dragLimitWarningGiven = false;
            }
            if (acceptanceReport.Accepted)
            {
                if (TradeSliders.dragSustainer == null)
                {
                    TradeSliders.DragAmountChangedSound.PlayOneShotOnCamera();
                }
                else
                {
                    float num4 = -mouseOffX;
                    if (num4 > 300f)
                    {
                        num4 = 300f;
                    }
                    if (num4 < -300f)
                    {
                        num4 = -300f;
                    }
                    TradeSliders.dragSustainer.externalParams["DragX"] = num4;
                }
                TradeSliders.lastDragRealTime = Time.realtimeSinceStartup;
            }
            if (TradeSliders.dragSustainer != null)
            {
                TradeSliders.dragSustainer.Maintain();
                TradeSliders.dragSustainer.externalParams["TimeSinceDrag"] = Time.realtimeSinceStartup - TradeSliders.lastDragRealTime;
            }
        }

        public static void TradeSliderDraggingCompleted(float mouseOffX, float rateFactor)
        {
            TradeSliders.dragSustainer = null;
            TradeSliders.DragEndSound.PlayOneShotOnCamera();
        }
    }
}
