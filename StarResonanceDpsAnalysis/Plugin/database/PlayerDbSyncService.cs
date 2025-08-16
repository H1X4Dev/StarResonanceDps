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
                if (needPower && dto.CombatPower > 0)
                { StatisticData._manager.SetCombatPower(uid, dto.CombatPower); if (uid == AppConfig.Uid) AppConfig.CombatPower = dto.CombatPower; changed = true; }

                if (changed) DpsStatisticsForm.RequestActiveViewRefresh();
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
                if (IsUnknown(nickname)) nickname = string.Empty;
                if (IsUnknown(profession)) profession = string.Empty;
                await Client.UpsertAsync(new PlayerDto(uid, nickname ?? string.Empty, profession ?? string.Empty, combatPower));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB-Upsert] uid={uid} ex: {ex.Message}");
            }
        }
    }
}
