using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reporting_inventory_aging
{
    class Aging
    {
        public string material { get; set; }
        public decimal quantity_current { get; set; }
        public decimal quantity_0_30 { get; set; }
        public decimal quantity_31_60 { get; set; }
        public decimal quantity_61_90 { get; set; }
        public decimal quantity_91_120 { get; set; }
        public decimal quantity_121_150 { get; set; }
        public decimal quantity_151_180 { get; set; }
        public decimal quantity_181_210 { get; set; }
        public decimal quantity_211_240 { get; set; }
        public decimal quantity_241_270 { get; set; }
        public decimal quantity_271_300 { get; set; }
        public decimal quantity_301_330 { get; set; }
        public decimal quantity_331_360 { get; set; }
        public decimal quantity_361_720 { get; set; }
        public decimal quantity_721_1080 { get; set; }
        public decimal quantity_1081_1800 { get; set; }
        public decimal quantity_1801_plus { get; set; }
        public decimal weight_current { get; set; }
        public decimal weight_0_30 { get; set; }
        public decimal weight_31_60 { get; set; }
        public decimal weight_61_90 { get; set; }
        public decimal weight_91_120 { get; set; }
        public decimal weight_121_150 { get; set; }
        public decimal weight_151_180 { get; set; }
        public decimal weight_181_210 { get; set; }
        public decimal weight_211_240 { get; set; }
        public decimal weight_241_270 { get; set; }
        public decimal weight_271_300 { get; set; }
        public decimal weight_301_330 { get; set; }
        public decimal weight_331_360 { get; set; }
        public decimal weight_361_720 { get; set; }
        public decimal weight_721_1080 { get; set; }
        public decimal weight_1081_1800 { get; set; }
        public decimal weight_1801_plus { get; set; }
        public Aging(string _material, decimal _quantity_current, decimal _quantity_0_30, decimal _quantity_31_60, decimal _quantity_61_90, decimal _quantity_91_120, decimal _quantity_121_150, decimal _quantity_151_180, decimal _quantity_181_210, decimal _quantity_211_240, decimal _quantity_241_270, decimal _quantity_271_300, decimal _quantity_301_330, decimal _quantity_331_360, decimal _quantity_361_720, decimal _quantity_721_1080, decimal _quantity_1081_1800, decimal _quantity_1801_plus, decimal _weight_current, decimal _weight_0_30, decimal _weight_31_60, decimal _weight_61_90, decimal _weight_91_120, decimal _weight_121_150, decimal _weight_151_180, decimal _weight_181_210, decimal _weight_211_240, decimal _weight_241_270, decimal _weight_271_300, decimal _weight_301_330, decimal _weight_331_360, decimal _weight_361_720, decimal _weight_721_1080, decimal _weight_1081_1800, decimal _weight_1801_plus)
        {
            material = _material;
            quantity_current = _quantity_current;
            quantity_0_30 = _quantity_0_30;
            quantity_31_60 = _quantity_31_60;
            quantity_61_90 = _quantity_61_90;
            quantity_91_120 = _quantity_91_120;
            quantity_121_150 = _quantity_121_150;
            quantity_151_180 = _quantity_151_180;
            quantity_181_210 = _quantity_181_210;
            quantity_211_240 = _quantity_211_240;
            quantity_241_270 = _quantity_241_270;
            quantity_271_300 = _quantity_271_300;
            quantity_301_330 = _quantity_301_330;
            quantity_331_360 = _quantity_331_360;
            quantity_361_720 = _quantity_361_720;
            quantity_721_1080 = _quantity_721_1080;
            quantity_1081_1800 = _quantity_1081_1800;
            quantity_1801_plus = _quantity_1801_plus;
            weight_current = _weight_current;
            weight_0_30 = _weight_0_30;
            weight_31_60 = _weight_31_60;
            weight_61_90 = _weight_61_90;
            weight_91_120 = _weight_91_120;
            weight_121_150 = _weight_121_150;
            weight_151_180 = _weight_151_180;
            weight_181_210 = _weight_181_210;
            weight_211_240 = _weight_211_240;
            weight_241_270 = _weight_241_270;
            weight_271_300 = _weight_271_300;
            weight_301_330 = _weight_301_330;
            weight_331_360 = _weight_331_360;
            weight_361_720 = _weight_361_720;
            weight_721_1080 = _weight_721_1080;
            weight_1081_1800 = _weight_1081_1800;
            weight_1801_plus = _weight_1801_plus;
        }
    }
}
