/* CIVI 3D · engine
   Renderer and toon look, input (keyboard, mouse, touch), third-person camera, movement and
   collision, NPCs, interaction prompts, speech bubbles, choices, objectives, the Debug Lens,
   particles, synthesized sound, save slot. Everything hangs off window.G. */
(function(){
"use strict";
if(!window.THREE){window.G=null;return;}
const T=window.THREE,G=window.G={T};
// hex colours are sRGB: convert them to linear so lighting and the sRGB output match what was picked
if(T.ColorManagement)T.ColorManagement.legacyMode=false;
G.displayColor=h=>new T.Color().setHex(h,T.LinearSRGBColorSpace||"srgb-linear");
const $=G.$=s=>document.querySelector(s);
const clamp=G.clamp=(x,a,b)=>Math.max(a,Math.min(b,x));
const damp=G.damp=(a,b,l,dt)=>a+(b-a)*(1-Math.exp(-l*dt));
const wrap=a=>{while(a>Math.PI)a-=Math.PI*2;while(a<-Math.PI)a+=Math.PI*2;return a;};
G.angDamp=(a,b,l,dt)=>a+wrap(b-a)*(1-Math.exp(-l*dt));
G.wait=ms=>new Promise(r=>setTimeout(r,ms));
G.ease=t=>t<.5?2*t*t:1-Math.pow(-2*t+2,2)/2;
G.touch=matchMedia("(pointer:coarse)").matches;
// light graphics: phones, narrow windows, or the player's choice (saved), or ?lq=1 for testing
G.lite=(()=>{try{return localStorage.getItem("civi3d-lite")==="1";}catch(e){return false;}})()||/[?&]lq=1/.test(location.search);
G.mobile=G.touch||innerWidth<760||G.lite;
G.reduce=matchMedia("(prefers-reduced-motion: reduce)").matches;
G.time=0;G.npcs=[];G.chars=[];G.updaters=new Set();G.interactables=[];G.flags={};
G.stats={seen:[],asked:[],declined:[],promises:[],log:[]};
G.log=(k,v)=>{G.stats.log.push({k,v,t:Math.round(G.time)});};

/* ---------- renderer ---------- */
G.init=function(){
  const canvas=$("#gl");let r;
  try{r=new T.WebGLRenderer({canvas,antialias:!G.mobile,powerPreference:"high-performance"});}catch(e){return false;}
  if(!r.getContext())return false;
  G.renderer=r;
  r.setPixelRatio(Math.min(devicePixelRatio||1,G.mobile?1.5:2));
  r.outputEncoding=T.sRGBEncoding;r.toneMapping=T.ACESFilmicToneMapping;r.toneMappingExposure=1;
  r.shadowMap.enabled=!G.mobile;r.shadowMap.type=T.PCFSoftShadowMap;
  G.scene=new T.Scene();G.cam=new T.PerspectiveCamera(50,1,.1,700);G.scene.add(G.cam);
  const d=new Uint8Array([105,105,105,255,185,185,185,255,255,255,255,255]);
  G.grad=new T.DataTexture(d,3,1,T.RGBAFormat);G.grad.minFilter=G.grad.magFilter=T.NearestFilter;G.grad.needsUpdate=true;
  const resize=()=>{r.setSize(innerWidth,innerHeight,false);G.cam.aspect=innerWidth/innerHeight;G.cam.updateProjectionMatrix();};
  addEventListener("resize",resize);resize();
  initInput();G.fx.init();
  return true;
};

/* toon material with an optional rim light (anime style edge glow) */
const matCache={};
G.toon=function(color,o){
  o=o||{};
  const key=[color,o.rim,o.emissive,o.ei,o.transparent,o.opacity,o.side,o.flat,o.vc].join("|");
  if(!o.map&&!o.noCache&&matCache[key])return matCache[key];
  const m=new T.MeshToonMaterial({color,gradientMap:G.grad});
  if(o.emissive!=null){m.emissive=new T.Color(o.emissive);m.emissiveIntensity=o.ei==null?1:o.ei;}
  if(o.map)m.map=o.map;
  if(o.vc)m.vertexColors=true;
  if(o.transparent){m.transparent=true;m.opacity=o.opacity==null?1:o.opacity;m.depthWrite=!!o.depthWrite;}
  if(o.side)m.side=o.side;
  if(o.flat)m.flatShading=true;
  const rim=o.rim||0;
  if(rim>0){
    const rc=new T.Color(o.rimColor||0xfff1dc);
    m.onBeforeCompile=sh=>{
      sh.uniforms.rimC={value:rc};sh.uniforms.rimS={value:rim};
      sh.fragmentShader="uniform vec3 rimC;uniform float rimS;\n"+sh.fragmentShader.replace("#include <output_fragment>",
        "{float rf=1.0-clamp(dot(normalize(normal),normalize(vViewPosition)),0.0,1.0);outgoingLight+=rimC*smoothstep(.58,1.0,rf)*rimS;}\n#include <output_fragment>");
    };
    m.customProgramCacheKey=()=>"rim";
  }
  if(!o.map&&!o.noCache)matCache[key]=m;
  return m;
};
G.basic=(color,o)=>new T.MeshBasicMaterial(Object.assign({color,toneMapped:false},o||{}));
/* inverted-hull outline, roughly constant width on screen */
const olCache={};
G.olMat=w=>olCache[w]||(olCache[w]=new T.ShaderMaterial({uniforms:{w:{value:w},c:{value:new T.Color(0x2a2030)}},side:T.BackSide,
  vertexShader:"uniform float w;void main(){vec4 mv=modelViewMatrix*vec4(position,1.);vec3 n=normalize(normalMatrix*normal);mv.xyz+=n*w*(-mv.z*.055+.35);gl_Position=projectionMatrix*mv;}",
  fragmentShader:"uniform vec3 c;void main(){gl_FragColor=vec4(c,1.);\n#include <encodings_fragment>\n}"}));
G.outline=(mesh,w,force)=>{if(G.mobile&&!force)return null;const o=new T.Mesh(mesh.geometry,G.olMat(w||.02));o.castShadow=false;mesh.add(o);return o;};
G.textTex=function(lines,o){
  o=o||{};const w=o.w||512,h=o.h||128,c=document.createElement("canvas");c.width=w;c.height=h;const g=c.getContext("2d");
  g.fillStyle=o.bg||"#20302c";g.fillRect(0,0,w,h);
  if(o.border){g.strokeStyle=o.border;g.lineWidth=6;g.strokeRect(6,6,w-12,h-12);}
  g.fillStyle=o.fg||"#f3ede0";g.textAlign="center";g.textBaseline="middle";
  const ls=Array.isArray(lines)?lines:[lines],fs=o.fs||Math.round(h/(ls.length+1.2));
  g.font=`${o.weight||800} ${fs}px ${o.font||'"Baloo 2","Be Vietnam Pro",sans-serif'}`;
  ls.forEach((l,i)=>g.fillText(l,w/2,h/2+(i-(ls.length-1)/2)*fs*1.15));
  const t=new T.CanvasTexture(c);t.encoding=T.sRGBEncoding;t.anisotropy=4;return t;
};

/* ---------- main loop ---------- */
G.start=function(){
  let last=performance.now();
  const tick=now=>{
    requestAnimationFrame(tick);
    const dt=Math.min(.1,(now-last)/1000);last=now;
    if(G.paused){G.renderer.render(G.scene,G.cam);return;}
    G.time+=dt;
    pollInput(dt);
    if(G.world&&G.world.update)G.world.update(dt,G.time);
    updatePlayer(dt);
    G.npcs.forEach(n=>updateNPC(n,dt));
    G.chars.forEach(c=>c.update(dt,G.time));
    G.updaters.forEach(f=>f(dt,G.time));
    updateCamera(dt);
    G.fx.update(dt);
    G.renderer.render(G.scene,G.cam);
    updateHUD(dt);
    I.jump=I.act=I.lens=I.esc=false;I.lookX=I.lookY=0;I.click=null;
  };
  requestAnimationFrame(tick);
};

/* ---------- input ---------- */
const I=G.input={mx:0,my:0,run:false,lookX:0,lookY:0,jump:false,act:false,lens:false,esc:false,keys:{},click:null,used:false};
const joy={id:null,ox:0,oy:0,x:0,y:0},look={id:null,x:0,y:0};
function initInput(){
  addEventListener("keydown",e=>{
    const tg=e.target&&e.target.tagName;if(tg==="INPUT"||tg==="TEXTAREA")return;
    I.keys[e.code]=true;I.used=true;G.audio.unlock();
    // a key that advances dialogue is consumed there, so it cannot also start a new interaction
    if(G._advance&&(e.code==="Space"||e.code==="Enter"||e.code==="KeyE"||e.code==="NumpadEnter")){e.preventDefault();G._advance();return;}
    if(e.code==="Space"){I.jump=true;e.preventDefault();}
    if(e.code==="KeyE"||e.code==="Enter"||e.code==="NumpadEnter")I.act=true;
    if(e.code==="KeyQ")I.lens=true;
    if(e.code==="Escape"||e.code==="KeyP")I.esc=true;
    if(/^Arrow/.test(e.code))e.preventDefault();
    if(G._choiceKey&&/^Digit[1-4]$/.test(e.code))G._choiceKey(+e.code.slice(5)-1);
  });
  addEventListener("keyup",e=>{I.keys[e.code]=false;});
  addEventListener("blur",()=>{I.keys={};});
  const cv=$("#gl");let drag=null,moved=0;
  cv.addEventListener("pointerdown",e=>{G.audio.unlock();if(e.pointerType==="touch")return;drag={x:e.clientX,y:e.clientY};moved=0;try{cv.setPointerCapture(e.pointerId);}catch(x){}});
  cv.addEventListener("pointermove",e=>{if(!drag)return;const dx=e.clientX-drag.x,dy=e.clientY-drag.y;moved+=Math.abs(dx)+Math.abs(dy);I.lookX+=dx*.0055;I.lookY+=dy*.004;drag.x=e.clientX;drag.y=e.clientY;});
  cv.addEventListener("pointerup",e=>{if(drag&&moved<6)I.click={x:e.clientX,y:e.clientY};drag=null;if(G._advance&&moved<6)G._advance();});
  cv.addEventListener("wheel",e=>{C.zoom=clamp(C.zoom+e.deltaY*.0015,.55,1.5);},{passive:true});
  // touch: left half is a floating joystick, right half turns the camera, a tap advances dialogue
  const tl=$("#touch");
  tl.addEventListener("touchstart",e=>{
    G.audio.unlock();I.used=true;
    for(const t of e.changedTouches){
      if(t.clientX<innerWidth*.45&&joy.id==null){joy.id=t.identifier;joy.ox=t.clientX;joy.oy=t.clientY;joy.x=joy.y=0;joy.t=performance.now();joy.max=0;if(!G._advance)showJoy(t.clientX,t.clientY);}
      else if(look.id==null){look.id=t.identifier;look.x=t.clientX;look.y=t.clientY;look.sx=t.clientX;look.sy=t.clientY;look.t=performance.now();}
    }
    e.preventDefault();
  },{passive:false});
  tl.addEventListener("touchmove",e=>{
    for(const t of e.changedTouches){
      if(t.identifier===joy.id){let dx=t.clientX-joy.ox,dy=t.clientY-joy.oy;const d=Math.hypot(dx,dy),R=52;joy.max=Math.max(joy.max,d);if(d>10&&$("#joy").hidden)showJoy(joy.ox,joy.oy);if(d>R){dx*=R/d;dy*=R/d;}joy.x=dx/R;joy.y=dy/R;$("#joy i").style.transform=`translate(${dx}px,${dy}px)`;}
      else if(t.identifier===look.id){I.lookX+=(t.clientX-look.x)*.008;I.lookY+=(t.clientY-look.y)*.006;look.x=t.clientX;look.y=t.clientY;}
    }
    e.preventDefault();
  },{passive:false});
  const end=e=>{for(const t of e.changedTouches){
    if(t.identifier===joy.id){joy.id=null;joy.x=joy.y=0;$("#joy").hidden=true;if(joy.max<12&&performance.now()-joy.t<350){I.click={x:t.clientX,y:t.clientY};if(G._advance)G._advance();}}
    if(t.identifier===look.id){if(Math.hypot(t.clientX-look.sx,t.clientY-look.sy)<12&&performance.now()-look.t<350){I.click={x:t.clientX,y:t.clientY};if(G._advance)G._advance();}look.id=null;}
  }};
  tl.addEventListener("touchend",end);tl.addEventListener("touchcancel",end);
  const hold=(id,fn)=>{const b=$(id);if(b)b.addEventListener("pointerdown",e=>{e.preventDefault();G.audio.unlock();fn();});};
  hold("#b-act",()=>{if(G._advance)G._advance();else I.act=true;});
  hold("#prompt",()=>{I.act=true;});
  hold("#b-jump",()=>{I.jump=true;});
  hold("#b-lens",()=>{I.lens=true;});
  hold("#b-run",()=>{I.runToggle=!I.runToggle;$("#b-run").classList.toggle("on",I.runToggle);});
}
function showJoy(x,y){const j=$("#joy");j.hidden=false;j.style.left=x+"px";j.style.top=y+"px";$("#joy i").style.transform="";}
function pollInput(){
  const k=I.keys;
  let x=(k.KeyD||k.ArrowRight?1:0)-(k.KeyA||k.ArrowLeft?1:0)+joy.x;
  let y=(k.KeyW||k.ArrowUp?1:0)-(k.KeyS||k.ArrowDown?1:0)-joy.y;
  const l=Math.hypot(x,y);if(l>1){x/=l;y/=l;}
  I.mx=x;I.my=y;
  I.run=!!(k.ShiftLeft||k.ShiftRight)||I.runToggle||Math.hypot(joy.x,joy.y)>.92;
  if(I.esc)G.togglePause();
  if(I.lens)G.lens.toggle();
}

/* ---------- camera ---------- */
const C=G.camCtl={yaw:Math.PI,pitch:.36,dist:6.4,zoom:1,shot:null,ret:null,idle:0,_d:6.4,_look:new T.Vector3(),shake:0};
const _off=new T.Vector3(),_tgt=new T.Vector3(),_want=new T.Vector3();
function rayBoxes(o,dir,maxD){
  let best=maxD;const w=G.world;if(!w)return best;
  for(const b of w.boxes){
    if(b.noCam||(b.h||9)<o.y-.6)continue;
    let t0=0,t1=best;
    const ax=[[o.x,dir.x,b.x0,b.x1],[o.y,dir.y,0,b.h||9],[o.z,dir.z,b.z0,b.z1]];
    let hit=true;
    for(const [p,d,lo,hi] of ax){
      if(Math.abs(d)<1e-6){if(p<lo||p>hi){hit=false;break;}continue;}
      let a=(lo-p)/d,c=(hi-p)/d;if(a>c){const s=a;a=c;c=s;}
      t0=Math.max(t0,a);t1=Math.min(t1,c);if(t0>t1){hit=false;break;}
    }
    if(hit&&t0>.05&&t0<best)best=t0;
  }
  return best;
}
G.portrait=()=>innerWidth<innerHeight*.85;
G.shot=(pos,look,speed,exact)=>{
  // portrait screens see a narrow slice horizontally: pull scripted shots back so both people fit
  if(!exact&&G.portrait())pos=look.clone().addScaledVector(pos.clone().sub(look),1.55);
  C.shot={pos:pos.clone(),look:look.clone(),speed:speed||3.2};if(G.reduce){G.cam.position.copy(pos);C._look.copy(look);}};
G.endShot=()=>{if(!C.shot)return;C.ret={pos:G.cam.position.clone(),look:C._look.clone(),t:0};C.shot=null;};
G.talkCam=function(a,b){
  // two-shot framing for a conversation: camera to the side of the line between the speakers,
  // on whichever side has a clear view (walls and houses cut the shot short)
  const pa=a.pos||a,pb=b.pos||b;
  const mid=new T.Vector3((pa.x+pb.x)/2,1.15,(pa.z+pb.z)/2);
  const dx=pb.x-pa.x,dz=pb.z-pa.z,d=Math.hypot(dx,dz)||1,want=(2.6+d*.9)*(G.portrait()?1.5:1),cp=G.cam.position,B=G.world&&G.world.bounds;
  let best=null;
  for(const sd of [1,-1]){
    const nx=-dz/d*sd,nz=dx/d*sd,dir=new T.Vector3(nx*want-dx/d*.6,.6,nz*want-dz/d*.6),L=dir.length();dir.divideScalar(L);
    let free=Math.min(L,rayBoxes(mid,dir,L)-.3);
    if(B){const ex=mid.x+dir.x*free,ez=mid.z+dir.z*free;if(ex<B[0]||ex>B[1]||ez<B[2]||ez>B[3])free=Math.min(free,1.2);}
    const score=free+(((cp.x-mid.x)*nx+(cp.z-mid.z)*nz)>=0?.4:0);
    if(!best||score>best.score)best={score,dir,free};
  }
  const pos=mid.clone().addScaledVector(best.dir,Math.max(1.5,best.free));
  if(B){pos.x=clamp(pos.x,B[0]+.25,B[1]-.25);pos.z=clamp(pos.z,B[2]+.25,B[3]-.25);}
  G.shot(pos,mid,3,true);
};
function updateCamera(dt){
  const cam=G.cam,P=G.player;
  if(C.shot){const f=1-Math.exp(-dt*C.shot.speed);cam.position.lerp(C.shot.pos,f);C._look.lerp(C.shot.look,f);cam.lookAt(C._look);return;}
  if(!P.char)return;
  // some places use a fixed rig instead of the orbit camera (the cutaway room)
  const rig=G.world&&G.world.camRig;
  if(rig){const r=rig(P);const f=1-Math.exp(-dt*3);cam.position.lerp(r.pos,f);C._look.lerp(r.look,f);cam.lookAt(C._look);C.ret=null;return;}
  const pr=(G.world&&G.world.pitch)||[-.08,1.1];
  C.yaw-=I.lookX;C.pitch=clamp(C.pitch+I.lookY,pr[0],pr[1]);
  if(Math.abs(I.lookX)+Math.abs(I.lookY)>0)C.idle=0;else C.idle+=dt;
  if(P.speed>1.2&&C.idle>1.4&&!G.talking)C.yaw=G.angDamp(C.yaw,P.yaw+Math.PI,.8,dt);
  _tgt.copy(P.pos);_tgt.y+=1.2;
  const d=C.dist*C.zoom*((G.world&&G.world.camDist)||1);
  _off.set(Math.sin(C.yaw)*Math.cos(C.pitch),Math.sin(C.pitch),Math.cos(C.yaw)*Math.cos(C.pitch));
  const hitD=Math.max(1.1,rayBoxes(_tgt,_off,d)-.25);
  C._d=damp(C._d,hitD,hitD<C._d?18:3,dt);
  _want.copy(_tgt).addScaledVector(_off,C._d);
  // a wall behind the player pushes the camera in; lift it so it looks over her head instead
  const squeeze=clamp(1-C._d/Math.max(d,.01),0,1);_want.y+=squeeze*squeeze*1.7;
  const cc=G.world&&G.world.camClamp;if(cc){_want.x=clamp(_want.x,cc[0],cc[1]);_want.z=clamp(_want.z,cc[2],cc[3]);}
  if(C.shake>0){C.shake=Math.max(0,C.shake-dt);_want.x+=(Math.random()-.5)*C.shake*.3;_want.y+=(Math.random()-.5)*C.shake*.3;}
  if(C.ret){C.ret.t=Math.min(1,C.ret.t+dt*1.8);const e=G.ease(C.ret.t);cam.position.lerpVectors(C.ret.pos,_want,e);C._look.lerpVectors(C.ret.look,_tgt,e);if(C.ret.t>=1)C.ret=null;}
  else{cam.position.copy(_want);C._look.copy(_tgt);}
  cam.lookAt(C._look);
}

/* ---------- player ---------- */
const P=G.player={pos:new T.Vector3(),vel:new T.Vector2(),vy:0,yaw:0,speed:0,char:null,locked:false,ground:true,r:.32,stepT:0};
G.placePlayer=(x,z,yaw)=>{P.pos.set(x,G.world?G.world.groundH(x,z):0,z);P.yaw=yaw||0;P.vel.set(0,0);C.yaw=P.yaw+Math.PI;C._d=C.dist;if(P.char){P.char.root.position.copy(P.pos);P.char.root.rotation.y=P.yaw;}};
function updatePlayer(dt){
  if(!P.char)return;
  const free=!P.locked&&!G.talking;
  const mx=free?I.mx:0,my=free?I.my:0,len=Math.min(1,Math.hypot(mx,my));
  const fx=-Math.sin(C.yaw),fz=-Math.cos(C.yaw),rx=Math.cos(C.yaw),rz=-Math.sin(C.yaw);
  let dx=fx*my+rx*mx,dz=fz*my+rz*mx;const dl=Math.hypot(dx,dz)||1;dx/=dl;dz/=dl;
  const w=G.world,slow=w&&w.speedMul?w.speedMul(P.pos.x,P.pos.z):1;
  const maxS=(I.run?6.1:3.3)*slow;
  P.vel.x=damp(P.vel.x,dx*len*maxS,len>0?12:16,dt);P.vel.y=damp(P.vel.y,dz*len*maxS,len>0?12:16,dt);
  if(P.auto){const a=P.auto,ddx=a.x-P.pos.x,ddz=a.z-P.pos.z,d=Math.hypot(ddx,ddz);if(d<.2){P.auto=null;a.res&&a.res();}else{P.vel.x=ddx/d*(a.speed||2.6);P.vel.y=ddz/d*(a.speed||2.6);dx=ddx/d;dz=ddz/d;}}
  P.pos.x+=P.vel.x*dt;P.pos.z+=P.vel.y*dt;
  G.collide(P.pos,P.r);
  P.speed=Math.hypot(P.vel.x,P.vel.y);
  if((len>.05||P.auto)&&P.speed>.2)P.yaw=G.angDamp(P.yaw,Math.atan2(dx,dz),13,dt);
  const gh=w?w.groundH(P.pos.x,P.pos.z,P.pos.y):0;
  if(I.jump&&P.ground&&free){P.vy=6.3;P.ground=false;G.sfx("jump");}
  P.vy-=21*dt;P.pos.y+=P.vy*dt;
  if(P.pos.y<=gh){if(!P.ground&&P.vy<-4){G.sfx("land");G.fx.dust(P.pos);}P.pos.y=gh;P.vy=0;P.ground=true;}
  else if(P.pos.y>gh+.08&&P.vy<0)P.ground=false;
  if(w&&w.onPlayer)w.onPlayer(P,dt);
  P.char.root.position.copy(P.pos);P.char.root.rotation.y=P.yaw;P.char.speed=P.speed;P.char.air=!P.ground;
  if(P.ground&&P.speed>1){P.stepT-=dt*P.speed;if(P.stepT<0){P.stepT=1.25;G.sfx(w&&w.wet?"stepWet":"step");}}
}
G.walkTo=(x,z,speed)=>new Promise(res=>{
  const a={x,z,speed,res},d=Math.hypot(x-P.pos.x,z-P.pos.z);P.auto=a;
  setTimeout(()=>{if(P.auto===a){P.auto=null;P.pos.x=x;P.pos.z=z;res();}},(d/(speed||2.6)+1.5)*1000);
});
G.collide=function(p,r){
  const w=G.world;if(!w)return;
  for(let it=0;it<2;it++){
    for(const b of w.boxes){
      if(b.walkTop&&p.y>=b.h-.05)continue;
      const cx=clamp(p.x,b.x0,b.x1),cz=clamp(p.z,b.z0,b.z1),dx=p.x-cx,dz=p.z-cz,d2=dx*dx+dz*dz;
      if(d2>=r*r)continue;
      if(d2>1e-7){const d=Math.sqrt(d2);p.x=cx+dx/d*r;p.z=cz+dz/d*r;}
      else{const l=p.x-b.x0,rr=b.x1-p.x,u=p.z-b.z0,dd=b.z1-p.z,m=Math.min(l,rr,u,dd);
        if(m===l)p.x=b.x0-r;else if(m===rr)p.x=b.x1+r;else if(m===u)p.z=b.z0-r;else p.z=b.z1+r;}
    }
    for(const c of w.circles){const dx=p.x-c.x,dz=p.z-c.z,rr=r+c.r,d2=dx*dx+dz*dz;if(d2<rr*rr&&d2>1e-7){const d=Math.sqrt(d2);p.x=c.x+dx/d*rr;p.z=c.z+dz/d*rr;}}
    for(const n of G.npcs){if(n.solid===false||n.hidden)continue;const dx=p.x-n.pos.x,dz=p.z-n.pos.z,rr=r+.28,d2=dx*dx+dz*dz;if(d2<rr*rr&&d2>1e-7){const d=Math.sqrt(d2);p.x=n.pos.x+dx/d*rr;p.z=n.pos.z+dz/d*rr;}}
    if(w.bounds){p.x=clamp(p.x,w.bounds[0]+r,w.bounds[1]-r);p.z=clamp(p.z,w.bounds[2]+r,w.bounds[3]-r);}
  }
};

/* ---------- NPCs ---------- */
G.addNPC=function(o){
  const n=Object.assign({yaw:0,speed:0,look:true,barkT:4+Math.random()*5,r:1.8},o);
  n.pos=new T.Vector3(o.pos[0],0,o.pos[1]);
  n.pos.y=G.world?G.world.groundH(n.pos.x,n.pos.z):0;
  n.char.root.position.copy(n.pos);n.char.root.rotation.y=n.yaw;
  if(!n.char.root.parent)G.scene.add(n.char.root);
  G.npcs.push(n);return n;
};
G.removeNPC=n=>{const i=G.npcs.indexOf(n);if(i>=0)G.npcs.splice(i,1);if(n.char.root.parent)n.char.root.parent.remove(n.char.root);n.char.dispose&&n.char.dispose();};
G.moveNPC=(n,pts,speed)=>new Promise(res=>{n.path=pts.map(p=>new T.Vector3(p[0],0,p[1]));n.walkSpeed=speed||2.4;n.onArrive=res;});
function updateNPC(n,dt){
  if(n.hidden){n.char.root.visible=false;return;}
  n.char.root.visible=true;
  if(n.path&&n.path.length){
    const t=n.path[0],dx=t.x-n.pos.x,dz=t.z-n.pos.z,d=Math.hypot(dx,dz);
    if(d<.2){n.path.shift();if(!n.path.length){const f=n.onArrive;n.onArrive=null;f&&f();}}
    else{const sp=n.walkSpeed;n.pos.x+=dx/d*Math.min(sp*dt,d);n.pos.z+=dz/d*Math.min(sp*dt,d);n.yaw=G.angDamp(n.yaw,Math.atan2(dx,dz),10,dt);n.speed=damp(n.speed,sp,10,dt);}
  }else n.speed=damp(n.speed,0,10,dt);
  if(n.wander&&!n.path&&!G.talking){n.wT=(n.wT||Math.random()*4)-dt;if(n.wT<0){n.wT=4+Math.random()*6;const h=n.wander;const a=Math.random()*Math.PI*2,r=Math.random()*h.r;G.moveNPC(n,[[h.x+Math.cos(a)*r,h.z+Math.sin(a)*r]],1.1);}}
  const p=P.pos,dp=Math.hypot(p.x-n.pos.x,p.z-n.pos.z);
  n.char.lookAt=n.lookTarget||(n.look&&dp<4.5&&P.char?P.char.headPos():null);
  if(n.face){const want=Math.atan2(n.face.x-n.pos.x,n.face.z-n.pos.z);n.yaw=G.angDamp(n.yaw,want,6,dt);}
  n.barkT-=dt;
  if(n.barks&&n.barks.length&&n.barkT<0&&dp<5.5&&!G.talking&&!n.path&&!n.busy){n.barkT=10+Math.random()*8;n.barkI=((n.barkI==null?-1:n.barkI)+1)%n.barks.length;G.bark(n,n.barks[n.barkI]);}
  n.pos.y=n.fixedY!=null?n.fixedY:G.world?G.world.groundH(n.pos.x,n.pos.z,n.pos.y):0;
  n.char.root.position.copy(n.pos);n.char.root.rotation.y=n.yaw;n.char.speed=n.speed;
}

/* ---------- interaction ---------- */
G.addInteract=o=>{G.interactables.push(o);return o;};
G.removeInteract=o=>{const i=G.interactables.indexOf(o);if(i>=0)G.interactables.splice(i,1);};
function nearest(){
  let best=null,bd=1e9;
  const list=G.interactables.concat(G.npcs.filter(n=>n.talk&&!n.hidden));
  for(const it of list){
    if(it.enabled&&!it.enabled())continue;
    const p=it.pos;const d=Math.hypot(p.x-P.pos.x,p.z-P.pos.z);
    if(d<(it.r||1.8)&&d<bd){bd=d;best=it;}
  }
  return best;
}
// scripted sequences nest: an interaction's run may still be unwinding when the next one starts,
// so only the outermost run hands control back to the player
let runDepth=0;
G.run=async function(fn){runDepth++;G.busy=true;P.locked=true;try{await fn();}catch(e){console.warn("CIVI:",e&&e.message);}finally{runDepth=Math.max(0,runDepth-1);if(!runDepth){G.busy=false;P.locked=false;G.endShot();}}};

/* ---------- HUD: bubbles, prompt, tags, waypoint ---------- */
const bubbles=[];const _p=new T.Vector3();
G.toScreen=(v,out)=>{_p.copy(v).project(G.cam);out=out||{};out.x=(_p.x*.5+.5)*innerWidth;out.y=(-_p.y*.5+.5)*innerHeight;out.vis=_p.z<1;return out;};
function bubbleEl(name,text,kind){
  const b=document.createElement("div");b.className="bub "+(kind||"");
  b.innerHTML=(name?`<b></b>`:"")+`<p></p>`;if(name)b.firstChild.textContent=name;
  $("#bubbles").appendChild(b);return b;
}
G.bark=function(n,text){
  const ch=n.char||n;if(bubbles.some(e=>e.ch===ch&&e.bark&&e.text===text&&G.time<e.until))return;
  const b=bubbleEl(ch.displayName,text,"bark"+(ch.isDrone?" civi":""));b.querySelector("p").textContent=text;
  const e={ch,b,until:G.time+2.6+text.length*.03,bark:true,text};bubbles.push(e);
};
G.say=function(who,text,o){
  o=o||{};
  const ch=who==="mai"?P.char:(who.char||who);
  return new Promise(res=>{
    G.talking=true;ch.talking=true;
    if(o.expr)ch.setExpr(o.expr);if(o.emote)ch.emote(o.emote);
    const b=bubbleEl(o.name||ch.displayName,text,o.kind||(ch.isPlayer?"me":ch.isDrone?"civi":""));
    const p=b.querySelector("p");let shown=0,done=false;
    const e={ch,b,until:1e9};bubbles.push(e);
    const full=text,rate=G.reduce?1e4:42;
    const typer=()=>{if(done)return;shown=Math.min(full.length,shown+Math.max(1,Math.round(rate*.03)));p.textContent=full.slice(0,shown);if(shown<full.length){G.sfx("blip",ch.voice);e.t=setTimeout(typer,30);}else{p.textContent=full;b.classList.add("done");}};
    typer();
    const finish=()=>{done=true;clearTimeout(e.t);G._advance=null;ch.talking=false;e.until=0;
      if(o.expr&&!o.keep)ch.setExpr("neutral");G.talking=false;res();};
    setTimeout(()=>{G._advance=()=>{if(shown<full.length){shown=full.length;p.textContent=full;b.classList.add("done");return;}finish();};},120);
    if(o.auto)setTimeout(()=>{if(!done)finish();},o.auto);
  });
};
G.choose=function(opts){
  return new Promise(res=>{
    G.talking=true;
    const box=$("#choices");box.innerHTML="";box.hidden=false;
    const pick=i=>{if(i<0||i>=opts.length)return;G._choiceKey=null;box.hidden=true;box.innerHTML="";G.sfx("ok");G.talking=false;res(i);};
    opts.forEach((o,i)=>{const b=document.createElement("button");b.className="ch "+(o.kind||"");b.innerHTML=`<kbd>${i+1}</kbd><span></span>`;b.querySelector("span").textContent=o.t;b.onclick=()=>pick(i);box.appendChild(b);});
    G._choiceKey=pick;
    setTimeout(()=>{try{box.firstChild.focus({preventScroll:true});}catch(e){}},50);
  });
};
G.objective=function(text,target,o){
  o=o||{};G.obj=text?{text,target:target||null,r:o.r||2}:null;
  const el=$("#obj");el.hidden=!text;
  if(text){$("#obj-t").textContent=text;el.classList.remove("new");void el.offsetWidth;el.classList.add("new");G.sfx("obj");}
  G.beacon.visible=!!(text&&target&&!o.noBeacon);
  if(target)G.beacon.position.set(target.x,G.world?G.world.groundH(target.x,target.z):0,target.z);
};
G.waitReach=(pos,r)=>new Promise(res=>{const f=()=>{if(Math.hypot(P.pos.x-pos.x,P.pos.z-pos.z)<(r||1.6)&&!G.talking){G.updaters.delete(f);res();}};G.updaters.add(f);});
G.waitFor=cond=>new Promise(res=>{const f=()=>{if(cond()){G.updaters.delete(f);res();}};G.updaters.add(f);});
G.waitInteract=function(pos,label,r){
  return new Promise(res=>{const it=G.addInteract({pos,label,r:r||1.8,fn:async()=>{G.removeInteract(it);res();}});});
};
G.toast=function(html,kind){
  const t=document.createElement("div");t.className="toast "+(kind||"");t.innerHTML=html;$("#toasts").appendChild(t);
  setTimeout(()=>t.classList.add("out"),3200);setTimeout(()=>t.remove(),3800);
};
G.hint=function(text,ms){const h=$("#hint");if(!text){h.hidden=true;return;}h.textContent=text;h.hidden=false;clearTimeout(G._hintT);if(ms)G._hintT=setTimeout(()=>h.hidden=true,ms);};
G.fade=function(on,ms){return new Promise(r=>{const f=$("#fade");f.style.transitionDuration=(G.reduce?10:(ms||500))+"ms";f.classList.toggle("on",on);setTimeout(r,G.reduce?20:(ms||500));});};
G.card=async function(small,title,ms){
  const c=$("#card");c.querySelector("small").textContent=small;c.querySelector("h1").textContent=title;c.hidden=false;c.classList.remove("show");void c.offsetWidth;c.classList.add("show");
  G.sfx("card");await G.wait(G.reduce?900:(ms||2600));c.classList.remove("show");await G.wait(400);c.hidden=true;
};
const tags=[],placed=[];
G.addTag=(pos,text,kind,o)=>{const el=document.createElement("div");el.className="tag "+(kind||"");el.textContent=text;$("#tags").appendChild(el);const t=Object.assign({pos,el,lensOnly:true},o||{});tags.push(t);return t;};
G.removeTag=t=>{const i=tags.indexOf(t);if(i>=0)tags.splice(i,1);t.el.remove();};
G.clearTags=()=>{tags.slice().forEach(G.removeTag);};
const sp={};
function updateHUD(){
  // bubbles follow heads
  placed.length=0;
  for(let i=bubbles.length-1;i>=0;i--){
    const e=bubbles[i];
    if(G.time>e.until||!e.ch.root.parent){e.b.classList.add("out");setTimeout(()=>e.b.remove(),250);bubbles.splice(i,1);continue;}
    G.toScreen(e.ch.headPos(.55),sp);
    const half=e.b.offsetWidth/2+10,off=!sp.vis||sp.x<-20||sp.x>innerWidth+20||sp.y<30||sp.y>innerHeight+20;
    // a line from someone off-screen docks at the bottom like a subtitle instead of vanishing
    if(off&&!e.bark){e.b.classList.add("dock","pos");e.b.style.transform=`translate(${innerWidth/2}px,${innerHeight-(G.touch?210:120)}px) translate(-50%,-100%)`;e.b.style.opacity="";continue;}
    e.b.classList.remove("dock");e.b.classList.add("pos");
    const x=clamp(sp.x,half,innerWidth-half),bh=e.b.offsetHeight;let y=clamp(sp.y,bh+70,innerHeight-120);
    for(let k=0;k<3;k++){const hit=placed.find(r=>Math.abs(r.x-x)<(r.w+half*2)/2&&y>r.y-r.h-4&&y-bh<r.y+4);if(!hit)break;y=hit.y-hit.h-8;}
    placed.push({x,y,w:half*2,h:bh});
    e.b.style.transform=`translate(${x}px,${y}px) translate(-50%,-100%)`;e.b.style.opacity=sp.vis&&!off?"":"0";
  }
  // interaction prompt
  const pr=$("#prompt");
  const it=(!G.talking&&!G.busy&&!P.locked&&P.char)?nearest():null;
  if(it){
    pr.hidden=false;pr.querySelector("span").textContent=it.label||("Nói chuyện với "+(it.char?it.char.displayName:""));
    if(I.act){I.act=false;G.sfx("ok");G.run(()=>it.talk?it.talk(it):it.fn(it));}
  }else pr.hidden=true;
  // lens tags
  for(const t of tags){
    const show=!t.lensOnly||G.lens.on;
    if(!show){t.el.classList.remove("on");continue;}
    G.toScreen(t.pos,sp);
    const tw=t.el.offsetWidth/2+8,inView=sp.vis&&sp.x>-60&&sp.x<innerWidth+60&&sp.y>-20&&sp.y<innerHeight+60;
    t.el.style.transform=`translate(${clamp(sp.x,tw,innerWidth-tw)}px,${clamp(sp.y,t.el.offsetHeight+60,innerHeight-90)}px) translate(-50%,-100%)`;t.el.classList.toggle("on",inView);
  }
  // objective distance and off-screen arrow
  const wp=$("#wp"),od=$("#obj-d");
  if(G.obj&&G.obj.target&&P.char){
    const tg=G.obj.target,d=Math.hypot(tg.x-P.pos.x,tg.z-P.pos.z);od.textContent=d>2?Math.round(d)+" m":"";
    _p.set(tg.x,1.6,tg.z);G.toScreen(_p,sp);
    const m=60,inside=sp.vis&&sp.x>m&&sp.x<innerWidth-m&&sp.y>m&&sp.y<innerHeight-m;
    if(!inside&&d>3){
      let x=sp.x-innerWidth/2,y=sp.y-innerHeight/2;if(!sp.vis){x=-x;y=-y;}
      const a=Math.atan2(y,x),r=Math.min(innerWidth,innerHeight)*.42;
      wp.hidden=false;wp.style.transform=`translate(${innerWidth/2+Math.cos(a)*r}px,${innerHeight/2+Math.sin(a)*r}px) rotate(${a}rad)`;
    }else wp.hidden=true;
  }else{wp.hidden=true;od.textContent="";}
  if(G.beacon.visible&&G.obj&&G.obj.target){const tg=G.obj.target;G.beacon.position.set(tg.x,G.world?G.world.groundH(tg.x,tg.z):0,tg.z);}
  if(G.beacon.visible){G.beacon.children[1].position.y=2.1+Math.sin(G.time*3)*.15;G.beacon.children[1].rotation.y+=.03;G.beacon.children[0].material.opacity=.5+.3*Math.sin(G.time*4);}
  // 3D click (minigames that pick objects)
  if(I.click&&G.onClick3D)G.onClick3D(I.click);
}
G.beacon=(function(){
  const g=new T.Group();
  const ring=new T.Mesh(new T.RingGeometry(.55,.72,40),new T.MeshBasicMaterial({color:0x3ff0c0,transparent:true,opacity:.7,side:T.DoubleSide,depthWrite:false,toneMapped:false}));ring.rotation.x=-Math.PI/2;ring.position.y=.05;g.add(ring);
  const gem=new T.Mesh(new T.OctahedronGeometry(.22),new T.MeshBasicMaterial({color:0x3ff0c0,toneMapped:false}));gem.scale.y=1.5;g.add(gem);
  g.visible=false;return g;
})();
G.pick=function(click,objects){
  const ray=new T.Raycaster(),v=new T.Vector2(click.x/innerWidth*2-1,-(click.y/innerHeight)*2+1);ray.setFromCamera(v,G.cam);
  const hit=ray.intersectObjects(objects,true)[0];if(!hit)return null;
  let o=hit.object;while(o&&!objects.includes(o))o=o.parent;return o;
};

/* ---------- Debug Lens ---------- */
G.lens={unlocked:false,on:false,toggle(on){
  if(!this.unlocked){if(on==null)G.hint("Kính Debug chưa mở khóa",1500);return;}
  this.on=on==null?!this.on:on;document.body.classList.toggle("lens",this.on);
  G.sfx(this.on?"lensOn":"lensOff");
  if(P.char&&P.char.glasses)P.char.glasses.forEach(m=>m.material.emissiveIntensity=this.on?2.2:.25);
  if(G.world&&G.world.onLens)G.world.onLens(this.on);
  if(G.onLens)G.onLens(this.on);
}};

/* ---------- pause ---------- */
G.togglePause=function(force){
  const p=$("#pause");if(!p||G.inTitle)return;
  G.paused=force==null?!G.paused:force;p.hidden=!G.paused;document.body.classList.toggle("paused",G.paused);
  if(G.paused){try{p.querySelector("button").focus();}catch(e){}}
};

/* ---------- particles ---------- */
G.fx={init(){
  const N=500,g=new T.BufferGeometry();this.N=N;
  this.pos=new Float32Array(N*3);this.col=new Float32Array(N*3);this.vel=new Float32Array(N*3);this.life=new Float32Array(N);this.base=new Float32Array(N*3);this.i=0;
  g.setAttribute("position",new T.BufferAttribute(this.pos,3));g.setAttribute("color",new T.BufferAttribute(this.col,3));
  const c=document.createElement("canvas");c.width=c.height=32;const x=c.getContext("2d"),gr=x.createRadialGradient(16,16,0,16,16,16);gr.addColorStop(0,"#fff");gr.addColorStop(.4,"rgba(255,255,255,.7)");gr.addColorStop(1,"rgba(255,255,255,0)");x.fillStyle=gr;x.fillRect(0,0,32,32);
  this.dot=new T.CanvasTexture(c);
  this.pts=new T.Points(g,new T.PointsMaterial({size:.16,map:this.dot,vertexColors:true,transparent:true,depthWrite:false,blending:T.AdditiveBlending,toneMapped:false}));
  this.pts.frustumCulled=false;this.pts.renderOrder=20;
  for(let i=0;i<N;i++)this.pos[i*3+1]=-999;
},attach(scene){scene.add(this.pts);},
emit(p,color,n,o){o=o||{};const c=new T.Color(color);
  for(let k=0;k<n;k++){const i=this.i=(this.i+1)%this.N;
    this.pos[i*3]=p.x+(Math.random()-.5)*(o.spread||.3);this.pos[i*3+1]=p.y+(o.y||0)+(Math.random()-.5)*(o.spreadY||.2);this.pos[i*3+2]=p.z+(Math.random()-.5)*(o.spread||.3);
    const s=o.speed||2;this.vel[i*3]=(Math.random()-.5)*s;this.vel[i*3+1]=(o.up||1.5)*Math.random()+(o.upMin||0);this.vel[i*3+2]=(Math.random()-.5)*s;
    this.life[i]=o.life||.8;this.base[i*3]=c.r;this.base[i*3+1]=c.g;this.base[i*3+2]=c.b;}
},
dust(p){this.emit(p,0xbfb8a8,8,{speed:1.6,up:.6,life:.5,y:.05});},
splash(p){this.emit(p,0x9fd8e8,14,{speed:2.4,up:3,life:.6,y:.2});},
sparkle(p,c){this.emit(p,c||0x3ff0c0,18,{speed:1.4,up:1.2,life:1,y:1,spread:.6,spreadY:1});},
hearts(p){this.emit(p,0xff8fb0,10,{speed:.8,up:1.5,upMin:.6,life:1.2,y:1.8,spread:.4});},
update(dt){if(!this.pts)return;
  for(let i=0;i<this.N;i++){if(this.life[i]<=0)continue;this.life[i]-=dt;
    if(this.life[i]<=0){this.pos[i*3+1]=-999;continue;}
    this.vel[i*3+1]-=4*dt;for(let a=0;a<3;a++)this.pos[i*3+a]+=this.vel[i*3+a]*dt;
    const f=Math.min(1,this.life[i]*2);for(let a=0;a<3;a++)this.col[i*3+a]=this.base[i*3+a]*f;}
  this.pts.geometry.attributes.position.needsUpdate=true;this.pts.geometry.attributes.color.needsUpdate=true;}};

/* ---------- sound: everything synthesized, nothing sampled ---------- */
G.audio={ctx:null,muted:false,amb:{},unlock(){
  if(this.ctx||this.muted)return;
  try{const AC=window.AudioContext||window.webkitAudioContext;this.ctx=new AC();this.out=this.ctx.createGain();this.out.gain.value=.8;this.out.connect(this.ctx.destination);
    const b=this.ctx.createBuffer(1,this.ctx.sampleRate*2,this.ctx.sampleRate),d=b.getChannelData(0);for(let i=0;i<d.length;i++)d[i]=Math.random()*2-1;this.noise=b;
    if(this.pending){this.pending.forEach(n=>G.ambience(n,true));this.pending=null;}}catch(e){this.ctx=null;}
}};
function tone(f,dur,type,vol,slide,delay){
  const A=G.audio,c=A.ctx;if(!c||A.muted)return;const t=c.currentTime+(delay||0),o=c.createOscillator(),g=c.createGain();
  o.type=type||"sine";o.frequency.setValueAtTime(f,t);if(slide)o.frequency.exponentialRampToValueAtTime(slide,t+dur);
  g.gain.setValueAtTime(0,t);g.gain.linearRampToValueAtTime(vol||.1,t+.01);g.gain.exponentialRampToValueAtTime(.0001,t+dur);
  o.connect(g).connect(A.out);o.start(t);o.stop(t+dur+.05);
}
function noiseHit(dur,freq,vol,q){
  const A=G.audio,c=A.ctx;if(!c||A.muted)return;const t=c.currentTime,s=c.createBufferSource(),f=c.createBiquadFilter(),g=c.createGain();
  s.buffer=A.noise;f.type="bandpass";f.frequency.value=freq;f.Q.value=q||1;g.gain.setValueAtTime(vol,t);g.gain.exponentialRampToValueAtTime(.0001,t+dur);
  s.connect(f).connect(g).connect(A.out);s.start(t,Math.random());s.stop(t+dur+.02);
}
G.sfx=function(n,v){
  if(!G.audio.ctx||G.audio.muted)return;
  switch(n){
    case "step":noiseHit(.05,700+Math.random()*300,.05,1.4);break;
    case "stepWet":noiseHit(.08,1500+Math.random()*500,.06,.8);break;
    case "jump":tone(420,.14,"sine",.06,700);break;
    case "land":noiseHit(.09,400,.09,.9);break;
    case "blip":if(Math.random()<.5)tone((v||520)*(1+Math.random()*.25),.04,"triangle",.025);break;
    case "ok":tone(880,.09,"sine",.08);tone(1320,.12,"sine",.06,null,.06);break;
    case "obj":tone(660,.1,"triangle",.07);tone(990,.18,"triangle",.07,null,.09);break;
    case "card":tone(330,.6,"sine",.06);tone(495,.8,"sine",.05,null,.15);tone(660,1,"sine",.04,null,.3);break;
    case "stamp":tone(90,.2,"square",.16,40);noiseHit(.12,300,.25,.6);break;
    case "lensOn":tone(300,.25,"sawtooth",.04,1200);break;
    case "lensOff":tone(1000,.2,"sawtooth",.03,250);break;
    case "alarm":tone(880,.35,"square",.05,440);tone(880,.35,"square",.05,440,.45);break;
    case "ring":tone(640,.22,"triangle",.08);tone(800,.22,"triangle",.08,null,.25);break;
    case "good":[523,659,784,1046].forEach((f,i)=>tone(f,.25,"triangle",.07,null,i*.09));break;
    case "bad":tone(300,.3,"sawtooth",.05,150);break;
    case "meow":tone(700,.28,"triangle",.06,500);tone(900,.18,"sine",.04,600,.05);break;
    case "splash":noiseHit(.35,900,.18,.5);break;
    case "pickup":tone(780,.08,"sine",.08);tone(1170,.12,"sine",.07,null,.07);break;
    case "type":noiseHit(.03,2500,.04,2);break;
    case "beep":tone(1200,.08,"square",.03);break;
  }
};
G.ambience=function(name,on){
  const A=G.audio;if(!A.ctx){if(on){A.pending=(A.pending||[]).filter(x=>x!==name);A.pending.push(name);}return;}
  if(!on){const a=A.amb[name];if(a){try{a.g.gain.linearRampToValueAtTime(0,A.ctx.currentTime+1);setTimeout(()=>a.s.forEach(s=>s.stop()),1100);}catch(e){}delete A.amb[name];}return;}
  if(A.amb[name])return;
  const c=A.ctx,g=c.createGain();g.gain.value=0;g.connect(A.out);g.gain.linearRampToValueAtTime(name==="rain"?.32:.12,c.currentTime+2);
  const src=c.createBufferSource();src.buffer=A.noise;src.loop=true;const f=c.createBiquadFilter();
  if(name==="rain"){f.type="lowpass";f.frequency.value=2600;}else{f.type="lowpass";f.frequency.value=380;}
  src.connect(f).connect(g);src.start();
  const srcs=[src];
  if(name==="office"){const o=c.createOscillator();o.frequency.value=58;const og=c.createGain();og.gain.value=.25;o.connect(og).connect(g);o.start();srcs.push(o);}
  A.amb[name]={g,s:srcs};
};
G.setMuted=m=>{G.audio.muted=m;if(G.audio.out)G.audio.out.gain.value=m?0:.8;};

/* ---------- save slot (this browser only) ---------- */
G.save=d=>{try{localStorage.setItem("civi3d",JSON.stringify(d));}catch(e){}};
G.load=()=>{try{return JSON.parse(localStorage.getItem("civi3d")||"null");}catch(e){return null;}};

/* ---------- worlds ---------- */
G.loadWorld=function(w){
  if(G.world){G.scene.remove(G.world.group);if(G.world.dispose)G.world.dispose();}
  G.npcs.slice().forEach(G.removeNPC);G.interactables.length=0;G.clearTags();G.updaters.clear();G.objective(null);G.onClick3D=null;
  G.chars=G.chars.filter(c=>c.isPlayer||c.persist);
  G.world=w;G.scene.add(w.group);G.scene.add(G.beacon);G.fx.attach(G.scene);
  if(P.char&&!P.char.root.parent)G.scene.add(P.char.root);
  if(G.civi&&!G.civi.root.parent)G.scene.add(G.civi.root);
  G.scene.fog=w.fog||null;G.scene.background=w.bg||null;
  G.renderer.toneMappingExposure=w.exposure||1;
  if(w.pitch)C.pitch=(w.pitch[0]+w.pitch[1])/2;
  G.renderer.shadowMap.needsUpdate=true;
  G.lens.toggle(false);
};
})();
