"""Apply the narrowly scoped movement hooks to the isolated chapter work copy."""
from pathlib import Path
root=Path(__file__).resolve().parents[1]/'artifacts/chapter1-polish-2026-11-01/work/Game'
base=root/'Assets/_Civi/Scripts/Application/Exploration'
p=base/'CiviSchoolWorldV2.cs'
s=p.read_text(encoding='utf-8-sig')
s=s.replace('public bool ChapterMode;', 'public bool ChapterMode;\n        public CiviChapterEnvironment ChapterEnvironment;')
s=s.replace('SpawnCanvas("Ground / seamless modular tile layout",Ground,-1200);','if(ChapterEnvironment!=null)ChapterEnvironment.Build();\n            if(ChapterEnvironment==null){\n            SpawnCanvas("Ground / seamless modular tile layout",Ground,-1200);')
s=s.replace('SpawnCanvas("Foreground / perimeter and gate",Foreground,800);','SpawnCanvas("Foreground / perimeter and gate",Foreground,800);\n            }')
s=s.replace('playerTransform=obj.transform;', 'playerTransform=obj.transform;\n            if(ChapterEnvironment!=null){playerTransform.position=ChapterEnvironment.Spawn;WalkFrames=ChapterEnvironment.Frames;}')
s=s.replace('visual.transform.localPosition=new Vector3(0,1.35f,0);','visual.transform.localPosition=ChapterEnvironment!=null?Vector3.zero:new Vector3(0,1.35f,0);\n            if(ChapterEnvironment!=null)visual.AddComponent<CiviChapterDepth>();')
s=s.replace('foreach(var hotspot in map.hotspots)','if(ChapterEnvironment!=null){follow.MinZoom=7.5f;follow.MaxZoom=8.5f;follow.Configure(playerTransform,ChapterEnvironment.Bounds,8f);}\n            foreach(var hotspot in ChapterEnvironment!=null?Array.Empty<CiviSchoolHotspot>():map.hotspots)')
s=s.replace('if(CiviSchoolV2Bridge.Returning)','if(ChapterEnvironment==null&&CiviSchoolV2Bridge.Returning)')
s=s.replace('// Feet are radius .22 world units; do not let the avatar cut corners.','if(ChapterEnvironment!=null)return ChapterEnvironment.CanStand(p);\n            // Feet are radius .22 world units; do not let the avatar cut corners.')
s=s.replace('1+((int)(Time.time*7f)%3)','((int)(Time.time*8f)%4)')
p.write_text(s,encoding='utf-8')
p=base/'CiviSchoolStudioV2.cs';s=p.read_text(encoding='utf-8-sig')
s=s.replace('public bool ChapterMode;', 'public bool ChapterMode;\n        public CiviChapterEnvironment ChapterEnvironment;')
s=s.replace('if(map==null)return false;', 'if(ChapterEnvironment!=null)return ChapterEnvironment.CanStand(v);\n            if(map==null)return false;')
s=s.replace('Render(Ground,"Parquet floor / modular", -1000);','if(ChapterEnvironment!=null)ChapterEnvironment.Build();\n            if(ChapterEnvironment==null){\n            Render(Ground,"Parquet floor / modular", -1000);')
s=s.replace('Render(Props,"Desks / computers / research notes", 120);','Render(Props,"Desks / computers / research notes", 120);\n            }')
s=s.replace('actor=go.transform;actor.position=Cell(map.spawn.x,map.spawn.y);','actor=go.transform;actor.position=ChapterEnvironment!=null?(Vector3)ChapterEnvironment.Spawn:Cell(map.spawn.x,map.spawn.y);\n            if(ChapterEnvironment!=null)StudentFrames=ChapterEnvironment.Frames;')
s=s.replace('art.transform.localPosition=new Vector3(0,1.35f,0);','art.transform.localPosition=ChapterEnvironment!=null?Vector3.zero:new Vector3(0,1.35f,0);\n            if(ChapterEnvironment!=null)art.AddComponent<CiviChapterDepth>();')
s=s.replace('cameraFollow.Configure(actor,new Rect(-16,-11,32,22),7.65f);','cameraFollow.Configure(actor,new Rect(-16,-11,32,22),7.65f);\n            if(ChapterEnvironment!=null){cameraFollow.MinZoom=7.5f;cameraFollow.MaxZoom=8.4f;cameraFollow.Configure(actor,ChapterEnvironment.Bounds,8.4f);}')
s=s.replace('1+((int)(timer*7f)%3)','((int)(timer*8f)%4)')
p.write_text(s,encoding='utf-8')
