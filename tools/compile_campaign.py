from pathlib import Path
import json,re,hashlib
base=Path(__file__).resolve().parents[1]/"Game"
root=base/"Assets/_Civi/Resources/NewStory"
source=(root/"novel_source.txt").read_text(encoding="utf-8")
markers=[
("C1","## Chương 1.","## Chương 2.","Lõi Sáng","core"),
("C2","## Chương 2.","## Chương 3.","Rìa Mưa","rain"),
("C3","## Chương 3.","## Chương 4.","Vành Đệm","core"),
("C4","## Chương 4.","## Chương 5.","Học viện Lưu Quang","core"),
("C5","## Chương 5.","<a id=\"diem-re-1\"></a>","Trạm y tế","filter"),
("I_A","## Ngã rẽ I-A","## Ngã rẽ I-B","Trạm y tế","filter"),
("I_B","## Ngã rẽ I-B","## Ngã rẽ II-A","Trạm y tế","filter"),
("C6","## Chương 6.","## Chương 7.","Kho lưu trữ","core"),
("C7","## Chương 7.","<a id=\"diem-re-2\"></a>","Xưởng cộng đồng","rain"),
("II_A","## Ngã rẽ II-A","## Ngã rẽ II-B","Rìa Mưa","rain"),
("II_B","## Ngã rẽ II-B","## Tuyến A —","Rìa Mưa","rain"),
("C8","## Chương 8.","## Chương 9.","Trạm trú bão","storm"),
("C9","## Chương 9.","<a id=\"diem-re-3\"></a>","Phòng tham vấn","core"),
("C9C","<a id=\"tuyen-c-tiep-tuc\"></a>","## Chương 10.","Phiên biểu quyết","filter"),
("C10","## Chương 10.","## Các cảnh phân nhánh","Lưu Quang sau bão","core"),
("END_A","## Tuyến A —","## Tuyến B —","Lưu Quang khẩn cấp","storm"),
("END_B","## Tuyến B —",None,"Lưu Quang khẩn cấp","storm"),
]
episodes=[]
for episode_id,start,end,location,visual in markers:
    b=source.find(start)
    assert b>=0,(episode_id,"start")
    e=source.find(end,b+len(start)) if end else len(source)
    assert e>b,(episode_id,"end")
    part=source[b:e]
    title=re.search(r"^#{2,3} (.+)$",part,re.M)
    assert title,episode_id
    title=title.group(1).strip()
    part=re.sub(r'<a id="[^"]+"></a>','',part)
    part=re.sub(r"\[([^\]]+)\]\(#[^)]+\)",r"\1",part)
    part=re.sub(r"^#{2,3} .*$","",part,flags=re.M)
    part=re.sub(r"\*\*|\*","",part)
    parts=[p.strip() for p in re.split(r"\n\s*\n",part) if p.strip()]
    # Editorial route instructions are not treated as narrative dialogue.
    parts=[p for p in parts if not p.startswith(("Đường trở lại mạch chính:","[Trở về hướng dẫn","Trở về hướng dẫn chọn nhánh")) and p!="---"]
    assert len(parts)>15,(episode_id,len(parts))
    unique=[0,1,len(parts)//3,(2*len(parts))//3,len(parts)-2,len(parts)-1]
    beats=[parts[i][:1050] for i in dict.fromkeys(unique)]
    episodes.append({"id":episode_id,"title":title,"region":location,"visual":visual,
       "paragraphs":parts,"beats":beats})
out={"version":"civi_new_story_2026_10_28","source_url":"https://drive.google.com/file/d/1Q7kzTn73t3rwkCjAXxl2AXTU7s1ZpvQf/view",
"source_sha256":hashlib.sha256(source.encode()).hexdigest(),
"title":"NHỮNG NGƯỜI KHÔNG CÓ CHỖ DƯỚI BẦU TRỜI","episodes":episodes}
(root/"campaign.json").write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding="utf-8")
print("PASS STORY COMPILE",len(episodes),"episodes",sum(len(e["paragraphs"]) for e in episodes),"paragraphs",
      "size",(root/"campaign.json").stat().st_size)
for e in episodes:print(e["id"],len(e["paragraphs"]))
