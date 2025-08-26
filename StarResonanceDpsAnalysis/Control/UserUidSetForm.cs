﻿using AntdUI;
using StarResonanceDpsAnalysis.Forms;
using StarResonanceDpsAnalysis.Plugin;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using System.Runtime.InteropServices;

namespace StarResonanceDpsAnalysis.Control
{
    public partial class UserUidSetForm : BorderlessForm
    {
        public UserUidSetForm()
        {
            InitializeComponent();
            FormGui.SetDefaultGUI(this);
        }

        /// <summary>
        /// 优化的UID设置控件 - 增强验证和用户体验
        /// </summary>
        private void UserUidSet_Load(object sender, EventArgs e)
        {
            // 从AppConfig加载已保存的设置到界面
            LoadCurrentSettingsToUI();

            // 添加实时验证
            InitializeValidation();

            // 显示当前用户信息
            DisplayCurrentUserInfo();
        }

        /// <summary>
        /// 从AppConfig加载当前设置到界面控件
        /// </summary>
        private void LoadCurrentSettingsToUI()
        {
            try
            {
                // 加载昵称设置
                string savedNickname = AppConfig.GetValue("UserConfig", "NickName", "Unknown Nickname");
                input2.Text = savedNickname;

                // 安全地加载UID设置
                string savedUidStr = AppConfig.GetValue("UserConfig", "Uid", "0");
                if (ulong.TryParse(savedUidStr, out ulong savedUid))
                {
                    inputNumber1.Value = savedUid;
                    Console.WriteLine($"Loaded saved settings - UID: {savedUid}, Nickname: {savedNickname}");
                }
                else
                {
                    inputNumber1.Value = 0;
                    Console.WriteLine($"Invalid UID format in config: {savedUidStr}, reset to 0");

                    // 修复损坏的配置
                    AppConfig.SetValue("UserConfig", "Uid", "0");
                }
                select1.SelectedValue = AppConfig.GetValue("UserConfig", "Profession", "Unknown Profession");



                // 确保AppConfig的全局属性与界面同步
                AppConfig.Uid = (ulong)inputNumber1.Value;
                AppConfig.NickName = input2.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user settings: {ex.Message}");

                // 出错时设置默认值
                inputNumber1.Value = 0;
                input2.Text = "Unknown Nickname";
            }
        }

        /// <summary>
        /// 初始化输入验证
        /// </summary>
        private void InitializeValidation()
        {
            // UID输入验证 - 确保是有效的ulong范围
            inputNumber1.ValueChanged += (s, e) =>
            {
                if (inputNumber1.Value > ulong.MaxValue || inputNumber1.Value < 0)
                {
                    inputNumber1.Value = 0;
                    MessageBox.Show("UID must be a number between 0 and " + ulong.MaxValue, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // 昵称输入验证
            input2.TextChanged += (s, e) =>
            {
                string nickname = input2.Text.Trim();
                if (nickname.Length > 20)
                {
                    input2.Text = nickname.Substring(0, 20);
                    input2.SelectionStart = input2.Text.Length;
                    MessageBox.Show("Nickname cannot exceed 20 characters", "Input Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
        }

        /// <summary>
        /// 显示当前用户信息
        /// </summary>
        private void DisplayCurrentUserInfo()
        {
            ulong currentUid = (ulong)inputNumber1.Value;
            if (currentUid > 0)
            {
                var (nickname, combatPower, profession) = StatisticData._manager.GetPlayerBasicInfo(currentUid);

                // 可以添加一个信息显示区域
                Console.WriteLine($"Current user - UID: {currentUid}, Nickname: {nickname}, Power: {combatPower}, Profession: {profession}");

                // 如果有UI标签可以显示这些信息
                // lblCurrentInfo.Text = $"当前: {nickname} (战力: {combatPower})";
            }
        }

        /// <summary>
        /// 公开的保存用户设置方法，供Modal调用
        /// </summary>
        public void SaveUserSettings()
        {
            // 验证输入数据
            if (!ValidateInput(out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            // 从界面获取当前输入的值
            ulong newUid = (ulong)inputNumber1.Value;
            string newNickname = input2.Text.Trim();
            string profession = select1.SelectedValue.ToString().Trim();

            // 获取原始配置值用于比较
            string oldUidStr = AppConfig.GetValue("UserConfig", "Uid", "0");
            string oldNickname = AppConfig.GetValue("UserConfig", "NickName", "Unknown Nickname");
            string oldProfession = AppConfig.GetValue("UserConfig", "Profession", "Unknown Profession");


            bool uidChanged = !ulong.TryParse(oldUidStr, out ulong oldUid) || oldUid != newUid;
            bool nicknameChanged = oldNickname != newNickname;
            bool professionChanged = oldProfession != profession;

            // 只有当值真正发生变化时才保存
            if (!uidChanged && !nicknameChanged && !professionChanged)
            {
                Console.WriteLine("No changes to user info; skip saving");
                return;
            }

            // 保存界面配置到AppConfig
            if (uidChanged)
            {
                AppConfig.SetValue("UserConfig", "Uid", newUid.ToString());
                Console.WriteLine($"UID updated: {oldUid} → {newUid}");
            }

            if (nicknameChanged)
            {
                AppConfig.SetValue("UserConfig", "NickName", newNickname);
                Console.WriteLine($"Nickname updated: {oldNickname} → {newNickname}");
            }

            if (professionChanged)
            {
                AppConfig.SetValue("UserConfig", "Profession", profession);
                Console.WriteLine($"Profession updated: {oldProfession} → {profession}");
            }

            // 更新全局AppConfig属性以保持一致性
            AppConfig.Uid = newUid;
            AppConfig.NickName = newNickname;
            AppConfig.Profession = profession;

            // 同步到统计数据管理器
            StatisticData._manager.SetNickname(newUid, newNickname);
            StatisticData._manager.SetProfession(newUid, profession);

            // 如果UID发生变化，询问用户是否清空统计数据
            if (uidChanged && oldUid != 0)
            {
                var result = MessageBox.Show(
                    $"Detected UID change from {oldUid} to {newUid}\n" +
                    "This may affect current statistics association.\n" +
                    "Clear current statistics to avoid confusion?",
                    "UID Change Notice",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    StatisticData._manager.ClearAll(false);
                    Console.WriteLine("Cleared statistics due to UID change");
                }
            }

            // 显示保存成功的反馈
            Console.WriteLine($"Settings saved - UID: {newUid}, Nickname: {newNickname}");
        }

        /// <summary>
        /// 验证用户输入
        /// </summary>
        private bool ValidateInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            // 验证UID
            if (inputNumber1.Value <= 0)
            {
                errorMessage = "UID must be greater than 0";
                return false;
            }

            // 验证昵称
            string nickname = input2.Text.Trim();
            if (string.IsNullOrEmpty(nickname))
            {
                errorMessage = "Nickname cannot be empty";
                return false;
            }

            if (nickname.Length > 20)
            {
                errorMessage = "Nickname cannot exceed 20 characters";
                return false;
            }

            // 可以添加更多验证规则，如特殊字符检查
            if (nickname.Contains("<") || nickname.Contains(">") || nickname.Contains("&"))
            {
                errorMessage = "Nickname cannot contain special characters < > &";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 原始的保存按钮逻辑 - 保留向后兼容性
        /// </summary>
        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SaveUserSettings();
                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user settings: {ex.Message}", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Exception saving user settings: {ex}");
            }
        }

        private void UserUidSet_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panel6_Click(object sender, EventArgs e)
        {

        }


        private void TitleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FormManager.ReleaseCapture();
                FormManager.SendMessage(this.Handle, FormManager.WM_NCLBUTTONDOWN, FormManager.HTCAPTION, 0);
            }
        }

    }
}
