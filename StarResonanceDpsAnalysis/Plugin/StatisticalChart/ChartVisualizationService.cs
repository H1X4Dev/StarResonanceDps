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

        /// <summary>�Ƿ����ڽ������ݲ�������ͼ������飩</summary>
        public static bool IsCapturing { get; private set; } = false;

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

            // ����������ݵ㣬����0ֵ�������ܱ���ͼ���������
            history.Add((now, Math.Max(0, dps))); // ȷ�������и�ֵ��������0ֵ

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

            // ����������ݵ㣬����0ֵ�������ܱ���ͼ���������
            history.Add((now, Math.Max(0, hps))); // ȷ�������и�ֵ��������0ֵ

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
            
            // ����ˢ��������ҵ�ʵʱͳ������
            foreach (var player in players)
            {
                player.UpdateRealtimeStats();
            }
            
            foreach (var player in players)
            {
                // ʹ��ʵʱDPS��������ƽ��DPS��������û���˺�ʱ����ȷ��ʾΪ0
                var dps = player.DamageStats.RealtimeValue; // ��Ϊʹ��ʵʱDPS
                var hps = player.HealingStats.RealtimeValue; // ��Ϊʹ��ʵʱHPS

                // �������DPS��HPS���ݵ㣬��ʹ��0�������ܱ���������
                AddDpsDataPoint(player.Uid, dps);
                AddHpsDataPoint(player.Uid, hps);
            }
            
            // Ϊ��ȷ����ս����������ʾ0ֵ��������Ҫ����Ƿ�����ҵ�DPS/HPS��Ϊ0
            // ��ȷ����Щ0ֵҲ����¼����ʷ��
            CheckAndAddZeroValuesForInactivePlayers();
        }

        /// <summary>
        /// ��鲢Ϊ����Ծ��������0ֵ���ݵ�
        /// </summary>
        private static void CheckAndAddZeroValuesForInactivePlayers()
        {
            var activePlayers = StatisticData._manager.GetPlayersWithCombatData();
            var activePlayerIds = activePlayers.Select(p => p.Uid).ToHashSet();
            
            // ��ȡ������ʷ��¼�е����ID
            var allDpsPlayerIds = _dpsHistory.Keys.ToList();
            var allHpsPlayerIds = _hpsHistory.Keys.ToList();
            
            var now = DateTime.Now;
            
            // ΪDPS��ʷ�е���ǰ����Ծ��������0ֵ
            foreach (var playerId in allDpsPlayerIds)
            {
                if (!activePlayerIds.Contains(playerId))
                {
                    // ������һ����¼��ʱ�䣬����������ڳ���һ��ʱ���Ҳ�Ϊ0�������0ֵ
                    var history = _dpsHistory[playerId];
                    if (history.Count > 0)
                    {
                        var lastRecord = history.Last();
                        var timeSinceLastRecord = (now - lastRecord.Time).TotalSeconds;
                        
                        // ������һ����¼�������ڳ���2���Ҳ�Ϊ0�����0ֵ���ݵ�
                        if (timeSinceLastRecord > 2.0 && lastRecord.Dps > 0)
                        {
                            AddDpsDataPoint(playerId, 0);
                        }
                    }
                }
            }
            
            // ΪHPS��ʷ�е���ǰ����Ծ��������0ֵ
            foreach (var playerId in allHpsPlayerIds)
            {
                if (!activePlayerIds.Contains(playerId))
                {
                    var history = _hpsHistory[playerId];
                    if (history.Count > 0)
                    {
                        var lastRecord = history.Last();
                        var timeSinceLastRecord = (now - lastRecord.Time).TotalSeconds;
                        
                        // ������һ����¼�������ڳ���2���Ҳ�Ϊ0�����0ֵ���ݵ�
                        if (timeSinceLastRecord > 2.0 && lastRecord.Hps > 0)
                        {
                            AddHpsDataPoint(playerId, 0);
                        }
                    }
                }
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
        /// ��ȫ��������ͼ������F9������ݣ�
        /// </summary>
        public static void FullResetAllCharts()
        {
            // ���������ʷ����
            ClearAllHistory();
            
            // ��������ע���ͼ��
            lock (_registeredCharts)
            {
                foreach (var weakRef in _registeredCharts.ToList())
                {
                    if (weakRef.IsAlive && weakRef.Target is FlatLineChart chart)
                    {
                        try
                        {
                            chart.FullReset();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"����ͼ��ʱ����: {ex.Message}");
                        }
                    }
                }
                
                // ����ʧЧ������
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
        }

        /// <summary>
        /// ��ս������ʱ��Ϊ��������ʷ��¼�����������յ�0ֵ���ݵ�
        /// </summary>
        public static void OnCombatEnd()
        {
            var now = DateTime.Now;
            
            // Ϊ����DPS��ʷ��¼�е�������0ֵ�յ�
            foreach (var playerId in _dpsHistory.Keys.ToList())
            {
                var history = _dpsHistory[playerId];
                if (history.Count > 0 && history.Last().Dps > 0)
                {
                    AddDpsDataPoint(playerId, 0);
                }
            }
            
            // Ϊ����HPS��ʷ��¼�е�������0ֵ�յ�
            foreach (var playerId in _hpsHistory.Keys.ToList())
            {
                var history = _hpsHistory[playerId];
                if (history.Count > 0 && history.Last().Hps > 0)
                {
                    AddHpsDataPoint(playerId, 0);
                }
            }
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
        public static FlatLineChart CreateDpsTrendChart(int width = 800, int height = 400, ulong? specificPlayerId = null)
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

            // ע��ͼ��ȫ�ֹ���
            RegisterChart(chart);

            // �����ǰ���ڲ������ݣ���������ͼ����Զ�ˢ��
            if (IsCapturing)
            {
                chart.StartAutoRefresh(1000);
            }

            RefreshDpsTrendChart(chart, specificPlayerId);
            return chart;
        }

        /// <summary>
        /// ˢ��DPS����ͼ����
        /// </summary>
        public static void RefreshDpsTrendChart(FlatLineChart chart, ulong? specificPlayerId = null, bool showHps = false)
        {
            // ���浱ǰ����ͼ״̬�����ⱻClearSeries����
            var currentTimeScale = chart.GetTimeScale();
            var currentViewOffset = chart.GetViewOffset();
            var hadPreviousData = chart.HasData();
            
            chart.ClearSeries();

            // ������ʾ����ѡ����ʵ���ʷ����
            var historyData = showHps ? _hpsHistory : _dpsHistory;
            var dataTypeName = showHps ? "HPS" : "DPS";

            if (historyData.Count == 0 || _combatStartTime == null)
            {
                return;
            }

            var startTime = _combatStartTime.Value;

            // ���ָ�����ض����ID��ֻ��ʾ����ҵ�����
            if (specificPlayerId.HasValue)
            {
                if (historyData.TryGetValue(specificPlayerId.Value, out var playerHistory) && playerHistory.Count > 0)
                {
                    // ��ȡ�����Ϣ
                    var playerInfo = StatisticData._manager.GetPlayerBasicInfo(specificPlayerId.Value);
                    var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"���{specificPlayerId.Value}" : playerInfo.Nickname;

                    // ת��Ϊ���ʱ�䣨�룩����ֵ�ĵ㼯��
                    List<PointF> points;
                    if (showHps)
                    {
                        points = ((List<(DateTime Time, double Hps)>)playerHistory).Select(h => new PointF(
                            (float)(h.Time - startTime).TotalSeconds,
                            (float)h.Hps
                        )).ToList();
                    }
                    else
                    {
                        points = ((List<(DateTime Time, double Dps)>)playerHistory).Select(h => new PointF(
                            (float)(h.Time - startTime).TotalSeconds,
                            (float)h.Dps
                        )).ToList();
                    }

                    if (points.Count > 0)
                    {
                        chart.AddSeries($"{playerName} - {dataTypeName}����", points);
                        
                        // ����ͼ�������ʾ��ǰ��Һ���������
                        chart.TitleText = $"{playerName} - ʵʱ{dataTypeName}����";
                    }
                }
                else
                {
                    // û���ҵ�ָ����ҵ�����
                    var playerInfo = StatisticData._manager.GetPlayerBasicInfo(specificPlayerId.Value);
                    var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"���{specificPlayerId.Value}" : playerInfo.Nickname;
                    chart.TitleText = $"{playerName} - ����{dataTypeName}����";
                }
            }
            else
            {
                // ��ʾ����������ݣ�ԭ���߼���
                // �����ID����ȷ�����ݼ��ص�һ����
                var sortedHistory = historyData.OrderBy(x => x.Key);

                foreach (var kvp in sortedHistory)
                {
                    var playerId = kvp.Key;
                    var history = kvp.Value;

                    if (history.Count == 0) continue;

                    // ��ȡ�����Ϣ
                    var playerInfo = StatisticData._manager.GetPlayerBasicInfo(playerId);
                    var playerName = string.IsNullOrEmpty(playerInfo.Nickname) ? $"���{playerId}" : playerInfo.Nickname;

                    // ת��Ϊ���ʱ�䣨�룩����ֵ�ĵ㼯��
                    List<PointF> points;
                    if (showHps)
                    {
                        points = ((List<(DateTime Time, double Hps)>)history).Select(h => new PointF(
                            (float)(h.Time - startTime).TotalSeconds,
                            (float)h.Hps
                        )).ToList();
                    }
                    else
                    {
                        points = ((List<(DateTime Time, double Dps)>)history).Select(h => new PointF(
                            (float)(h.Time - startTime).TotalSeconds,
                            (float)h.Dps
                        )).ToList();
                    }

                    if (points.Count > 0)
                    {
                        chart.AddSeries(playerName, points);
                    }
                }
                
                chart.TitleText = $"ʵʱ{dataTypeName}����ͼ";
            }
            
            // ���֮ǰ���������û��й��������ָ���ͼ״̬
            if (hadPreviousData && chart.HasUserInteracted())
            {
                chart.SetTimeScale(currentTimeScale);
                chart.SetViewOffset(currentViewOffset);
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

        #region ȫ��ͼ�����

        /// <summary>
        /// ע���ͼ��ʵ���б�����ȫ�ֿ��ƣ�
        /// </summary>
        private static readonly List<WeakReference> _registeredCharts = new();

        /// <summary>
        /// ע��ͼ��ʵ���Ա�ȫ�ֹ���
        /// </summary>
        public static void RegisterChart(FlatLineChart chart)
        {
            lock (_registeredCharts)
            {
                // ������ʧЧ��������
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
                
                // ����µ�������
                _registeredCharts.Add(new WeakReference(chart));
            }
        }

        /// <summary>
        /// ֹͣ����ע���ͼ���Զ�ˢ��
        /// </summary>
        public static void StopAllChartsAutoRefresh()
        {
            IsCapturing = false; // �������״̬
            
            lock (_registeredCharts)
            {
                foreach (var weakRef in _registeredCharts.ToList())
                {
                    if (weakRef.IsAlive && weakRef.Target is FlatLineChart chart)
                    {
                        try
                        {
                            chart.StopAutoRefresh();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ֹͣͼ���Զ�ˢ��ʱ����: {ex.Message}");
                        }
                    }
                }
                
                // ����ʧЧ������
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
        }

        /// <summary>
        /// ��������ע���ͼ���Զ�ˢ��
        /// </summary>
        public static void StartAllChartsAutoRefresh(int intervalMs = 1000)
        {
            IsCapturing = true; // ���ò���״̬
            
            lock (_registeredCharts)
            {
                foreach (var weakRef in _registeredCharts.ToList())
                {
                    if (weakRef.IsAlive && weakRef.Target is FlatLineChart chart)
                    {
                        try
                        {
                            chart.StartAutoRefresh(intervalMs);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"����ͼ���Զ�ˢ��ʱ����: {ex.Message}");
                        }
                    }
                }
                
                // ����ʧЧ������
                _registeredCharts.RemoveAll(wr => !wr.IsAlive);
            }
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