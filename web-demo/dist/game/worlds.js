/* CIVI 3D · places
   Tầng 42 (the coordination office), Rìa Mưa (the rain district at the dome's edge) and the
   dark room in bà Dung's house. Each builder returns a world: a scene group, colliders,
   ground height, and per-frame updates. */
(function(){
"use strict";
if(!window.G)return;
const G=window.G,T=G.T;
const geoB={};
const BOX=(x,y,z)=>{const k=x+"_"+y+"_"+z;return geoB[k]||(geoB[k]=new T.BoxGeometry(x,y,z));};
function newWorld(name){return {name,group:new T.Group(),boxes:[],circles:[],bounds:null,groundH:()=>0,camDist:1,spots:{}};}
function mk(w,geo,mat,p,r,s,o){
  const m=new T.Mesh(geo,mat);if(p)m.position.set(p[0],p[1],p[2]);if(r)m.rotation.set(r[0],r[1],r[2]);if(s)m.scale.set(s[0],s[1],s[2]);
  m.castShadow=!(o&&o.noShadow);m.receiveShadow=true;(o&&o.parent||w.group).add(m);return m;
}
function block(w,sx,sy,sz,x,y,z,mat,o){
  o=o||{};const m=mk(w,BOX(sx,sy,sz),mat,[x,y+sy/2,z],o.rot?[0,o.rot,0]:null,null,o);
  if(o.col!==false)w.boxes.push({x0:x-sx/2-(o.pad||0),x1:x+sx/2+(o.pad||0),z0:z-sz/2-(o.pad||0),z1:z+sz/2+(o.pad||0),h:y+sy,noCam:o.noCam});
  return m;
}
function sign(w,lines,x,y,z,ry,sw,sh,o){
  o=o||{};const tex=G.textTex(lines,{w:o.tw||512,h:o.th||Math.round(512*sh/sw),bg:o.bg||"#1d3430",fg:o.fg||"#f3ede0",border:o.border,fs:o.fs,font:o.font,weight:o.weight});
  const m=new T.Mesh(new T.PlaneGeometry(sw,sh),new T.MeshBasicMaterial({map:tex,toneMapped:false,side:o.double?T.DoubleSide:T.FrontSide}));
  m.position.set(x,y,z);m.rotation.y=ry||0;w.group.add(m);if(o.dim)m.material.color.setScalar(o.dim);return m;
}
function screenTex(kind){
  const c=document.createElement("canvas");c.width=256;c.height=160;const g=c.getContext("2d");
  g.fillStyle="#0c1b1a";g.fillRect(0,0,256,160);
  g.strokeStyle="#3ff0c0";g.lineWidth=3;g.beginPath();for(let x=0;x<=256;x+=16){const y=110-Math.sin(x*.05+kind)*24-Math.random()*16;x?g.lineTo(x,y):g.moveTo(x,y);}g.stroke();
  g.fillStyle="rgba(63,240,192,.35)";for(let i=0;i<6;i++)g.fillRect(14+i*40,130,26,-10-Math.random()*40);
  g.fillStyle="#9ff8e0";g.font="bold 16px IBM Plex Mono,monospace";g.fillText(kind%2?"MW 48,0":"KHÍ 97%",12,26);
  const t=new T.CanvasTexture(c);t.encoding=T.sRGBEncoding;return t;
}
function plant(w,x,z,s){
  s=s||1;mk(w,new T.CylinderGeometry(.28*s,.22*s,.5*s,14),G.toon(0xb4603c),[x,.25*s,z]);
  const lm=G.toon(0x4f8a4c,{flat:true});
  for(let i=0;i<4;i++)mk(w,new T.IcosahedronGeometry(.36*s,0),lm,[x+(Math.random()-.5)*.35*s,.75*s+i*.22*s,z+(Math.random()-.5)*.35*s],[Math.random(),Math.random(),0],[1,1.25,1]);
  w.circles.push({x,z,r:.32*s});
}

/* ======================= Tầng 42 ======================= */
G.buildOffice=function(){
  const w=newWorld("office");w.bounds=[-18,18,-11,11];
  w.bg=G.displayColor(0xc9dcdd);w.exposure=1.05;
  const g=w.group;
  g.add(new T.HemisphereLight(0xfffaf0,0xb5a893,.95));
  const sun=new T.DirectionalLight(0xffeed6,1.05);sun.position.set(-8,16,-18);sun.castShadow=true;
  sun.shadow.mapSize.set(2048,2048);Object.assign(sun.shadow.camera,{left:-22,right:22,top:16,bottom:-16,near:1,far:60});sun.shadow.bias=-.0005;g.add(sun);g.add(sun.target);
  w.sun=sun;
  // floor with carpet tiles
  const fc=document.createElement("canvas");fc.width=fc.height=128;const fg=fc.getContext("2d");fg.fillStyle="#d6d0c2";fg.fillRect(0,0,128,128);fg.strokeStyle="rgba(120,110,95,.14)";fg.lineWidth=2;fg.strokeRect(1,1,126,126);
  for(let i=0;i<300;i++){fg.fillStyle=`rgba(${110+Math.random()*40|0},${100+Math.random()*30|0},90,.06)`;fg.fillRect(Math.random()*128,Math.random()*128,2,2);}
  const ft=new T.CanvasTexture(fc);ft.wrapS=ft.wrapT=T.RepeatWrapping;ft.repeat.set(18,12);ft.encoding=T.sRGBEncoding;
  mk(w,new T.PlaneGeometry(36,22),G.toon(0xffffff,{map:ft}),[0,0,0],[-Math.PI/2,0,0],null,{noShadow:true});
  const rug=G.toon(0x8fa9a4);
  [[-8,3.6,5,3.6],[0,3.6,5,3.6],[0,-4.6,5.4,5.4]].forEach(([x,z,a,b])=>mk(w,new T.PlaneGeometry(a,b),rug,[x,.01,z],[-Math.PI/2,0,0],null,{noShadow:true}));
  // walls
  const wallM=G.toon(0xefe8da),baseM=G.toon(0x8f8474),trim=G.toon(0x2c6e68);
  block(w,36.6,3.4,.3,0,0,11.15,wallM);block(w,.3,3.4,22.3,-18.15,0,0,wallM);block(w,.3,3.4,22.3,18.15,0,0,wallM);
  block(w,36,.14,.06,0,0,10.98,baseM,{col:false});
  // window wall to the north, with Lưu Quang outside
  block(w,36.6,.7,.35,0,0,-11.15,G.toon(0xe3dccd));
  for(let x=-18;x<=18;x+=3)block(w,.16,2.7,.22,x,.7,-11.15,trim,{col:false});
  block(w,36.6,.3,.3,0,3.25,-11.15,trim,{col:false});
  mk(w,new T.PlaneGeometry(36,2.55),new T.MeshBasicMaterial({color:0xcff3ee,transparent:true,opacity:.12,depthWrite:false}),[0,1.98,-11.1],null,null,{noShadow:true});
  w.boxes.push({x0:-18,x1:18,z0:-11.4,z1:-10.95,h:3.4});
  const loader=new T.TextureLoader();
  const pano=new T.MeshBasicMaterial({color:0xffffff,toneMapped:false,fog:false});
  loader.load("assets/city-window.jpg",t=>{t.encoding=T.sRGBEncoding;pano.map=t;pano.needsUpdate=true;});
  const pm=mk(w,new T.PlaneGeometry(78,26),pano,[0,6,-19],null,null,{noShadow:true});w.pano=pm;
  mk(w,new T.PlaneGeometry(90,40),new T.MeshBasicMaterial({color:0xbcd8de,toneMapped:false,fog:false}),[0,20,-20],null,null,{noShadow:true});
  // signage
  sign(w,["CƠ QUAN ĐIỀU PHỐI MÁI VÒM","TẦNG 42"],-9,2.2,10.98,Math.PI,5,1.2,{bg:"#2c6e68"});
  sign(w,["THANG MÁY"],-15,2.9,10.97,Math.PI,1.6,.4,{bg:"#2a2f33"});
  // elevator
  const metal=G.toon(0xb9c2c4,{rim:.2});
  block(w,1.1,2.6,.12,-15.56,0,10.9,metal,{col:false});block(w,1.1,2.6,.12,-14.44,0,10.9,metal,{col:false});
  block(w,2.6,.2,.2,-15,2.6,10.9,G.toon(0x4a5054),{col:false});
  w.spots.elevator=new T.Vector3(-15,0,9.4);
  // desks: two clusters of four
  const deskTop=G.toon(0xf1ece2),deskLeg=G.toon(0x5a6266),monB=G.toon(0x26292d),chairM=G.toon(0x2a8c84),chairB=G.toon(0x30353a);
  const scr=[0,1,2,3].map(i=>new T.MeshBasicMaterial({map:screenTex(i),toneMapped:false}));
  w.desks={};
  const desk=(id,x,z,face)=>{
    const d=new T.Group();d.position.set(x,0,z);d.rotation.y=face;g.add(d);
    const top=mk(w,BOX(1.6,.06,.8),deskTop,[0,.74,0],null,null,{parent:d});
    [[-.74,-.34],[.74,-.34],[-.74,.34],[.74,.34]].forEach(([a,b])=>mk(w,BOX(.05,.74,.05),deskLeg,[a,.37,b],null,null,{parent:d}));
    mk(w,BOX(.62,.4,.04),monB,[0,1.03,-.25],null,null,{parent:d});
    mk(w,new T.PlaneGeometry(.56,.34),scr[(x*7+z*3|0)&3],[0,1.03,-.228],null,null,{parent:d,noShadow:true});
    mk(w,BOX(.06,.2,.06),monB,[0,.83,-.27],null,null,{parent:d});
    mk(w,BOX(.44,.02,.14),G.toon(0xe0e0dc),[0,.78,.05],null,null,{parent:d});
    const ch=new T.Group();ch.position.set(0,0,.62);d.add(ch);
    mk(w,BOX(.5,.08,.48),chairM,[0,.42,0],null,null,{parent:ch});mk(w,BOX(.5,.55,.07),chairM,[0,.72,.22],null,null,{parent:ch});mk(w,new T.CylinderGeometry(.04,.04,.4,8),chairB,[0,.2,0],null,null,{parent:ch});
    mk(w,new T.CylinderGeometry(.28,.28,.04,10),chairB,[0,.03,0],null,null,{parent:ch});
    w.desks[id]={group:d,chair:ch,seat:new T.Vector3().copy(d.position).add(new T.Vector3(Math.sin(face)*.62,0,Math.cos(face)*.62)),face};
  };
  desk("khoa",-8.85,2.95,Math.PI);desk("hang",-7.15,2.95,Math.PI);desk("a3",-8.85,4.25,0);desk("a4",-7.15,4.25,0);
  desk("b1",-.85,2.95,Math.PI);desk("b2",.85,2.95,Math.PI);desk("linh",-.85,4.25,0);desk("mai",.85,4.25,0);
  w.boxes.push({x0:-9.7,x1:-6.3,z0:2.5,z1:4.7,h:1.2},{x0:-1.7,x1:1.7,z0:2.5,z1:4.7,h:1.2});
  // the coordination holotable
  const holo=new T.Group();holo.position.set(0,0,-4.6);g.add(holo);
  mk(w,new T.CylinderGeometry(1.55,1.7,.9,40),G.toon(0x2a3134,{rim:.2}),[0,.45,0],null,null,{parent:holo});
  mk(w,new T.CylinderGeometry(1.45,1.45,.04,40),new T.MeshBasicMaterial({color:0x1f6f66,toneMapped:false}),[0,.92,0],null,null,{parent:holo,noShadow:true});
  const hr=new T.Mesh(new T.TorusGeometry(1.5,.035,8,64),G.basic(0x3ff0c0));hr.rotation.x=Math.PI/2;hr.position.y=.93;holo.add(hr);
  const city=new T.Group();city.position.y=1.0;holo.add(city);
  const hm=new T.MeshBasicMaterial({color:0x5ff0c8,transparent:true,opacity:.55,blending:T.AdditiveBlending,depthWrite:false,toneMapped:false});
  const n=70,im=new T.InstancedMesh(BOX(1,1,1),hm,n),M=new T.Object3D();
  for(let i=0;i<n;i++){const a=Math.random()*Math.PI*2,r=Math.sqrt(Math.random())*1.2,h=.05+Math.exp(-r*2)*.7*Math.random()+.04;M.position.set(Math.cos(a)*r,h/2,Math.sin(a)*r);M.scale.set(.09,h,.09);M.updateMatrix();im.setMatrixAt(i,M.matrix);}
  city.add(im);
  const dome=new T.Mesh(new T.SphereGeometry(1.35,24,12,0,Math.PI*2,0,Math.PI/2),new T.MeshBasicMaterial({color:0x5ff0c8,wireframe:true,transparent:true,opacity:.18,toneMapped:false}));city.add(dome);
  const beam=new T.Mesh(new T.CylinderGeometry(1.4,.2,1.6,24,1,true),new T.MeshBasicMaterial({color:0x3ff0c0,transparent:true,opacity:.07,blending:T.AdditiveBlending,depthWrite:false,side:T.DoubleSide,toneMapped:false}));beam.position.y=1.7;holo.add(beam);
  w.circles.push({x:0,z:-4.6,r:1.75});w.holo=holo;w.holoCity=city;
  // Vinh's glass office
  const glass=new T.MeshBasicMaterial({color:0xbfe8e2,transparent:true,opacity:.22,depthWrite:false});const frame=G.toon(0x4a5054);
  mk(w,BOX(.06,2.8,6.6),glass,[10.5,1.4,-7.7],null,null,{noShadow:true});block(w,.08,.1,6.6,10.5,2.8,-7.7,frame,{col:false});
  w.boxes.push({x0:10.4,x1:10.6,z0:-11,z1:-4.4,h:2.9,noCam:true});
  mk(w,BOX(1.5,2.8,.06),glass,[11.25,1.4,-4.4],null,null,{noShadow:true});mk(w,BOX(4.4,2.8,.06),glass,[15.8,1.4,-4.4],null,null,{noShadow:true});
  w.boxes.push({x0:10.5,x1:12,z0:-4.5,z1:-4.3,h:2.9,noCam:true},{x0:13.6,x1:18,z0:-4.5,z1:-4.3,h:2.9,noCam:true});
  sign(w,["GĐ. VINH"],15.8,1.9,-4.36,0,1.4,.36,{bg:"#2a2f33"});
  block(w,1.9,.06,.9,15.5,.74,-8.4,G.toon(0x6a4a34));block(w,1.8,.72,.8,15.5,0,-8.4,G.toon(0x5a3d2a),{col:false});
  mk(w,new T.CylinderGeometry(.07,.06,.1,12),G.toon(0xf3efe6),[15.1,.82,-8.3]);
  plant(w,17.3,-10.3,1.1);
  w.spots.vinh=new T.Vector3(15.5,0,-7.3);
  // pantry, with this week's menu
  block(w,4.6,.95,.9,-15.6,0,-9.9,G.toon(0xd9d2c4));block(w,4.7,.06,1,-15.6,.95,-9.9,G.toon(0x7a8a86),{col:false});
  block(w,.5,.55,.45,-16.8,.95,-9.95,G.toon(0x2a2f33),{col:false});mk(w,new T.SphereGeometry(.04,8,6),G.basic(0xff5a4a),[-16.6,1.35,-9.7]);
  sign(w,["THỰC ĐƠN TUẦN NÀY","đậu · đậu sốt cà · chè đậu"],-17.98,1.9,-7.4,Math.PI/2,2.4,1,{bg:"#f3ede0",fg:"#2a3a36",border:"#c9a467"});
  mk(w,new T.CylinderGeometry(.55,.55,.05,24),G.toon(0xf1ece2),[-14.4,.72,-7]);mk(w,new T.CylinderGeometry(.06,.06,.7,8),deskLeg,[-14.4,.36,-7]);w.circles.push({x:-14.4,z:-7,r:.6});
  // whiteboard
  sign(w,["BÃO: CÒN 12 NGÀY","KPI: 0 lỗi / 2.190 ngày","(đừng xóa — Vinh)"],-17.98,1.7,2.2,Math.PI/2,3,1.5,{bg:"#fbfaf6",fg:"#2c6e68",weight:700});
  // servers: the IT corner
  const rack=G.toon(0x22272b,{rim:.15});
  const ledPos=[],ledCol=[];
  [[14.2,8.6],[15.5,8.6],[16.8,8.6]].forEach(([x,z])=>{block(w,1.1,2.2,.9,x,0,z,rack);for(let k=0;k<14;k++){ledPos.push(x-.4+Math.random()*.8,.3+Math.random()*1.8,z-.46);ledCol.push(0,1,.7);}});
  const lg=new T.BufferGeometry();lg.setAttribute("position",new T.Float32BufferAttribute(ledPos,3));lg.setAttribute("color",new T.Float32BufferAttribute(ledCol,3));
  const leds=new T.Points(lg,new T.PointsMaterial({size:.07,vertexColors:true,toneMapped:false}));g.add(leds);
  sign(w,["PHÒNG MÁY · CIVI v2.190"],15.5,2.5,8.12,Math.PI,2.8,.4,{bg:"#2a2f33",fg:"#3ff0c0"});
  // B4 pressure panel on the east wall
  block(w,.3,1.9,3.8,17.85,.3,0,G.toon(0x3a4246,{rim:.2}));
  sign(w,["TRẠM KHÍ B4 · ĐIỀU ÁP"],17.68,2.5,0,-Math.PI/2,2.6,.36,{bg:"#2a2f33",fg:"#f0b050"});
  w.valves=[-1.15,0,1.15].map(z=>{const v=new T.Mesh(new T.TorusGeometry(.26,.05,8,24),G.toon(0xd9343a,{rim:.3}));v.position.set(17.62,1.25,z);v.rotation.y=Math.PI/2;g.add(v);
    const sp=new T.Mesh(new T.CylinderGeometry(.03,.03,.5,6),G.toon(0x9aa2a4));sp.rotation.x=Math.PI/2;sp.rotation.z=Math.PI/2;v.add(sp);return v;});
  w.alarmLight=mk(w,new T.SphereGeometry(.13,12,8),new T.MeshBasicMaterial({color:0x5a2a26,toneMapped:false}),[17.6,2.05,0]);
  w.spots.panel=new T.Vector3(16.6,0,0);
  // plants and props
  plant(w,-17,9.8);plant(w,-11.5,-10.2,1.2);plant(w,8,-10.3,1.1);plant(w,-17.2,5,1);plant(w,6.5,10.2,.9);
  const leaf=leds.geometry.attributes.color;
  w.update=(dt,t)=>{
    city.rotation.y+=dt*.15;hm.opacity=.45+Math.sin(t*2)*.1;
    if(Math.random()<.3){const i=(Math.random()*leaf.count)|0;const on=Math.random()<.8;leaf.setXYZ(i,on?0:1,on?1:.4,on?.7:.3);leaf.needsUpdate=true;}
    if(w.alarm){const on=Math.sin(t*10)>0;w.alarmLight.material.color.setHex(on?0xff3a2a:0x5a2a26);}
  };
  w.camDist=.95;
  return w;
};

/* ======================= Rìa Mưa ======================= */
G.buildRiaMua=function(){
  const w=newWorld("riamua");w.bounds=[-28,28,-28,26];w.wet=true;
  w.bg=G.displayColor(0x141d29);w.fog=new T.FogExp2(0,.024);w.fog.color=G.displayColor(0x18222e);w.exposure=1.04;
  const g=w.group;
  g.add(new T.HemisphereLight(0x6680aa,0x141218,.58));
  const moon=new T.DirectionalLight(0xaec2e6,.55);moon.position.set(-12,24,10);moon.castShadow=true;moon.shadow.mapSize.set(2048,2048);
  Object.assign(moon.shadow.camera,{left:-32,right:32,top:32,bottom:-32,near:1,far:80});moon.shadow.bias=-.0006;g.add(moon);g.add(moon.target);
  // sky dome with a glow where Lõi Sáng is (north)
  const sky=new T.Mesh(new T.SphereGeometry(300,32,16),new T.ShaderMaterial({side:T.BackSide,depthWrite:false,fog:false,
    uniforms:{a:{value:new T.Color(0x0b1524)},b:{value:new T.Color(0x34304a)},c:{value:new T.Color(0x6a5a5a)},glow:{value:new T.Color(0x3fd0b0)}},
    vertexShader:"varying vec3 vP;void main(){vP=normalize(position);gl_Position=projectionMatrix*modelViewMatrix*vec4(position,1.);}",
    fragmentShader:"uniform vec3 a,b,c,glow;varying vec3 vP;void main(){float h=vP.y;vec3 col=mix(b,a,smoothstep(.02,.6,h));col=mix(col,c,smoothstep(.15,-.05,h)*.6);float n=max(-vP.z,0.);col+=glow*pow(n,6.)*smoothstep(.35,.0,h)*.35;gl_FragColor=vec4(col,1.);\n#include <encodings_fragment>\n}"}));
  g.add(sky);
  // the dome's ribs high above, and the bright core far to the north
  const rib=G.basic(0x8aa6a8,{transparent:true,opacity:.35,fog:false});
  for(let i=0;i<7;i++){const a=-.9+i*.3,pts=[];for(let j=0;j<=30;j++){const t=j/30*Math.PI;pts.push(new T.Vector3(Math.cos(t)*150*Math.sin(a+Math.PI/2)*.9,Math.sin(t)*95-6,-60+Math.cos(t)*150*Math.cos(a+Math.PI/2)));}
    g.add(new T.Mesh(new T.TubeGeometry(new T.CatmullRomCurve3(pts),60,.35,5,false),rib));}
  // Lõi Sáng: a bright skyline far to the north, hazed toward the sky colour
  const winC=document.createElement("canvas");winC.width=64;winC.height=128;const wg=winC.getContext("2d");
  const tg=wg.createLinearGradient(0,0,0,128);tg.addColorStop(0,"#2f5552");tg.addColorStop(1,"#1d3533");wg.fillStyle=tg;wg.fillRect(0,0,64,128);
  for(let y=3;y<128;y+=6)for(let x=3;x<64;x+=8)if(Math.random()<.55){wg.fillStyle=Math.random()<.3?"#ffd8a6":"#c9fbee";wg.fillRect(x,y,4,3);}
  const wt=new T.CanvasTexture(winC);wt.encoding=T.sRGBEncoding;wt.wrapS=wt.wrapT=T.RepeatWrapping;wt.repeat.set(3,5);
  const tw=new T.MeshBasicMaterial({map:wt,toneMapped:false,fog:false,color:0xb8d4cf});
  [[-34,-160,58,8],[-10,-172,76,10],[16,-158,52,7],[42,-176,64,9],[-60,-168,42,7],[4,-148,38,6],[66,-160,36,6]].forEach(([x,z,h,r])=>{
    const m=new T.Mesh(new T.CylinderGeometry(r*.6,r*.78,h,18),tw);m.position.set(x,h/2-4,z);g.add(m);
    const ring=new T.Mesh(new T.TorusGeometry(r*1.3,r*.1,8,40),G.basic(0x5ff6d0,{fog:false}));ring.rotation.x=Math.PI/2;ring.position.set(x,h*.72-4,z);g.add(ring);});
  const coreGlow=new T.Sprite(new T.SpriteMaterial({map:G.fx.dot,color:0x3fd0b0,transparent:true,opacity:.35,blending:T.AdditiveBlending,depthWrite:false,fog:false}));
  coreGlow.scale.set(260,120,1);coreGlow.position.set(0,30,-190);g.add(coreGlow);
  // ground
  const asphalt=G.toon(0x22272b),street=G.toon(0x2e353a),curb=G.toon(0x56605f);
  mk(w,new T.PlaneGeometry(60,60),asphalt,[0,0,-1],[-Math.PI/2,0,0],null,{noShadow:true});
  const R=(x0,x1,z0,z1)=>({x0,x1,z0,z1});
  const walk=w.walk=[R(-3.2,3.2,-24,25),R(-20,20,3.5,8.5),R(-20,-13.5,8.5,15),R(-12,12,-8.6,-3.6),R(-11,11,-21,-8.6),R(-25,-11,-18.5,-13.5),R(-27,-19,-26,-12),R(11,22,-18.5,-13.5),R(14.5,22,-22.5,-18.5),R(-7,10,17,25)];
  walk.forEach(r=>mk(w,new T.PlaneGeometry(r.x1-r.x0,r.z1-r.z0),street,[(r.x0+r.x1)/2,.012,(r.z0+r.z1)/2],[-Math.PI/2,0,0],null,{noShadow:true}));
  const inR=(r,x,z,m)=>x>r.x0-(m||0)&&x<r.x1+(m||0)&&z>r.z0-(m||0)&&z<r.z1+(m||0);
  const isWalk=(x,z,m)=>walk.some(r=>inR(r,x,z,m));
  const CANAL={z0:-8.6,z1:-5.6},BRIDGE={x0:5,x1:7.6},FLOOD={x0:-13.4,x1:-9.4,z0:3.5,z1:8.5};
  w.CANAL=CANAL;w.BRIDGE=BRIDGE;w.FLOOD=FLOOD;
  // water: canal and the flooded steps of ống 13
  const waterMat=new T.ShaderMaterial({transparent:true,fog:true,uniforms:Object.assign({t:{value:0},col:{value:new T.Color(0x1c4752)}},T.UniformsLib.fog),
    vertexShader:"varying vec3 vW;\n#include <fog_pars_vertex>\nvoid main(){vec4 wp=modelMatrix*vec4(position,1.);vW=wp.xyz;vec4 mvPosition=viewMatrix*wp;gl_Position=projectionMatrix*mvPosition;\n#include <fog_vertex>\n}",
    fragmentShader:"uniform float t;uniform vec3 col;varying vec3 vW;\n#include <fog_pars_fragment>\nvoid main(){float r=sin(vW.x*1.7+t*1.3)*sin(vW.z*2.1-t*1.1)+sin((vW.x+vW.z)*3.3+t*2.)*.5;vec3 c=col+vec3(.12,.2,.22)*smoothstep(.9,1.4,r)+vec3(.03,.05,.06)*r;gl_FragColor=vec4(c,.93);\n#include <encodings_fragment>\n#include <fog_fragment>\n}"});
  w.waterMat=waterMat;
  mk(w,new T.PlaneGeometry(60,CANAL.z1-CANAL.z0),waterMat,[0,.06,(CANAL.z0+CANAL.z1)/2],[-Math.PI/2,0,0],null,{noShadow:true});
  w.boxes.push({x0:-30,x1:BRIDGE.x0,z0:CANAL.z0,z1:CANAL.z1,h:.1},{x0:BRIDGE.x1,x1:30,z0:CANAL.z0,z1:CANAL.z1,h:.1});
  const floodM=mk(w,new T.PlaneGeometry(FLOOD.x1-FLOOD.x0,FLOOD.z1-FLOOD.z0),waterMat,[(FLOOD.x0+FLOOD.x1)/2,.3,(FLOOD.z0+FLOOD.z1)/2],[-Math.PI/2,0,0],null,{noShadow:true});
  // canal walls and the plastic bridge
  block(w,60,.25,.25,0,0,CANAL.z1+.12,curb,{col:false});block(w,60,.25,.25,0,0,CANAL.z0-.12,curb,{col:false});
  const plastic=G.toon(0xd5ddd8);
  block(w,BRIDGE.x1-BRIDGE.x0,.12,3.8,(BRIDGE.x0+BRIDGE.x1)/2,.18,(CANAL.z0+CANAL.z1)/2,plastic,{col:false});
  [BRIDGE.x0+.05,BRIDGE.x1-.05].forEach(x=>{block(w,.08,.8,3.8,x,.3,(CANAL.z0+CANAL.z1)/2,G.toon(0x7a8a8a),{col:false});w.boxes.push({x0:x-.05,x1:x+.05,z0:CANAL.z0-.3,z1:CANAL.z1+.3,h:1.1,noCam:true});});
  // the main bridge is broken
  block(w,6.4,1,.3,0,0,CANAL.z1+.6,G.toon(0xd9a441),{noCam:true});
  sign(w,["CẦU HỎNG · CHỜ SỬA","(từ năm ấy)"],0,1.5,CANAL.z1+.78,0,2.4,.8,{bg:"#d9a441",fg:"#2a2420"});
  // stepping stones over the flood (An's way)
  const stones=w.stones=[[-9.9,7.7],[-11.3,7.9],[-12.6,7.6],[-13.9,7.9]];
  const stoneM=G.toon(0x8a8a82,{rim:.15});stones.forEach(([x,z])=>mk(w,new T.CylinderGeometry(.48,.55,.42,12),stoneM,[x,.15,z]));
  // stairs going down under the flood water
  for(let i=0;i<4;i++)mk(w,BOX(.6,.14,5),G.toon(0x4a5054),[-10-i*.8,.12-i*.1,6],null,null,{noShadow:true});
  // buildings: fill the blocks between streets
  const reserved=[R(-9.6,-5,8.5,10.8),R(15,22,-27,-22.4),R(-20,-14.5,15,19.5),R(-27,-19,-26,-12),R(-8,8,-21,-9),R(4,9,17,21.5),R(7,11,8.5,11)];
  const palette=[0x8fb3a6,0xc9a467,0xc98a78,0x7f95a3,0xd9cfb8,0x9aa58a,0xb89aa8];
  const houses=[];
  for(let x=-26;x<=26;x+=4.3)for(let z=-26;z<=24;z+=4.3){
    const hx=x+(Math.random()-.5)*.4,hz=z+(Math.random()-.5)*.4,sx=3.4+Math.random()*.6,sz=3.4+Math.random()*.6;
    // a house fronts a street: its walls stay a step clear of the street, and it is never more than a block away
    const half=Math.max(sx,sz)/2,near=isWalk(hx,hz,half+4.4);
    if(isWalk(hx,hz,half+.3)||(hz>CANAL.z0-2.2&&hz<CANAL.z1+2.2)||reserved.some(r=>inR(r,hx,hz,half+.2))||!near)continue;
    if(Math.abs(hx)>27||hz<-27||hz>25)continue;
    houses.push({x:hx,z:hz,sx,sz,h:3+Math.random()*3.6+(Math.random()<.25?2.2:0),c:palette[(Math.random()*palette.length)|0]});
  }
  const wallMat=G.toon(0xffffff),roofMat=G.toon(0x5a5f62),M=new T.Object3D(),col=new T.Color();
  const him=new T.InstancedMesh(BOX(1,1,1),wallMat,houses.length),rim=new T.InstancedMesh(BOX(1,1,1),roofMat,houses.length);
  houses.forEach((h,i)=>{M.position.set(h.x,h.h/2,h.z);M.rotation.set(0,0,0);M.scale.set(h.sx,h.h,h.sz);M.updateMatrix();him.setMatrixAt(i,M.matrix);col.setHex(h.c).multiplyScalar(.85+Math.random()*.2);him.setColorAt(i,col);
    M.position.set(h.x,h.h+.12,h.z);M.rotation.set((Math.random()-.5)*.12,0,(Math.random()-.5)*.12);M.scale.set(h.sx+.3,.22,h.sz+.3);M.updateMatrix();rim.setMatrixAt(i,M.matrix);col.setHex(Math.random()<.5?0x6a4a3a:0x55606a);rim.setColorAt(i,col);
    w.boxes.push({x0:h.x-h.sx/2,x1:h.x+h.sx/2,z0:h.z-h.sz/2,z1:h.z+h.sz/2,h:h.h});});
  him.castShadow=him.receiveShadow=rim.castShadow=true;g.add(him);g.add(rim);
  // lit and dark windows facing the streets
  const wins=[];
  houses.forEach(h=>{const faces=[[0,1],[0,-1],[1,0],[-1,0]];
    faces.forEach(([fx,fz])=>{const px=h.x+fx*(h.sx/2+.3),pz=h.z+fz*(h.sz/2+.3);if(!isWalk(px,pz,2.6))return;
      const floors=Math.max(1,Math.floor(h.h/1.9)),along=fx?h.sz:h.sx;
      for(let f=0;f<floors;f++)for(let k=-1;k<=1;k+=2){if(Math.random()<.25)continue;
        wins.push({x:h.x+fx*(h.sx/2+.02)+(fz?k*along*.22:0),z:h.z+fz*(h.sz/2+.02)+(fx?k*along*.22:0),y:1.2+f*1.9,ry:fx?(fx>0?Math.PI/2:-Math.PI/2):(fz>0?0:Math.PI),lit:Math.random()<.62});}});});
  const winMat=new T.MeshBasicMaterial({color:0xffffff,toneMapped:false}),wim=new T.InstancedMesh(new T.PlaneGeometry(.7,.85),winMat,wins.length);
  wins.forEach((v,i)=>{M.position.set(v.x,v.y,v.z);M.rotation.set(0,v.ry,0);M.scale.set(1,1,1);M.updateMatrix();wim.setMatrixAt(i,M.matrix);col.setHex(v.lit?(Math.random()<.8?0xffb45a:0xbff0e6):0x1c2228);if(v.lit)col.multiplyScalar(.7+Math.random()*.5);wim.setColorAt(i,col);});
  g.add(wim);w.windows=wins;
  // laundry lines and wires across the streets
  const lineM=new T.LineBasicMaterial({color:0x1a1c20}),wire=[];
  for(let i=0;i<26;i++){const r=walk[(Math.random()*walk.length)|0],x=r.x0+Math.random()*(r.x1-r.x0),z=r.z0+Math.random()*(r.z1-r.z0),a=Math.random()<.5,L=6;
    wire.push(new T.Vector3(x-(a?L:0),4.2+Math.random()*1.5,z-(a?0:L)),new T.Vector3(x+(a?L:0),4.2+Math.random()*1.5,z+(a?0:L)));}
  g.add(new T.LineSegments(new T.BufferGeometry().setFromPoints(wire),lineM));
  const cloth=[0xd94a3a,0xf0c75a,0x6a9ad0,0xf3ede0,0x7ab08a,0xd08ab0];
  for(let i=0;i<10;i++){const r=walk[i%9],x=r.x0+1+Math.random()*(r.x1-r.x0-2),z=Math.min(r.z0+1+Math.random()*(r.z1-r.z0-2),13);
    for(let k=0;k<4;k++)mk(w,new T.PlaneGeometry(.38,.55),G.toon(cloth[(Math.random()*cloth.length)|0],{side:T.DoubleSide}),[x+k*.7-1,3.4,z],[0,Math.random()*.3,0],null,{noShadow:true});}
  // lamps: warm points of light
  const lampAt=(x,z,y,int)=>{const l=new T.PointLight(0xffb060,(int||1.5)*1.6,14,2);l.position.set(x,y||2.6,z);g.add(l);mk(w,new T.SphereGeometry(.12,10,8),G.basic(0xffd8a0),[x,y||2.6,z],null,null,{noShadow:true});return l;};
  lampAt(-7.2,9.8,2.5);lampAt(0,-14,3,1.8);lampAt(18.5,-21.7,2.4);lampAt(3.6,23.8,3.4);lampAt(9,9.6,2.6,1.1);lampAt(-22,-15,3,1.3);
  // gate
  const gateM=G.toon(0x3e4a4e,{rim:.15});block(w,.8,4.6,.8,-4.2,0,24.6,gateM);block(w,.8,4.6,.8,4.2,0,24.6,gateM);block(w,9.2,.7,.9,0,4.6,24.6,gateM,{col:false});
  sign(w,["RÌA MƯA"],0,4.95,24.12,Math.PI,3.6,.62,{bg:"#1d3430",fg:"#f0b050"});
  sign(w,["CỔNG KIỂM SOÁT · VÀNH ĐỆM"],0,4.95,25.08,0,4.2,.5,{bg:"#1d3430"});
  w.spots.gate=new T.Vector3(0,0,22.6);
  // An's umbrella stall at the corner of ống 13
  const crate=G.toon(0x8a6a44);block(w,2.4,.95,.9,-7.4,0,9.9,crate);
  sign(w,["VÁ Ô · SỬA NAN","trả bằng pin cũ cũng được"],-7.4,2.25,10.42,Math.PI,2.6,.7,{bg:"#f3ede0",fg:"#7a3a2a",weight:700});
  [[-8.3,0x2f6f9a],[-7.4,0xb5473a],[-6.5,0x5c7a5a]].forEach(([x,c],i)=>{const u=mk(w,new T.ConeGeometry(.48,.3,8,1,true),G.toon(c,{side:T.DoubleSide}),[x,2.05+(i%2)*.12,10.15]);u.rotation.set(Math.PI+.35,0,(i-1)*.15);
    mk(w,new T.CylinderGeometry(.012,.012,.5,5),G.toon(0x3a3330),[x,1.8+(i%2)*.12,10.1]);});
  w.spots.stall=new T.Vector3(-7.4,0,8.9);
  // the tailor's, Tín and a bicycle without its front wheel
  sign(w,["TIỆM MAY"],9.2,2.6,10.1,Math.PI,1.8,.5,{bg:"#7a5a8a"});
  const bike=new T.Group();bike.position.set(10.6,0,8.8);g.add(bike);
  mk(w,new T.TorusGeometry(.33,.04,8,20),G.toon(0x222222),[.45,.34,0],null,null,{parent:bike});mk(w,BOX(.9,.05,.05),G.toon(0xd04a3a),[0,.55,0],[0,0,.3],null,{parent:bike});
  w.circles.push({x:10.6,z:8.8,r:.5});
  // the market under joined tarps
  const tarpC=[0x9c4a3c,0x2f6f73,0xb08a3a,0x5a6f4a,0x7a5a7a,0x8a6a4a];
  [[-4,-12],[0,-12.5],[4,-12],[-4,-16],[0,-16.5],[4,-16]].forEach(([x,z],i)=>{
    mk(w,BOX(3.6,.06,3.4),G.toon(tarpC[i],{side:T.DoubleSide}),[x,2.9+(i%2)*.15,z],[(Math.random()-.5)*.12,0,(Math.random()-.5)*.12]);
    [[-1.7,-1.6],[1.7,-1.6],[-1.7,1.6],[1.7,1.6]].forEach(([a,b])=>mk(w,new T.CylinderGeometry(.05,.05,2.9,6),G.toon(0x3a3a36),[x+a,1.45,z+b]));});
  [[-4,-11.2],[4,-11.2],[-4,-17.3],[4,-17.3]].forEach(([x,z])=>block(w,2.4,.9,.9,x,0,z,crate));
  const pot=mk(w,new T.CylinderGeometry(.42,.38,.5,16),G.toon(0x5a5f62,{rim:.2}),[-4.6,.9,-11.25]);
  w.spots.market=new T.Vector3(-3,0,-13.4);w.potPos=new T.Vector3(-4.6,1.25,-11.25);
  // trạm lọc 19
  const st=G.toon(0x8f9a95,{rim:.25});
  mk(w,new T.CylinderGeometry(2.4,2.6,5.4,28),st,[-23.5,2.7,-20.5]);mk(w,new T.SphereGeometry(2.4,24,10,0,Math.PI*2,0,Math.PI/2),st,[-23.5,5.4,-20.5]);
  const band=new T.Mesh(new T.TorusGeometry(2.62,.12,8,40),G.basic(0xf0b050));band.rotation.x=Math.PI/2;band.position.set(-23.5,3.5,-20.5);g.add(band);
  w.circles.push({x:-23.5,z:-20.5,r:2.7});
  [[-20.2,-19],[-21,-23.6]].forEach(([x,z])=>mk(w,new T.CylinderGeometry(.22,.22,4,10),G.toon(0x8a4a32),[x,1.2,z],[0,0,Math.PI/2]));
  sign(w,["TRẠM LỌC SỐ 19"],-20.8,3.6,-17.9,0,3,.6,{bg:"#2a3a36",fg:"#f0b050"});
  w.spots.station=new T.Vector3(-21.2,0,-15.6);
  // the house in Linh's photo (Phân khu 8-12)
  const hc=G.toon(0x7d8f88);block(w,6,3.4,4.2,18.5,0,-24.6,hc);block(w,6.6,.3,4.8,18.5,3.4,-24.6,G.toon(0x6a4a3a),{col:false});
  mk(w,BOX(1,2,.05),G.toon(0x2f5a5a),[18.5,1,-22.47]);
  mk(w,BOX(1.2,.9,.05),new T.MeshBasicMaterial({color:0xffc070,toneMapped:false}),[16.6,1.7,-22.47]);
  mk(w,BOX(.16,.06,.2),G.toon(0x7ab0d0),[16.45,1.27,-22.4]);mk(w,BOX(.16,.06,.2),G.toon(0x7ab0d0),[16.7,1.27,-22.4]);
  mk(w,new T.CylinderGeometry(.13,.1,.2,10),G.toon(0xb4603c),[17,1.32,-22.38]);mk(w,new T.IcosahedronGeometry(.16,0),G.toon(0x4f8a4c,{flat:true}),[17,1.52,-22.38]);
  mk(w,new T.CylinderGeometry(.015,.015,3.2,4),G.toon(0x3a3a36),[20.3,2.3,-22.3],[0,0,Math.PI/2]);
  mk(w,new T.PlaneGeometry(.6,.75),G.toon(0xf3ede0,{side:T.DoubleSide}),[19.7,1.85,-22.3],null,null,{noShadow:true});mk(w,new T.PlaneGeometry(.45,.55),G.toon(0x6a9ad0,{side:T.DoubleSide}),[20.6,1.95,-22.3],null,null,{noShadow:true});
  const umb=new T.Group();umb.position.set(19.4,2.5,-22.2);g.add(umb);
  const uc=new T.ConeGeometry(.75,.34,8,1,true).toNonIndexed(),cc=new Float32Array(uc.attributes.position.count*3),pal=[0x2f6f9a,0x2f6f9a,0xb5473a,0x2f6f9a,0xd9a441,0x2f6f9a,0x2f6f9a,0xb5473a];
  for(let i=0;i<uc.attributes.position.count;i++){col.setHex(pal[Math.floor(i/6)%8]);cc[i*3]=col.r;cc[i*3+1]=col.g;cc[i*3+2]=col.b;}
  uc.setAttribute("color",new T.BufferAttribute(cc,3));
  w.umbrella=mk(w,uc,new T.MeshToonMaterial({vertexColors:true,gradientMap:G.grad,side:T.DoubleSide}),[0,0,0],null,null,{parent:umb});w.umbrellaG=umb;
  sign(w,["PHÂN KHU 8-12"],15.2,2.4,-22.45,0,1.8,.42,{bg:"#2a3a36"});
  w.spots.anHouse=new T.Vector3(18.5,0,-21);
  // bà Dung's house at the end of the ống 13 alley: its windows stay dark
  block(w,5.2,3,4.2,-17,0,17.3,G.toon(0x6f7570));block(w,5.8,.3,4.8,-17,3,17.3,G.toon(0x55606a),{col:false});
  mk(w,BOX(1,2,.05),G.toon(0x3a3330),[-17,1,15.17]);mk(w,BOX(1,.8,.05),G.toon(0x14181c),[-15.6,1.7,15.17]);
  w.spots.dung=new T.Vector3(-17,0,14.2);
  // mobile audit station near the gate
  block(w,3.2,2.4,2,6.5,0,19.6,G.toon(0x2c6e68,{rim:.2}));
  const kscr=mk(w,new T.PlaneGeometry(1.6,.9),new T.MeshBasicMaterial({map:screenTex(5),toneMapped:false}),[6.5,1.5,18.58],[0,Math.PI,0],null,{noShadow:true});
  sign(w,["TRẠM KIỂM TOÁN LƯU ĐỘNG"],6.5,2.7,18.58,Math.PI,3,.42,{bg:"#1d3430",fg:"#3ff0c0"});
  w.spots.audit=new T.Vector3(6.5,0,17.6);
  // puddles
  const pud=G.toon(0x1c2a32,{emissive:0x0e1a22,ei:.6});
  for(let i=0;i<22;i++){const r=walk[(Math.random()*walk.length)|0];const x=r.x0+.8+Math.random()*(r.x1-r.x0-1.6),z=r.z0+.8+Math.random()*(r.z1-r.z0-1.6);
    mk(w,new T.CircleGeometry(.5+Math.random()*.8,18),pud,[x,.02,z],[-Math.PI/2,0,0],[1,.7+Math.random()*.6,1],{noShadow:true});}
  // rain around the player
  const N=G.mobile?650:1500,rp=new Float32Array(N*6);
  for(let i=0;i<N;i++){const x=(Math.random()-.5)*36,y=Math.random()*14,z=(Math.random()-.5)*36;rp.set([x,y,z,x-.05,y-.45,z],i*6);}
  const rg=new T.BufferGeometry();rg.setAttribute("position",new T.BufferAttribute(rp,3));
  const rain=new T.LineSegments(rg,new T.LineBasicMaterial({color:0xa9c8e0,transparent:true,opacity:.38,depthWrite:false}));rain.frustumCulled=false;g.add(rain);
  // ground height: bridge, stepping stones, and the deep water
  const inFlood=(x,z)=>x>FLOOD.x0&&x<FLOOD.x1&&z>FLOOD.z0&&z<FLOOD.z1;
  const onStone=(x,z)=>stones.some(([sx,sz])=>Math.hypot(x-sx,z-sz)<.55);
  w.groundH=(x,z)=>{
    if(x>BRIDGE.x0&&x<BRIDGE.x1&&z>CANAL.z0-.3&&z<CANAL.z1+.3)return .25;
    if(onStone(x,z))return .36;
    if(inFlood(x,z))return -.28;
    return 0;
  };
  w.inFlood=(x,z)=>inFlood(x,z)&&!onStone(x,z);
  w.speedMul=(x,z)=>w.inFlood(x,z)?.38:1;
  w.onPlayer=(P,dt)=>{
    if(w.inFlood(P.pos.x,P.pos.z)&&P.ground){
      P.pos.x+=dt*(w.floodPush==null?1.3:w.floodPush);
      if(Math.random()<.12)G.fx.splash(P.pos);
      if(!w._wetT||G.time-w._wetT>2.5){w._wetT=G.time;G.sfx("splash");if(w.onWet)w.onWet();}
    }
  };
  let rt=0;
  w.update=(dt,t)=>{
    waterMat.uniforms.t.value=t;
    const P=G.player.pos,a=rg.attributes.position.array;
    for(let i=0;i<N;i++){const o=i*6;a[o+1]-=dt*15;a[o+4]-=dt*15;a[o]-=dt*.6;a[o+3]-=dt*.6;
      if(a[o+4]<0){const x=P.x+(Math.random()-.5)*36,z=P.z+(Math.random()-.5)*36,y=12+Math.random()*3;a[o]=x;a[o+1]=y;a[o+2]=z;a[o+3]=x-.05;a[o+4]=y-.45;a[o+5]=z;}}
    rg.attributes.position.needsUpdate=true;
    rt-=dt;if(rt<0&&!G.mobile){rt=.06;const r=walk[(Math.random()*walk.length)|0];G.fx.emit({x:r.x0+Math.random()*(r.x1-r.x0),y:.05,z:r.z0+Math.random()*(r.z1-r.z0)},0x9fc8e0,2,{speed:.8,up:.8,life:.25});}
    if(Math.random()<.25)G.fx.emit(w.potPos,0xeeeeee,1,{speed:.1,up:.6,life:1.4,spread:.4});
  };
  w.camDist=1;
  return w;
};

/* ======================= bà Dung's dark room ======================= */
G.buildRoom=function(){
  const w=newWorld("room");w.bounds=[-3.6,3.6,-2.9,2.9];w.bg=G.displayColor(0x050707);w.exposure=1.25;
  // a dollhouse cutaway: the front wall is hidden (its collider stays) and the camera looks in from the front
  const _cp=new T.Vector3(),_cl=new T.Vector3();
  w.camRig=P=>{_cl.set(P.pos.x*.55,.75,P.pos.z*.45-.7);_cp.set(P.pos.x*.35,3.7,5.2+P.pos.z*.15);
    if(G.portrait())_cp.sub(_cl).multiplyScalar(1.7).add(_cl);return {pos:_cp,look:_cl};};
  const g=w.group;
  g.add(new T.HemisphereLight(0x4a5e72,0x0e0e12,.42));
  const wl=new T.PointLight(0x4a6a8a,.7,7,2);wl.position.set(1,2,-2.4);g.add(wl);
  const wall=G.toon(0x59605b),floor=G.toon(0x2b302d),wood=G.toon(0x5a4632);
  mk(w,new T.PlaneGeometry(7.4,6),floor,[0,0,0],[-Math.PI/2,0,0],null,{noShadow:true});
  // a wet patch where she slipped
  mk(w,new T.CircleGeometry(.7,20),G.toon(0x1d2a2c,{emissive:0x0c1618,ei:.5}),[-1.5,.01,-.7],[-Math.PI/2,0,0],[1.4,1,1],{noShadow:true});
  block(w,7.4,2.8,.2,0,0,-3,wall);block(w,.2,2.8,6,-3.7,0,0,wall);block(w,.2,2.8,6,3.7,0,0,wall);
  [block(w,2.8,2.8,.2,-2.3,0,3,wall),block(w,2.8,2.8,.2,2.3,0,3,wall),block(w,1.8,2.8,.12,0,0,3.02,G.toon(0x3a3330))].forEach(m=>m.visible=false);
  // the cut edge of the front wall, drawn as a low sill so the room reads as a cutaway
  block(w,7.4,.16,.2,0,0,3,G.toon(0x3e4441),{col:false});
  mk(w,new T.PlaneGeometry(1,.9),new T.MeshBasicMaterial({color:0x2c4852,toneMapped:false}),[1,1.75,-2.88]);
  // the pipe across the back wall: the leak on the left, its code plate on the right
  mk(w,new T.CylinderGeometry(.12,.12,7.2,12),G.toon(0x8a4a32,{rim:.2}),[0,2.35,-2.55],[0,0,Math.PI/2]);
  mk(w,new T.TorusGeometry(.14,.03,6,14),G.toon(0x6a3a28),[-1.6,2.35,-2.55],[0,Math.PI/2,0]);
  w.leak=new T.Vector3(-1.6,2.3,-2.5);
  sign(w,["13-C-0412"],2.4,2.12,-2.42,0,.6,.18,{bg:"#bcb59e",fg:"#2b2418",weight:700});
  w.plate=new T.Vector3(2.4,2.12,-2.4);
  // a crate for An to stand on under the leak
  block(w,.55,.4,.55,-1.6,0,-2.1,wood);
  // bed, the medicine shelf on the left wall, bottles catching drips, the heater
  block(w,2.2,.55,1.1,-2.4,0,-1.6,G.toon(0x5a5650));mk(w,BOX(.7,.15,.5),G.toon(0x8a857a),[-3,.62,-1.6]);
  block(w,.36,.06,1.3,-3.42,1.4,1.0,wood,{col:false});
  w.medicine=mk(w,BOX(.16,.22,.2),G.toon(0xc9a24a,{emissive:0x3a2a0a,ei:.4}),[-3.4,1.57,1.1]).position.clone();
  mk(w,BOX(.12,.16,.12),G.toon(0x7a8a9a),[-3.42,1.54,.7]);
  const bm=new T.MeshToonMaterial({color:0x9cc3c8,gradientMap:G.grad,transparent:true,opacity:.55});
  for(let i=0;i<11;i++)mk(w,new T.CylinderGeometry(.07,.08,.28,8),bm,[-.7+i*.165,.14,-2.5+(i%2)*.14]);
  w.bottles=new T.Vector3(.15,.3,-2.45);
  block(w,.8,.9,.5,2.6,0,-.6,G.toon(0x454b48));w.heater=new T.Vector3(2.6,.6,-.6);
  mk(w,new T.TorusGeometry(.16,.03,6,16),G.toon(0xb07a4a),[2.6,.62,-.34]);
  w.spots.door=new T.Vector3(0,0,2.3);w.spots.dung=new T.Vector3(-1.9,0,-.45);w.spots.an=new T.Vector3(-1.6,.4,-2.1);
  // drips from the leak
  w.update=(dt,t)=>{if(Math.random()<.1)G.fx.emit(w.leak,0x86bcc6,1,{speed:.05,up:-.5,life:.7,spread:.05,spreadY:0});};
  return w;
};
})();
