from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
import os, json, random

rng=random.Random(1707)
root=Path(__file__).resolve().parents[2]
game=root if (root/"Assets").exists() else root/"Game"
tiles=root/"source-assets/kenney-rpg-urban-pack/Tiles"
if not tiles.exists():
    tiles=Path(os.path.expandvars(r"%LOCALAPPDATA%\Temp\CIVI_SCHOOL_MODULAR_V1\kenney-city\assets\kenney\rpg-urban-pack\Tiles"))
out=game/"Assets/_Civi/Art/SchoolWorld"
out.mkdir(parents=True,exist_ok=True)
W,H,S=152,60,16
cache={}
def get(id):
    if id not in cache:cache[id]=Image.open(tiles/f"tile_{id:04d}.png").convert("RGBA")
    return cache[id]
layers=[Image.new("RGBA",(W*S,H*S)) for _ in range(4)]
ground,building,props,overhang=layers
nav=[[False]*H for _ in range(W)]
def place(canvas,id,x,y):
    if 0<=x<W and 0<=y<H:canvas.alpha_composite(get(id),(x*S,(H-1-y)*S))
def fill(xa,ya,xb,yb,ids,walk=True):
    for x in range(max(0,xa),min(W,xb+1)):
        for y in range(max(0,ya),min(H,yb+1)):
            place(ground,ids[(x*3+y*7+(x//4))%len(ids)],x,y)
            if walk:nav[x][y]=True
def block(xa,ya,xb,yb):
    for x in range(max(0,xa),min(W,xb+1)):
        for y in range(max(0,ya),min(H,yb+1)):nav[x][y]=False
# Natural lawn as baseline, single continuous geography
for x in range(W):
    for y in range(H):
        place(ground,32 if rng.random()<.91 else 5,x,y)
        if 3<x<149 and 3<y<39:nav[x][y]=True
fill(3,17,149,29,[109,114,113,109])
fill(29,9,71,38,[109,114,113,109])
fill(8,4,15,29,[109,114,113])
fill(74,27,149,39,[108,109,113,114])
for l,r in [(80,107),(111,135),(139,150)]:fill(l,41,r,57,[108,109,113,114])
# uninterrupted school hallway with door cutaways
for x in range(73,151):
    if x in range(91,96) or x in range(118,122) or x in range(142,146):
        fill(x,40,x,40,[109])
    else:place(building,139,x,40);block(x,40,x,40)
for x in (79,107,110,135,138,151):
    for y in range(41,59):
        place(building,116 if x%2 else 118,x,y)
        block(x,y,x,y)
for x in range(78,151):
    place(building,139,x,58);block(x,58,x,58)
# School windows, white-blue trim, door frames, school signs
for x in [83,87,92,97,101,114,119,124,129,132,141,146]:
    place(props,335 if x%2 else 390,x,56)
for x in [91,95,118,121,142,145]:place(props,385,x,40)
for x in [88,94,100,118,124,130,143,147]:
    place(props,329,x,55)
# campus perimeter: gap for front gate
for x in range(3,149):
    if x not in range(10,15):
        place(overhang,218,x,3);block(x,3,x,3)
for y in range(4,59):
    place(overhang,216,3,y);place(overhang,216,149,y)
    block(3,y,3,y);block(149,y,149,y)
for x in (9,15):
    place(props,216,x,4);place(props,216,x,5)
for x in range(9,16):place(overhang,218,x,6)
# bike parking
fill(16,9,29,15,[13,14,36,13])
# Bike parking receives hand-drawn bicycle silhouettes rather than cars.
for x in range(17,29,3):
    for y in (11,14):
        block(x,y,x+1,y)
# court foliage / planters; physically collidable
for x,y,id in [(34,33,289),(68,32,286),(37,13,286),(66,12,289),
               (24,35,260),(13,34,286),(73,16,289),(28,39,289),
               (60,39,286),(109,27,286),(129,27,260)]:
    # Trees are drawn 3 tiles tall to give the school campus useful visual scale.
    big=get(id).resize((48,48),Image.Resampling.NEAREST)
    props.alpha_composite(big,((x-1)*S,(H-y-2)*S))
    block(x-1,y-1,x+1,y+1)
for x in range(32,70,6):
    for y in [14,35]:
        place(props,232 if x%2 else 238,x,y);block(x,y,x,y)
for x,y in [(32,24),(48,10),(63,25),(70,33),(35,18),(22,28),(53,36)]:
    place(props,381,x,y);block(x,y,x+1,y)
# flag pole on center plaza
for y in range(21,28):place(props,194,54,y)
place(props,166,54,28);block(54,21,54,22)
# small classroom desks, no wall between door and back half of room
for xs in [[84,90,98,103],[114,120,127,132]]:
    for x in xs:
        for y in [46,50,54]:
            place(props,381,x,y);place(props,302,x+1,y)
            block(x,y,x+1,y)
# book shelves as library furniture
for x in [141,146,148]:
    for y in [47,51,55]:
        place(props,303,x,y);block(x,y,x,y)
# benches, information panels
for x,y in [(7,19),(22,19),(73,24),(83,31),(106,32),(133,32),(140,34)]:
    place(props,381,x,y);block(x,y,x+1,y)
for x,y in [(6,27),(19,31),(29,26),(77,30),(102,28),(128,37),(145,26)]:
    place(props,167,x,y);block(x,y,x,y)
# Custom CC0-compatible pixel accents, reusable with the source tilemap.
ink=ImageDraw.Draw(props)
for xx in range(17,29,3):
    for yy in (11,14):
        cx=(xx+.5)*S;cy=(H-yy-.5)*S
        color=(32,58,68,255) if xx%2 else (173,64,58,255)
        # Two wheels and triangular bicycle frame. No raster automobile sprites.
        ink.ellipse((cx-5,cy-3,cx+3,cy+5),outline=(24,46,55,255),width=2)
        ink.ellipse((cx+11,cy-3,cx+19,cy+5),outline=(24,46,55,255),width=2)
        ink.line([(cx-1,cy+1),(cx+7,cy-4),(cx+15,cy+1),(cx-1,cy+1)],fill=color,width=2)
        ink.line([(cx+7,cy-4),(cx+5,cy-7),(cx+2,cy-7)],fill=color,width=2)
        ink.line([(cx+15,cy+1),(cx+17,cy-6),(cx+21,cy-6)],fill=color,width=2)

# School sign, intentionally authored after world kit construction (not AI text).
board_x,board_y=17*S,(H-33)*S
ink.rounded_rectangle((board_x,board_y,board_x+201,board_y+35),radius=2,
    fill=(221,197,126,255),outline=(35,75,76,255),width=3)
try:font=ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf",17)
except OSError:font=ImageFont.load_default()
ink.text((board_x+23,board_y+7),"TRƯỜNG HỌC",font=font,fill=(31,73,72,255))
ink.line((board_x+10,board_y+7,board_x+10,board_y+27),fill=(152,77,56,255),width=3)
block(17,32,28,33)

# Actual Vietnamese national flag near the central assembly area.
flag_x,flag_y=54*S,(H-29)*S
ink.line((flag_x+9,flag_y+3,flag_x+9,flag_y+80),fill=(59,60,62,255),width=3)
ink.polygon([(flag_x+11,flag_y+5),(flag_x+47,flag_y+9),
             (flag_x+46,flag_y+30),(flag_x+11,flag_y+26)],
            fill=(192,55,48,255))
ink.polygon([(flag_x+29,flag_y+12),(flag_x+32,flag_y+18),
             (flag_x+39,flag_y+18),(flag_x+34,flag_y+22),
             (flag_x+36,flag_y+29),(flag_x+29,flag_y+25),
             (flag_x+22,flag_y+29),(flag_x+24,flag_y+22),
             (flag_x+19,flag_y+18),(flag_x+26,flag_y+18)],
            fill=(245,210,85,255))

# Interior noticeboards along the cutaway classroom/club building.
for xx,title in ((83,"LỚP HỌC"),(115,"CÂU LẠC BỘ"),(140,"THƯ VIỆN")):
    x=xx*S;y=(H-58)*S
    ink.rectangle((x,y,x+144,y+19),fill=(37,83,82,255),outline=(207,182,115,255),width=2)
    ink.text((x+9,y+3),title,font=ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf",12),
             fill=(246,238,203,255))

# export exact modular layers; not cinematic photograph collage
for im,name in zip(layers,["school_ground.png","school_architecture.png","school_props.png","school_foreground.png"]):
    im.save(out/name,optimize=True)
for name,id in [("front",24),("back",25),("left",23),("right",26)]:
    get(id).save(out/f"an_{name}.png")
landmarks=[
    ("GATE","Cổng trường",12,11,"Bắt đầu chuyến đi qua khuôn viên."),
    ("COURT","Sân trường",49,27,"Không gian sinh hoạt và chụp ảnh kỷ yếu."),
    ("HALL","Hành lang",82,33,"Lối đi liền mạch giữa sân và lớp học."),
    ("CLASS","Lớp học",92,45,"Không gian học tập của An và các bạn."),
    ("CLUB","Phòng thiết kế",119,45,"Nơi xem xét những tư liệu và bản ảnh."),
    ("LIBRARY","Thư viện",143,45,"Khu đọc sách và tài liệu của trường.")
]
payload={"width":W,"height":H,"ppu":16,
"navigationRows":["".join("1" if nav[x][y] else "0" for x in range(W)) for y in range(H)],
"hotspots":[{"id":n,"title":title,"x":x,"y":y,"note":note} for n,title,x,y,note in landmarks],
"spawn":{"x":12,"y":11},"worldVersion":"school_world_modular_v1"}
(out/"school_world.json").write_text(json.dumps(payload,ensure_ascii=False,indent=2),encoding="utf-8")
(out/"LICENSE.txt").write_text("Kenney RPG Urban Pack (2019); CC0 1.0. https://kenney.nl/assets/rpg-urban-pack\nTiles modified/rearranged into a modular school level. Attribution optional.\n",encoding="utf-8")
assert all(nav[x][y] for x,y in [(12,11),(49,27),(82,33),(92,45),(119,45),(143,45)]),"hotspot inaccessible"
print("SCHOOL_MAP_OK",W,H,"walkable",sum(sum(int(v) for v in row) for row in payload["navigationRows"]))
for path in out.iterdir():print(path.name,path.stat().st_size)
