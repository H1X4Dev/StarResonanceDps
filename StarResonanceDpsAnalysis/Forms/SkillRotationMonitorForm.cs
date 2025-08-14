using AntdUI;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System.ComponentModel;

namespace StarResonanceDpsAnalysis.Forms
{
    /// <summary>
    /// �����ͷ�ѭ����ⴰ��
    /// </summary>
    public partial class SkillRotationMonitorForm : BorderlessForm
    {
        #region ˽���ֶ�

        private readonly System.Windows.Forms.Timer _refreshTimer;
        private readonly System.Windows.Forms.Timer _playerListTimer; // ����������б��Զ���ⶨʱ��
        private ulong _selectedPlayerId = 0;
        private readonly List<SkillRotationData> _skillRotationHistory = new();
        private readonly Dictionary<ulong, DateTime> _lastSkillUsage = new();
        private const int MAX_HISTORY_COUNT = 200; // ��ౣ��200����ʷ��¼����ʾ���༼��
        private FlowLayoutPanel? _skillFlowPanel; // FlowLayoutPanelʵ������Ӧ����
        
        // ��ȷ�Ŀ�Ƭ�ߴ綨��
        private const int SKILL_CARD_WIDTH = 120; // ���ܿ�Ƭʵ�ʿ��
        private const int SKILL_CARD_HEIGHT = 80; // ���ܿ�Ƭʵ�ʸ߶�
        
        // FlowLayoutPanel���ֿ���
        private const int FLOW_PANEL_LEFT_PADDING = 8; // FlowLayoutPanel��߾�
        private const int FLOW_PANEL_RIGHT_PADDING = 8; // FlowLayoutPanel�ұ߾�  
        private const int FLOW_PANEL_TOP_PADDING = 5; // FlowLayoutPanel�ϱ߾�
        private const int FLOW_PANEL_BOTTOM_PADDING = 5; // FlowLayoutPanel�±߾�

        #endregion

        #region ���캯��

        public SkillRotationMonitorForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);

            // ��ʼ��ˢ�¶�ʱ��
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 500, // 500ms ˢ��һ��
                Enabled = false
            };
            _refreshTimer.Tick += RefreshTimer_Tick;

            // ��ʼ������б��Զ���ⶨʱ��
            _playerListTimer = new System.Windows.Forms.Timer
            {
                Interval = 2000, // ÿ2����һ������б�仯
                Enabled = true // Ĭ�������Զ����
            };
            _playerListTimer.Tick += PlayerListTimer_Tick;

            // ��������б�
            LoadPlayerList();
        }

        #endregion

        #region �����¼�

        private void SkillRotationMonitorForm_Load(object sender, EventArgs e)
        {
            FormGui.SetColorMode(this, AppConfig.IsLight);
            
            // ���¼�������б���ȷ����������
            LoadPlayerList();
            
            // ��ʼ��������ʾ����
            InitializeSkillDisplayArea();
            
            // ȷ��ѡ�����ʾ��ȷ�ĳ�ʼ״̬
            EnsurePlayerSelectionDisplay();

            // ע�͵��Զ���ʼ��أ���Ϊ�ֶ�����
            // StartMonitoring();
        }

        /// <summary>
        /// ����VirtualPanel����ɫ��
        /// </summary>
        private void UpdateVirtualPanelTheme()
        {
            try
            {
                if (panel_SkillRotation != null)
                {
                    panel_SkillRotation.BackColor = AppConfig.IsLight ? Color.FromArgb(245, 245, 245) : Color.FromArgb(30, 30, 30);
                    Console.WriteLine($"VirtualPanel�����Ѹ��� - ǳɫģʽ: {AppConfig.IsLight}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"����VirtualPanel����ʱ����: {ex.Message}");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopMonitoring();
            _playerListTimer?.Stop();
            _playerListTimer?.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>
        /// ȷ�����ѡ�����ʾ��ȷ��ѡ��״̬
        /// </summary>
        private void EnsurePlayerSelectionDisplay()
        {
            try
            {
                if (dropdown_PlayerSelect.Items.Count > 0 && !dropdown_PlayerSelect.Items[0].ToString().Contains("����"))
                {
                    if (_selectedPlayerId == 0 || dropdown_PlayerSelect.SelectedValue == null)
                    {
                        var firstItem = dropdown_PlayerSelect.Items[0].ToString();
                        dropdown_PlayerSelect.SelectedValue = firstItem;
                        dropdown_PlayerSelect.Text = firstItem;
                        
                        if (dropdown_PlayerSelect.Tag is Dictionary<string, ulong> playerMap && 
                            playerMap.TryGetValue(firstItem, out ulong playerId))
                        {
                            _selectedPlayerId = playerId;
                            UpdatePlayerStats();
                        }
                    }
                    else
                    {
                        var playerMap = dropdown_PlayerSelect.Tag as Dictionary<string, ulong>;
                        if (playerMap != null)
                        {
                            var currentPlayerItem = playerMap.FirstOrDefault(x => x.Value == _selectedPlayerId);
                            if (!string.IsNullOrEmpty(currentPlayerItem.Key))
                            {
                                dropdown_PlayerSelect.SelectedValue = currentPlayerItem.Key;
                                dropdown_PlayerSelect.Text = currentPlayerItem.Key;
                            }
                        }
                    }
                }

                dropdown_PlayerSelect.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ȷ�����ѡ����ʾʱ����: {ex.Message}");
            }
        }

        #endregion

        #region �����ʼ��

        /// <summary>
        /// ��ʼ��������ʾ����
        /// </summary>
        private void InitializeSkillDisplayArea()
        {
            panel_SkillRotation.Controls.Clear();
            panel_SkillRotation.BackColor = AppConfig.IsLight ? Color.FromArgb(245, 245, 245) : Color.FromArgb(30, 30, 30);
            
            _skillFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(FLOW_PANEL_LEFT_PADDING, FLOW_PANEL_TOP_PADDING,
                                       FLOW_PANEL_RIGHT_PADDING, FLOW_PANEL_BOTTOM_PADDING),
                BackColor = Color.Transparent
            };
            
            panel_SkillRotation.Controls.Add(_skillFlowPanel);
            
            // ��ʼ���ռλ
            AddNoDataPlaceholder();
            AddInstructionToStatsPanel();
        }

        /// <summary>
        /// ���˵�����ֵ�ͳ�����
        /// </summary>
        private void AddInstructionToStatsPanel()
        {
            // ��ͳ����嶥�����һ��˵�����֣�ʹ�ò�ɫ�ı�
            var instructionText = "˵����#���� ��ʾ�����ͷ�˳��+ʱ��s ��ʾ��ǰһ�����ܵļ��ʱ��";
            
            // �������ı���ǩ��֧�ֲ�ɫ�ı�
            var instructionLabel = new RichTextBox
            {
                Text = instructionText,
                Location = new Point(0, -2),
                Size = new Size(880, 22),
                Font = new Font("Microsoft YaHei", 8, FontStyle.Regular),
                BackColor = panel_Stats.BackColor,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ScrollBars = RichTextBoxScrollBars.None
            };
            
            // ���ò�ɫ�ı�
            SetInstructionTextColors(instructionLabel);
            
            // ���µ������б�ǩ��λ�ã�Ϊ˵�������ڳ��ռ�
            label_PlayerName.Location = new Point(0, 20);
            label_TotalSkills.Location = new Point(220, 20);
            label_LastSkillTime.Location = new Point(440, 20);
            label_AvgInterval.Location = new Point(660, 20);
            
            // ���˵����ǩ��ͳ�����
            panel_Stats.Controls.Add(instructionLabel);
        }

        /// <summary>
        /// ����˵�����ֵ���ɫ���뼼�ܿ�Ƭ��ɫ��Ӧ
        /// </summary>
        private void SetInstructionTextColors(RichTextBox richTextBox)
        {
            try
            {
                // ���� "#����" ���ֵ���ɫΪ��ɫ���뼼�ܿ�Ƭ�е������ɫһ�£�"
                var hashIndex = richTextBox.Text.IndexOf("#����");
                if (hashIndex >= 0)
                {
                    richTextBox.Select(hashIndex, 3);
                    richTextBox.SelectionColor = Color.Blue;
                }

                // ���� "+ʱ��s" ���ֵ���ɫΪ��ɫ���뼼�ܿ�Ƭ�е�ʱ������ɫһ�£�
                var timeIndex = richTextBox.Text.IndexOf("+ʱ��s");
                if (timeIndex >= 0)
                {
                    richTextBox.Select(timeIndex, 4);
                    richTextBox.SelectionColor = Color.Orange;
                }

                // �ָ�ѡ�񵽿�ͷ
                richTextBox.Select(0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"����˵��������ɫʱ����: {ex.Message}");
            }
        }

        #endregion

        #region ��ؿ���

        /// <summary>
        /// ��ʼ���
        /// </summary>
        private void StartMonitoring()
        {
            if (_refreshTimer.Enabled) return;

            // ÿ�ο�ʼ�����Ϊ��һ�ּ�⣬����״̬��UI
            ResetDetectionState(rebuildUi: true);

            _refreshTimer.Enabled = true;
            button_StartStop.Text = "ֹͣ���";
            button_StartStop.Type = AntdUI.TTypeMini.Error;
        }

        /// <summary>
        /// ֹͣ���
        /// </summary>
        private void StopMonitoring()
        {
            if (!_refreshTimer.Enabled) return;

            _refreshTimer.Enabled = false;
            button_StartStop.Text = "��ʼ���";
            button_StartStop.Type = AntdUI.TTypeMini.Primary;
        }

        /// <summary>
        /// ���ñ��μ����ڴ�״̬����ѡ��UI
        /// </summary>
        private void ResetDetectionState(bool rebuildUi)
        {
            _skillRotationHistory.Clear();
            _lastSkillUsage.Clear();

            if (rebuildUi)
            {
                RebuildAllSkillCards();
                UpdatePlayerStats();
            }
        }

        #endregion

        #region ���ݸ���

        /// <summary>
        /// ����б��Զ���ⶨʱ���¼�
        /// </summary>
        private void PlayerListTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // ��ȡ��ǰ����б�
                var currentPlayers = StatisticData._manager.GetPlayersWithCombatData().ToList();
                var currentPlayerMap = dropdown_PlayerSelect.Tag as Dictionary<string, ulong>;
                
                // ���������������仯�����¼����б�
                if (currentPlayerMap == null || currentPlayers.Count != currentPlayerMap.Count)
                {
                    Console.WriteLine("��⵽����б�仯���Զ�����...");
                    
                    // ���浱ǰѡ��״̬
                    var previousSelection = dropdown_PlayerSelect.SelectedValue?.ToString();
                    
                    // ���¼����б�
                    LoadPlayerList();
                    
                    // ȷ��ѡ��״̬��ȷ��ʾ
                    EnsurePlayerSelectionDisplay();
                    
                    // ���ѡ�����˱仯�������־
                    var newSelection = dropdown_PlayerSelect.SelectedValue?.ToString();
                    if (previousSelection != newSelection)
                    {
                        Console.WriteLine($"���ѡ���Ѹ���: '{previousSelection}' -> '{newSelection}'");
                    }
                }
                else if (dropdown_PlayerSelect.SelectedValue == null && _selectedPlayerId != 0)
                {
                    // ����ѡ�����ʾΪ�յ��ڲ���ѡ����ҵ����
                    EnsurePlayerSelectionDisplay();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"����б��Զ����ʱ����: {ex.Message}");
            }
        }

        /// <summary>
        /// ˢ�¶�ʱ���¼������ģ�ֻ����ǰѡ����ҵ��������ܣ�
        /// </summary>
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_selectedPlayerId == 0) return;

            try
            {
                var playerData = StatisticData._manager.GetOrCreate(_selectedPlayerId);
                var skillSummaries = playerData.GetSkillSummaries(
                    topN: null,
                    orderByTotalDesc: false,
                    filterType: StarResonanceDpsAnalysis.Core.SkillType.Damage);

                // ��������
                CheckAndAddNewSkills(skillSummaries);

                // ֻ���¶���ͳ��
                UpdatePlayerStats();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"����ѭ�����ˢ��ʱ����: {ex.Message}");
            }
        }

        /// <summary>
        /// ��skillSummaries��δ���ֹ���ʱ����¹��ļ���������뿨Ƭ
        /// </summary>
        private void CheckAndAddNewSkills(List<SkillSummary> skillSummaries)
        {
            if (_skillFlowPanel == null || skillSummaries == null || skillSummaries.Count == 0) return;

            foreach (var skill in skillSummaries)
            {
                if (!skill.LastTime.HasValue) continue;

                var id = skill.SkillId;
                var lastTime = skill.LastTime.Value;

                // ���ü����ǵ�һ�γ��ֻ�lastTime���£����Ϊһ��ʹ��
                if (!_lastSkillUsage.TryGetValue(id, out var prev) || lastTime > prev)
                {
                    _lastSkillUsage[id] = lastTime;

                    var data = new SkillRotationData
                    {
                        SkillId = id,
                        SkillName = skill.SkillName,
                        UseTime = lastTime,
                        Damage = skill.Total,
                        HitCount = skill.HitCount,
                        SequenceNumber = _skillRotationHistory.Count + 1
                    };

                    _skillRotationHistory.Add(data);

                    // ������ʷ���Ȳ�ά��UI
                    if (_skillRotationHistory.Count > MAX_HISTORY_COUNT)
                    {
                        RemoveOldestSkill();
                        UpdateAllCardSequenceNumbers();
                    }

                    AddNewSkillCard(data);
                }
            }
        }
        /// <summary>
        /// �Ƴ����ϵļ��ܼ�¼�Ϳ�Ƭ
        /// </summary>
        private void RemoveOldestSkill()
        {
            if (_skillRotationHistory.Count == 0 || _skillFlowPanel == null) return;
            
            // �Ƴ����ϵļ�¼
            _skillRotationHistory.RemoveAt(0);
            
            // ���±��
            for (int i = 0; i < _skillRotationHistory.Count; i++)
            {
                _skillRotationHistory[i].SequenceNumber = i + 1;
            }

            // �Ƴ����ϵļ��ܿ�Ƭ
            if (_skillFlowPanel.Controls.Count > 0)
            {
                var oldestCard = _skillFlowPanel.Controls[0];
                _skillFlowPanel.Controls.Remove(oldestCard);
                oldestCard.Dispose();
            }
        }

        /// <summary>
        /// ��������¼��ܿ�Ƭ
        /// </summary>
        private void AddNewSkillCard(SkillRotationData skill)
        {
            if (_skillFlowPanel == null) return;
            
            var skillCard = CreateSkillCard(skill);
            
            this.BeginInvoke(new Action(() =>
            {
                // ��������ڡ����޼����ͷż�¼����ռλ��ǩ�����Ƴ�
                RemoveNoDataPlaceholderIfPresent();
                
                _skillFlowPanel.SuspendLayout();
                try
                {
                    _skillFlowPanel.Controls.Add(skillCard);
                }
                finally
                {
                    _skillFlowPanel.ResumeLayout(true);
                }
                
                // �Զ�������������ӵļ��ܿ�Ƭ
                if (_skillFlowPanel.AutoScroll)
                {
                    _skillFlowPanel.ScrollControlIntoView(skillCard);
                }
            }));
        }

        /// <summary>
        /// ������ڡ����޼����ͷż�¼����ռλ��ǩ�������Ƴ�
        /// </summary>
        private void RemoveNoDataPlaceholderIfPresent()
        {
            if (_skillFlowPanel == null || _skillFlowPanel.Controls.Count == 0) return;

            // ����ռλ��ǩ���� Dock = Fill ���ı�ƥ�䣬������ɾ��
            var placeholders = _skillFlowPanel.Controls
                .OfType<AntdUI.Label>()
                .Where(l => (l.Dock == DockStyle.Fill) || string.Equals(l.Text, "���޼����ͷż�¼", StringComparison.Ordinal))
                .ToList();

            foreach (var ph in placeholders)
            {
                _skillFlowPanel.Controls.Remove(ph);
                ph.Dispose();
            }
        }

        /// <summary>
        /// �����������п�Ƭ�����
        /// </summary>
        private void UpdateAllCardSequenceNumbers()
        {
            if (_skillFlowPanel == null) return;
            
            var cards = _skillFlowPanel.Controls.OfType<System.Windows.Forms.Panel>().ToList();
            
            for (int i = 0; i < cards.Count && i < _skillRotationHistory.Count; i++)
            {
                var card = cards[i];
                var skill = _skillRotationHistory[i];
                
                // ���¿�Ƭ�ϵ���ű�ǩ
                var sequenceLabel = card.Controls.OfType<AntdUI.Label>()
                    .FirstOrDefault(l => l.Text.StartsWith("#"));
                
                if (sequenceLabel != null)
                {
                    sequenceLabel.Text = $"#{skill.SequenceNumber}";
                }
            }
        }

        /// <summary>
        /// �ؽ����м��ܿ�Ƭ��ͳһ���ؽ�������
        /// </summary>
        private void RebuildAllSkillCards()
        {
            if (_skillFlowPanel == null) return;
            
            try
            {
                // ������пؼ�
                foreach (System.Windows.Forms.Control control in _skillFlowPanel.Controls)
                {
                    control?.Dispose();
                }
                _skillFlowPanel.Controls.Clear();

                if (_skillRotationHistory.Count == 0)
                {
                    AddNoDataPlaceholder();
                    return;
                }

                // ����������������м��ܿ�Ƭ
                var cardsToAdd = _skillRotationHistory.Select(CreateSkillCard).ToArray();
                _skillFlowPanel.Controls.AddRange(cardsToAdd);

                Console.WriteLine($"�ؽ����ܿ�Ƭ��ɣ�����: {cardsToAdd.Length}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"�ؽ����ܿ�Ƭʱ����: {ex.Message}");
            }
        }

        /// <summary>
        /// ��ӡ����޼����ͷż�¼����ռλ��ǩ
        /// </summary>
        private void AddNoDataPlaceholder()
        {
            if (_skillFlowPanel == null) return;
            var noDataLabel = new AntdUI.Label
            {
                Text = "���޼����ͷż�¼",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            _skillFlowPanel.Controls.Add(noDataLabel);
        }

        /// <summary>
        /// �������ܿ�Ƭ
        /// </summary>
        private System.Windows.Forms.Panel CreateSkillCard(SkillRotationData skill)
        {
            var card = new System.Windows.Forms.Panel
            {
                Size = new Size(SKILL_CARD_WIDTH, SKILL_CARD_HEIGHT),
                Margin = new Padding(2, 2, 2, 2),
                BackColor = AppConfig.IsLight ? Color.FromArgb(250, 250, 250) : Color.FromArgb(45, 45, 45),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = skill.SkillId.ToString()
            };

            // ��������
            var nameLabel = new AntdUI.Label
            {
                Text = skill.SkillName.Length > 8 ? skill.SkillName.Substring(0, 8) + "..." : skill.SkillName,
                Location = new Point(5, 5),
                Size = new Size(SKILL_CARD_WIDTH - 10, 20),
                Font = new Font("Microsoft YaHei", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.TopCenter
            };

            // ʹ��ʱ��
            var timeLabel = new AntdUI.Label
            {
                Text = skill.UseTime.ToString("HH:mm:ss"),
                Location = new Point(5, 25),
                Size = new Size(SKILL_CARD_WIDTH - 10, 15),
                Font = new Font("Microsoft YaHei", 7),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter
            };

            // ���
            var sequenceLabel = new AntdUI.Label
            {
                Text = $"#{skill.SequenceNumber}",
                Location = new Point(5, 45),
                Size = new Size(SKILL_CARD_WIDTH - 10, 15),
                Font = new Font("Microsoft YaHei", 7),
                ForeColor = Color.Blue,
                TextAlign = ContentAlignment.TopCenter
            };

            card.Controls.AddRange(new System.Windows.Forms.Control[] { nameLabel, timeLabel, sequenceLabel });

            // ���ʱ�䣨������ǵ�һ�����ܣ�
            if (skill.SequenceNumber > 1)
            {
                var prevSkill = _skillRotationHistory.FirstOrDefault(s => s.SequenceNumber == skill.SequenceNumber - 1);
                if (prevSkill != null)
                {
                    var interval = (skill.UseTime - prevSkill.UseTime).TotalSeconds;
                    var intervalLabel = new AntdUI.Label
                    {
                        Text = $"+{interval:F1}s",
                        Location = new Point(5, 60),
                        Size = new Size(SKILL_CARD_WIDTH - 10, 15),
                        Font = new Font("Microsoft YaHei", 6),
                        ForeColor = Color.Orange,
                        TextAlign = ContentAlignment.TopCenter
                    };
                    card.Controls.Add(intervalLabel);
                }
            }

            return card;
        }

        /// <summary>
        /// �������ͳ����Ϣ
        /// </summary>
        private void UpdatePlayerStats()
        {
            if (_selectedPlayerId == 0) return;

            try
            {
                var playerData = StatisticData._manager.GetOrCreate(_selectedPlayerId);
                var playerInfo = StatisticData._manager.GetPlayerBasicInfo(_selectedPlayerId);

                label_PlayerName.Text = $"���: {playerInfo.Nickname}";
                label_TotalSkills.Text = $"��������: {_skillRotationHistory.Count}";
                label_LastSkillTime.Text = _skillRotationHistory.Count > 0 
                    ? $"�����: {_skillRotationHistory.Last().UseTime:HH:mm:ss}"
                    : "�����: ��";

                // ����ƽ�����ܼ��
                if (_skillRotationHistory.Count > 1)
                {
                    var intervals = new List<double>();
                    for (int i = 1; i < _skillRotationHistory.Count; i++)
                    {
                        var interval = (_skillRotationHistory[i].UseTime - _skillRotationHistory[i - 1].UseTime).TotalSeconds;
                        intervals.Add(interval);
                    }
                    var avgInterval = intervals.Average();
                    label_AvgInterval.Text = $"ƽ�����: {avgInterval:F1}s";
                }
                else
                {
                    label_AvgInterval.Text = "ƽ�����: ��";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"�������ͳ����Ϣʱ����: {ex.Message}");
            }
        }

        /// <summary>
        /// ��������б�
        /// </summary>
        private void LoadPlayerList()
        {
            try
            {
                var currentSelectedValue = dropdown_PlayerSelect.SelectedValue?.ToString();
                var currentSelectedPlayerId = _selectedPlayerId;
                
                dropdown_PlayerSelect.Items.Clear();
                dropdown_PlayerSelect.Tag = new Dictionary<string, ulong>();

                var players = StatisticData._manager.GetPlayersWithCombatData().ToList();
                
                if (players.Count == 0)
                {
                    const string noData = "�����������";
                    dropdown_PlayerSelect.Items.Add(noData);
                    dropdown_PlayerSelect.SelectedValue = noData;
                    dropdown_PlayerSelect.Text = noData;
                    return;
                }

                var playerMap = (Dictionary<string, ulong>)dropdown_PlayerSelect.Tag;
                string? itemToSelect = null;

                foreach (var player in players)
                {
                    var playerInfo = StatisticData._manager.GetPlayerBasicInfo(player.Uid);
                    var displayText = $"{playerInfo.Nickname} (UID: {player.Uid})";
                    
                    dropdown_PlayerSelect.Items.Add(displayText);
                    playerMap[displayText] = player.Uid;
                    
                    if (currentSelectedPlayerId != 0 && player.Uid == currentSelectedPlayerId)
                    {
                        itemToSelect = displayText;
                    }
                    else if (string.IsNullOrEmpty(itemToSelect) && !string.IsNullOrEmpty(currentSelectedValue) && displayText == currentSelectedValue)
                    {
                        itemToSelect = displayText;
                    }
                }

                if (!string.IsNullOrEmpty(itemToSelect))
                {
                    dropdown_PlayerSelect.SelectedValue = itemToSelect;
                    dropdown_PlayerSelect.Text = itemToSelect;
                }
                else if (dropdown_PlayerSelect.Items.Count > 0)
                {
                    var firstItem = dropdown_PlayerSelect.Items[0].ToString();
                    dropdown_PlayerSelect.SelectedValue = firstItem;
                    dropdown_PlayerSelect.Text = firstItem;
                    
                    if (playerMap.TryGetValue(firstItem, out ulong firstPlayerId))
                    {
                        _selectedPlayerId = firstPlayerId;
                    }
                }

                dropdown_PlayerSelect.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"��������б�ʱ����: {ex.Message}");
            }
        }

        #endregion

        #region �ؼ��¼�

        /// <summary>
        /// ���ѡ��ı��¼����л��ӽǼ���Ϊ��һ�ּ�⣩
        /// </summary>
        private void dropdown_PlayerSelect_SelectedValueChanged(object sender, ObjectNEventArgs e)
        {
            try
            {
                var selectedText = dropdown_PlayerSelect.SelectedValue?.ToString();
                if (string.IsNullOrEmpty(selectedText)) return;
                
                // ͬ����ʾ�ı����޸�ѡ�к���ʾ������
                if (dropdown_PlayerSelect.Text != selectedText)
                {
                    dropdown_PlayerSelect.Text = selectedText;
                }

                if (dropdown_PlayerSelect.Tag is not Dictionary<string, ulong> playerMap) return;
                if (!playerMap.TryGetValue(selectedText, out var playerId)) return;

                if (_selectedPlayerId == playerId) { UpdatePlayerStats(); return; }

                _selectedPlayerId = playerId;
                ResetDetectionState(rebuildUi: true);
                UpdatePlayerStats();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"���ѡ��ı�ʱ����: {ex.Message}");
            }
        }

        /// <summary>
        /// ��ʼ/ֹͣ��ذ�ť����¼�
        /// </summary>
        private void button_StartStop_Click(object sender, EventArgs e)
        {
            if (_refreshTimer.Enabled)
            {
                StopMonitoring();
            }
            else
            {
                StartMonitoring();
            }
        }

        /// <summary>
        /// ������ݣ����ı�ѡ����ң�
        /// </summary>
        private void button_Clear_Click(object sender, EventArgs e)
        {
            ResetDetectionState(rebuildUi: true);
            Console.WriteLine("����������");
        }

        /// <summary>
        /// ˢ������б�ť����¼�
        /// </summary>
        private void button_RefreshPlayers_Click(object sender, EventArgs e)
        {
            LoadPlayerList();
        }

        /// <summary>
        /// �رմ��ڰ�ť����¼�
        /// </summary>
        private void button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// ������ק
        /// </summary>
        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

        #endregion

        #region ����ģ��

        /// <summary>
        /// ����ѭ������
        /// </summary>
        private class SkillRotationData
        {
            public ulong SkillId { get; set; }
            public string SkillName { get; set; } = "";
            public DateTime UseTime { get; set; }
            public ulong Damage { get; set; }
            public int HitCount { get; set; }
            public int SequenceNumber { get; set; }
        }

        #endregion

        #region ���ÿ�Ƭ���ֺ����´����ؼ�

        /// <summary>
        /// ���ÿ�Ƭ���ֲ����´������пؼ�
        /// </summary>
        private void ResetCardLayoutAndRecreateControls()
        {
            if (_skillFlowPanel == null) return;
            
            try
            {
                // ��ͣ���ּ���
                _skillFlowPanel.SuspendLayout();
                if (panel_SkillRotation is System.Windows.Forms.Control control)
                {
                    control.SuspendLayout();
                }
                
                // ����FlowLayoutPanel״̬
                _skillFlowPanel.AutoScrollPosition = new Point(0, 0);
                _skillFlowPanel.HorizontalScroll.Value = 0;
                _skillFlowPanel.VerticalScroll.Value = 0;
                
                // �ؽ����п�Ƭ
                RebuildAllSkillCards();
                
                // �ָ����ּ���
                _skillFlowPanel.ResumeLayout(true);
                if (panel_SkillRotation is System.Windows.Forms.Control resumeControl)
                {
                    resumeControl.ResumeLayout(true);
                }
                
                Console.WriteLine($"��Ƭ����������� - ��������: {_skillRotationHistory.Count}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"���ÿ�Ƭ����ʱ����: {ex.Message}");
                
                // ����ʱȷ�����ָֻ�
                _skillFlowPanel?.ResumeLayout(true);
                if (panel_SkillRotation is System.Windows.Forms.Control errorControl)
                {
                    errorControl.ResumeLayout(true);
                }
            }
        }

        #endregion
    }
}