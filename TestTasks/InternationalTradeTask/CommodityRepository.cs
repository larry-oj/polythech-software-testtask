using System;
using System.Collections.Generic;
using System.Linq;
using TestTasks.InternationalTradeTask.Models;

namespace TestTasks.InternationalTradeTask
{
    public class CommodityRepository
    {
        public double GetImportTariff(string commodityName)
        {
            var commodityGroup = FindCommodityGroup(commodityName);
            if (commodityGroup == null)
                throw new ArgumentException($"Commodity \'{commodityName}\' not found.");

            return GetTariff(commodityGroup, cg => cg.ImportTarif);
        }

        public double GetExportTariff(string commodityName)
        {
            var commodityGroup = FindCommodityGroup(commodityName);
            if (commodityGroup == null)
                throw new ArgumentException($"Commodity \'{commodityName}\' not found.");

            return GetTariff(commodityGroup, cg => cg.ExportTarif);
        }

        private ICommodityGroup FindCommodityGroup(string commodityName)
        {
            return _allCommodityGroups
                .Select(group => FindCommodityRecursive(group, commodityName))
                .FirstOrDefault(found => found != null);
        }

        private ICommodityGroup FindCommodityRecursive(ICommodityGroup group, string commodityName)
        {
            if (group.Name.Equals(commodityName, StringComparison.OrdinalIgnoreCase))
                return group;

            foreach (var subgroup in group.SubGroups ?? Array.Empty<ICommodityGroup>())
            {
                var found = FindCommodityRecursive(subgroup, commodityName);
                if (found != null)
                    return found;
            }

            return null;
        }

        private double GetTariff(ICommodityGroup commodityGroup, Func<ICommodityGroup, double?> tariffSelector)
        {
            var current = commodityGroup;
            while (current != null)
            {
                var tariff = tariffSelector(current);
                if (tariff.HasValue)
                    return tariff.Value;

                current = _allCommodityGroups.FirstOrDefault(g => g.SubGroups?.Contains(current) == true);
            }

            return default;
        }

        private FullySpecifiedCommodityGroup[] _allCommodityGroups = new FullySpecifiedCommodityGroup[]
        {
            new FullySpecifiedCommodityGroup("06", "Sugar, sugar preparations and honey", 0.05, 0)
            {
                SubGroups = new CommodityGroup[]
                {
                    new CommodityGroup("061", "Sugar and honey")
                    {
                        SubGroups = new CommodityGroup[]
                        {
                            new CommodityGroup("0611", "Raw sugar,beet & cane"),
                            new CommodityGroup("0612", "Refined sugar & other prod.of refining,no syrup"),
                            new CommodityGroup("0615", "Molasses", 0, 0),
                            new CommodityGroup("0616", "Natural honey", 0, 0),
                            new CommodityGroup("0619", "Sugars & syrups nes incl.art.honey & caramel"),
                        }
                    },
                    new CommodityGroup("062", "Sugar confy, sugar preps. Ex chocolate confy", 0, 0)
                }
            },
            new FullySpecifiedCommodityGroup("282", "Iron and steel scrap", 0, 0.1)
            {
                SubGroups = new CommodityGroup[]
                {
                    new CommodityGroup("28201", "Iron/steel scrap not sorted or graded"),
                    new CommodityGroup("28202", "Iron/steel scrap sorted or graded/cast iron"),
                    new CommodityGroup("28203", "Iron/steel scrap sort.or graded/tinned iron"),
                    new CommodityGroup("28204", "Rest of 282.0")
                }
            }
        };
    }
}
