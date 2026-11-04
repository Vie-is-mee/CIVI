from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance
import json, hashlib, math

root=Path(__file__).resolve().parent
assets=root/"Assets/_Civi/Art/Environments/NewStory"
out=assets/"Production/ArtPreview"
out.mkdir(parents=True,exist_ok=True)
mapping=[
    ("BG_RM_NORMAL", "ENV_RM_ALLEY"),
    ("BG_RM_STORM", "ENV_RM_ALLEY_STORM"),
    ("BG_FILTER19", "ENV_RM_FILTER19"),
    ("BG_LOISANG", "ENV_LQ_PLAZA")
]
sizes=[]
W,H=1672,941
for name,ident in mapping:
    src=assets/"Concepts"/(name+"_canva_preview.png")
    if not src.exists():raise FileNotFoundError(src)
    im=Image.open(src).convert("RGB")
    original=im.size
    im=im.resize((W,H),Image.Resampling.LANCZOS)
    im=ImageEnhance.Sharpness(im).enhance(1.14)
    # Keep the original color grading, no invented object details.
    target=out/(ident+"_v02.png")
    im.save(target,optimize=True)
    alpha=Image.new("L",(W,H),0)
    d=ImageDraw.Draw(alpha)
    # Duplicate portions of the same scene to occlude the character without
    # misaligning sprite art. Masks are intentionally editable and reversible.
    def poly(xy):
        d.polygon([(round(x*W),round(y*H)) for x,y in xy],fill=255)
    if "RM_ALLEY" in ident:
        poly([(0,.59),(.08,.61),(.19,.71),(.33,.73),(.44,.90),(.51,1),(0,1)])
        poly([(.89,.57),(1,.58),(1,1),(.93,1),(.89,.72)])
    elif "FILTER19" in ident:
        poly([(0,.71),(.09,.66),(.18,.83),(.29,.91),(.36,1),(0,1)])
        poly([(.84,.75),(1,.67),(1,1),(.90,1)])
    else:
        poly([(0,.83),(.17,.90),(.26,1),(0,1)])
        poly([(.87,.77),(1,.67),(1,1),(.94,1)])
    overlay=Image.new("RGBA",(W,H),(0,0,0,0))
    overlay.paste(im.convert("RGBA"),(0,0),alpha)
    overlay.save(out/(ident+"_foreground_v02.png"),optimize=True)
    sizes.append(dict(id=ident,from_thumbnail=original,asset_resolution=[W,H],actual_source_resolution=original,
        filename=target.name,foreground=ident+"_foreground_v02.png",quality="upscaled_canva_preview_not_original",sha256=hashlib.sha256(target.read_bytes()).hexdigest()))

sprites=out/"Characters";sprites.mkdir(exist_ok=True)
scale=3
sw,sh=144,216
for direction in ["front","back","side"]:
    S=(sw*scale,sh*scale)
    im=Image.new("RGBA",S,(0,0,0,0))
    a=ImageDraw.Draw(im)
    def box(b,fill,outline=None,width=2):
        a.rounded_rectangle(tuple(int(v*scale) for v in b),radius=round(10*scale),fill=fill,
            outline=outline,width=round(width*scale) if outline else 1)
    def ellipse(b,fill,outline=None,width=2):
        a.ellipse(tuple(int(v*scale) for v in b),fill=fill,outline=outline,
            width=round(width*scale) if outline else 1)
    # Muted outfit is a temporary non-canon visual for Mai.
    ellipse((23,191,124,212),(20,28,32,75))
    box((39,134,62,198),(40,49,54,255),(19,23,28,255),2)
    box((80,134,103,198),(37,49,55,255),(19,23,28,255),2)
    box((34,192,64,207),(20,25,31,255))
    box((78,192,109,207),(18,24,28,255))
    box((36,71,106,156),(35,87,86,255),(18,38,43,255),3)
    box((30,78,47,136),(46,72,74,255))
    box((95,78,112,136),(49,73,75,255))
    box((50,86,92,117),(52,111,107,255))
    box((56,132,84,142),(175,135,74,255))
    box((64,60,80,81),(190,142,105,255))
    if direction=="back":
        ellipse((41,12,104,79),(30,33,38,255),(20,23,27,255),3)
        box((46,84,99,130),(35,59,64,255))
        box((53,84,67,135),(182,125,71,255))
    else:
        ellipse((43,17,105,83),(218,162,118,255),(22,29,32,255),2)
        ellipse((39,11,105,44),(30,32,38,255))
        if direction=="side":
            ellipse((87,53,93,58),(17,31,33,255))
            box((42,86,51,133),(165,113,70,255))
        else:
            ellipse((56,56,62,60),(19,31,33,255))
            ellipse((87,56,93,60),(19,31,33,255))
            a.arc((52*scale,52*scale,94*scale,72*scale),0,150,fill=(118,69,58,255),width=2*scale)
        a.polygon([(38*scale,31*scale),(56*scale,8*scale),(99*scale,13*scale),(111*scale,36*scale)],
             fill=(34,36,41,255))
    im=im.resize((sw,sh),Image.Resampling.LANCZOS)
    im.save(sprites/("MAI_placeholder_"+direction+".png"),optimize=True)

manifest={"note":"preview source 595x335 from Canva; cannot claim native 1672px fidelity","sources":sizes,
"actor":"temporary illustrated placeholder, not confirmed character design",
"status":"playable art prototype only"}
(out/"art_preview_manifest.json").write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding="utf-8")
print("EXPORT OK",len(sizes),"background/foreground pairs + 3 avatar sprites")
for z in sizes:print(z["filename"],(out/z["filename"]).stat().st_size)
