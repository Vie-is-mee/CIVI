/* CIVI 3D · characters
   Chibi people built from simple shapes with cel shading and outlines, anime faces drawn on
   canvas (several expressions, blinking, talking), procedural animation (walk, run, jump,
   idle breathing, gestures, look-at, hair springs). Also CIVI the drone and the cat Bé Điện. */
(function(){
"use strict";
if(!window.G)return;
const G=window.G,T=G.T,damp=G.damp,clamp=G.clamp;
const lerp=(a,b,t)=>a+(b-a)*t;
const geo={};
const capG=(r,l)=>geo["c"+r+"_"+l]||(geo["c"+r+"_"+l]=new T.CapsuleGeometry(r,l,5,14));
const sphG=(r,w,h)=>geo["s"+r+"_"+(w||22)]||(geo["s"+r+"_"+(w||22)]=new T.SphereGeometry(r,w||22,h||16));
const boxG=(x,y,z)=>{const k="b"+x+"_"+y+"_"+z;return geo[k]||(geo[k]=new T.BoxGeometry(x,y,z));};
const cylG=(a,b,h,s)=>{const k="y"+a+"_"+b+"_"+h+"_"+(s||18);return geo[k]||(geo[k]=new T.CylinderGeometry(a,b,h,s||18));};
const torG=(r,t,s)=>{const k="t"+r+"_"+t+"_"+(s||1);return geo[k]||(geo[k]=new T.TorusGeometry(r,t,8,28,Math.PI*2*(s||1)));};
function part(g,m,parent,p,r,s,ol){
  const o=new T.Mesh(g,m);if(p)o.position.set(p[0],p[1],p[2]);if(r){o.rotation.set(r[0],r[1],r[2]);if(r[3])o.rotation.order=r[3];}
  if(s)o.scale.set(s[0],s[1],s[2]);o.castShadow=true;parent.add(o);if(ol)G.outline(o,typeof ol==="number"?ol:.016);return o;
}
const hex=c=>"#"+new T.Color(c).getHexString();
/* anime hair: strand lines, and a highlight ring on the crown */
const hairCache={};
function hairTex(c,crown){
  const key=c+"_"+crown;if(hairCache[key])return hairCache[key];
  const cv=document.createElement("canvas");cv.width=256;cv.height=128;const g=cv.getContext("2d"),base=hex(c);
  g.fillStyle=base;g.fillRect(0,0,256,128);
  const gr=g.createLinearGradient(0,0,0,128);gr.addColorStop(0,"rgba(255,255,255,.05)");gr.addColorStop(1,"rgba(0,0,0,.28)");g.fillStyle=gr;g.fillRect(0,0,256,128);
  for(let i=0;i<70;i++){const x=Math.random()*256,w=1+Math.random()*2.2;g.strokeStyle=i%3?shade(c,-.35):shade(c,.22);g.globalAlpha=i%3?.45:.3;g.lineWidth=w;
    g.beginPath();g.moveTo(x,0);g.quadraticCurveTo(x+(Math.random()-.5)*10,64,x+(Math.random()-.5)*14,128);g.stroke();}
  g.globalAlpha=1;
  if(crown){const y0=70,y1=86;g.fillStyle=shade(c,.5);g.globalAlpha=.75;g.beginPath();g.moveTo(0,y0);
    for(let x=0;x<=256;x+=8)g.lineTo(x,y0+Math.sin(x*.21)*2.5);for(let x=256;x>=0;x-=8)g.lineTo(x,y1-(x/8%2?7:0));g.closePath();g.fill();g.globalAlpha=1;}
  const t=new T.CanvasTexture(cv);t.encoding=T.sRGBEncoding;t.anisotropy=4;t.wrapS=T.RepeatWrapping;return hairCache[key]=t;
}
function shade(h,k){const c=new T.Color(h);const hsl={};c.getHSL(hsl);c.setHSL(hsl.h,hsl.s,clamp(hsl.l+k*(k<0?hsl.l:1-hsl.l),0,1));return hex(c);}

/* ---------- faces ---------- */
const faceCache={};
function faceTex(st,expr,talk){
  const key=[st.iris,st.lash,st.brow,st.skin,st.kid,st.elder,expr,talk].join("|");
  if(faceCache[key])return faceCache[key];
  const S=128,c=document.createElement("canvas");c.width=c.height=S;const g=c.getContext("2d");
  const s=(st.kid?1.1:1)*(st.elder?.86:1),ey=st.kid?62:59,dx=st.kid?25:23.5,ex=[64-dx,64+dx],my=st.kid?95:93;
  g.lineCap="round";g.lineJoin="round";
  const ink="#3a1f1c",lash=st.lash||"#26181a",brow=st.brow||"#3a2721",skin=st.skin;
  const circle=(x,y,r)=>{g.beginPath();g.arc(x,y,r,0,Math.PI*2);g.fill();};
  // blush
  const ba={happy:.42,shy:.55,cry:.35}[expr]||(st.kid?.26:.15);
  g.fillStyle=`rgba(236,112,112,${ba})`;[[ex[0]-5,ey+19],[ex[1]+5,ey+19]].forEach(([x,y])=>{g.beginPath();g.ellipse(x,y,9.5,5,0,0,7);g.fill();});
  if(expr==="shy"){g.strokeStyle="rgba(200,80,80,.55)";g.lineWidth=1.2;for(const x0 of [ex[0]-10,ex[1]+1])for(let k=0;k<3;k++){g.beginPath();g.moveTo(x0+k*4,ey+22);g.lineTo(x0+k*4+3,ey+16);g.stroke();}}
  const lookX=expr==="shy"?-3:0,lookY={sad:2.5,cry:2.5,worry:1.5}[expr]||0;
  const open=(x,side,mode)=>{
    const rx=12*s,ry=(mode==="wide"?16.5:14.5)*s;
    g.save();g.fillStyle="#fffdf9";g.beginPath();g.ellipse(x,y0(),rx,ry,0,0,Math.PI*2);g.fill();g.clip();
    const ir=(mode==="wide"?7*s:9.6*s),iry=(mode==="wide"?9*s:12.2*s),ix=x+lookX*s,iy=y0()+1.5*s+lookY*s;
    const gr=g.createLinearGradient(0,iy-iry,0,iy+iry);gr.addColorStop(0,shade(st.iris,-.55));gr.addColorStop(.5,st.iris);gr.addColorStop(1,shade(st.iris,.45));
    g.fillStyle=gr;g.beginPath();g.ellipse(ix,iy,ir,iry,0,0,Math.PI*2);g.fill();
    g.fillStyle="#170e0c";g.beginPath();g.ellipse(ix,iy+1*s,ir*.46,iry*.52,0,0,Math.PI*2);g.fill();
    g.fillStyle="rgba(50,25,25,.22)";g.fillRect(x-rx,y0()-ry,rx*2,ry*.5);
    g.restore();
    g.fillStyle="#fff";circle(ix-3.6*s,iy-4.6*s,3.3*s);circle(ix+3.3*s,iy+4.4*s,1.6*s);
    g.strokeStyle=lash;g.lineWidth=3.8*s;g.beginPath();g.ellipse(x,y0()+1*s,rx+1*s,ry+.6*s,0,Math.PI*1.07,Math.PI*1.93);g.stroke();
    g.lineWidth=2.6*s;g.beginPath();g.moveTo(x+side*(rx-1.5*s),y0()-ry*.5);g.lineTo(x+side*(rx+4.5*s),y0()-ry*.82);g.stroke();
    g.globalAlpha=.55;g.lineWidth=1.3*s;g.beginPath();g.ellipse(x,y0(),rx-1*s,ry,0,Math.PI*.28,Math.PI*.72);g.stroke();g.globalAlpha=1;
    if(mode==="half"||mode==="angry"){
      g.fillStyle=skin;g.beginPath();const tilt=mode==="angry"?side*-4*s:0;
      g.moveTo(x-rx-3,y0()-ry-3);g.lineTo(x+rx+3,y0()-ry-3);g.lineTo(x+rx+3,y0()-ry*.1+tilt);g.lineTo(x-rx-3,y0()-ry*.1-tilt);g.closePath();g.fill();
      g.strokeStyle=lash;g.lineWidth=3.4*s;g.beginPath();g.moveTo(x-rx-1,y0()-ry*.1-tilt);g.lineTo(x+rx+1,y0()-ry*.1+tilt);g.stroke();
    }
  };
  const y0=()=>ey;
  const closed=(x,happy)=>{g.strokeStyle=lash;g.lineWidth=3.6*s;g.beginPath();
    if(happy)g.ellipse(x,ey+5*s,11*s,10*s,0,Math.PI*1.12,Math.PI*1.88);else g.ellipse(x,ey-3*s,11*s,6*s,0,Math.PI*.12,Math.PI*.88);g.stroke();};
  const browL=(x,side,inner,outer)=>{g.strokeStyle=brow;g.lineWidth=3*s;g.beginPath();g.moveTo(x-side*9*s,ey+inner*s);g.quadraticCurveTo(x,ey+(inner+outer)/2*s-2.5*s,x+side*10*s,ey+outer*s);g.stroke();};
  const BR={neutral:[-21,-21],happy:[-23,-22],sad:[-27,-19],cry:[-27,-19],worry:[-26,-20],surprised:[-28,-26],angry:[-15,-24],smug:[-21,-24],shy:[-23,-21],think:[-20,-25],blink:[-21,-21]}[expr]||[-21,-21];
  ex.forEach((x,i)=>{const side=i?1:-1;browL(x,side,BR[0],BR[1]);});
  ex.forEach((x,i)=>{const side=i?1:-1;
    if(expr==="blink")closed(x,false);
    else if(expr==="happy")closed(x,true);
    else if(expr==="surprised")open(x,side,"wide");
    else if(expr==="angry")open(x,side,"angry");
    else if(expr==="smug"||expr==="think")open(x,side,"half");
    else open(x,side,"open");
  });
  if(st.elder){g.strokeStyle="rgba(120,70,50,.35)";g.lineWidth=1.1;ex.forEach((x,i)=>{const sd=i?1:-1;for(let k=0;k<2;k++){g.beginPath();g.moveTo(x+sd*15,ey+2+k*4);g.lineTo(x+sd*19,ey+k*5);g.stroke();}});}
  if(expr==="cry"){g.fillStyle="rgba(140,200,240,.85)";ex.forEach(x=>{g.beginPath();g.moveTo(x-2,ey+12);g.quadraticCurveTo(x-6,ey+30,x,ey+36);g.quadraticCurveTo(x+5,ey+28,x+2,ey+12);g.fill();});}
  // nose
  g.strokeStyle="rgba(150,80,60,.4)";g.lineWidth=1.4;g.beginPath();g.moveTo(64,my-14);g.lineTo(62.5,my-10.5);g.stroke();
  // mouth
  g.strokeStyle=ink;g.fillStyle="#7a2b2e";g.lineWidth=2.3;
  const tongue=(x,y,r)=>{g.fillStyle="#e98a8a";g.beginPath();g.ellipse(x,y,r,r*.55,0,0,Math.PI);g.fill();};
  const mouthOpen=(w,h,oy)=>{g.fillStyle="#7a2b2e";g.beginPath();g.ellipse(64,my+(oy||0),w,h,0,0,Math.PI*2);g.fill();tongue(64,my+(oy||0)+h*.35,w*.6);};
  const m=talk?{neutral:"o",happy:"D",sad:"o",worry:"o",angry:"A",surprised:"O",smug:"o",think:"o",shy:"o",cry:"o"}[expr]||"o":expr;
  if(m==="o")mouthOpen(5.5,4.6);
  else if(m==="D"||expr==="happy"){g.fillStyle="#7a2b2e";g.beginPath();g.moveTo(55,my-2);g.quadraticCurveTo(64,my+(m==="D"?14:11),73,my-2);g.closePath();g.fill();tongue(64,my+4,4.5);}
  else if(m==="A"){g.fillStyle="#7a2b2e";g.beginPath();g.moveTo(56,my+4);g.lineTo(72,my+4);g.lineTo(68,my-3);g.lineTo(60,my-3);g.closePath();g.fill();g.fillStyle="#fff";g.fillRect(59,my-3,10,2.2);}
  else if(m==="O"||expr==="surprised"){g.fillStyle="#7a2b2e";g.beginPath();g.ellipse(64,my+2,4.6,5.8,0,0,Math.PI*2);g.fill();}
  else if(expr==="sad"||expr==="cry"){g.beginPath();g.ellipse(64,my+5,6.5,4,0,Math.PI*1.15,Math.PI*1.85);g.stroke();}
  else if(expr==="worry"){g.beginPath();g.moveTo(57,my+1);g.quadraticCurveTo(60.5,my-2,64,my+1);g.quadraticCurveTo(67.5,my+4,71,my+1);g.stroke();}
  else if(expr==="angry"){g.beginPath();g.ellipse(64,my+5,7,3.5,0,Math.PI*1.1,Math.PI*1.9);g.stroke();}
  else if(expr==="smug"){g.beginPath();g.moveTo(57,my+1);g.quadraticCurveTo(66,my+5,72,my-3);g.stroke();}
  else if(expr==="think"){g.beginPath();g.moveTo(59,my+2);g.lineTo(69,my);g.stroke();}
  else{g.beginPath();g.ellipse(64,my-1,6,3.6,0,Math.PI*.15,Math.PI*.85);g.stroke();}
  const t=new T.CanvasTexture(c);t.encoding=T.sRGBEncoding;t.anisotropy=4;
  return faceCache[key]=t;
}

/* ---------- emotes and poses ---------- */
const env=(t,d)=>Math.min(1,t/.16)*Math.min(1,Math.max(0,(d-t)/.22));
const EM={
  wave:{d:1.6,x:"happy",f:(q,t,w)=>{q.aRz=lerp(q.aRz,-2.7+Math.sin(t*13)*.32,w);q.aRx=lerp(q.aRx,-.15,w);q.hz+=.08*w;}},
  nod:{d:.8,f:(q,t,w)=>{q.hx+=Math.sin(t*15)*.22*w;}},
  shake:{d:.9,f:(q,t,w)=>{q.hy+=Math.sin(t*15)*.34*w;}},
  cheer:{d:1.4,x:"happy",f:(q,t,w)=>{q.aRz=lerp(q.aRz,-2.8,w);q.aLz=lerp(q.aLz,2.8,w);q.rootY+=Math.abs(Math.sin(t*9))*.2*w;}},
  jump:{d:.62,x:"surprised",f:(q,t,w)=>{const k=t/.62;q.rootY+=Math.sin(k*Math.PI)*.42;q.sq=1+Math.sin(k*Math.PI*2)*.09;q.aRz=lerp(q.aRz,-2.3,w);q.aLz=lerp(q.aLz,2.3,w);}},
  shrug:{d:1.3,f:(q,t,w)=>{q.aRz=lerp(q.aRz,-1.05,w);q.aLz=lerp(q.aLz,1.05,w);q.aRx=lerp(q.aRx,-.55,w);q.aLx=lerp(q.aLx,-.55,w);q.hz+=.22*w;}},
  facepalm:{d:1.7,x:"sad",f:(q,t,w)=>{q.aRx=lerp(q.aRx,-2.35,w);q.aRz=lerp(q.aRz,.42,w);q.hx+=.32*w;}},
  point:{d:1.4,f:(q,t,w)=>{q.aRx=lerp(q.aRx,-1.55,w);q.aRz=lerp(q.aRz,-.08,w);}},
  think:{d:2.2,x:"think",f:(q,t,w)=>{q.aRx=lerp(q.aRx,-2.0,w);q.aRz=lerp(q.aRz,.55,w);q.hz+=.15*w;q.hx-=.08*w;}},
  laugh:{d:1.5,x:"happy",f:(q,t,w)=>{q.tx-=.12*w;q.hx-=.22*w;q.rootY+=Math.abs(Math.sin(t*16))*.045*w;q.aRz=lerp(q.aRz,-.55,w);q.aLz=lerp(q.aLz,.55,w);}},
  stomp:{d:1,x:"angry",f:(q,t,w)=>{q.lR=lerp(q.lR,-.7*Math.abs(Math.sin(t*9)),w);q.aRx=lerp(q.aRx,-.4,w);q.aLx=lerp(q.aLx,-.4,w);q.aRz=lerp(q.aRz,-.62,w);q.aLz=lerp(q.aLz,.62,w);q.hy+=Math.sin(t*22)*.05*w;}},
  surprise:{d:.95,x:"surprised",f:(q,t,w)=>{q.rootY+=Math.sin(Math.min(1,t/.35)*Math.PI)*.16;q.aRz=lerp(q.aRz,-1.25,w);q.aLz=lerp(q.aLz,1.25,w);q.tx-=.12*w;}},
  bow:{d:1.3,f:(q,t,w)=>{q.tx+=.5*w;q.hx+=.2*w;}},
  cry:{d:2.4,x:"cry",f:(q,t,w)=>{q.hx+=.34*w;q.tz+=Math.sin(t*18)*.03*w;q.aRx=lerp(q.aRx,-2.15,w);q.aLx=lerp(q.aLx,-2.15,w);q.aRz=lerp(q.aRz,.42,w);q.aLz=lerp(q.aLz,-.42,w);}},
  give:{d:1.3,f:(q,t,w)=>{q.aRx=lerp(q.aRx,-1.3,w);q.aLx=lerp(q.aLx,-1.3,w);q.aRz=lerp(q.aRz,.2,w);q.aLz=lerp(q.aLz,-.2,w);}},
  type:{d:1.4,f:(q,t,w)=>{q.aRx=lerp(q.aRx,-1.2+Math.sin(t*30)*.07,w);q.aLx=lerp(q.aLx,-1.2+Math.cos(t*27)*.07,w);q.aRz=lerp(q.aRz,.15,w);q.aLz=lerp(q.aLz,-.15,w);}},
  dance:{d:2.4,x:"happy",f:(q,t,w)=>{q.rootY+=Math.abs(Math.sin(t*8))*.12*w;q.aRz=lerp(q.aRz,-1.6+Math.sin(t*8)*.6,w);q.aLz=lerp(q.aLz,1.6+Math.sin(t*8)*.6,w);q.hz+=Math.sin(t*8)*.15*w;q.tz+=Math.sin(t*8)*.08*w;}},
  eat:{d:2,x:"happy",f:(q,t,w)=>{q.aRx=lerp(q.aRx,-2.1+Math.abs(Math.sin(t*6))*.5,w);q.aRz=lerp(q.aRz,.3,w);q.aLx=lerp(q.aLx,-1.1,w);}},
};
const POSES={
  sit:q=>{q.lL=-1.5;q.lR=-1.5;q.hip=-.22;q.tx=-.04;q.aLz=.14;q.aRz=-.14;},
  sitType:(q,t)=>{POSES.sit(q);q.aRx=-1.15+Math.sin(t*29)*.05;q.aLx=-1.15+Math.cos(t*27)*.05;q.aRz=.16;q.aLz=-.16;},
  hold:q=>{q.aRx=-1.15;q.aRz=-.05;},
  holdUp:q=>{q.aRx=-2.7;q.aRz=-.25;},
  phone:q=>{q.aRx=-2.45;q.aRz=.62;q.hz+=.14;},
  crossArms:q=>{q.aRx=-1.05;q.aLx=-1.05;q.aRz=.95;q.aLz=-.95;},
  hips:q=>{q.aRz=-.55;q.aLz=.55;q.aRx=.22;q.aLx=.22;},
  floorSit:q=>{q.hip=-.47;q.lL=-1.5;q.lR=-1.25;q.tx=-.1;q.aLz=.5;q.aRx=-.9;},
  kneel:q=>{q.hip=-.24;q.lL=-1.25;q.lR=.45;q.tx=.32;q.aRx=-1;q.aLx=-1;},
  search:(q,t)=>{q.tx=.55;q.aRx=-1.2+Math.sin(t*5)*.3;q.aLx=-1.2+Math.cos(t*5)*.3;q.hx=.2;},
  cook:(q,t)=>{q.aRx=-1.1+Math.sin(t*4)*.2;q.aRz=-.3+Math.cos(t*4)*.2;q.aLx=-.6;},
};

/* ---------- people ---------- */
G.makeChar=function(d){
  const toon=(c,o)=>G.toon(c,Object.assign({rim:.32},o||{}));
  const skin=toon(d.skin||0xf2d2b6),top=toon(d.top.color),inner=toon(d.top.inner||0xf2ede2),bot=toon(d.bottom.color),shoeM=toon(d.shoes||0x4a3a30),hairM=toon(d.hair.color,{rim:.4,side:T.DoubleSide});
  hairM.crown=G.toon(0xffffff,{map:hairTex(d.hair.color,true),rim:.4,side:T.DoubleSide});hairM.back=G.toon(0xffffff,{map:hairTex(d.hair.color,false),rim:.4,side:T.DoubleSide});
  const handM=d.acc&&d.acc.includes("oil")?toon(0x8a6a52):skin;
  const sc=(d.scale||1)*(d.kid?.8:1)*(d.elder?.95:1),headS=d.kid?1.1:1;
  const ch={def:d,displayName:d.name,voice:d.voice||520,seed:Math.random()*10,phase:0,mov:0,speed:0,expr:"neutral",lookY:0,lookP:0,blinkT:2+Math.random()*3,talkT:0,mouth:false,springs:[],q:{},isPlayer:!!d.player,persist:!!d.player};
  const root=ch.root=new T.Group(),body=ch.body=new T.Group();root.add(body);body.scale.setScalar(sc);ch.baseScale=sc;
  const hipY=ch.hipY=.55;
  const hips=ch.hips=new T.Group();hips.position.y=hipY;body.add(hips);
  // legs
  const legC=d.bottom.style==="shorts"||d.bottom.style==="skirt"?skin:bot;
  const mkLeg=x=>{const p=new T.Group();p.position.set(x,0,0);hips.add(p);part(capG(.072,.34),legC,p,[0,-.27,0],null,null,true);
    part(sphG(.1,16,10),shoeM,p,[0,-.5,.035],null,[.9,.6,1.35],true);return p;};
  ch.legL=mkLeg(.1);ch.legR=mkLeg(-.1);
  // torso
  const torso=ch.torso=new T.Group();hips.add(torso);
  const tst=d.top.style;
  const bodyC=(tst==="blouse"||tst==="tee"||tst==="tunic"||tst==="coverall"||tst==="hoodie"||tst==="jacket"||tst==="cardigan"||tst==="suit")?top:inner;
  part(capG(.165,.22),bodyC,torso,[0,.21,0],null,[1,1,.92],true);
  if(d.bottom.style==="skirt")part(cylG(.17,.27,.27,20),bot,torso,[0,-.04,0],null,null,true);
  if(d.bottom.style==="shorts")part(cylG(.168,.19,.15,18),bot,torso,[0,-.02,0],null,null,true);
  if(tst==="cardigan"){part(cylG(.065,0,.2,3),inner,torso,[0,.35,.148],[0,Math.PI,0],[1,1,.22]);part(cylG(.168,.2,.13,20),top,torso,[0,.0,0],null,null,true);part(torG(.1,.024),inner,torso,[0,.46,.02],[Math.PI/2,0,0]);}
  if(tst==="suit"){part(boxG(.085,.3,.03),inner,torso,[0,.24,.148]);part(boxG(.032,.22,.02),toon(d.top.accent||0x8a2f2f),torso,[0,.22,.163]);part(cylG(.168,.19,.12,20),top,torso,[0,.0,0],null,null,true);}
  if(tst==="hoodie"){part(torG(.12,.05),top,torso,[0,.45,-.05],[Math.PI/2.4,0,0]);part(boxG(.18,.08,.03),toon(shade(d.top.color,-.2)),torso,[0,.1,.15]);}
  if(tst==="jacket"){part(boxG(.07,.28,.025),inner,torso,[0,.24,.149]);}
  if(tst==="overalls"){part(boxG(.22,.2,.05),bot,torso,[0,.17,.135],null,null,true);part(boxG(.035,.2,.02),bot,torso,[.07,.36,.14]);part(boxG(.035,.2,.02),bot,torso,[-.07,.36,.14]);part(cylG(.168,.18,.14,18),bot,torso,[0,0,0]);}
  if(tst==="apron"){part(boxG(.27,.44,.03),inner,torso,[0,.06,.15],null,null,true);}
  if(tst==="coverall"){part(torG(.16,.022),toon(0x3a3328),torso,[0,.04,0],[Math.PI/2,0,0]);part(boxG(.08,.07,.02),toon(shade(d.top.color,-.2)),torso,[.07,.25,.152]);}
  if(tst==="tunic"){const st=G.toon(0xf0e05a,{emissive:0x8a7a20,ei:.4});part(boxG(.34,.03,.33),st,torso,[0,.12,0]);part(boxG(.34,.03,.33),st,torso,[0,.3,0]);}
  if(tst==="tee"&&d.top.inner){part(cylG(.169,.169,.05,18),inner,torso,[0,.22,0]);}
  if(tst==="blouse"){part(torG(.1,.024),inner,torso,[0,.46,.02],[Math.PI/2,0,0]);}
  // arms
  const sleeve=tst==="overalls"||tst==="apron"?inner:top;
  const mkArm=x=>{const p=new T.Group();p.position.set(x,.41,0);torso.add(p);
    if(tst==="jacket"){part(capG(.058,.1),sleeve,p,[0,-.08,0],null,null,true);part(capG(.052,.12),skin,p,[0,-.21,0],null,null,true);}
    else part(capG(.056,.24),tst==="tee"||tst==="overalls"?skin:sleeve,p,[0,-.17,0],null,null,true);
    if(tst==="tee"||tst==="overalls")part(capG(.06,.04),sleeve,p,[0,-.04,0]);
    const hand=part(sphG(.064,14,10),handM,p,[0,-.355,0],null,null,true);p.userData.hand=hand;return p;};
  ch.armL=mkArm(.205);ch.armR=mkArm(-.205);
  ch.armL.rotation.order=ch.armR.rotation.order="XZY";
  // head
  const neck=ch.neck=new T.Group();neck.position.y=.5;torso.add(neck);
  const head=ch.head=new T.Group();head.position.y=.2;head.scale.setScalar(headS);head.rotation.order="YXZ";neck.add(head);
  const R=.3;ch.headR=R*headS*sc;
  part(sphG(R,26,18),skin,head,[0,0,0],null,[1,.97,.98],true);
  part(sphG(.06,10,8),skin,head,[.29,-.02,0],null,[.5,1,.8]);part(sphG(.06,10,8),skin,head,[-.29,-.02,0],null,[.5,1,.8]);
  // face decal
  ch.faceStyle={iris:d.eyes||"#5a3a2a",lash:d.lash,brow:d.brow||hex(d.hair.color),skin:hex(d.skin||0xf2d2b6),kid:!!d.kid,elder:!!d.elder};
  const fm=new T.MeshToonMaterial({map:faceTex(ch.faceStyle,"neutral",false),transparent:true,gradientMap:G.grad,depthWrite:false,polygonOffset:true,polygonOffsetFactor:-2});
  ch.faceMat=fm;
  const face=new T.Mesh(new T.SphereGeometry(R*1.012,28,18,Math.PI/2-1.0,2.0,.78,1.22),fm);face.scale.set(1,.97,.98);face.renderOrder=2;head.add(face);
  buildHair(ch,d,head,R,hairM,toon);
  buildAcc(ch,d,head,torso,R,toon);
  ch.headTop=(R+.06)*headS*sc;
  G.chars.push(ch);
  return Object.assign(ch,CHAR_API);
};
function onSphere(R,a,e,k){return [R*k*Math.sin(a)*Math.cos(e),R*k*Math.sin(e),R*k*Math.cos(a)*Math.cos(e)];}
function buildHair(ch,d,head,R,hm,toon){
  const st=d.hair.style;
  if(st==="scarf"){
    const cl=toon(d.hair.cloth||0x9a5a3a,{side:T.DoubleSide});
    part(new T.SphereGeometry(R*1.09,24,12,0,Math.PI*2,0,1.12),cl,head,[0,.01,-.005],null,null,true);
    part(new T.SphereGeometry(R*1.07,24,10,Math.PI/2+1.1,Math.PI*2-2.2,1.2,.75),cl,head,[0,0,0],null,null,true);
    part(sphG(.075,12,8),cl,head,onSphere(R,Math.PI,-.1,1.12),null,[1.2,.9,1],true);
    part(capG(.04,.12),cl,head,onSphere(R,Math.PI+.25,-.45,1.1),[.4,0,.3],null,true);
    return;
  }
  // crown and back
  const backLen={bob:1.28,pony:1.1,highpony:.95,short:.82,spiky:.8,bun:1.0,salt:.85}[st]||1;
  part(new T.SphereGeometry(R*1.075,26,12,0,Math.PI*2,0,1.12),hm.crown||hm,head,[0,.012,-.008],null,[1,1.02,1.03],true);
  part(new T.SphereGeometry(R*1.072,26,14,Math.PI/2+1.04,Math.PI*2-2.08,1.0,backLen),hm.back||hm,head,[0,0,-.004],null,null,true);
  // a layered cut: a few tips along the lower edge at the back
  if(st==="bob"||st==="pony"||st==="bun"||st==="highpony"){const tips=st==="bob"?[-.75,-.25,.25,.75]:[-.45,.45];
    tips.forEach(o=>{const a=Math.PI+o,e=Math.PI/2-1.0-backLen+.16;part(sphG(.06,10,8),hm,head,onSphere(R,a,e,1.0),[0,a,0,"YXZ"],[1,1.3,.5],true);});}
  // bangs
  const strands={bob:[-.64,-.34,-.06,.22,.5,.72],pony:[-.6,-.3,0,.3,.6],highpony:[-.55,-.2,.15,.5],short:[-.45,-.15,.15,.45],spiky:[-.4,0,.4],bun:[-.55,-.2,.2,.55],salt:[-.4,0,.4]}[st]||[];
  // bangs: flattened cones pointing down, tips ending just above the eyes
  const sg=geo.bang||(geo.bang=new T.ConeGeometry(.088,.17,8).rotateX(Math.PI));
  strands.forEach((a,i)=>{const e=.62-Math.abs(a)*.1,p=onSphere(R,a,e,1.025);p[1]+=.03;
    part(sg,hm,head,p,[-e+.12,a,(a>0?-1:1)*.1,"YXZ"],[st==="short"||st==="spiky"||st==="salt"?.85:1.04,st==="bob"?1.12:.98,.42],true);});
  if(st==="bob"){[[1.22,1],[-1.22,-1]].forEach(([a,s])=>{part(capG(.046,.16),hm,head,onSphere(R,a,-.1,.99),[.1,a,s*.08,"YXZ"],[.85,1,.5],true);});}
  if(st==="pony"){const p=new T.Group();p.position.set(...onSphere(R,Math.PI,.42,1.03));head.add(p);
    part(sphG(.05,10,8),toon(0xb5473a),p,[0,0,0]);part(capG(.08,.34),hm,p,[0,-.24,-.04],[.15,0,0],null,true);ch.springs.push({o:p,a:.35,v:0,base:.35,k:55,c:7,amp:.4});}
  if(st==="highpony"){const p=new T.Group();p.position.set(...onSphere(R,Math.PI,1.0,1.02));head.add(p);
    part(sphG(.055,10,8),toon(0xd94a3a),p,[0,0,0]);part(sphG(.07,12,10),hm,p,[0,.02,-.05],null,[1,.9,1.1],true);part(capG(.08,.28),hm,p,[0,-.15,-.13],[.62,0,0],[1,1,.85],true);ch.springs.push({o:p,a:.12,v:0,base:.12,k:50,c:6,amp:.45});}
  if(st==="bun"){part(sphG(.13,16,12),hm,head,onSphere(R,Math.PI,.55,1.05),null,null,true);}
  if(st==="spiky"){for(let i=0;i<7;i++){const a=(i/7)*Math.PI*2,e=1.0+Math.random()*.25;part(cylG(0,.07,.2,6),hm,head,onSphere(R,a,e,1.05),[-(Math.PI/2-e)*.8*Math.cos(a)+.3,a,0,"YXZ"],null,true);}}
  if(st==="salt"){const gm=toon(0xbab4ac);[1.3,-1.3].forEach(a=>part(sphG(.07,12,10),gm,head,onSphere(R,a,.12,1.02),[0,a,0,"YXZ"],[.8,.85,.3]));}
}
function buildAcc(ch,d,head,torso,R,toon){
  const acc=d.acc||[];ch.glasses=null;
  const handR=ch.armR.userData.hand,handL=ch.armL.userData.hand;
  if(acc.includes("glasses")||acc.includes("roundglasses")){
    const glow=acc.includes("glasses");
    const gm=glow?new T.MeshToonMaterial({color:0x1f6f66,gradientMap:G.grad,emissive:0x3ff0c0,emissiveIntensity:.25}):toon(0x3a2e2a);
    ch.glasses=[];
    [-.37,.37].forEach(a=>{const r=part(torG(.066,.009),gm,head,onSphere(R,a,.2,1.04),[0,a,0,"YXZ"]);ch.glasses.push(r);});
    part(boxG(.06,.009,.009),gm,head,onSphere(R,0,.22,1.05));
  }
  if(acc.includes("badge"))part(boxG(.05,.035,.012),G.toon(0xd9b45a,{emissive:0x6a5020,ei:.5}),torso,[.085,.33,.16]);
  if(acc.includes("headphones")){const hm=toon(0x2b2b30);part(torG(.33,.03,.5),hm,head,[0,0,0],[0,Math.PI/2,0]);[-1,1].forEach(s=>part(cylG(.075,.075,.06,14),toon(0xd94a3a),head,[s*.31,0,0],[0,0,Math.PI/2]));}
  if(acc.includes("cup")){const cm=toon(0xf3efe6);part(cylG(.045,.038,.1,12),cm,handR,[0,-.02,.05]);}
  if(acc.includes("clipboard"))part(boxG(.14,.2,.015),toon(0xb08a5a),handL,[0,-.04,.07],[.4,0,0]);
  const stache=c=>{const mm=toon(c);[-1,1].forEach(sd=>part(capG(.019,.055),mm,head,onSphere(R,sd*.12,-.2,.985),[0,sd*.12,sd*(Math.PI/2-.4),"YXZ"],[1,1,.5]));};
  if(acc.includes("mustache"))stache(0x3a3330);
  if(acc.includes("mustacheW"))stache(0xe8e4dc);
  if(acc.includes("satchel")){part(boxG(.16,.14,.07),toon(0xc9963a),torso,[-.2,.03,.02],null,null,true);part(torG(.2,.012),toon(0x8a6a3a),torso,[-.02,.22,0],[0,0,.7]);}
  if(acc.includes("shawl"))part(torG(.165,.06),toon(0x7a5a8a),torso,[0,.4,0],[Math.PI/2,0,0],[1,1,.7],true);
  if(acc.includes("bandage"))part(torG(.058,.018),toon(0xf2efe8),ch.armL,[0,-.3,0],[Math.PI/2,0,0]);
  if(acc.includes("nonla")){const hm=toon(0xdcc58f,{side:T.DoubleSide});part(cylG(.02,.5,.26,24),hm,head,[0,.3,0],[-.08,0,0],null,true);}
  if(acc.includes("cap")){const cm=toon(0xd04a3a);part(new T.SphereGeometry(R*1.1,20,10,0,Math.PI*2,0,1.05),cm,head,[0,.02,0],null,null,true);part(cylG(.17,.17,.02,18),cm,head,onSphere(R,0,.55,1.05),[.35,0,0],[1,1,.8]);}
  if(acc.includes("hardhat")){const hm=toon(0xf0c030);part(new T.SphereGeometry(R*1.14,20,10,0,Math.PI*2,0,1.1),hm,head,[0,.03,0],null,null,true);part(cylG(.39,.39,.025,24),hm,head,[0,.05,.02]);}
}
const _v=new T.Vector3(),_hp=new T.Vector3();
const CHAR_API={
  setExpr(e){this.expr=e||"neutral";this.faceMat.map=faceTex(this.faceStyle,this.expr,this.mouth&&this.talking);},
  emote(name){const E=EM[name];if(!E)return;this.em=name;this.emT=0;if(E.x){this.prevExpr=this.expr;this.setExpr(E.x);this.emExpr=true;}else this.emExpr=false;},
  setPose(p){this.pose=p||null;},
  headPos(extra){this.head.getWorldPosition(_hp);_hp.y+=this.headTop+(extra||0);return _hp;},
  handPos(){return this.armR.userData.hand.getWorldPosition(new T.Vector3());},
  update(dt,t){
    const q=this.q,sp=this.speed||0;
    this.mov=damp(this.mov,sp>.15?Math.min(1.3,sp/3.3):0,9,dt);
    if(sp>.15)this.phase+=dt*(5.4+sp*1.25);
    const a=Math.min(1,this.mov)*.95,run=clamp((sp-3.8)/2.2,0,1),s=Math.sin(this.phase),c=Math.cos(this.phase);
    q.lL=s*a*.85;q.lR=-s*a*.85;q.aLx=-s*a*(.72+run*.4);q.aRx=s*a*(.72+run*.4);q.aLz=.1+run*.12;q.aRz=-.1-run*.12;
    q.tx=Math.min(1,this.mov)*.07+run*.16;q.ty=-s*a*.09;q.tz=0;q.hx=0;q.hy=0;q.hz=0;q.hip=Math.abs(c)*.05*a;q.rootY=0;q.sq=1;
    const idle=1-Math.min(1,this.mov),br=Math.sin(t*2+this.seed);
    q.aLz+=idle*(.07+br*.025);q.aRz-=idle*(.07+br*.025);q.hx+=idle*br*.018;
    if(this.air){q.lL=.6;q.lR=-.3;q.aLx=-.5;q.aRx=-.5;q.aLz=.8;q.aRz=-.8;}
    if(this.pose&&POSES[this.pose])POSES[this.pose](q,t,this);
    if(this.em){this.emT+=dt;const E=EM[this.em];E.f(q,this.emT,env(this.emT,E.d),this);if(this.emT>=E.d){this.em=null;if(this.emExpr)this.setExpr(this.prevExpr||"neutral");}}
    if(this.talking&&!this.em&&!this.pose&&idle>.5){q.aRx+=-.3-.22*Math.sin(t*4.5+1);q.aRz+=-.12;q.hx+=Math.sin(t*6.5)*.035;q.hz+=Math.sin(t*3.3)*.04;}
    this.legL.rotation.x=q.lL;this.legR.rotation.x=q.lR;
    this.armL.rotation.set(q.aLx,0,q.aLz);this.armR.rotation.set(q.aRx,0,q.aRz);
    this.torso.rotation.set(q.tx,q.ty,q.tz);
    if(this.def.elder)this.torso.rotation.x+=.14;
    this.hips.position.y=this.hipY+q.hip;this.body.position.y=q.rootY;
    const sq=q.sq,bs=this.baseScale;this.body.scale.set(bs/Math.sqrt(sq),bs*sq,bs/Math.sqrt(sq));
    // look at a point (the person talking, the player passing by)
    let wy=0,wp=0;
    if(this.lookAt){this.head.getWorldPosition(_v);const dx=this.lookAt.x-_v.x,dy=this.lookAt.y-_v.y,dz=this.lookAt.z-_v.z;
      let ang=Math.atan2(dx,dz)-this.root.rotation.y;while(ang>Math.PI)ang-=Math.PI*2;while(ang<-Math.PI)ang+=Math.PI*2;
      wy=clamp(ang-this.torso.rotation.y,-1.05,1.05);wp=clamp(-Math.atan2(dy,Math.hypot(dx,dz)),-.45,.5);}
    this.lookY=damp(this.lookY,wy,6,dt);this.lookP=damp(this.lookP,wp,6,dt);
    this.head.rotation.set(q.hx+this.lookP,q.hy+this.lookY,q.hz);
    // hair springs
    for(const h of this.springs){const target=h.base+this.mov*h.amp*.6+Math.sin(this.phase*2)*.06*this.mov-q.tx*.5-this.lookP*.5;
      h.v+=(target-h.a)*h.k*dt-h.v*h.c*dt;h.a+=h.v*dt;h.o.rotation.x=h.a;h.o.rotation.z=Math.sin(t*1.3+this.seed)*.04+this.lookY*-.2;}
    // blink and talk
    this.blinkT-=dt;
    let want=this.expr,talkOpen=false;
    if(this.talking){this.talkT-=dt;if(this.talkT<0){this.talkT=.07+Math.random()*.09;this.mouth=!this.mouth;}talkOpen=this.mouth;}
    if(this.blinkT<0){if(this.blinkT<-.12)this.blinkT=2+Math.random()*3.5;else if(want!=="happy"&&want!=="blink")want="blink";}
    const key=want+(talkOpen?"+":"");
    if(key!==this._fk){this._fk=key;this.faceMat.map=faceTex(this.faceStyle,want,talkOpen);}
  },
};

/* ---------- CIVI, the drone ---------- */
function droneFace(e){
  const c=document.createElement("canvas");c.width=c.height=96;const g=c.getContext("2d");
  g.fillStyle="#0a1616";g.fillRect(0,0,96,96);
  const col=e==="glitch"?"#ff6a58":e==="love"?"#ff8fb0":"#3ff0c0";
  g.fillStyle=col;g.strokeStyle=col;g.lineWidth=5;g.lineCap="round";
  const L=[33,63],y=44;
  const rr=(x,y,w,h,r)=>{g.beginPath();g.moveTo(x+r,y);g.arcTo(x+w,y,x+w,y+h,r);g.arcTo(x+w,y+h,x,y+h,r);g.arcTo(x,y+h,x,y,r);g.arcTo(x,y,x+w,y,r);g.fill();};
  if(e==="happy"){L.forEach(x=>{g.beginPath();g.arc(x,y+5,9,Math.PI*1.1,Math.PI*1.9);g.stroke();});}
  else if(e==="think"){L.forEach(x=>{g.beginPath();g.moveTo(x-8,y);g.lineTo(x+8,y);g.stroke();});[40,50,60].forEach(x=>{g.beginPath();g.arc(x,y+22,2.5,0,7);g.fill();});}
  else if(e==="alert"){L.forEach(x=>{rr(x-3,y-14,6,18,3);g.beginPath();g.arc(x,y+10,3.2,0,7);g.fill();});}
  else if(e==="sad"){L.forEach((x,i)=>{g.beginPath();g.arc(x,y-4,9,Math.PI*.15,Math.PI*.85);g.stroke();});}
  else if(e==="glitch"){L.forEach(x=>{g.beginPath();g.moveTo(x-7,y-7);g.lineTo(x+7,y+7);g.moveTo(x+7,y-7);g.lineTo(x-7,y+7);g.stroke();});g.fillStyle="rgba(255,106,88,.35)";for(let i=0;i<6;i++)g.fillRect(Math.random()*80,Math.random()*90,16+Math.random()*20,3);}
  else if(e==="love"){L.forEach(x=>{g.beginPath();g.moveTo(x,y+9);g.bezierCurveTo(x-14,y-2,x-6,y-13,x,y-4);g.bezierCurveTo(x+6,y-13,x+14,y-2,x,y+9);g.fill();});}
  else if(e==="wink"){g.beginPath();g.arc(L[0],y+5,9,Math.PI*1.1,Math.PI*1.9);g.stroke();rr(L[1]-6,y-10,12,20,6);}
  else{L.forEach(x=>rr(x-6,y-11,12,22,6));}
  if(e!=="think"&&e!=="glitch"){g.globalAlpha=.8;g.beginPath();g.arc(48,y+24,e==="happy"||e==="love"?7:4,0,Math.PI);g.stroke();g.globalAlpha=1;}
  g.fillStyle="rgba(63,240,192,.06)";for(let y2=0;y2<96;y2+=4)g.fillRect(0,y2,96,1);
  const t=new T.CanvasTexture(c);t.encoding=T.sRGBEncoding;return t;
}
const droneFaces={};
G.makeDrone=function(){
  const root=new T.Group(),float=new T.Group();root.add(float);
  const shell=G.toon(0xf2f4f0,{rim:.6,rimColor:0x9ff8e0}),dark=G.toon(0x5d6866,{rim:.2});
  const bodyM=part(sphG(.2,26,18),shell,float,[0,0,0],null,null,true);
  part(new T.SphereGeometry(.202,26,10,0,Math.PI*2,Math.PI*.62,Math.PI*.38),dark,float,[0,0,0]);
  const fm=new T.MeshBasicMaterial({map:null,toneMapped:false});
  const face=new T.Mesh(new T.SphereGeometry(.205,24,16,Math.PI/2-.85,1.7,.95,1.0),fm);float.add(face);
  const ringM=G.basic(0x3ff0c0);const ring=new T.Mesh(torG(.235,.018),ringM);ring.rotation.x=Math.PI/2;float.add(ring);
  part(cylG(.008,.008,.14,6),dark,float,[0,.25,0]);
  const tipM=G.basic(0xff6a58);const tip=new T.Mesh(sphG(.025,8,6),tipM);tip.position.y=.33;float.add(tip);
  const glow=new T.Sprite(new T.SpriteMaterial({map:G.fx.dot,color:0x3ff0c0,transparent:true,opacity:.55,blending:T.AdditiveBlending,depthWrite:false}));glow.scale.set(.5,.5,1);glow.position.y=-.24;float.add(glow);
  const d={root,displayName:"CIVI",voice:950,isDrone:true,seed:0,talking:false,expr:"happy",vel:new T.Vector3(),em:null,emT:0,
    headTop:.12,
    headPos(extra){root.getWorldPosition(_hp);_hp.y+=.25+(extra||0);return _hp;},
    setExpr(e){this.expr=e;fm.map=droneFaces[e]||(droneFaces[e]=droneFace(e));fm.needsUpdate=true;},
    emote(e){this.em=e;this.emT=0;},
    follow:null,
    update(dt,t){
      if(this.follow){const tg=this.follow();_v.copy(tg);
        const k=1-Math.exp(-dt*3.2);root.position.lerp(_v,k);}
      float.position.y=Math.sin(t*2.2)*.06;
      ring.rotation.z+=dt*(this.talking?6:1.6);
      tipM.color.setHex(Math.sin(t*5)>0?0xff6a58:0x5a2a26);
      glow.material.opacity=.4+Math.sin(t*9)*.15;
      if(this.look){const dx=this.look.x-root.position.x,dz=this.look.z-root.position.z;root.rotation.y=G.angDamp(root.rotation.y,Math.atan2(dx,dz),6,dt);}
      if(this.em){this.emT+=dt;const k=this.emT;
        if(this.em==="spin")float.rotation.y=Math.min(1,k/.8)*Math.PI*2;
        if(this.em==="bounce")float.position.y+=Math.abs(Math.sin(k*10))*.15*(1-k/1.2);
        if(this.em==="shake")float.position.x=Math.sin(k*40)*.04*(1-k/.9);
        if(this.emT>1.2){this.em=null;float.rotation.y=0;float.position.x=0;}}
      if(this.talking){this.talkT=(this.talkT||0)-dt;if(this.talkT<0){this.talkT=.12;face.scale.y=face.scale.y>1?1:1.06;}}else face.scale.y=1;
    }};
  d.setExpr("happy");
  G.chars.push(d);
  return d;
};

/* ---------- Bé Điện, the cat ---------- */
function catFace(e){
  const c=document.createElement("canvas");c.width=c.height=96;const g=c.getContext("2d");
  g.lineCap="round";const L=[30,66],y=44;
  if(e==="sleep"||e==="happy"){g.strokeStyle="#3a2418";g.lineWidth=4;L.forEach(x=>{g.beginPath();g.arc(x,y+(e==="happy"?6:0),9,e==="happy"?Math.PI*1.1:Math.PI*.1,e==="happy"?Math.PI*1.9:Math.PI*.9);g.stroke();});}
  else{L.forEach(x=>{g.fillStyle="#e8c84a";g.beginPath();g.ellipse(x,y,11,12,0,0,7);g.fill();g.fillStyle="#1a1210";g.beginPath();g.ellipse(x,y,e==="grumpy"?2.5:4,10,0,0,7);g.fill();g.fillStyle="#fff";g.beginPath();g.arc(x-3,y-5,2.4,0,7);g.fill();
    if(e==="grumpy"){g.fillStyle="#d9893a";g.fillRect(x-13,y-14,26,9);}});}
  g.fillStyle="#e87a8a";g.beginPath();g.moveTo(44,58);g.lineTo(52,58);g.lineTo(48,62);g.fill();
  g.strokeStyle="#3a2418";g.lineWidth=2.2;g.beginPath();g.arc(44,64,4,0,Math.PI);g.arc(52,64,4,0,Math.PI);g.stroke();
  if(e==="meow"){g.fillStyle="#7a2b2e";g.beginPath();g.ellipse(48,68,5,6,0,0,7);g.fill();}
  g.strokeStyle="rgba(255,255,255,.8)";g.lineWidth=1.2;[[18,60,4,57],[18,64,3,66],[78,60,92,57],[78,64,93,66]].forEach(([a,b,c2,d2])=>{g.beginPath();g.moveTo(a,b);g.lineTo(c2,d2);g.stroke();});
  const t=new T.CanvasTexture(c);t.encoding=T.sRGBEncoding;return t;
}
G.makeCat=function(){
  const root=new T.Group(),body=new T.Group();root.add(body);
  const fur=G.toon(0xe0923a,{rim:.5}),fur2=G.toon(0xf3d6a8,{rim:.4});
  const torso=part(capG(.12,.24),fur,body,[0,.2,0],[Math.PI/2,0,0],null,true);
  part(capG(.09,.14),fur2,body,[0,.16,.04],[Math.PI/2,0,0],[1,.9,1]);
  const head=new T.Group();head.position.set(0,.34,.2);body.add(head);
  part(sphG(.13,20,14),fur,head,[0,0,0],null,[1.08,.95,1],true);
  [-1,1].forEach(s=>part(cylG(0,.05,.1,4),fur,head,[s*.075,.12,-.01],[0,0,-s*.25],null,true));
  const faces={};const fm=new T.MeshToonMaterial({map:null,transparent:true,gradientMap:G.grad,depthWrite:false,polygonOffset:true,polygonOffsetFactor:-2});
  const f=new T.Mesh(new T.SphereGeometry(.132,22,14,Math.PI/2-.95,1.9,.8,1.25),fm);f.scale.set(1.08,.95,1);head.add(f);
  const bow=G.toon(0xd9343a);part(cylG(0,.05,.07,4),bow,body,[.05,.3,.13],[0,0,Math.PI/2]);part(cylG(0,.05,.07,4),bow,body,[-.05,.3,.13],[0,0,-Math.PI/2]);
  const legs=[[.06,.11],[-.06,.11],[.06,-.11],[-.06,-.11]].map(([x,z])=>{const p=new T.Group();p.position.set(x,.14,z);body.add(p);part(capG(.03,.09),fur,p,[0,-.07,0],null,null,true);return p;});
  const tail=[];let parent=body,pos=[0,.22,-.2];
  for(let i=0;i<4;i++){const p=new T.Group();p.position.set(...pos);parent.add(p);part(capG(.026,.07),fur,p,[0,.05,0],null,null,true);tail.push(p);parent=p;pos=[0,.1,0];}
  const c={root,displayName:"Bé Điện",voice:800,speed:0,phase:0,pose:"walk",expr:"open",isCat:true,headTop:.12,
    headPos(extra){head.getWorldPosition(_hp);_hp.y+=.16+(extra||0);return _hp;},
    setExpr(e){this.expr=e;fm.map=faces[e]||(faces[e]=catFace(e));fm.needsUpdate=true;},
    emote(e){if(e==="meow"){this.meowT=1;this.setExpr("meow");G.sfx("meow");}},
    setPose(p){this.pose=p;},
    update(dt,t){
      const sp=this.speed;if(sp>.1)this.phase+=dt*(8+sp*3);
      const a=Math.min(1,sp/1.5)*.6,s=Math.sin(this.phase);
      legs.forEach((l,i)=>l.rotation.x=(i%3===0?s:-s)*a);
      body.position.y=Math.abs(Math.cos(this.phase))*.02*a;
      tail.forEach((p,i)=>{p.rotation.x=-.5+Math.sin(t*2.2+i*.6)*.15*(i+1)*.5;p.rotation.z=Math.sin(t*1.6+i)*.12;});
      if(this.pose==="sleep"){body.scale.y=.82+Math.sin(t*1.5)*.025;head.rotation.x=.45;legs.forEach(l=>l.rotation.x=-1.4);if(this.expr!=="sleep")this.setExpr("sleep");}
      else if(this.pose==="sit"){body.scale.y=1;torso.rotation.x=Math.PI/2-.6;head.rotation.x=-.1;legs[2].rotation.x=-1.2;legs[3].rotation.x=-1.2;}
      else{body.scale.y=1;torso.rotation.x=Math.PI/2;head.rotation.x=Math.sin(t*1.3)*.05;}
      if(this.meowT>0){this.meowT-=dt;head.rotation.x-=.35;if(this.meowT<=0)this.setExpr(this.pose==="sleep"?"sleep":"open");}
      if(this.lookAt&&this.pose!=="sleep"){head.getWorldPosition(_v);const ang=Math.atan2(this.lookAt.x-_v.x,this.lookAt.z-_v.z)-root.rotation.y;head.rotation.y=damp(head.rotation.y,clamp(Math.atan2(Math.sin(ang),Math.cos(ang)),-.9,.9),5,dt);}
    }};
  c.setExpr("open");G.chars.push(c);return c;
};

/* ---------- the cast ---------- */
G.CAST={
  mai:{name:"Mai",player:true,skin:0xf3d5bd,hair:{style:"bob",color:0x3a2721},eyes:"#7a4a2e",top:{style:"cardigan",color:0x2a8c84,inner:0xf4efe2},bottom:{style:"pants",color:0x353a42},shoes:0x6a4a34,acc:["glasses","badge"],voice:560},
  linh:{name:"Linh",skin:0xefcfb5,hair:{style:"pony",color:0x1e1a1c},eyes:"#4a2f24",top:{style:"jacket",color:0x56728f,inner:0xe9e4d8},bottom:{style:"pants",color:0x343842},shoes:0x2e2a2a,acc:["satchel"],voice:610},
  vinh:{name:"Sếp Vinh",skin:0xe6c4a6,hair:{style:"salt",color:0x55504c},eyes:"#3a2b22",top:{style:"suit",color:0x5d6a4a,inner:0xf3f0e6,accent:0x8a2f2f},bottom:{style:"pants",color:0x3f4636},shoes:0x2a2420,acc:["mustache","cup"],voice:380,scale:1.05},
  khoa:{name:"Khoa",skin:0xf0cfb2,hair:{style:"spiky",color:0x2a2220},eyes:"#4a3326",top:{style:"hoodie",color:0xd9734a},bottom:{style:"pants",color:0x44506a},shoes:0xeeeeee,acc:["headphones","cup"],voice:470},
  hang:{name:"Chị Hằng",skin:0xf1d2b8,hair:{style:"bun",color:0x2b2120},eyes:"#5a3a26",top:{style:"cardigan",color:0xc9879a,inner:0xf7f1e7},bottom:{style:"skirt",color:0x4b4f63},shoes:0x6a3a3a,acc:["roundglasses","clipboard"],voice:650},
  an:{name:"An",kid:true,skin:0xe9c29d,hair:{style:"highpony",color:0x2a1f1b},eyes:"#6a4226",top:{style:"overalls",color:0x3f6f8f,inner:0xf0c75a},bottom:{style:"pants",color:0x3f6f8f},shoes:0x8a4a2e,acc:["oil"],voice:740},
  tin:{name:"Tín",kid:true,skin:0xeac6a2,hair:{style:"short",color:0x231b18},eyes:"#4a3326",top:{style:"tee",color:0xe0b84a,inner:0x3a7a5a},bottom:{style:"shorts",color:0x4a5a7a},shoes:0x3a3a3a,acc:["cap"],voice:700},
  be:{name:"Bé Tí",kid:true,scale:.92,skin:0xe9c29d,hair:{style:"short",color:0x2a201c},eyes:"#4a3326",top:{style:"tee",color:0x7ab0d0},bottom:{style:"shorts",color:0x5a4a3a},shoes:0x6a5a4a,voice:780},
  dung:{name:"Bà Dung",elder:true,skin:0xe6c3a5,hair:{style:"bun",color:0xbdb8b0},eyes:"#4a3326",top:{style:"blouse",color:0x8f7a9a,inner:0xe8dccf},bottom:{style:"pants",color:0x4a4048},shoes:0x3a2a2a,acc:["shawl"],voice:430},
  ha:{name:"Bà Hạ",skin:0xdcb592,hair:{style:"scarf",color:0x2a201c,cloth:0x9a5a3a},eyes:"#3a2a22",top:{style:"coverall",color:0x67705a},bottom:{style:"pants",color:0x5a5f4c},shoes:0x2a2622,acc:["bandage"],voice:480},
  chao:{name:"Bà bán cháo",elder:true,skin:0xe0b994,hair:{style:"bun",color:0x4a3f3a},eyes:"#3a2a22",top:{style:"apron",color:0x6a8a7a,inner:0xf1ebdd},bottom:{style:"pants",color:0x3a3a40},shoes:0x2a2622,acc:["nonla"],voice:500},
  onglao:{name:"Ông lão",elder:true,skin:0xdcb592,hair:{style:"short",color:0xd8d4cc},eyes:"#3a2a22",top:{style:"tee",color:0x8a8f7a},bottom:{style:"pants",color:0x4a4a52},shoes:0x2a2622,acc:["mustacheW"],voice:360},
  quandoc:{name:"Quản đốc",skin:0xd8b08c,hair:{style:"short",color:0x2a2420},eyes:"#3a2a22",top:{style:"tunic",color:0x3e6a8a},bottom:{style:"pants",color:0x2f3a48},shoes:0x2a2420,acc:["hardhat"],voice:420},
  sau:{name:"Chú Sáu",skin:0xd8b08c,hair:{style:"short",color:0x2a2420},eyes:"#3a2a22",top:{style:"tee",color:0x9a6a4a},bottom:{style:"shorts",color:0x3a4a5a},shoes:0x2a2420,voice:400},
  cuc:{name:"Cô Cúc",skin:0xe8c4a2,hair:{style:"pony",color:0x2a201c},eyes:"#4a3326",top:{style:"blouse",color:0x7aa08a,inner:0xf0e8da},bottom:{style:"pants",color:0x3a3a48},shoes:0x4a3a30,voice:600},
  com:{name:"Bà Sáu cơm",elder:true,skin:0xe0b994,hair:{style:"bun",color:0xcfcac2},eyes:"#3a2a22",top:{style:"apron",color:0xa06a5a,inner:0xf1ebdd},bottom:{style:"pants",color:0x3a3a40},shoes:0x2a2622,voice:470},
};
})();
