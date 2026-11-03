using System;
using System.Collections.Generic;
using System.Linq;

namespace Civi.Runtime
{
    // Authoring route exactly follows "Cách đọc bản phân nhánh" (28 Oct 2026):
    // I-A/B after Ch5 -> Ch6; II-A/B after Ch7 -> Ch8;
    // A/B replace the original end of Ch9 and all Ch10; C continues Ch9/10.
    public static class CiviCampaignGraph
    {
        public static readonly string[] RequiredEpisodes =
        {
            "C1","C2","C3","C4","C5","I_A","I_B","C6","C7",
            "II_A","II_B","C8","C9","C9C","C10","END_A","END_B"
        };

        public static readonly string[] PrimaryOrder =
        {"C1","C2","C3","C4","C5","C6","C7","C8","C9","C9C","C10"};

        public static bool IsDecision(string id)
        { return id=="C5"||id=="C7"||id=="C9"; }

        public static string[] Options(string id)
        {
            switch(id)
            {
                case "C5":return new[]{"I_A","I_B"};
                case "C7":return new[]{"II_A","II_B"};
                case "C9":return new[]{"END_A","END_B","C9C"};
                default:return Array.Empty<string>();
            }
        }

        public static string OptionLabel(string id)
        {
            switch(id)
            {
                case "I_A":return "I-A  ·  ĐỐI THOẠI VỚI GIA ĐÌNH, BẢO VỆ HỒ SƠ";
                case "I_B":return "I-B  ·  KIỂM TRA KÍN, GIẢM RỦI RO RÒ RỈ";
                case "II_A":return "II-A  ·  CÔNG BỐ TỔNG HỢP, CẢNH BÁO SAI SỐ";
                case "II_B":return "II-B  ·  XÁC MINH THÊM VỚI CƯ DÂN";
                case "END_A":return "A  ·  BẢO TOÀN NGUỒN DỰ TRỮ";
                case "END_B":return "B  ·  CÔNG BỐ DỮ LIỆU CHI TIẾT";
                case "C9C":return "C  ·  BIỂU QUYẾT CHIA SẺ CÓ ĐIỀU KIỆN";
                default:throw new ArgumentException("Unknown authored route "+id);
            }
        }
        public static bool IsEnding(string id)
        {return id=="END_A"||id=="END_B"||id=="C10";}
        public static string EndingLabel(string id)
        {
            switch(id)
            {
                case "END_A":return "KẾT THÚC A  /  NHỮNG CỬA SỔ VẪN SÁNG";
                case "END_B":return "KẾT THÚC B  /  MỘT SỰ THẬT KHÔNG ĐỦ";
                case "C10":return "KẾT THÚC C  /  SAU CƠN MƯA";
                default:return "";
            }
        }
        public static string Next(string current,string choice=null)
        {
            switch(current)
            {
                case "C1":return "C2";case "C2":return "C3";
                case "C3":return "C4";case "C4":return "C5";
                case "C5":return Choose(current,choice);
                case "I_A":case "I_B":return "C6";
                case "C6":return "C7";case "C7":return Choose(current,choice);
                case "II_A":case "II_B":return "C8";
                case "C8":return "C9";case "C9":return Choose(current,choice);
                case "C9C":return "C10";
                case "END_A":case "END_B":case "C10":return null;
                default:throw new ArgumentException("Unknown chapter "+current);
            }
        }
        static string Choose(string current,string selected)
        {
            if(!Options(current).Contains(selected))
                throw new ArgumentException("Invalid decision route "+current+" -> "+selected);
            return selected;
        }
        public static void Validate(IEnumerable<string> ids)
        {
            var provided=new HashSet<string>(ids,StringComparer.Ordinal);
            if(!provided.SetEquals(RequiredEpisodes))
                throw new InvalidOperationException("Campaign content nodes do not match branching graph.");
            // Every route, across three choices, must terminate and never loop.
            foreach(var a in Options("C5"))
            foreach(var b in Options("C7"))
            foreach(var c in Options("C9"))
            {
                string node="C1";var seen=new HashSet<string>();
                while(node!=null)
                {
                    if(!seen.Add(node))throw new InvalidOperationException("Campaign loop: "+node);
                    string pick=node=="C5"?a:node=="C7"?b:node=="C9"?c:null;
                    node=Next(node,pick);
                }
                if(seen.Count != (c=="C9C"?13:12))
                    throw new InvalidOperationException("Unexpected route length: "+seen.Count);
            }
        }
    }
}
