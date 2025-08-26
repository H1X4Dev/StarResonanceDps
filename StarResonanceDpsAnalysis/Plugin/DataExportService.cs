using ClosedXML.Excel;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System.Text;
using System.Linq;
using System.Windows.Forms;

namespace StarResonanceDpsAnalysis.Plugin
{
    /// <summary>
    /// Data export utilities (Excel/CSV/Screenshot) for DPS/HPS tables.
    /// </summary>
    public static class DataExportService
    {
        #region Excel Export

        /// <summary>
        /// Export DPS data to an Excel file with multiple sheets.
        /// </summary>
        public static bool ExportToExcel(List<PlayerData> players, bool includeSkillDetails = true)
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"DPS_Report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx",
                    Title = "Export DPS Report"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return false;

                using var workbook = new XLWorkbook();

                // Overview
                CreatePlayerOverviewSheet(workbook, players);

                if (includeSkillDetails)
                {
                    // Skill details per player
                    CreateSkillDetailsSheet(workbook, players);

                    // Team skill statistics
                    CreateTeamSkillStatsSheet(workbook, players);
                }

                workbook.SaveAs(saveDialog.FileName);

                MessageBox.Show($"Exported successfully to:\n{saveDialog.FileName}", "Export Succeeded",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving Excel file:\n{ex.Message}", "Export Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static void CreatePlayerOverviewSheet(XLWorkbook workbook, List<PlayerData> players)
        {
            var worksheet = workbook.Worksheets.Add("Overview");

            // Header
            var headers = new[]
            {
                "Nickname", "Profession", "Power", "Total Damage", "DPS",
                "Critical Damage", "Lucky Damage", "Crit Rate", "Luck Rate",
                "Max Instant DPS", "Total Healing", "HPS", "Total Taken", "Hit Count"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            }

            // Rows
            int row = 2;
            foreach (var player in players.OrderByDescending(p => p.DamageStats.Total))
            {
                worksheet.Cell(row, 1).Value = player.Nickname;
                worksheet.Cell(row, 2).Value = player.Profession;
                worksheet.Cell(row, 3).Value = player.CombatPower;
                worksheet.Cell(row, 4).Value = (double)player.DamageStats.Total;
                worksheet.Cell(row, 5).Value = Math.Round(player.GetTotalDps(), 1);
                worksheet.Cell(row, 6).Value = (double)player.DamageStats.Critical;
                worksheet.Cell(row, 7).Value = (double)player.DamageStats.Lucky;
                worksheet.Cell(row, 8).Value = $"{player.DamageStats.GetCritRate()}%";
                worksheet.Cell(row, 9).Value = $"{player.DamageStats.GetLuckyRate()}%";
                worksheet.Cell(row, 10).Value = (double)player.DamageStats.RealtimeMax;
                worksheet.Cell(row, 11).Value = (double)player.HealingStats.Total;
                worksheet.Cell(row, 12).Value = Math.Round(player.GetTotalHps(), 1);
                worksheet.Cell(row, 13).Value = (double)player.TakenDamage;
                worksheet.Cell(row, 14).Value = player.DamageStats.CountTotal;
                row++;
            }

            worksheet.ColumnsUsed().AdjustToContents();
            worksheet.Range(1, 1, Math.Max(1, row - 1), headers.Length).SetAutoFilter();
        }

        private static void CreateSkillDetailsSheet(XLWorkbook workbook, List<PlayerData> players)
        {
            var worksheet = workbook.Worksheets.Add("Skill Details");

            var headers = new[]
            {
                "Nickname", "Skill Name", "Total Damage", "Hits", "Avg Damage",
                "Crit Rate", "Luck Rate", "DPS", "Share"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
            }

            int row = 2;
            foreach (var player in players.OrderByDescending(p => p.DamageStats.Total))
            {
                var skills = StatisticData._manager.GetPlayerSkillSummaries(player.Uid, topN: null, orderByTotalDesc: true);
                foreach (var skill in skills)
                {
                    worksheet.Cell(row, 1).Value = player.Nickname;
                    worksheet.Cell(row, 2).Value = skill.SkillName;
                    worksheet.Cell(row, 3).Value = (double)skill.Total;
                    worksheet.Cell(row, 4).Value = skill.HitCount;
                    worksheet.Cell(row, 5).Value = Math.Round(skill.AvgPerHit, 1);
                    worksheet.Cell(row, 6).Value = $"{skill.CritRate * 100:F1}%";
                    worksheet.Cell(row, 7).Value = $"{skill.LuckyRate * 100:F1}%";
                    worksheet.Cell(row, 8).Value = Math.Round(skill.TotalDps, 1);
                    worksheet.Cell(row, 9).Value = $"{skill.ShareOfTotal * 100:F1}%";
                    row++;
                }
            }

            worksheet.ColumnsUsed().AdjustToContents();
            if (row > 2) worksheet.Range(1, 1, row - 1, headers.Length).SetAutoFilter();
        }

        private static void CreateTeamSkillStatsSheet(XLWorkbook workbook, List<PlayerData> players)
        {
            var worksheet = workbook.Worksheets.Add("Team Skills");

            var headers = new[] { "Skill Name", "Total Damage", "Hit Count", "Team Share" };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
            }

            var teamSkills = StatisticData._manager.GetTeamTopSkillsByTotal(50);
            double totalTeamDamage = teamSkills.Sum(s => (double)s.Total);

            int row = 2;
            foreach (var skill in teamSkills)
            {
                worksheet.Cell(row, 1).Value = skill.SkillName;
                worksheet.Cell(row, 2).Value = (double)skill.Total;
                worksheet.Cell(row, 3).Value = skill.HitCount;
                worksheet.Cell(row, 4).Value = totalTeamDamage > 0 ? $"{((double)skill.Total / totalTeamDamage) * 100:F1}%" : "0%";
                row++;
            }

            worksheet.ColumnsUsed().AdjustToContents();
            if (row > 2) worksheet.Range(1, 1, row - 1, headers.Length).SetAutoFilter();
        }

        #endregion

        #region CSV Export

        /// <summary>
        /// Export DPS data to CSV.
        /// </summary>
        public static bool ExportToCsv(List<PlayerData> players)
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"DPS_Report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv",
                    Title = "Export DPS Report (CSV)"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return false;

                var csv = new StringBuilder();
                // BOM for Excel compatibility
                csv.Append('\uFEFF');

                // Header
                csv.AppendLine("Nickname,Profession,Power,Total Damage,DPS,Critical Damage,Lucky Damage,Crit Rate,Luck Rate,Max Instant DPS,Total Healing,HPS,Total Taken,Hit Count");

                foreach (var player in players.OrderByDescending(p => p.DamageStats.Total))
                {
                    csv.AppendLine(
                        $"{Quote(player.Nickname)},{Quote(player.Profession)},{player.CombatPower}," +
                        $"{player.DamageStats.Total},{player.GetTotalDps():F1},{player.DamageStats.Critical},{player.DamageStats.Lucky}," +
                        $"{player.DamageStats.GetCritRate()}%,{player.DamageStats.GetLuckyRate()}%,{player.DamageStats.RealtimeMax}," +
                        $"{player.HealingStats.Total},{player.GetTotalHps():F1},{player.TakenDamage},{player.DamageStats.CountTotal}");
                }

                File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);

                MessageBox.Show($"Exported successfully to:\n{saveDialog.FileName}", "Export Succeeded",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving CSV file:\n{ex.Message}", "Export Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static string Quote(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return '"' + field.Replace("\"", "\"\"") + '"';
            }
            return field;
        }

        #endregion

        #region Screenshot

        /// <summary>
        /// Save a screenshot of the given form.
        /// </summary>
        public static bool SaveScreenshot(Form form)
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg;*.jpeg)|*.jpg;*.jpeg",
                    DefaultExt = "png",
                    FileName = $"DPS_Screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png",
                    Title = "Save DPS Screenshot"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return false;

                var bounds = form.Bounds;
                using var bitmap = new System.Drawing.Bitmap(bounds.Width, bounds.Height);
                using var graphics = System.Drawing.Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);

                var extension = Path.GetExtension(saveDialog.FileName).ToLowerInvariant();
                var format = extension switch
                {
                    ".jpg" or ".jpeg" => System.Drawing.Imaging.ImageFormat.Jpeg,
                    _ => System.Drawing.Imaging.ImageFormat.Png
                };

                bitmap.Save(saveDialog.FileName, format);

                MessageBox.Show($"Screenshot saved to:\n{saveDialog.FileName}", "Screenshot Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving screenshot:\n{ex.Message}", "Screenshot Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Helpers

        public static List<PlayerData> GetCurrentPlayerData()
        {
            return StatisticData._manager.GetPlayersWithCombatData().ToList();
        }

        public static bool HasDataToExport() => GetCurrentPlayerData().Count > 0;

        #endregion
    }
}

