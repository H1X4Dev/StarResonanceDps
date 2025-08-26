using StarResonanceDpsAnalysis.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Forms
{
    public partial class HistoricalBattlesForm
    {
        public void ToggleTableView()
        {

            table_DpsDetailDataTable.Columns.Clear();

            table_DpsDetailDataTable.Columns = new AntdUI.ColumnCollection
            {
              new AntdUI.Column("Uid", "UID"),
                new AntdUI.Column("NickName", "Nickname"),
                new AntdUI.Column("Profession", "Profession"),
                new AntdUI.Column("CombatPower", "Power"),
                new AntdUI.Column("TotalDamage", "Total Damage"),
                new AntdUI.Column("TotalDps", "DPS"),
                new AntdUI.Column("CritRate", "Crit Rate"),
                new AntdUI.Column("LuckyRate", "Luck Rate"),
                new AntdUI.Column("CriticalDamage", "Critical Damage"),
                new AntdUI.Column("LuckyDamage", "Lucky Damage"),
                new AntdUI.Column("CritLuckyDamage", "Crit+Lucky Damage"),
                new AntdUI.Column("MaxInstantDps", "Max Instant DPS"),

                new AntdUI.Column("TotalHealingDone", "Total Healing"),
                new AntdUI.Column("TotalHps", "HPS"),
                new AntdUI.Column("CriticalHealingDone", "Critical Healing"),
                new AntdUI.Column("LuckyHealingDone", "Lucky Healing"),
                new AntdUI.Column("CritLuckyHealingDone", "Crit+Lucky Healing"),
                new AntdUI.Column("MaxInstantHps", "Max Instant HPS"),
                new AntdUI.Column("DamageTaken", "Total Damage Taken"),
               // new AntdUI.Column("Share","占比"),
                new AntdUI.Column("DmgShare","Team Damage Share (%)"),
            };

            table_DpsDetailDataTable.Binding(DpsTableDatas.DpsTable);


        }
    }


}
