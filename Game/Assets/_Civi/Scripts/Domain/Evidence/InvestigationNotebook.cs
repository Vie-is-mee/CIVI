using System;
using System.Collections.Generic;
using System.Linq;

namespace Civi.Runtime
{
    // Independent UI notebook; it never grants permission or changes story facts.
    // IDs and descriptions are grounded in TV1 S03 and canon-v4.1 F04/F05/F06.
    public sealed class InvestigationNotebook
    {
        public sealed class Entry
        {
            public readonly string Id, Title, Observation, Question;
            public Entry(string id, string title, string observation, string question)
            { Id=id;Title=title;Observation=observation;Question=question; }
        }

        public enum Verdict { NeedMoreSources, PlausibleNeedsVerification, Unsupported }

        public sealed class Deduction
        {
            public readonly Verdict Result;
            public readonly string Feedback;
            public Deduction(Verdict result, string feedback)
            { Result=result;Feedback=feedback; }
        }

        public static readonly Entry[] Sources =
        {
            new Entry("PHOTO_0441", "Ảnh 0441 / đèn tắt",
                "Nhi ở mép khung và đang cầm dây; điều này không chứng minh toàn bộ phần việc của Nhi.",
                "Ai đã xử lý thiết bị, và sau đó camera đổi tay khi nào?"),
            new Entry("PHOTO_0442", "Ảnh 0442 / đèn sáng",
                "Đèn đã sáng, ổ chia ở ghế đỏ, Nhi nhìn về phía An.",
                "Chuyện gì xảy ra giữa thời điểm này và ảnh 0443?"),
            new Entry("PHOTO_0443", "Ảnh 0443 / ba người",
                "Ảnh gốc chỉ có An, Hạ, Minh. Nới crop không thể làm xuất hiện Nhi.",
                "Vì sao Nhi không ở trong khung vào đúng khoảnh khắc này?"),
            new Entry("PHOTO_CLAMP_EMPTY", "Chi tiết chiếc kẹp",
                "Kẹp trống là dấu hiệu cần đối chiếu; một ảnh riêng lẻ chưa đủ xác nhận ai cầm máy.",
                "Có video hoặc lời kể trực tiếp nào xác nhận việc giao máy?")
        };

        readonly HashSet<string> observed = new HashSet<string>(StringComparer.Ordinal);
        public string LastHypothesis {get;private set;} = "";
        public bool HasWorkingHypothesis {get;private set;}
        public int ObservedCount {get {return observed.Count;}}
        public bool Has(string id) {return observed.Contains(id);}
        public string[] ObservedIds {get {return observed.OrderBy(x=>x,StringComparer.Ordinal).ToArray();}}
        public static bool IsAllowed(string id)
        {
            foreach(var entry in Sources)if(entry.Id==id)return true;
            return false;
        }

        public bool Observe(string id)
        {
            if(!IsAllowed(id))throw new ArgumentException("Unknown authored evidence: "+id);
            return observed.Add(id);
        }

        public void Restore(IEnumerable<string> ids, string lastHypothesis, bool working)
        {
            var next=new HashSet<string>(StringComparer.Ordinal);
            if(ids!=null)
                foreach(string id in ids)
                {
                    if(!IsAllowed(id))throw new FormatException("Unknown evidence in notebook");
                    if(!next.Add(id))throw new FormatException("Duplicate evidence in notebook");
                }
            if(lastHypothesis!=null && lastHypothesis.Length>0 &&
                lastHypothesis!="intentional"&&lastHypothesis!="unconfirmed"&&lastHypothesis!="consent")
                throw new FormatException("Unknown notebook hypothesis");
            if(working && (lastHypothesis!="unconfirmed"||next.Count<2||!next.Contains("PHOTO_0443")))
                throw new FormatException("Invalid working hypothesis");
            observed.Clear();
            observed.UnionWith(next);
            LastHypothesis=lastHypothesis??"";
            HasWorkingHypothesis=working;
        }

        // Conclusions stay provisional, and affect ONLY this local notebook UI.
        // Player may still question Nhi directly as authored in S02.
        public Deduction Propose(string hypothesis)
        {
            if(hypothesis!="intentional" && hypothesis!="unconfirmed" && hypothesis!="consent")
                throw new ArgumentException("Unknown hypothesis: "+hypothesis);
            LastHypothesis=hypothesis;
            if(hypothesis=="intentional")
            {
                HasWorkingHypothesis=false;
                return new Deduction(Verdict.Unsupported,
                    "Không đủ cơ sở để nói ai cố tình loại Nhi. Ảnh chỉ cho thấy khoảnh khắc, không cho biết động cơ.");
            }
            if(hypothesis=="consent")
            {
                HasWorkingHypothesis=false;
                return new Deduction(Verdict.Unsupported,
                    "Có mặt trong ảnh hoặc xuất hiện ở nguồn cũ không đồng nghĩa đồng ý in/đăng. Phải hỏi rõ quyền theo từng nguồn.");
            }
            if(!Has("PHOTO_0443") || ObservedCount<2)
            {
                HasWorkingHypothesis=false;
                return new Deduction(Verdict.NeedMoreSources,
                    "Hãy quan sát ảnh 0443 và ít nhất một nguồn khác trước khi đặt giả thuyết.");
            }
            HasWorkingHypothesis=true;
            return new Deduction(Verdict.PlausibleNeedsVerification,
                "Giả thuyết làm việc: Nhi ở ngoài khung 0443. Chưa biết đủ nguyên nhân; cần hỏi người liên quan hoặc xem video nguồn của Vy.");
        }
    }
}
