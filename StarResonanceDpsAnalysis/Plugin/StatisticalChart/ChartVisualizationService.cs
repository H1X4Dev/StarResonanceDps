using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using StarResonanceDpsAnalysis.Plugin.Charts;

namespace StarResonanceDpsAnalysis.Plugin
{
    /// <summary>
    /// ʵʱͼ����ӻ����� - �����Զ����ƽ��ͼ��ؼ�
    /// </summary>
    public static class ChartVisualizationService
    {
        #region ���ݵ�洢

        /// <summary>DPS�������ݵ�</summary>
        private static readonly Dictionary<ulong, List<(DateTime Time, double Dps)>> _dpsHistory = new();

        /// <summary>HPS�������ݵ�</summary>
        private static readonly Dictionary<ulong, List<(DateTime Time, double Hps)>> _hpsHistory = new();

        /// <summary>ս����ʼʱ�䣨����X��ʱ����㣩</summary>
        private static DateTime? _combatStartTime;

        /// <summary>�����ʷ���ݵ���</summary>
        private const int MaxHistoryPoints = 500; // ���ӵ�500������֧�ָ���ʱ�����ʷ

        #endregion

        #region ���ݸ��·���

        /// <summary>
        /// ���DPS���ݵ�
        /// </summary>
        public static void AddDpsDataPoint(ulong playerId, double dps)
        {
            var now = DateTime.Now;
            _combatStartTime ??= now;

            if (!_dpsHistory.TryGetValue(playerId, out var history))
            {
                history = new List<(DateTime, double)>();
                _dpsHistory[playerId] = history;
            }

            // ֻ�е�DPSֵ������ʱ��������ݵ�
            if (dps >= 0) // ����0ֵ�������˸�ֵ
            {
                history.Add((now, dps));
            }

            // ������ʷ���ݵ�����
            if (history.Count > MaxHistoryPoints)
            {
                history.RemoveAt(0);
            }
        }

        /// <summary>
        /// ���HPS���ݵ�
        /// </summary>
        public static void AddHpsDataPoint(ulong playerId, double hps)
        {
            var now = DateTime.Now;
            _combatStartTime ??= now;

            if (!_hpsHistory.TryGetValue(playerId, out var history))
            {
                history = new List<(DateTime, double)>();
                _hpsHistory[playerId] = history;
            }

            // ֻ�е�HPSֵ������ʱ��������ݵ�
            if (hps >= 0) // ����0ֵ�������˸�ֵ
            {
                history.Add((now, hps));
            }

            // ������ʷ���ݵ�����
            if (history.Count > MaxHistoryPoints)
            {
                history.RemoveAt(0);
            }
        }

        /// <summary>
        /// ������������������ݵ�
        /// </summary>
        public static void UpdateAllDataPoints()
        {
            var players = StatisticData._manager.GetPlayersWithCombatData();
            
            foreach (var player in players)
            {
                // ʹ����DPS������ʵʱDPS���Ի�ø�ƽ��������
                var dps = player.GetTotalDps();
                var hps = player.GetTotalHps();

                // ����������ݵ㣬��ʹ��0�������ܱ���������
                AddDpsDataPoint(player.Uid, dps);
                if (hps > 0) AddHpsDataPoint(player.Uid, hps);
            }
        }

        /// <summary>
        /// ���������ʷ����
        /// </summary>
        public static void ClearAllHistory()
        {
            _dpsHistory.Clear();
            _hpsHistory.Clear();
            _combatStartTime = null;
        }

        /// <summary>
        /// ��ȡս������ʱ�䣨�룩
        /// </summary>
        public static double GetCombatDurationSeconds()
        {
            if (_combatStartTime == null) return 0;
            return (DateTime.Now - _combatStartTime.Value).TotalSeconds;
        }

        #endregion

        #region DPS����ͼ

        /// <summary>
        /// ����DPS����ͼ
        /// </summary>
        public static FlatLineChart CreateDpsTrendChart(int width = 800, int height = 400)
        {
            var chart = new FlatLineChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                TitleText = "ʵʱDPS����ͼ",
                XAxisLabel = "ʱ��",
                YAxisLabel = "DPS",
                ShowLegend = true,
                ShowGrid = true,
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshDpsTrendChart(chart);
            return chart;
        }

        /// <summary>
        /// ˢ��DPS����ͼ����
        /// </summary>
        public static void RefreshDpsTrendChart(FlatLineChart chart)
        {
            chart.ClearSeries();

            if (_dpsHistory.Count == 0 || _combatStartTime == null)
            {
                return;
            }

            var startTime = _combatStartTime.Value;

            // �����ID����ȷ�����ݼ��ص�һ����
            var sortedHistory = _dpsHistory.OrderBy(x => x.Key);

            foreach (var kvp in sortedHistory)
            {
                var playerId = kvp.Key;
                var history = kvp.Value;

                if (history.Count == 0) continue;

                // ��ȡ�����Ϣ
                var playerInfo = StatisticData._manager.GetPlayerBasicInfo(playerId);
                var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"���{playerId}" : playerInfo.Nickname;

                // ת��Ϊ���ʱ�䣨�룩��DPSֵ�ĵ㼯��
                var points = history.Select(h => new PointF(
                    (float)(h.Time - startTime).TotalSeconds,
                    (float)h.Dps
                )).ToList();

                if (points.Count > 0)
                {
                    chart.AddSeries(playerName, points);
                }
            }
        }

        #endregion

        #region �����˺���ͼ

        /// <summary>
        /// ���������˺�ռ�ȱ�ͼ
        /// </summary>
        public static FlatPieChart CreateSkillDamagePieChart(ulong playerId, int width = 400, int height = 400)
        {
            var chart = new FlatPieChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                ShowLabels = true,
                ShowPercentages = true,
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshSkillDamagePieChart(chart, playerId);
            return chart;
        }

        /// <summary>
        /// ˢ�¼����˺���ͼ
        /// </summary>
        public static void RefreshSkillDamagePieChart(FlatPieChart chart, ulong playerId)
        {
            chart.ClearData();

            try
            {
                // ��ȡ��Ҽ�������
                var skillData = StatisticData._manager.GetPlayerSkillSummaries(playerId, topN: 8, orderByTotalDesc: true);
                
                if (skillData.Count == 0)
                {
                    chart.TitleText = "�����˺�ռ�� - ��������";
                    return;
                }

                // ��ȡ�����Ϣ
                var playerInfo = StatisticData._manager.GetPlayerBasicInfo(playerId);
                var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"���{playerId}" : playerInfo.Nickname;
                chart.TitleText = $"{playerName} - �����˺�ռ��";

                // ׼����ͼ����
                var pieData = skillData.Select(s => (
                    Label: $"{s.SkillName}: {Common.FormatWithEnglishUnits(s.Total)}",
                    Value: (double)s.Total
                )).ToList();

                chart.SetData(pieData);
            }
            catch (Exception ex)
            {
                chart.TitleText = $"�����˺�ռ�� - ���ݼ��ش���: {ex.Message}";
            }
        }

        #endregion

        #region �Ŷ�DPS�Ա�����ͼ

        /// <summary>
        /// �����Ŷ�DPS�Ա�����ͼ
        /// </summary>
        public static FlatBarChart CreateTeamDpsBarChart(int width = 600, int height = 400)
        {
            var chart = new FlatBarChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                TitleText = "�Ŷ�DPS�Ա�",
                XAxisLabel = "���",
                YAxisLabel = "DPS",
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshTeamDpsBarChart(chart);
            return chart;
        }

        /// <summary>
        /// ˢ���Ŷ�DPS�Ա�����ͼ
        /// </summary>
        public static void RefreshTeamDpsBarChart(FlatBarChart chart)
        {
            chart.ClearData();

            var players = StatisticData._manager.GetPlayersWithCombatData().ToList();
            
            if (players.Count == 0)
            {
                chart.TitleText = "�Ŷ�DPS�Ա� - ��������";
                return;
            }

            // ����DPS����
            players = players.OrderByDescending(p => p.GetTotalDps()).ToList();

            // ׼������ͼ����
            var barData = players.Select(p => (
                Label: string.IsNullOrEmpty(p.Nickname) ? $"���{p.Uid}" : p.Nickname,
                Value: p.GetTotalDps()
            )).ToList();

            chart.SetData(barData);
            chart.TitleText = "�Ŷ�DPS�Ա�";
        }

        #endregion

        #region ��ά��ɢ��ͼ

        /// <summary>
        /// ������ά��ɢ��ͼ
        /// </summary>
        public static FlatScatterChart CreateDpsRadarChart(int width = 400, int height = 400)
        {
            var chart = new FlatScatterChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                TitleText = "DPS�뱩���ʶԱ�",
                XAxisLabel = "������ (%)",
                YAxisLabel = "��DPS",
                ShowLegend = true,
                ShowGrid = true,
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshDpsRadarChart(chart);
            return chart;
        }

        /// <summary>
        /// ˢ�¶�ά��ɢ��ͼ
        /// </summary>
        public static void RefreshDpsRadarChart(FlatScatterChart chart)
        {
            chart.ClearSeries();

            var players = StatisticData._manager.GetPlayersWithCombatData().Take(5).ToList();
            
            if (players.Count == 0)
            {
                chart.TitleText = "DPS�뱩���ʶԱ� - ��������";
                return;
            }

            foreach (var player in players)
            {
                var totalDps = player.GetTotalDps();
                var critRate = player.DamageStats.GetCritRate() * 100;
                
                var playerName = string.IsNullOrEmpty(player.Nickname) ? $"���{player.Uid}" : player.Nickname;
                var points = new List<PointF> { new PointF((float)critRate, (float)totalDps) };
                
                chart.AddSeries(playerName, points);
            }

            chart.TitleText = "DPS�뱩���ʶԱ�";
        }

        #endregion

        #region �˺����ͷֲ�����ͼ

        /// <summary>
        /// �����˺����ͷֲ�����ͼ
        /// </summary>
        public static FlatBarChart CreateDamageTypeStackedChart(int width = 600, int height = 400)
        {
            var chart = new FlatBarChart()
            {
                Size = new Size(width, height),
                Dock = DockStyle.Fill,
                TitleText = "����˺����ͷֲ�",
                XAxisLabel = "���",
                YAxisLabel = "�˺�ֵ",
                IsDarkTheme = !AppConfig.IsLight
            };

            RefreshDamageTypeStackedChart(chart);
            return chart;
        }

        /// <summary>
        /// ˢ���˺����ͷֲ�����ͼ
        /// </summary>
        public static void RefreshDamageTypeStackedChart(FlatBarChart chart)
        {
            chart.ClearData();

            var players = StatisticData._manager.GetPlayersWithCombatData().ToList();
            
            if (players.Count == 0)
            {
                chart.TitleText = "����˺����ͷֲ� - ��������";
                return;
            }

            // �����˺�����������ʾ����
            players = players.OrderByDescending(p => p.DamageStats.Total).Take(6).ToList();

            // ׼�����ݣ�ʹ�����˺���Ϊ��Ҫ�Ա�ָ��
            var barData = players.Select(p => (
                Label: string.IsNullOrEmpty(p.Nickname) ? $"���{p.Uid}" : p.Nickname,
                Value: (double)p.DamageStats.Total
            )).ToList();

            chart.SetData(barData);
            chart.TitleText = "����˺����ͷֲ�";
        }

        #endregion

        #region ���߷���

        /// <summary>
        /// ����Ƿ������ݿ���ʾ
        /// </summary>
        public static bool HasDataToVisualize()
        {
            return StatisticData._manager.GetPlayersWithCombatData().Any();
        }

        /// <summary>
        /// ˢ�����д򿪵�ͼ������
        /// </summary>
        public static void RefreshAllChartThemes()
        {
            // ���ͼ���ڴ��ţ�ˢ����������
            //if (Common.realtimeChartsForm != null && !Common.realtimeChartsForm.IsDisposed)
            //{
            //    Common.realtimeChartsForm.RefreshChartsTheme();
            //}
        }

        /// <summary>
        /// ��ȡDPS��ʷ���ݵ����������ڵ��ԣ�
        /// </summary>
        public static int GetDpsHistoryPointCount()
        {
            return _dpsHistory.Sum(kvp => kvp.Value.Count);
        }

        #endregion
    }
}