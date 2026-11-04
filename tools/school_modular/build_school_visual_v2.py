from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
import random,json,math
rng=random.Random(23)
root=Path(__file__).resolve().parents[2]
src=root/"Assets/_Civi/Art/SchoolWorld"
out=root/"Assets/_Civi/Art/SchoolVisualV2"
out.mkdir(parents=True,exist_ok=True)
data=json.loads((src/"school_world.json").read_text(encoding="utf8"))
W,H,T=152,60,16
imgs=[Image.new("RGBA",(W*T,H*T))for _ in range(4)]
floor,walls,props,fg=imgs
def pt(x,y):return (round(x*T),round((H-y)*T))
def rect(im,x1,y1,x2,y2,c,outline=None,w=1):
    ImageDraw.Draw(im).rectangle([pt(min(x1,x2),max(y1,y2)),pt(max(x1,x2),min(y1,y2))],fill=c,outline=outline,width=w)
def oval(im,x1,y1,x2,y2,c,outline=None,w=1):
    ImageDraw.Draw(im).ellipse([pt(x1,y2),pt(x2,y1)],fill=c,outline=outline,width=w)
def ln(im,xy,color,w=2):
    ImageDraw.Draw(im).line([pt(*p)for p in xy],fill=color,width=w)
def fnt(s):
    try:return ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf",s)
    except: return ImageFont.load_default()
rect(floor,0,0,W,H,"#82B68B")
for i in range(16000):
    x=rng.randrange(W*T);y=rng.randrange(H*T)
    ImageDraw.Draw(floor).rectangle((x,y,x+1,y+1),fill=rng.choice(["#92C69C","#75AA84","#8FBC90"]))
for a,b,c,d in [(2,17,151,29),(28,8,72,38),(8,3,16,30),(73,26,151,40)]:
    rect(floor,a,b,c,d,"#D4CBBA")
    rect(floor,a+.25,b+.25,c-.25,d-.25,"#E7DECD")
    for x in range(a+4,c,5):ln(floor,[(x,b+.3),(x,d-.3)],"#D6CBB7",1)
    for y in range(b+4,d,5):ln(floor,[(a+.3,y),(c-.3,y)],"#D6CBB7",1)
for a,b,c,d in [(38,14,46,18),(61,14,68,18),(35,33,40,36),(63,33,68,36)]:
    rect(floor,a,b,c,d,"#C9D7B2",w=2)
    rect(floor,a+.4,b+.4,c-.4,d-.4,"#81B48B")
    for i in range(20):
        x=rng.uniform(a+.5,c-.7);y=rng.uniform(b+.5,d-.7)
        oval(floor,x,y,x+.23,y+.18,rng.choice(["#EDD58C","#F0A8BB","#C8E0A4"]))
rect(floor,42,19,64,31,"#E9DFD0",outline="#FCF5E8",w=3)
oval(floor,50,23,57,28,None,outline="#FCF5E8",w=2)
ln(floor,[(53.5,19),(53.5,31)],"#FCF5E8",2)
rect(floor,16,8,29,15,"#8CABB0")
for x in range(17,29,3):ln(floor,[(x,9),(x,14)],"#D7ECE6",2)
for a,c in [(80,107),(111,135),(139,151)]:
    rect(floor,a,40,c,58,"#DCC6A9")
    for y in range(41,58,2):
        rect(floor,a+.3,y,c-.3,y+.8,"#EAD4B9" if y%4==1 else "#D8BDA0")
rect(floor,74,27,151,40,"#E7E0D2")
for x in range(75,151,5):ln(floor,[(x,27),(x,40)],"#D1CEBF",1)
rect(walls,78,40,151,59,"#F7E9D8",outline="#8FA9A6",w=3)
rect(walls,78,56,151,59,"#6EADB0")
rect(walls,78,57.2,151,59,"#90C8BF")
rect(walls,78,55.4,151,56,"#4C888C")
for x in (79,107,110,135,138,150):
    rect(walls,x,40,x+.8,55,"#CFD4C8",outline="#88A29E",w=2)
for x in [83,88,96,102,114,123,129,142,147]:
    rect(walls,x,50.5,x+3,54.5,"#7EA8AC",outline="#F9F5E9",w=3)
    rect(walls,x+.2,51,x+2.8,54.2,"#BAE5DF")
    ln(walls,[(x+1.5,51),(x+1.5,54.2)],"#F9F5E9",2)
    rect(walls,x-.2,50.4,x+3.2,50.65,"#B7A98E")
for x in [91,118,142]:
    rect(walls,x,39.8,x+4,41,"#B4D8CA")
    rect(walls,x,40,x+.25,44,"#688F87")
    rect(walls,x+3.75,40,x+4,44,"#688F87")
    rect(fg,x,43.8,x+4,44.1,"#51878D")
for x,title in [(83,"LỚP HỌC"),(114,"CÂU LẠC BỘ"),(140,"THƯ VIỆN")]:
    rect(props,x,56,x+min(13,151-x),58,"#377A79",outline="#F0D6A2",w=3)
    ix,iy=pt(x+.5,57.6)
    ImageDraw.Draw(props).text((ix,iy),title,font=fnt(17),fill="#FFF8DD")
def desk(x,y):
    oval(props,x-.2,y-.6,x+2.2,y+.4,"#B8B9A4")
    rect(props,x,y,x+2,y+.8,"#B4845E",outline="#6F6657",w=2)
    rect(props,x+.2,y+.52,x+1.8,y+.72,"#EACAA0")
    rect(props,x+.2,y-.65,x+.5,y,"#73685D")
    rect(props,x+1.65,y-.65,x+1.95,y,"#73685D")
    rect(props,x+.7,y+.6,x+1.3,y+.75,"#FEF7E7")
for xs in [[84,90,98,103],[114,120,127,132]]:
    for x in xs:
        for y in (46,50,54):desk(x,y)
for x in (140,144,148):
    rect(props,x,48,x+2.5,55,"#956B54",outline="#634F43",w=2)
    for dx in range(5):
        rect(props,x+.15+dx*.45,49.5,x+.42+dx*.45,54,
             rng.choice(["#D68E93","#75A3A3","#EAC58C","#9BB8A5"]))
    ln(props,[(x,52),(x+2.5,52)],"#D7BE91",2)
def tree(x,y,s):
    oval(props,x-s*.64,y-s*.58,x+s*.6,y-s*.1,"#5C9676")
    rect(props,x-.25,y-.3,x+.25,y+1.8,"#997B60")
    for dx,dy,k in [(-.7,1.9,1),(.6,1.9,.9),(.1,2.7,1.2),(-.2,1.2,.86)]:
        oval(props,x+dx-k*s*.45,y+dy-k*s*.35,x+dx+k*s*.45,y+dy+k*s*.35,
             "#619D7B",outline="#4D816D",w=2)
        oval(props,x+dx-k*s*.35,y+dy,x+dx+k*s*.15,y+dy+k*s*.26,"#A2CDA0")
for x,y,s in [(34,33,4.6),(68,32,4.9),(37,13,3.7),(66,12,4.4),(24,35,4.0),
              (13,34,4.0),(73,16,3.8),(28,39,4.2),(60,39,4.2),
              (109,27,4.0),(129,27,4.0)]:tree(x,y,s)
for x in range(32,70,6):
    for y in (14,35):
        rect(props,x-.4,y-.3,x+.8,y+.3,"#C48570",outline="#94665A")
        for i in range(6):
            dx=rng.uniform(-.3,.7);dy=rng.uniform(.3,1)
            oval(props,x+dx,y+dy,x+dx+.20,y+dy+.25,rng.choice(["#EF9EB0","#F6DCA1"]))
for x,y in [(32,24),(48,10),(63,25),(70,33),(35,18),(22,28),(53,36),
            (7,19),(22,19),(73,24),(83,31),(106,32),(133,32),(140,34)]:
    rect(props,x,y-.3,x+2,y+.35,"#AB8060",outline="#776456",w=2)
    rect(props,x+.2,y-.2,x+.4,y-.7,"#736C61")
    rect(props,x+1.7,y-.2,x+1.9,y-.7,"#736C61")
for x in range(17,29,3):
    for y in (11,14):
        for dx in [0,1.4]:oval(props,x+dx,y-.1,x+dx+.7,y+.6,None,outline="#455863",w=2)
        ln(props,[(x+.35,y+.25),(x+.95,y+.9),(x+1.75,y+.25),(x+.35,y+.25)],"#C37783",2)
# gate & flag
rect(props,8.5,3.7,9.5,8,"#C1D1C7",outline="#557D83",w=3)
rect(props,15,3.7,16,8,"#C1D1C7",outline="#557D83",w=3)
rect(fg,8.7,7.2,15.8,8.2,"#71A8A4",outline="#417780",w=2)
rect(fg,9.1,7.4,15.4,8.0,"#FCF0D8")
ImageDraw.Draw(fg).text(pt(10,7.98),"CIVI",font=fnt(26),fill="#487E85")
ln(props,[(54,20),(54,32)],"#52686B",3)
ImageDraw.Draw(props).polygon([pt(*a)for a in [(54,32),(60,31),(59,28),(54,29)]],fill="#D95B55")
# save 4 layered backgrounds
for n,im in zip(["school_ground","school_architecture","school_props","school_foreground"],imgs):
    im.save(out/(n+".png"),optimize=True)
# 4 directions * 4 frames: original coherent 32x48 pixel-art student
def char(direction,frame):
    im=Image.new("RGBA",(32,48));d=ImageDraw.Draw(im)
    bob=int(frame in (1,3));step={0:0,1:2,2:0,3:-2}[frame]
    navy="#365E81";sky="#709AC0";outline="#3E5262";skin="#EBC29F";hair="#403C48"
    d.ellipse((6,44,28,47),fill=(25,50,56,75))
    for x,off in [(10,step),(21,-step)]:
        d.rounded_rectangle((x+off-3,33-bob,x+off+3,43-bob),radius=2,
                            fill="#F6E4CC",outline=outline)
        d.rounded_rectangle((x+off-5,41-bob,x+off+5,46-bob),radius=2,fill="#495766")
    d.rounded_rectangle((7,20-bob,26,36-bob),radius=4,fill=sky,outline=navy,width=2)
    d.polygon([(12,21-bob),(21,21-bob),(18,31-bob),(15,31-bob)],fill="#FCF7E9")
    d.line((16,23-bob,16,33-bob),fill=navy,width=2)
    for x,dx in [(6,step),(27,-step)]:
        d.rounded_rectangle((x-2+dx,22-bob,x+2+dx,33-bob),radius=2,
                            fill=sky,outline=navy)
        d.ellipse((x-2+dx,32-bob,x+2+dx,36-bob),fill=skin)
    d.rounded_rectangle((8,5-bob,26,24-bob),radius=6,fill=skin,outline="#D8A181",width=1)
    if direction=="back":
        d.rounded_rectangle((8,3-bob,26,22-bob),radius=5,fill=hair)
        d.rounded_rectangle((12,23-bob,23,35-bob),radius=3,fill="#CBA276",outline="#9D825F")
    else:
        d.rounded_rectangle((8,3-bob,26,10-bob),radius=4,fill=hair)
        d.polygon([(8,9-bob),(10,16-bob),(14,8-bob)],fill=hair)
        if direction=="front":
            for x in (14,22):d.rectangle((x,16-bob,x+1,17-bob),fill=outline)
            d.line((16,20-bob,20,20-bob),fill="#BB8779")
        else:
            d.rectangle(((11 if direction=="left" else 23),16-bob,
                         (12 if direction=="left" else 24),17-bob),fill=outline)
    return im
for direction in ("front","back","left","right"):
    for frame in range(4):char(direction,frame).save(out/f"student_{direction}_{frame}.png",optimize=True)
# Studio (separate Unity scene): matching floor, wall and desks, 32x22 tiles.
RW,RH=32,22
room=[Image.new("RGBA",(RW*T,RH*T))for _ in range(3)]
rg,rw,rp=room
def roomrect(im,a,b,c,d,color,outline=None,w=1):
    ImageDraw.Draw(im).rectangle((a*T,(RH-d)*T,c*T,(RH-b)*T),
                                 fill=color,outline=outline,width=w)
roomrect(rg,0,0,RW,RH,"#42666A")
roomrect(rg,1,1,RW-1,RH-1,"#E7D5B9")
for x in range(3,30,4):roomrect(rg,x,1,x+.15,RH-1,"#D8C2A4")
roomrect(rw,1,RH-4,RW-1,RH-1,"#F1E7D7")
roomrect(rw,1,RH-1,RW-1,RH,"#6BA8A8")
for x in (3,10,17,24):roomrect(rw,x,RH-3,x+5,RH-1,"#B5E4DA",outline="#F8F5E7",w=3)
rows=[list("0"*RW) for _ in range(RH)]
for y in range(1,RH-1):
    for x in range(1,RW-1):rows[y][x]="1"
for x,y in [(5,12),(12,12),(20,12),(5,6),(12,6),(20,6)]:
    roomrect(rp,x,y,x+4,y+2,"#B4815F",outline="#6B5A50",w=3)
    roomrect(rp,x+.5,y+1,x+3.5,y+1.8,"#F0D2A8")
    roomrect(rp,x+1,y+1.2,x+3,y+2.7,"#537D85",outline="#364E56",w=2)
    roomrect(rp,x+1.2,y+1.4,x+2.8,y+2.4,"#B6E4E1")
    for yy in range(y,y+3):
        for xx in range(x,x+5):rows[yy][xx]="0"
roomrect(rp,26,6,30,16,"#A2785B",outline="#584D48",w=3)
for y in range(8,16,2):
    for x in range(27,30):
        roomrect(rp,x,y,x+1,y+1,rng.choice(["#DA9A9F","#9CC0BE","#EAD293"]))
        rows[y][x]="0"
roomrect(rg,14,0,19,3,"#B5D8C7")
for y in range(0,4):
    for x in range(14,19):rows[y][x]="1"
for n,im in zip(["studio_ground","studio_architecture","studio_props"],room):
    im.save(out/(n+".png"),optimize=True)
(out/"studio_world.json").write_text(json.dumps({"width":RW,"height":RH,
    "navigationRows":["".join(row)for row in rows],"spawn":{"x":16,"y":4},
    "exit":{"x":16,"y":2}},ensure_ascii=False,indent=2),encoding="utf8")
data["worldVersion"]="school_visual_v2"
(out/"school_world.json").write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding="utf8")
(out/"ART_SOURCE.md").write_text(
    "Original pixel-art school environment and student walk-cycle generated by build_school_visual_v2.py.\n"
    "Four layered 2432x960 canvases; 4-direction 4-frame student sprite; separate studio interior.\n"
    "No generative raster collage or unverified external asset is embedded.\n",encoding="utf8")
print("ART_V2_COMPLETE",len(list(out.glob("*.png"))),"PNG")
