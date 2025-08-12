﻿using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using OpenTK.Graphics.ES11;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using StarResonanceDpsAnalysis.Control;
using StarResonanceDpsAnalysis.Forms;


namespace StarResonanceDpsAnalysis.Plugin
{
    public class Common
    {
        public static SkillDiary skillDiary;//

 

        public static SkillDetailForm skillDetailForm;//技能详情窗体

        public static SettingsForm settingsForm;//设置窗体




        private static readonly Dictionary<string, List<ulong>> professionSkills = new()
        {
            { "吉他", new List<ulong> {
                 2301, 2302, 2303, 2304, 2313, 2332, 2336, 23401, 23501,
                55301, 55302, 55304, 55314, 55339, 55341, 55342,
                230101, 230401, 230501, 230901, 2031111
            }},
            { "神射", new List<ulong> {
                2233, 2288, 2289, 2295, 55231,
                220101, 220102, 220104, 220109, 220110,
                1700824, 1700826, 1700827, 2203101, 2203512
            }},
            { "神盾", new List<ulong> {
                 2401, 2402, 2403, 2404, 2405, 2407,
                2410, 2412, 2421,
                55404, 55412, 55421, 240101, 240102
            }},
            { "雷影", new List<ulong> {
                1705, 1713, 1717, 1718, 1719, 1724, 2410, 44701
            }},
            { "冰法", new List<ulong> {
                1203, 1240, 1248, 1250, 1256, 1257, 1259, 1262, 1263,
                27009, 120201, 120301, 120401, 120501,
                120901, 120902, 121302, 121501, 2204081, 2204241
            }},
            { "森语", new List<ulong> {
                1501, 1502, 1503, 1504, 1529, 1560,
                20301, 21404, 21406, 2202091,
                150103, 150104, 150106, 150107, 1550
            }},
            { "青岚", new List<ulong> {
                1401, 1402, 1403, 1404, 1419, 1420, 1421, 1422, 1424, 1425,
                1426, 1427, 1431, 149905, 149907, 31901
            }},
            { "巨刃", new List<ulong> {
                1907, 1924, 1925, 1927, 1937, 50049, 5033
            }},
        };


        #region


        ////{ 2031102, "幸运一击-" },

        ////1008440 飞鱼,1005240 姆克尖兵, 1006940 蜘蛛 1700440慕克头目 2110083 火焰食人魔 3901 火焰食人魔 2002853 火焰哥布林

        //吉他-狂音
        //private static Dictionary<ulong, string> skills15 = new()
        //{

        //    {2301,"琴弦撩拨" },
        //    {2302,"琴弦撩拨" },
        //    {2303,"琴弦撩拨" },
        //    {2304,"琴弦撩拨" },


        //    {55302,"愈合节拍" },//治疗

        //    { 55341, "鸣奏·英勇乐章" },//治疗


        //    {230401,"聚合乐章" },
        //    {230501,"聚合乐章" },
        //    {230901,"烈焰狂想" },
        //    {55301,"烈焰狂想" },

        //    {55339,"升格·巡演曲" },
        //    {2336,"升格·巡演曲" },

        //};
        ////吉他-协奏
        //private static Dictionary<ulong, string> skills16 = new()
        //{


        //    {230101,"激五重奏" },//治疗
        //     {55314,"激五重奏" },//治疗
        //    {55304,"激五重奏" },//治疗
        //    {2031111,"激五重奏" },//治疗

        //    {2332,"热情挥洒" },//派生
        //    {23401,"热情挥洒" },
        //    {23501,"热情挥洒" },
        //     {2313,"热情挥洒" },

        //    { 55342, "鸣奏·愈合乐章" },//治疗


        //};

        ////狼弓
        ////220101 平A，220104 1技能
        //private static Dictionary<ulong, string> skills1 = new()
        //{
        //    { 220101,"弹无虚发" },
        //    { 220104,"暴风箭失" },
        //    { 1700826, "狂野呼唤" },
        //    { 1700827, "野狼平A" },
        //    { 1700824,"幻影狼" },
        //    { 2203512,"野狼憾地" },
        //    { 220102,"怒涛射击" },
        //    { 2289,"箭雨" },
        //    { 2203101,"野狼甩尾" },
        //    { 2295,"锐眼·光能巨箭" },

        //};
        ////鹰弓
        //private static Dictionary<ulong, string> skills2 = new()
        //{    
        //    { 220101,"弹无虚发" },
        //    { 220104,"暴风箭失" },
        //    { 2233, "聚能射击" },
        //    { 220110, "爆炸射击" },
        //    { 55231, "爆炸射击-爆炸" },
        //    { 220109,"威慑射击" },
        //    { 2288,"光能轰炸" },
        //};

        ////神盾-防护
        ////2401 2402 2403 2404 平A 2405 英勇盾击
        //private static Dictionary<ulong, string> skills3 = new()
        //{
        //    { 2401,"公正之剑-一段" },
        //    { 2402,"公正之剑-二段" },
        //    { 2403,"公正之剑-三段" },
        //    { 2404,"公正之剑-四段" },

        //    { 2405, "英勇盾击" },
        //    { 240101, "投掷盾牌" },
        //    { 55404, "裁决-持续伤害" },
        //    { 2410, "裁决" },
        //    { 55421,"裁决2段" },
        //    { 2412,"清算" },
        //    { 55404,"圣环" },//回复技能
        //    { 2407,"凛威·圣光灌注" }
        //};
        ////神盾 光盾
        ////2401 2402 2403 2404 平A 2405 英勇盾击
        //private static Dictionary<ulong, string> skills4 = new()
        //{
        //    { 2401,"公正之剑-一段" },
        //    { 2402,"公正之剑-二段" },
        //    { 2403,"公正之剑-三段" },
        //    { 2404,"公正之剑-四段" },
        //    { 2405,"英勇盾击" },
        //    { 2421, "圣剑" },
        //    { 55404, "裁决-持续伤害" },
        //    { 2410, "裁决" },
        //    { 55421,"裁决2段" },
        //    { 240102,"光明决心" },
        //    { 55412,"冷酷征伐" },
        //    { 55404,"圣环" },//回复技能
        //    { 2407,"凛威·圣光灌注" }
        //};
        ////雷影-居合
        ////1701 平A 1702 1703 1704 // 1714 居合斩
        //private static Dictionary<ulong, string> skills5 = new()
        //{
        //    { 1705, "超高出力" },
        //    { 1717, "一闪" },
        //    { 1718, "飞雷神" },

        //    { 1713, "极诣·大破灭连斩" },
        //};
        ////雷影 月刃
        //private static Dictionary<ulong, string> skills6 = new()
        //{
        //    { 1705, "超高出力" },
        //    { 1719, "镰车" },
        //    {44701,"月刃" },
        //    { 1724, "霹雳连斩" },
        //    { 2410, "千雷闪影之意" },
        //};
        ////冰魔-冰矛
        //private static Dictionary<ulong, string> skills7 = new()
        //{
        //    { 120401, "雨打潮生" },
        //    { 1203, "雨打潮生" },
        //    { 120501, "雨打潮生" },
        //    { 120201, "雨打潮生" },
        //    { 120301, "雨打潮生" },
        //    { 120902, "冰霜之矛" },

        //    { 1248, "极寒·冰雪颂歌" },
        //    { 1263, "极寒·冰雪颂歌" },
        //    { 1262, "陨星风暴" },//1
        //    { 121501, "清淹绕珠" },//4
        //    { 1257, "寒冰风暴" },
        //    { 1250,"冰之灌注" },

        //    { 2204081, "冰箭爆炸" },
        //    { 121302, "冰箭" },
        //    { 1259, "冰霜彗星" },
        //    { 120901, "贯穿冰矛" },


        //};
        ////冰魔-射线
        //private static Dictionary<ulong, string> skills8 = new()
        //{
        //     { 1250, "水之涡流" },//2
        //     { 2204241, "冰爽冲击" },
        //     { 1240, "冻结寒风" },
        //     { 1256, "浪潮汇聚" },
        //     { 1250, "冰之灌注" },//3
        //      {27009,"寒冰庇护" }
        //};

        ////森语-惩击
        //private static Dictionary<ulong, string> skills9 = new()
        //{
        //    { 1501,"掌控藤曼-一段" },
        //    { 1502, "掌控藤曼-二段" },
        //    { 1503, "掌控藤曼-三段" },
        //    { 1504, "掌控藤曼-四段" },
        //    { 20301, "生命绽放" },//治疗
        //    { 150103, "不羁之种一段" },
        //    { 150104, "不羁之种二段" },
        //    { 1550, "不羁之种三段" },
        //    { 1560, "再生脉冲" },//治疗
        //    { 150106,"灌注一段" },
        //    { 150107,"灌注二段" },
        //   // { 0, "自然庇护" },
        //    //{ 0, "生机涌动" },

        //};
        ////森语-愈合
        //private static Dictionary<ulong, string> skills10 = new()
        //{
        //    { 21406, "森之祈愿" },//治疗
        //    {2202091,"治疗链接" },//治疗
        //    { 21404, "滋养" },//治疗
        //    {1529,"盛放注能" },//治疗
        //    //{ 0, "加速生长" },

        //};

        ////青岚-重装
        //private static Dictionary<ulong, string> skills12 = new()
        //{   
        //    {1401,"风华翔舞-一段" },
        //    {1402,"风华翔舞-二段" },
        //    {1403,"风华翔舞-三段" },
        //    {1404,"风华翔舞-四段" },
        //    {1419,"翔反" },
        //    { 1420, "风姿绰绝" },
        //    { 1421,"螺旋击刺" },
        //    { 1422, "破追" },
        //    { 1427, "破追-二段" },
        //    { 31901,"勇气风环" },


        //};

        ////青岚-空战
        //private static Dictionary<ulong, string> skills11 = new()
        //{
        //    {1401,"风华翔舞-一段" },
        //    {1402,"风华翔舞-二段" },
        //    {1403,"风华翔舞-三段" },
        //    {1404,"风华翔舞-四段" },
        //    {1419,"翔反" },
        //    {1426,"风神·破阵之风" },
        //    { 1420, "风姿绰绝" },

        //    {1425,"飞鸟投" },
        //    {149905,"飞鸟投-二段" },
        //    { 1424, "刹那" },
        //    {149907,"锐利冲击-二段" },
        //    {1431,"锐利冲击" },

        //};

        ////巨刃-岩盾
        //private static Dictionary<ulong, string> skills13 = new()
        //{
        //    {1924,"碎星冲" },
        //    {1927,"砂石斗篷" },
        //    {50049,"砂石斗篷" },
        //    //{0,"巨岩躯体" },
        //    {1925,"怒爆" },

        //};
        ////巨刃-格挡 1901-1904  一技能1922
        //private static Dictionary<ulong, string> skills14 = new()
        //{

        //    {1937,"岩怒之击" },
        //    {1927,"砂石斗篷" },
        //    {50049,"砂石斗篷" },
        //    {5033,"砂岩之握" },

        //    {1907,"岩御·崩裂回环" },
        //};




        #endregion
        private static readonly Dictionary<ulong, string> skillToProfession = new();

        static Common()
        {
            foreach (var kvp in professionSkills)
            {
                foreach (var skill in kvp.Value)
                {
                    if (!skillToProfession.TryAdd(skill, kvp.Key))
                    {
                        Console.WriteLine($"[重复技能] {skill} 已存在于 {skillToProfession[skill]}，试图加入 {kvp.Key}");
                    }
                }
            }
        }

        public static string GetProfessionBySkill(ulong skillId)
        {
            if (skillToProfession.TryGetValue(skillId, out var profession))
            {
                return profession;
            }

            //Console.WriteLine($"[未识别技能] {skillId} 不在映射中！");
            return "";
        }


        /// <summary>
        /// get请求封装
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="queryParams">请求参数</param>
        /// <param name="cookies">请求cookies</param>
        /// <returns></returns>
        public async static Task<JObject> RequestGet(string url, object queryParams = null, string cookies = "", object headers = null)
        {
            JObject data;

            try
            {
                var response = await url
                    .SetQueryParams(queryParams)
                    .GetAsync();

                // 获取响应的内容并解析为 JSON
                var result = await response.GetJsonAsync();
                data = JObject.FromObject(result);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error in HTTP request: {ex.Message}");
                data = JObject.FromObject(new { code = 401, error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                data = JObject.FromObject(new { code = 500, error = ex.Message });
            }

            return data;
        }

        /// <summary>
        /// post请求封装
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="queryParams">请求参数</param>
        /// <param name="cookies">请求cookies</param>
        /// <returns></returns>
        public async static Task<JObject> RequestPost(string url, object queryParams, string cookies = "", object headers = null)
        {
            JObject data;

            try
            {
                // 发送 POST 请求并接收 JSON 数据
                var result = await url
                    .WithCookies(cookies)
                    .WithHeaders(headers)
                    .PostJsonAsync(queryParams)
                    .ReceiveJson();
                // 将 JSON 数据转换为 JObject

                data = JObject.FromObject(result);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error in HTTP request: {ex.Message}");
                data = JObject.FromObject(new { code = 401, error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                data = JObject.FromObject(new { code = 500, error = ex.Message });
            }

            return data;
        }

        /// <summary>
        /// 请求id查看角色名
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async static Task<JObject> player_uid_map(List<string> uid)
        {
            string url = "https://api.jx3rec.com/player_uid_map";
            var query = new
            {
                uid = uid,

            };
            return await Common.RequestPost(url, query);


        }

        /// <summary>
        /// 输出单位换算
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatWithEnglishUnits<T>(T number)
        {
            double value = Convert.ToDouble(number);

            if (value < 10_000) // 小于一万直接原样（带千分位可改 ToString("N0")）
                return value % 1 == 0 ? ((long)value).ToString() : value.ToString("0.##");

            if (value >= 1_000_000_000) return (value / 1_000_000_000.0).ToString("0.##") + "B";
            if (value >= 1_000_000) return (value / 1_000_000.0).ToString("0.##") + "M";
            return (value / 1_000.0).ToString("0.##") + "K";
        }





    }






}

