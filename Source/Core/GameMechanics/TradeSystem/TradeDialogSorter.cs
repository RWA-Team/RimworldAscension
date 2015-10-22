using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class TradeDialogSorter
    {
        public string name;

        public TradeableComparer comparer;

        public void ComparerSelector (string name)
        {
            switch (name)
            {
                case "None":
                    {
                        comparer = new TradeableComparer_None();
                        break;
                    }
                case "Name":
                    {
                        comparer = new TradeableComparer_Name();
                        break;
                    }
                case "Category":
                    {
                        comparer = new TradeableComparer_Category();
                        break;
                    }
                case "Market value":
                    {
                        comparer = new TradeableComparer_MarketValue();
                        break;
                    }
                case "Quality":
                    {
                        comparer = new TradeableComparer_Quality();
                        break;
                    }
                case "Hit points percentage":
                    {
                        comparer = new TradeableComparer_HitPointsPercentage();
                        break;
                    }
                default:
                    Log.Message("No Such Comparer");
                    break;
            }
        }
    }
}
