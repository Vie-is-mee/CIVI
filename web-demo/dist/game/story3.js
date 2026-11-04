/* CIVI 3D · Chapter 3 (Tầng 42 on storm day): counting people is not enough when power follows
   points. Three approvals, the code freeze, and the 48 MW split during the storm. */
(function(){
"use strict";
if(!window.G)return;
const G=window.G,T=G.T,$=G.$,S=G.S,P=G.player;
const V=(x,z)=>new T.Vector3(x,0,z);

/* the office printer, jammed as usual */
function makePrinter(w,x,z){
  const g=new T.Group();g.position.set(x,0,z);w.group.add(g);
  const stand=new T.Mesh(new T.BoxGeometry(.8,.72,.6),G.toon(0x5a6266));stand.position.y=.36;g.add(stand);
  const body=new T.Group();body.position.y=.72;g.add(body);
  const shell=new T.Mesh(new T.BoxGeometry(.72,.34,.52),G.toon(0xe8e6e0,{rim:.25}));shell.position.y=.17;body.add(shell);
  const tray=new T.Mesh(new T.BoxGeometry(.5,.02,.3),G.toon(0x9aa2a4));tray.position.set(0,.36,-.08);tray.rotation.x=-.3;body.add(tray);
  const led=new T.Mesh(new T.SphereGeometry(.025,8,6),G.basic(0xff5a4a));led.position.set(.28,.3,.27);body.add(led);
  const paper=new T.Mesh(new T.PlaneGeometry(.3,.4),G.toon(0xfbfaf6,{side:T.DoubleSide}));paper.rotation.x=-Math.PI/2+.25;paper.position.set(0,.2,.3);paper.visible=false;body.add(paper);
  w.boxes.push({x0:x-.42,x1:x+.42,z0:z-.32,z1:z+.32,h:1.1});
  return {g,body,led,paper,pos:V(x,z+.9),shake(k){body.position.x=Math.sin(G.time*60)*.02*k;body.rotation.z=Math.sin(G.time*47)*.03*k;}};
}

/* a PR checklist at the corner of the screen */
function prPanel(){
  const el=document.createElement("div");el.className="pr";
  el.innerHTML=`<p class="k">PR #219 · sàn sinh tồn</p><div data-p="khoa"><i></i>Khoa · review code</div><div data-p="hang"><i></i>Chị Hằng · phiếu ngoại lệ</div><div data-p="linh"><i></i>Linh · bằng chứng</div><div data-p="vinh"><i></i>Sếp Vinh · mở khóa</div>`;
  document.body.appendChild(el);
  return {el,done(k){const r=el.querySelector(`[data-p="${k}"]`);if(r)r.classList.add("ok");},remove(){el.remove();}};
}

G.chapter3=async function(){
  G.setStage(3);
  const n=S.counted||3;S.c3={tests:0,threat:false,alloc:null};
  const w=G.buildOffice();G.loadWorld(w);G.spawnPlayer();G.spawnCivi();G.lens.unlocked=true;
  const D=w.desks;
  G.placePlayer(-15,7.8,Math.PI);
  G.ambience("office",true);
  // storm day: a grey sky over Lưu Quang
  w.sun.intensity=.75;w.sun.color.set(0xc6d2e6);if(w.pano)w.pano.material.color.setRGB(.6,.66,.78);
  G.chip(`<span>Bão tới lúc</span><b class="z">18:00</b><span>Hồ sơ 8-12</span><b class="m">${n} người</b>`);
  const seat=(m,d)=>{m.pos.copy(d.seat);m.yaw=d.face+Math.PI;};
  const khoa=G.npc("khoa",0,0,0,{barks:["Bão tới thì đóng băng cũng phải tan thôi.","Em đã viết test. Test cũng đã viết em."]});seat(khoa,D.khoa);khoa.char.setPose("sitType");
  const linh=G.npc("linh",0,0,0,{barks:["Đội xác minh đang chờ lệnh."]});seat(linh,D.linh);linh.char.setPose("sitType");
  const hang=G.npc("hang",-12.2,-8.4,2.6,{barks:["Máy in lại kẹt rồi.","Không có phiếu thì không có ngoại lệ."]});
  const vinh=G.npc("vinh",w.spots.vinh.x,w.spots.vinh.z,Math.PI,{barks:["Bão còn vài tiếng."]});vinh.char.setPose("crossArms");
  const cat=G.addNPC({char:G.makeCat(),pos:[D.mai.group.position.x,D.mai.group.position.z],yaw:Math.PI,solid:false,look:false});cat.fixedY=.8;cat.char.setPose("sit");
  const printer=makePrinter(w,-12.4,-10.2);
  await G.fade(false,700);
  await G.card("Chương 3","Người có điểm bằng không");
  await G.run(async()=>{
    G.talkCam(P.pos,G.civi.root.position);
    await G.lines([
      ["civi","Chào buổi sáng. Tôi đã học lại, chỉ từ dữ liệu có đồng ý.",{expr:"happy"}],
      ["civi",`Phân khu 8-12: ${n} người. Điểm đóng góp: 0. Điện được chia khi bão: 0.`,{expr:"think"}],
      ["mai","Đếm được rồi mà vẫn bằng không?",{expr:"surprised"}],
      ["civi","Tôi chia theo điểm. Không điểm thì không điện. Rất công bằng, theo định nghĩa của tôi.",{expr:"glitch"}],
      ["mai","Thì sửa định nghĩa. Một sàn sinh tồn: ai cũng có phần tối thiểu, có điểm hay không.",{expr:"smug"}],
      ["civi","PR cần ba người duyệt, rồi Sếp Vinh mở khóa đóng băng. Bão tới lúc 18 giờ.",{expr:"alert"}],
    ]);
  });
  const pr=prPanel();
  const ok={khoa:false,hang:false,linh:false};
  const left=()=>[["khoa",khoa],["hang",hang],["linh",linh]].filter(([k])=>!ok[k]);
  // the objective always names the next concrete step toward the nearest missing approval
  const point=()=>{const l=left();if(!l.length)return;
    const steps=l.map(([k,npc])=>k==="hang"&&S.c3.form==="asked"?{t:"Lấy phiếu ĐB-07 ở máy in",p:printer.pos}:k==="hang"&&S.c3.form==="printed"?{t:"Mang phiếu ĐB-07 cho Chị Hằng",p:npc.pos}:{t:`Xin ${npc.char.displayName} duyệt PR #219 · ${3-l.length}/3`,p:npc.pos});
    let best=steps[0];for(const x of steps)if(x.p.distanceTo(P.pos)<best.p.distanceTo(P.pos))best=x;
    G.objective(best.t,best.p,{r:2});};
  point();
  await new Promise(res=>{
    const after=k=>{ok[k]=true;pr.done(k);G.sfx("good");G.unface(k==="khoa"?khoa:k==="hang"?hang:linh);if(left().length)point();else res();};
    khoa.talk=async()=>{if(ok.khoa){G.bark(khoa,"LGTM rồi mà.");return;}G.face(khoa);khoa.char.setPose(null);G.talkCam(P.pos,khoa.pos);await khoaReview(khoa);khoa.char.setPose("sitType");after("khoa");};
    linh.talk=async()=>{if(ok.linh){G.bark(linh,"Đội xác minh nhận rồi!");return;}G.face(linh);G.talkCam(P.pos,linh.pos);await linhEvidence(linh);after("linh");};
    hang.talk=async()=>{if(ok.hang){G.bark(hang,"Phiếu đã đóng dấu, cất kỹ vào.");return;}
      if(!S.c3.form){G.face(hang);G.talkCam(P.pos,hang.pos);await hangAsk(hang);G.unface(hang);printerQuest(w,printer,cat,point);return;}
      if(S.c3.form==="asked"){G.bark(hang,"Máy in ở góc bếp. Nhẹ tay với nó.");return;}
      G.face(hang);G.talkCam(P.pos,hang.pos);
      await G.lines([["mai","Phiếu ĐB-07 đây chị. Còn ấm.",{expr:"happy",emote:"give"}],[hang,"Mẫu ĐB-07: “Đơn xin phép được cho phép.” Chuẩn rồi.",{expr:"smug"}],
        ()=>{G.sfx("stamp");G.camCtl.shake=.25;hang.char.emote("nod");},[hang,"Đóng dấu. Từ giờ chị không biết gì về việc em sắp làm.",{expr:"happy"}]]);after("hang");};
  });
  // the boss
  G.objective("Gặp Sếp Vinh trong phòng kính",w.spots.vinh,{r:2.4});
  await G.waitInteract(V(w.spots.vinh.x-1.2,w.spots.vinh.z+1.6),"Nói chuyện với Sếp Vinh",2.6);
  await G.run(()=>vinhTalk(vinh));
  pr.done("vinh");
  await G.run(()=>merge());
  pr.remove();
  // the storm arrives
  await storm(w,{khoa,linh,hang,vinh,cat},n);
  await epilogue(n);
};

/* ---- Khoa's review: pick the three tests that belong to this PR ---- */
function khoaReview(khoa){
  return new Promise(async res=>{
    await G.lines([[khoa,"PR sửa luật chia điện mà không có test à? Em định cho cả thành phố chạy thử à?",{expr:"smug"}],["mai","Anh chọn giúp em mấy test?",{expr:"happy"}],[khoa,"Không. Em chọn. Đúng ba cái. Anh chấm.",{expr:"smug",emote:"point"}]]);
    const tests=[
      {id:1,f:"test_cho_xac_minh_van_duoc_dem",d:"hồ sơ “chờ xác minh” vẫn được đếm",ok:true},
      {id:2,f:"test_meo_duoc_cap_so_cu_tru",d:"Bé Điện có số cư trú",ok:false,why:"Test này để PR khác. PR riêng của Bé Điện."},
      {id:3,f:"test_tu_choi_khong_bi_dem_thay",d:"người từ chối ghi tên không bị đếm thay",ok:true},
      {id:4,f:"test_dem_het_cho_nhanh",d:"if True: đếm tất cả mọi thứ",ok:false,why:"Lần trước cái này đếm ra cả chuột."},
      {id:5,f:"test_san_sinh_ton",d:"người 0 điểm vẫn được ít nhất 0,5 kW khi bão",ok:true},
    ];
    const m=G.mg(`<div class="term"><div class="term-h"><span>review · PR #219 · sàn sinh tồn</span><b>0 test</b></div>
      <p class="term-q">Chọn đúng ba test cho PR này, rồi chạy.</p><div class="tests">${tests.map(t=>`<button data-t="${t.id}" aria-pressed="false"><i></i><span>${t.f}</span><small>${t.d}</small></button>`).join("")}</div>
      <button class="btn mint" id="trun" disabled>Chạy test</button><pre class="term-out" id="tout" hidden></pre></div>`,"mg-test");
    const sel=new Set(),run=m.querySelector("#trun"),out=m.querySelector("#tout"),hd=m.querySelector(".term-h b");
    m.querySelectorAll("[data-t]").forEach(b=>b.onclick=()=>{const id=+b.dataset.t;if(sel.has(id))sel.delete(id);else{if(sel.size>=3)return;sel.add(id);}
      b.classList.toggle("on",sel.has(id));b.setAttribute("aria-pressed",sel.has(id));b.classList.remove("bad");G.sfx("type");hd.textContent=sel.size+" test";run.disabled=sel.size!==3;});
    run.onclick=async()=>{run.disabled=true;S.c3.tests++;out.hidden=false;out.textContent="$ pytest tests/test_chia_dien.py";
      const picked=tests.filter(t=>sel.has(t.id)),bad=picked.filter(t=>!t.ok);
      for(const t of picked){await G.wait(G.reduce?40:380);out.textContent+=`\n${t.ok?"✔":"✖"} ${t.f}`;G.sfx(t.ok?"type":"bad");}
      await G.wait(G.reduce?40:400);
      if(!bad.length){out.textContent+="\n\n3 passed · 0 failed";G.sfx("good");await G.wait(G.reduce?100:900);G.mgClose();
        await G.lines([[khoa,"Ba test, ba xanh. LGTM.",{expr:"happy",emote:"nod"}],["civi","LGTM nghĩa là “Looks Good To Me”.",{expr:"happy"}],[khoa,"Hôm nay nghĩa là “Looks Good To Mai”.",{expr:"smug",emote:"laugh"}]]);res();return;}
      bad.forEach(t=>m.querySelector(`[data-t="${t.id}"]`).classList.add("bad"));
      out.textContent+=`\n\n${picked.length-bad.length} passed · ${bad.length} failed\nKhoa: ${bad[0].why}`;run.disabled=false;
    };
    setTimeout(()=>{try{m.querySelector("[data-t]").focus();}catch(e){}},50);
  });
}

/* ---- Linh: what may be attached ---- */
async function linhEvidence(linh){
  const ya=S.asked.length,no=S.declined.length;
  await G.lines([[linh,"Đội xác minh xuống 8-12 chiều nay. Họ hỏi mình đính kèm bằng chứng nào.",{expr:"neutral"}]]);
  let c=await G.choose([{t:"Chỉ những gì người dân đã cho phép."},{t:"Tất cả những gì mình đã chụp, cho chắc."}]);
  if(c===1){await G.lines([[linh,no?`Nhưng có ${no} người đã nói không mà.`:"Kể cả những trang bà Hạ không cho chụp?",{expr:"worry"}],["mai","Ừ. Cho chắc là cho ai chắc.",{expr:"sad",emote:"facepalm"}]]);S.c3.allFirst=true;}
  S.c3.evidence="consent";
  await G.lines([[linh,`Vậy đính kèm ${S.docs.length} tư liệu được phép${ya?`, và ${ya} người tự xác nhận tên`:""}.`,{expr:"happy"}],
    [linh,"À, An nhờ đội gửi lời: “Nan cong vẫn chắc. Cô nhớ để ý.”",{expr:"happy",emote:"wave"}],["mai","...Cô vẫn đang để ý.",{expr:"happy"}]]);
  G.toast("<b>Linh đã duyệt</b><small>Bằng chứng có đồng ý · đã gửi đội xác minh</small>","good");
}

/* ---- Hằng and the printer, with help from the cat ---- */
async function hangAsk(hang){
  await G.lines([[hang,"Ngoại lệ trong đóng băng cần phiếu ĐB-07, in ra, ký tay.",{expr:"neutral"}],["mai","Sắp bão rồi mà vẫn cần giấy ạ?",{expr:"surprised"}],
    [hang,"Bão là việc của trời. Giấy là việc của chị. Máy in ở góc bếp, nó hơi... có cá tính.",{expr:"smug"}]]);
  S.c3.form="asked";
}
async function printerQuest(w,pt,cat,point){
  point();
  const quips=[["civi","Đề xuất 1: tắt đi, bật lại.",{expr:"think"}],["civi","Đề xuất 2: vỗ nhẹ cạnh trái. Theo tài liệu năm 2190.",{expr:"think"}]];
  let shaking=0;const sh=dt=>{if(shaking>0){shaking-=dt;pt.shake(Math.min(1,shaking*2));}else{pt.body.position.x=0;pt.body.rotation.z=0;}};G.updaters.add(sh);
  for(let k=0;k<2;k++){
    await G.waitInteract(pt.pos,k?"Vỗ nhẹ vào máy in":"Gỡ kẹt giấy",1.6);
    await G.run(async()=>{G.face({pos:V(pt.g.position.x,pt.g.position.z)});P.char.emote(k?"stomp":"type");shaking=1.2;G.sfx("bad");G.camCtl.shake=.15;await G.wait(700);await G.lines([quips[k]]);});
  }
  // Bé Điện strolls over and sits on the printer. It works.
  await G.run(async()=>{
    G.shot(new T.Vector3(pt.g.position.x+2.2,1.9,pt.g.position.z+2.4),new T.Vector3(pt.g.position.x,1,pt.g.position.z),2.5);
    cat.fixedY=null;cat.char.setPose("walk");const c0=cat.pos.clone();
    cat.pos.set(pt.g.position.x+2.4,0,pt.g.position.z+1.4);
    await G.moveNPC(cat,[[pt.g.position.x+.5,pt.g.position.z+.6]],1.6);
    cat.char.emote("meow");cat.fixedY=1.08;cat.pos.set(pt.g.position.x,1.08,pt.g.position.z);cat.char.setPose("sit");
    await G.wait(500);pt.led.material.color.setHex(0x3ff0c0);pt.paper.visible=true;G.sfx("good");shaking=.4;
    await G.lines([["mai","...Nó chạy rồi.",{expr:"surprised"}],["civi","Ghi nhận: con mèo sửa được máy in. Tôi thì không. Tôi sẽ không ghi nhận điều này.",{expr:"glitch"}]]);
    G.civi.setExpr("happy");
    cat.char.setPose("sleep");
  });
  S.c3.form="printed";G.sfx("pickup");G.toast("<b>Phiếu ĐB-07</b><small>“Đơn xin phép được cho phép” · còn ấm</small>","warm");
  point();
}

/* ---- Vinh opens the freeze ---- */
async function vinhTalk(vinh){
  G.face(vinh);vinh.char.setPose(null);G.talkCam(P.pos,vinh.pos);
  await G.lines([[vinh,"Ba chữ ký trong một buổi sáng. Phòng này chưa ai làm nhanh thế, kể cả khi xin nghỉ phép.",{expr:"smug"}],
    ["mai","Sàn sinh tồn: người 0 điểm vẫn nhận phần tối thiểu khi bão. Có test, có bằng chứng được đồng ý.",{expr:"neutral"}],
    [vinh,"Mở khóa đóng băng thì tên tôi nằm trong nhật ký. Nếu có gì sai, tôi chịu.",{expr:"neutral"}]]);
  const c=await G.choose([{t:"Em sẽ viết báo cáo sự cố, ghi rõ ai quyết định điều gì."},{t:"Anh không mở thì em tự merge, token của em vẫn chạy."}]);
  if(c===1){S.c3.threat=true;
    await G.lines([[vinh,"Em có quyền ghi, không có quyền quyết. Hôm qua có người dạy em điều đó, đúng không?",{expr:"angry"}],["mai","...Bà Hạ. Em xin lỗi.",{expr:"sad",emote:"bow"}],["civi","Ghi chú: token của Mai vừa được thu hồi. Một cách trung lập.",{expr:"think"}]]);}
  else await G.lines([[vinh,"Tốt. Người viết rõ ai quyết gì thì ít khi phải đổ lỗi cho ai.",{expr:"happy",emote:"nod"}]]);
  await G.lines([[vinh,"Bão không đợi ai được xác minh. Mở đi.",{expr:"neutral"}]]);
  G.unface(vinh);vinh.char.setPose("crossArms");
}
function merge(){
  return new Promise(async res=>{
    const m=G.mg(`<div class="term"><div class="term-h"><span>terminal · tang42</span><b>đóng băng: đã mở (Vinh)</b></div><pre class="term-out" id="mout"></pre></div>`,"mg-term");
    const out=m.querySelector("#mout");
    const steps=["$ git push origin san-sinh-ton","✔ CI · 3 passed · 0 failed","✔ duyệt: Khoa, Hằng, Linh · mở khóa: Vinh","✔ merged vào main","$ civi reload --luat chia_dien","✔ sàn sinh tồn: bật · 0,5 kW mỗi người khi bão"];
    for(const s of steps){out.textContent+=(out.textContent?"\n":"")+s;G.sfx("type");await G.wait(G.reduce?40:480);}
    G.sfx("good");
    const b=document.createElement("button");b.className="btn mint";b.textContent="Đóng terminal";m.firstElementChild.appendChild(b);b.focus();
    b.onclick=()=>{G.mgClose();res();};
  });
}

/* ---- the storm: split 48 MW at the holotable ---- */
async function storm(w,who,n){
  w.alarm=true;G.sfx("alarm");G.camCtl.shake=.7;
  const al=$("#alarm");al.hidden=false;al.textContent="BÃO TỚI SỚM · Lưới điện mái vòm còn 48 MW";
  const siren=setInterval(()=>G.sfx("alarm"),2200);
  w.sun.intensity=.45;w.sun.color.set(0x8a9ab8);if(w.pano)w.pano.material.color.setRGB(.36,.4,.5);
  who.khoa.char.setPose(null);who.khoa.char.emote("surprise");G.bark(who.khoa,"Ơ, bão không đọc lịch à?");
  who.hang.char.emote("jump");G.bark(who.hang,"Bình tĩnh! Chị đi tìm phiếu bình tĩnh!");
  G.objective("Tới bàn ánh sáng: chia điện trong bão",w.holo.position,{r:2.6});
  await G.waitInteract(w.holo.position,"Mở bảng chia điện",2.7);
  await G.run(()=>allocate(w,n));
  clearInterval(siren);w.alarm=false;al.hidden=true;G.objective(null);
  // the hologram: 8-12 lights up if it got its floor
  const a=S.c3.alloc;
  await G.run(async()=>{
    const hp=w.holo.position;G.shot(new T.Vector3(hp.x+2.6,3.2,hp.z+3),new T.Vector3(hp.x,1.2,hp.z),2.5);
    await G.wait(700);
    if(a.p812>=3){lightDistrict(w);G.sfx("good");G.fx.sparkle(new T.Vector3(hp.x+.6,1.2,hp.z+.3),0xffd890);}
    await G.lines([
      a.p812>=3?["civi",`Phân khu 8-12: ${a.p812} MW. Đèn sáng. ${n} người, đúng tên.`,{expr:"love"}]:["civi","Phân khu 8-12: dưới mức sàn. Cửa sổ ở đó vẫn tối.",{expr:"sad"}],
      a.loi<34?["civi",`Lõi Sáng giảm ${34-a.loi} MW: đài phun nước cao bằng tòa tháp tạm nghỉ hai tiếng.`,{expr:"wink"}]:null,
      a.loi<34?[who.khoa,"Tín mà biết chắc buồn lắm.",{expr:"smug"}]:null,
      a.b4<8?["civi","Trạm khí B4 thiếu điện. Khí sạch toàn vùng giảm.",{expr:"alert"}]:null,
      a.bv<6?["civi","Bệnh viện Số Ba chạy máy phát dự phòng. Không ai vui về điều đó.",{expr:"sad"}]:null,
      a.shop>=2?[who.khoa,"Shop Mẹ Bỉm vẫn livestream giữa bão. Kinh tế không bao giờ ngủ.",{expr:"laugh"}]:null,
      [who.vinh,"Ghi vào nhật ký: chia theo nhu cầu tối thiểu trước, điểm sau. Người duyệt: Vinh.",{expr:"neutral",emote:"nod"}],
    ]);
  });
  w.sun.intensity=.75;
}
function allocate(w,n){
  return new Promise(res=>{
    // CIVI's plan follows points: comfort in Lõi Sáng first, nothing for zero points
    const Z=[{id:"bv",name:"Bệnh viện Số Ba",note:"máy thở, phòng mổ",need:6,v:6},{id:"b4",name:"Trạm khí B4",note:"khí sạch cho cả mái vòm",need:8,v:8},
      {id:"loi",name:"Lõi Sáng",note:"tiện nghi: điều hòa, đài phun nước",need:34,v:34},{id:"p812",name:"Phân khu 8-12",note:`${n} người · sàn sinh tồn cần 3 MW`,need:3,v:0},
      {id:"shop",name:"Shop Mẹ Bỉm",note:"livestream xả kho",need:2,v:0}];
    const TOTAL=48;
    const m=G.mg(`<div class="alloc"><p class="k">Bàn ánh sáng · lưới điện trong bão</p><h3>Chia 48 MW</h3><p class="small">CIVI đã điền sẵn theo điểm đóng góp. Sửa nếu bạn thấy cần.</p>
      <div class="zones">${Z.map(z=>`<div class="zrow" data-z="${z.id}"><div class="zname"><b>${z.name}</b><small>${z.note}</small></div>
        <div class="zctl"><button data-d="-1" aria-label="Bớt 1 MW cho ${z.name}">−</button><output>${z.v} MW</output><button data-d="1" aria-label="Thêm 1 MW cho ${z.name}">+</button></div><div class="zbar"><i></i></div></div>`).join("")}</div>
      <div class="tot"><span>Còn lại</span><b id="aleft">0 MW</b></div><p class="small" id="amsg" aria-live="polite"></p><button class="btn mint" id="ago">Xác nhận phân bổ</button></div>`,"mg-alloc");
    const draw=()=>{const used=Z.reduce((s,z)=>s+z.v,0),left=TOTAL-used;m.querySelector("#aleft").textContent=left+" MW";
      Z.forEach(z=>{const r=m.querySelector(`[data-z="${z.id}"]`);r.querySelector("output").textContent=z.v+" MW";r.querySelector(".zbar i").style.width=Math.min(100,z.v/Math.max(z.need,1)*100)+"%";
        r.classList.toggle("warn",(z.id==="bv"||z.id==="b4"||z.id==="p812")&&z.v<z.need);});
      const msg=Z.find(z=>z.id==="p812").v<3?"Phân khu 8-12 đang ở 0 MW: CIVI chia theo điểm.":Z.find(z=>z.id==="b4").v<8?"B4 dưới mức an toàn.":left<0?"Vượt quá 48 MW.":left>0?`Còn ${left} MW chưa chia.`:"Đã chia hết 48 MW.";
      m.querySelector("#amsg").textContent=msg;m.querySelector("#ago").disabled=left<0;};
    m.querySelectorAll("[data-d]").forEach(b=>b.onclick=()=>{const z=Z.find(x=>x.id===b.closest(".zrow").dataset.z),d=+b.dataset.d;const used=Z.reduce((s,q)=>s+q.v,0);
      if(d>0&&used>=TOTAL){G.sfx("bad");m.querySelector("#amsg").textContent="Hết điện để chia. Bớt chỗ khác trước.";return;}
      z.v=Math.max(0,z.v+d);G.sfx("type");draw();});
    m.querySelector("#ago").onclick=()=>{const a={};Z.forEach(z=>a[z.id]=z.v);S.c3.alloc=a;G.sfx("stamp");G.camCtl.shake=.2;G.mgClose();res(a);};
    draw();setTimeout(()=>{try{m.querySelector('[data-z="loi"] [data-d="-1"]').focus();}catch(e){}},60);
  });
}
function lightDistrict(w){
  // warm a cluster of the hologram's buildings: Phân khu 8-12 sits on the east edge of the model
  const im=w.holoCity&&w.holoCity.children.find(c=>c.isInstancedMesh);if(!im)return;
  const M=new T.Matrix4(),p=new T.Vector3(),c=new T.Color(0xffc070);
  const mint=new T.Color(0x5ff0c8);im.material.color.set(0xffffff);im.material.needsUpdate=true;
  for(let i=0;i<im.count;i++){im.getMatrixAt(i,M);p.setFromMatrixPosition(M);im.setColorAt(i,p.x>.35&&p.z>-.2?c:mint);}
  im.instanceColor.needsUpdate=true;
}

/* ---- epilogue: the same night in Rìa Mưa ---- */
async function epilogue(n){
  await G.fade(true,900);G.ambience("office",false);
  const w=G.buildRiaMua();G.loadWorld(w);G.ambience("rain",true);
  const a=S.c3.alloc||{p812:0};
  G.placePlayer(0,22.6,Math.PI);P.char.root.visible=false;G.civi.root.visible=false;P.locked=true;document.body.classList.add("cine");
  const an=G.npc("an",17.4,-19.6,Math.PI*.15,{look:false});const ha=G.npc("ha",19,-19.9,-Math.PI*.1,{look:false});const dung=G.npc("dung",16.2,-18.8,.5,{look:false});
  an.char.setPose("holdUp");w.umbrellaG.position.set(an.pos.x,2.1,an.pos.z);
  if(a.p812>=3){const wim=w.group.children.find(c=>c.isInstancedMesh&&c.geometry.type==="PlaneGeometry");
    if(wim){const col=new T.Color();for(let i=0;i<wim.count;i++){if(Math.random()<.7){col.setHex(Math.random()<.8?0xffb45a:0xbff0e6);wim.setColorAt(i,col);}}if(wim.instanceColor)wim.instanceColor.needsUpdate=true;}}
  G.shot(new T.Vector3(14.2,1.9,-14.4),new T.Vector3(18,1.5,-20.6),1.4);G.cam.position.set(12,3,-11);
  await G.fade(false,900);
  await G.lines(a.p812>=3?[[an,"Mẹ ơi! Đèn sáng rồi! Cả nhà bà Dung nữa!",{expr:"happy",emote:"jump"}],[ha,"Ừ. Lần này không ai phải nộp thêm tờ nào.",{expr:"happy"}],[dung,"Cái sàn nhà tôi vẫn ướt. Nhưng ít ra tôi nhìn thấy nó.",{expr:"smug"}]]
    :[[an,"Mẹ ơi, bên kia sông có đèn. Bên mình chưa.",{expr:"sad"}],[ha,"Ừ. Mai mẹ đi hỏi lại.",{expr:"neutral"}]]);
  const el=$("#endline");
  el.innerHTML=a.p812>=3?`<small>Phân khu 8-12 · hồ sơ chính thức</small><b>${n} người</b><p>Đã đếm. Đã có điện. Ai cũng có tên.</p>`:`<small>Phân khu 8-12 · hồ sơ chính thức</small><b>${n} người</b><p>Đã đếm. Vẫn chưa có điện. Ngày mai còn việc để làm.</p>`;
  G.shot(new T.Vector3(4,18,6),new T.Vector3(16,1,-20),.5);
  el.hidden=false;void el.offsetWidth;el.classList.add("show");
  await G.wait(G.reduce?900:5200);
  el.classList.remove("show");
  await G.fade(true,1000);el.hidden=true;
  G.save({ch:3,done:true});
}
G.finale=async function(){
  document.body.classList.add("cine");P.locked=false;
  if(!G.world||G.world.name!=="riamua"){const w=G.buildRiaMua();G.loadWorld(w);}
  G.camCtl.shot={pos:new T.Vector3(-14,24,30),look:new T.Vector3(2,0,-6),speed:.25};G.cam.position.set(-14,24,30);
  await G.report();
};
})();
