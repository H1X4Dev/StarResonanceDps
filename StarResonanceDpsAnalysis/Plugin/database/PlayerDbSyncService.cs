using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using StarResonanceDpsAnalysis.Plugin.DamageStatistics;
using StarResonanceDpsAnalysis.Forms; // Ϊ����󴥷�UIˢ��

namespace StarResonanceDpsAnalysis.Plugin.Database
{
    /// <summary>
    /// API ͬ�����������ȣ�δ֪ʱ�ٻ���������д��
    /// </summary>
    public static class PlayerDbSyncService
    {
        private static readonly PlayerDbClient Client = new();
        private static readonly ConcurrentDictionary<ulong, byte> _dbFillQueued = new(); // �����ͬһUID�ظ���������

        private static bool IsUnknown(string? s) => string.IsNullOrWhiteSpace(s)
            || s == "δ֪" || s == "δ֪�ǳ�" || s == "δְ֪ҵ" || s == "Unknown";

        /// <summary>
        /// ֻ��ͬһ UID ����һ�λ������δ֪ʱ�Ż��������� API����
        /// �����ڶദ����������ʵ��/�˺��¼���ʱ��ֹ�ظ����á�
        /// </summary>
        public static void TryFillFromDbOnce(ulong uid)
        {
            if (uid == 0) return;
            if (!_dbFillQueued.TryAdd(uid, 1)) return; // �Ѿ��Ŷ�/ִ�й�
            _ = Task.Run(() => TryFillFromDbAsync(uid));
        }

        /// <summary>
        /// ������Ϊδ֪���� UID �� API ���ֻ��δ֪�ֶΣ���������֪����
        /// </summary>
        public static async Task TryFillFromDbAsync(ulong uid)
        {
            try
            {
                if (uid == 0) return;
                var (localName, localPower, localProf) = StatisticData._manager.GetPlayerBasicInfo(uid);
                bool needName = IsUnknown(localName);
                bool needProf = IsUnknown(localProf);
                bool needPower = localPower <= 0;
                if (!(needName || needProf || needPower)) return; // ������֪�������� API

                var dto = await Client.GetByUidAsync(uid);
                if (dto == null) return;

                bool changed = false;
                if (needName && !string.IsNullOrWhiteSpace(dto.Nickname))
                { StatisticData._manager.SetNickname(uid, dto.Nickname); if (uid == AppConfig.Uid) AppConfig.NickName = dto.Nickname; changed = true; }
                if (needProf && !string.IsNullOrWhiteSpace(dto.Profession))
                { StatisticData._manager.SetProfession(uid, dto.Profession); if (uid == AppConfig.Uid) AppConfig.Profession = dto.Profession; changed = true; }
                if (needPower && (dto.CombatPower ?? 0) > 0)
                { StatisticData._manager.SetCombatPower(uid, dto.CombatPower!.Value); if (uid == AppConfig.Uid) AppConfig.CombatPower = dto.CombatPower.Value; changed = true; }

                if (changed)
                {
                    // 1) ˢ�����񵥣�ְҵͼ������ Profession �ı���
                    DpsStatisticsForm.RequestActiveViewRefresh();

                    // 2) ����������鴰������չʾ����ң�����ˢ�¶���ͷ��/ְҵ����
                    var f = FormManager.skillDetailForm;
                    if (f != null && !f.IsDisposed && f.Uid == uid)
                    {
                        var info = StatisticData._manager.GetPlayerBasicInfo(uid);
                        void UpdateDetail()
                        {
                            try { f.GetPlayerInfo(info.Nickname, info.CombatPower, info.Profession); }
                            catch { }
                        }
                        if (f.InvokeRequired) f.BeginInvoke((Action)UpdateDetail); else UpdateDetail();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB-FILL] uid={uid} ex: {ex.Message}");
            }
        }

        /// <summary>
        /// �����ؽ�����������ֵд�ط�������MessageAnalyzer �н����� updated ʱ�����ã���
        /// δ֪�ַ����ᱻ��պ�д�룬���⸲����Ч���ݡ�
        /// </summary>
        public static async Task UpsertCurrentAsync(ulong uid)
        {
            try
            {
                if (uid == 0) return;
                var (nickname, combatPower, profession) = StatisticData._manager.GetPlayerBasicInfo(uid);

                // �ջ�δ֪�����ǣ����� Upsert �˵�ѡ�����ֶ��ϴ�
                string? safeNickname = IsUnknown(nickname) ? null : nickname;
                string? safeProfession = IsUnknown(profession) ? null : profession;
                int? safePower = combatPower > 0 ? combatPower : (int?)null;

                await Client.UpsertAsync(new PlayerDto(uid, safeNickname, safeProfession, safePower));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB-Upsert] uid={uid} ex: {ex.Message}");
            }
        }
    }
}
