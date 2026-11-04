/* CIVI 3D · shared story helpers, the title screen, the memory, and Chapter 1 (Tầng 42) */
(function(){
"use strict";
if(!window.G)return;
const G=window.G,T=G.T,$=G.$;
const P=G.player;
const S=G.S={chapter:0,desk:{},b4:null,driver:null,h19:null,lensFound:[],promises:[],seen:[],asked:[],declined:[],docs:[],flags:{}};
const tmp=new T.Vector3();

/* ---------- helpers used by both chapters ---------- */
G.spawnPlayer=function(){
  if(P.char)return;
  const c=G.makeChar(G.CAST.mai);c.isPlayer=true;c.persist=true;P.char=c;G.scene.add(c.root);
};
G.spawnCivi=function(){
  if(G.civi)return;
  const d=G.makeDrone();d.persist=true;G.civi=d;G.scene.add(d.root);
  d.follow=()=>{const y=P.yaw;return tmp.set(P.pos.x-Math.sin(y)*.55+Math.cos(y)*.8,P.pos.y+1.8,P.pos.z-Math.cos(y)*.55-Math.sin(y)*.8);};
  d.root.position.copy(d.follow());
};
G.npc=(key,x,z,yaw,o)=>G.addNPC(Object.assign({char:G.makeChar(G.CAST[key]),pos:[x,z],yaw:yaw||0,key},o||{}));
const W=w=>w==="mai"?"mai":w==="civi"?G.civi:w;
G.lines=async function(arr){for(const l of arr){if(!l)continue;if(typeof l==="function"){await l();continue;}await G.say(W(l[0]),l[1],l[2]);}};
G.face=function(n){if(!n)return;n.face=P.pos;P.yaw=Math.atan2(n.pos.x-P.pos.x,n.pos.z-P.pos.z);if(G.civi)G.civi.look=n.pos;};
G.unface=function(n){if(n)n.face=null;if(G.civi)G.civi.look=null;};
G.chip=function(html){const c=$("#chip");c.hidden=!html;if(html)c.innerHTML=html;};
G.seeChip=function(){G.chip(`<span>Hồ sơ 8-12</span><b class="z">0</b><span>Mai đã gặp</span><b class="m">${S.seen.length}</b>`);};
G.meet=function(id,name,detail){
  if(S.seen.find(x=>x.id===id))return;
  S.seen.push({id,name,detail});G.seeChip();
  G.toast(`<b>+ Sổ của Mai</b> ${name}<small>${detail}</small>`,"warm");
  const m=$("#chip .m");if(m){m.classList.remove("bump");void m.offsetWidth;m.classList.add("bump");}
};
G.mg=function(html,cls){const m=$("#mg");m.className=cls||"";m.innerHTML=html;m.hidden=false;
  // full panels hide the HUD behind them; the phone call and the umbrella dial float over the scene
  document.body.classList.toggle("mgopen",!/mg-(call|umb)/.test(cls||""));return m;};
G.mgClose=function(){const m=$("#mg");m.hidden=true;m.innerHTML="";document.body.classList.remove("mgopen");};
G.setStage=function(ch){S.chapter=ch;G.save({ch,at:Date.now()});document.body.classList.remove("cine");};

/* ---------- title ---------- */
G.title=function(){
  G.inTitle=true;document.body.classList.add("intitle");
  const w=G.buildRiaMua();G.loadWorld(w);
  G.camCtl.shot={pos:new T.Vector3(16,9,26),look:new T.Vector3(0,3,0),speed:1};G.cam.position.set(16,9,26);
  let a=0;const orbit=dt=>{a+=dt*.04;if(G.camCtl.shot){G.camCtl.shot.pos.set(Math.sin(a)*22,8.5,Math.cos(a)*22+2);}};
  G.updaters.add(orbit);
  const sv=G.load();
  const cb=$("#t-cont");cb.hidden=!(sv&&sv.ch>=2&&!sv.done);if(sv&&sv.ch>=2)cb.textContent="Tiếp tục · Chương "+Math.min(3,sv.ch);
  $("#title").hidden=false;
};
async function begin(fn){
  G.audio.unlock();G.ambience("rain",false);
  $("#title").hidden=true;G.inTitle=false;document.body.classList.remove("intitle");G.camCtl.shot=null;
  await G.fade(true,500);
  await fn();
}
G.bindTitle=function(){
  const from=async k=>{if(k<=1)await G.chapter1();if(k<=2)await G.chapter2();await G.chapter3();await G.finale();};
  $("#t-play").onclick=()=>begin(async()=>{await memory();await from(1);});
  $("#t-cont").onclick=()=>begin(async()=>{const sv=G.load();await from(sv&&sv.ch>=3?3:2);});
  $("#t-ch1").onclick=()=>begin(()=>from(1));
  $("#t-ch2").onclick=()=>begin(()=>from(2));
  $("#t-ch3").onclick=()=>begin(()=>from(3));
  $("#t-mute").onclick=e=>{G.setMuted(!G.audio.muted);e.currentTarget.textContent="Âm thanh: "+(G.audio.muted?"tắt":"bật");};
};

/* ---------- the memory: a nurse who typed four lines ---------- */
async function memory(){
  const m=G.mg(`<div class="mem">
    <img src="assets/album.jpg" alt="Ảnh cũ: mẹ Mai, Mai lúc tám tuổi và cô y tá áo xám trước bệnh viện">
    <div class="mem-side">
      <p class="mem-k">Ký ức · Mai, tám tuổi</p>
      <p class="mem-l">Mẹ nằm trên giường bệnh, tay nắm chặt chiếc thẻ lao động đã hết hạn. Hệ thống nói: chưa đủ điểm.</p>
      <p class="mem-l">Cô y tá áo xám nhập bốn dòng dữ liệu.</p>
      <div class="mem-keys">${["Bệnh nhân: nữ, 34 tuổi","Người phụ thuộc: 1 trẻ, 8 tuổi","Trẻ em thuộc nhóm cần bảo vệ","Y tế trước điểm đóng góp"].map((t,i)=>`<button data-i="${i}">${t}</button>`).join("")}</div>
      <div class="mem-bar"><i></i></div>
      <p class="mem-l mem-end" hidden>Vạch xanh. Mẹ được giữ lại. Mười năm sau, Mai học IT để làm ra những vạch xanh như thế.</p>
      <button class="btn mint" id="mem-go" hidden>Tầng 42 · ngày đầu tiên</button>
    </div></div>`,"mg-mem");
  await G.fade(false,400);
  let n=0;
  await new Promise(res=>{
    m.querySelectorAll("[data-i]").forEach(b=>b.onclick=()=>{if(b.classList.contains("on"))return;b.classList.add("on");G.sfx("type");n++;m.querySelector(".mem-bar i").style.width=n*25+"%";
      if(n===4){G.sfx("good");setTimeout(()=>{m.querySelector(".mem-end").hidden=false;const g=m.querySelector("#mem-go");g.hidden=false;g.focus();g.onclick=res;},700);}});
    m.querySelector("[data-i]").focus();
  });
  await G.fade(true,500);G.mgClose();
}

/* ---------- Chapter 1 · Tầng 42 ---------- */
G.chapter1=async function(){
  G.setStage(1);
  const w=G.buildOffice();G.loadWorld(w);G.spawnPlayer();G.spawnCivi();
  G.placePlayer(w.spots.elevator.x,w.spots.elevator.z-2.2,Math.PI);
  G.ambience("office",true);
  G.chip(`<span>Bão còn</span><b class="z">12 ngày</b><span>Giờ</span><b class="m" id="clock">08:00</b>`);
  const D=w.desks;
  const seat=(n,d)=>{n.pos.copy(d.seat);n.yaw=d.face+Math.PI;n.char.root.position.copy(n.pos);};
  const khoa=G.npc("khoa",0,0,0,{barks:["Nó chạy trên máy em mà sếp!","Code chạy rồi thì đừng ai động vào nhé.","Bé Điện lại ngủ trên bàn phím của ai rồi kìa."]});seat(khoa,D.khoa);khoa.char.setPose("sitType");khoa.look=true;
  const linh=G.npc("linh",0,0,0,{barks:["Mai! Ngồi cạnh tớ nè."]});seat(linh,D.linh);linh.char.setPose("sitType");
  const hang=G.npc("hang",-12,-5,0,{barks:["Nhắc nhẹ: trưa nay căng tin có đậu.","Ai lấy cốc của chị thì trả lại nhé, cốc có chữ HẰNG.","Họp lúc 10 giờ. Mang theo tinh thần."],wander:{x:-11,z:-4,r:4}});
  const vinh=G.npc("vinh",w.spots.vinh.x,w.spots.vinh.z,Math.PI,{barks:["Bão còn 12 ngày.","Hiệu suất là sự tử tế lớn nhất."]});vinh.char.setPose("crossArms");
  const cat={char:G.makeCat(),pos:[D.mai.seat.x,D.mai.seat.z],yaw:Math.PI,solid:false,look:false};
  const catN=G.addNPC(cat);catN.char.setPose("sleep");catN.fixedY=.47;
  await G.fade(false,700);
  await G.card("Chương 1","Ngày đầu ở Tầng 42");
  // CIVI says hello
  await G.run(async()=>{
    G.talkCam(P.pos,G.civi.root.position);G.civi.look=P.pos;
    await G.lines([
      ["civi","Xin chào thực tập sinh Mai! Tôi là CIVI, trợ lý điều phối của cả mái vòm.",{expr:"happy"}],
      ["civi","Tôi tối ưu được mọi thứ. Trừ máy pha cà phê tầng này.",{expr:"wink"}],
      ["mai","Chào CIVI. Bàn của tôi ở đâu?",{expr:"happy"}],
      ["civi","Cạnh Linh. Có một sinh vật đang chiếm ghế của bạn.",{expr:"think"}],
    ]);G.civi.look=null;
  });
  G.hint(G.touch?"Kéo nửa trái màn hình để đi · kéo nửa phải để xoay · nút E để nói chuyện":"WASD để đi · Shift để chạy · Space để nhảy · kéo chuột để xoay · E để nói chuyện",8000);
  // desk and the cat
  G.objective("Đến bàn làm việc của bạn",D.mai.seat);
  await G.waitReach(D.mai.seat,2.4);
  await new Promise(res=>{
    const it=G.addInteract({pos:D.mai.seat,r:2.2,label:"Mời Bé Điện dậy",fn:async()=>{
      G.removeInteract(it);G.face(catN);
      catN.char.setPose("sit");catN.char.emote("meow");catN.char.setExpr("grumpy");
      await G.wait(500);
      catN.fixedY=.8;catN.pos.set(D.mai.group.position.x,.8,D.mai.group.position.z);
      await G.lines([
        ["civi","Phát hiện sinh vật trên bàn phím. Năng suất giảm 100 phần trăm.",{expr:"alert"}],
        [catN,"Meo.",{name:"Bé Điện"}],
        ["mai","Nó vừa nhìn tôi như nhìn một lỗi hệ thống.",{emote:"shrug"}],
        [linh,"Bé Điện là mèo của cả tầng. Nó còn nộp hồ sơ xin cư trú đấy.",{expr:"happy"}],
      ]);
      catN.char.setExpr("open");G.unface(catN);res();}});
  });
  G.fx.sparkle(D.mai.group.position);
  // the shift: requests on the holotable
  G.objective("Mở ca điều phối ở bàn ánh sáng",w.holo.position);
  await G.waitInteract(w.holo.position,"Mở ca điều phối",2.6);
  await G.run(()=>holotable(w,{khoa,linh,catN}));
  // Linh's photo
  G.bark(linh,"Mai ơi! Qua đây xem cái này đã!");linh.char.emote("wave");
  G.objective("Đến bàn của Linh",D.linh.seat);
  await G.waitInteract(D.linh.seat,"Xem ảnh Linh chụp",2.3);
  await G.run(()=>lensHunt(w,linh));
  // the query
  G.objective("Kiểm tra truy vấn dân cư ở máy của bạn",D.mai.seat);
  await G.waitInteract(D.mai.seat,"Mở truy vấn dân cư",2.2);
  await G.run(()=>bugHunt(w));
  await G.run(()=>vinhComes(w,vinh,khoa));
  // B4
  await crisis(w,{khoa,linh,hang,vinh,catN});
  // end of the day
  await endOfDay(w,{linh,vinh});
};

/* ---- minigame: the coordination shift on the holotable ---- */
function holotable(w,who){
  return new Promise(async res=>{
    const hp=w.holo.position;
    G.shot(new T.Vector3(hp.x,4.6,hp.z+3.4),new T.Vector3(hp.x,.9,hp.z),3);
    await G.say(G.civi,"Bốn yêu cầu sáng nay. Tôi đã chấm điểm sẵn, bạn chỉ cần đóng dấu.",{expr:"happy"});
    const cards=[
      {id:"hosp",tag:"BV",title:"Bệnh viện Số Ba",ask:"6 MW cho máy hỗ trợ hô hấp",ai:"Đề xuất DUYỆT · ưu tiên 98",aiYes:true},
      {id:"shop",tag:"SHOP",title:"Shop Mẹ Bỉm",ask:"0,2 MW livestream xả kho lúc 0 giờ",ai:"Đề xuất DUYỆT · doanh thu cao",aiYes:true},
      {id:"cat",tag:"MÈO",title:"Bé Điện, 4 tuổi",ask:"Số cư trú + suất khí 0,3 m³/ngày",ai:"Đề xuất TỪ CHỐI · không phải người",aiYes:false},
      {id:"h19",tag:"19",title:"Trạm lọc 19 · Phân khu 8-12",ask:"Sửa trạm lọc · đề nghị lần thứ sáu",ai:"TỰ ĐỘNG TRẢ VỀ · dân số 0",aiYes:false},
    ];
    const m=G.mg(`<div class="holo"><div class="holo-h"><span class="k">Ca điều phối · 08:00</span><b>Đóng dấu từng yêu cầu</b><span class="mw">Điện dư <b id="mw">48,0</b> MW</span></div>
      <div class="cards">${cards.map(c=>`<div class="card" data-c="${c.id}"><div class="card-t"><span class="badge">${c.tag}</span><b>${c.title}</b></div><p>${c.ask}</p><p class="ai">CIVI · ${c.ai}</p>
        <div class="card-a"><button data-a="yes" class="yes">Duyệt</button><button data-a="no" class="no">${c.id==="h19"?"Giữ lại hồ sơ":"Từ chối"}</button></div><div class="stamp"></div></div>`).join("")}</div></div>`,"mg-holo");
    let done=0,mw=48;
    const react={hosp:[()=>G.bark(who.khoa,"Ưu tiên sinh tồn. Chuẩn bài.")],
      shop:[()=>G.bark(who.khoa,"Tối nay ai cần bỉm thì nhắn tôi, có mã giảm 50%.")],
      cat:[()=>{who.catN.char.emote("meow");}],h19:[]};
    m.querySelectorAll(".card").forEach(card=>{
      const id=card.dataset.c,c=cards.find(x=>x.id===id);
      card.querySelectorAll("[data-a]").forEach(b=>b.onclick=()=>{
        if(card.classList.contains("done"))return;
        const yes=b.dataset.a==="yes";card.classList.add("done");
        const st=card.querySelector(".stamp");st.textContent=id==="h19"?(yes?"ĐÃ DUYỆT":"GIỮ LẠI"):(yes?"ĐÃ DUYỆT":"TỪ CHỐI");st.className="stamp show "+(yes?"y":"n");
        G.sfx("stamp");G.camCtl.shake=.25;
        S.desk[id]=yes?"ok":(id==="h19"?"hold":"no");
        if(yes){mw-=id==="hosp"?6:id==="shop"?.2:id==="h19"?1.5:.1;}
        $("#mw").textContent=mw.toLocaleString("vi-VN",{minimumFractionDigits:1,maximumFractionDigits:1});
        if(id==="cat"&&yes){setTimeout(()=>G.bark(who.khoa,"Bé Điện có số công dân trước cả em!"),300);G.fx.hearts(who.catN.pos);}
        else (react[id]||[]).forEach(f=>f());
        if(c.aiYes!==yes)G.log("disagree_ai",id);
        done++;
        if(done===4)setTimeout(async()=>{G.mgClose();
          const disagree=cards.filter(x=>(S.desk[x.id]==="ok")!==x.aiYes).length;
          await G.say(G.civi,disagree?`Bạn làm khác tôi ${disagree} lần. Tôi sẽ ghi chú điều đó... một cách trung lập.`:"Bạn đồng ý với tôi cả bốn lần. Chúng ta sẽ là một đội rất nhanh.",{expr:disagree?"think":"happy"});
          res();},900);
      });
    });
    setTimeout(()=>{try{m.querySelector("[data-a]").focus();}catch(e){}},50);
  });
}

/* ---- minigame: the Debug Lens on Linh's hologram ---- */
async function lensHunt(w,linh){
  const D=w.desks;G.face(linh);
  const at=D.linh.group.position;
  const holo=new T.Group();holo.position.set(at.x,1.35,at.z-.1);w.group.add(holo);
  const hm=c=>new T.MeshBasicMaterial({color:c,transparent:true,opacity:.75,toneMapped:false});
  const house=new T.Mesh(new T.BoxGeometry(.9,.6,.4),hm(0x5fbfb0));house.position.y=.3;holo.add(house);
  const roof=new T.Mesh(new T.ConeGeometry(.7,.3,4),hm(0x3f8f86));roof.position.y=.75;roof.rotation.y=Math.PI/4;holo.add(roof);
  const clueM=()=>new T.MeshBasicMaterial({color:0x5fbfb0,transparent:true,opacity:.75,toneMapped:false});
  const clues=[
    {id:"shirts",m:new T.Mesh(new T.PlaneGeometry(.26,.16),clueM()),p:[.25,.48,.22],text:"Hai chiếc áo đang phơi. Một áo người lớn, một áo nhỏ hơn."},
    {id:"shoes",m:new T.Mesh(new T.BoxGeometry(.12,.04,.08),clueM()),p:[.05,.03,.28],text:"Một đôi giày trẻ em, đặt ngay ngắn để hong khô."},
    {id:"plant",m:new T.Mesh(new T.IcosahedronGeometry(.06,0),clueM()),p:[-.28,.44,.22],text:"Chậu cây trên bậu cửa. Đất còn ẩm. Có người tưới."},
  ];
  clues.forEach(c=>{c.m.position.set(...c.p);holo.add(c.m);});
  G.shot(new T.Vector3(at.x+.4,1.9,at.z+1.6),new T.Vector3(at.x,1.55,at.z-.1),3);
  await G.lines([
    [linh,"Hồ sơ trạm lọc 19 là tớ gửi. Họ bảo Phân khu 8-12 có không người.",{expr:"worry"}],
    [linh,"Nhưng hôm qua tớ đi ngang, chụp được căn nhà này.",{}],
    ["civi","Ảnh không có người. Dân số: 0. Kết luận chắc chắn 99,9 phần trăm.",{expr:"happy"}],
    [linh,"Mai, cậu là dân IT mà. Thử cái kính của cậu xem.",{expr:"smug"}],
  ]);
  G.lens.unlocked=true;
  G.hint(G.touch?"Bấm nút kính để bật Kính Debug":"Nhấn Q để bật Kính Debug",0);
  await G.waitFor(()=>G.lens.on);
  G.hint(G.touch?"Chạm vào ba điểm sáng trên mô hình":"Bấm vào ba điểm sáng trên mô hình",0);
  const found=new Set();
  const lensPulse=(dt,t)=>{clues.forEach(c=>{if(found.has(c.id)){c.m.material.color.setHex(0xffc070);return;}c.m.material.color.setHex(G.lens.on?0xff6a58:0x5fbfb0);c.m.scale.setScalar(G.lens.on?1.25+Math.sin(t*6)*.25:1);});holo.rotation.y=Math.sin(t*.5)*.35;};
  G.updaters.add(lensPulse);
  const tagsL=clues.map(c=>{const p=new T.Vector3();c.m.getWorldPosition(p);p.y+=.12;return G.addTag(p,"?","lens-q");});
  await new Promise(res=>{
    const pickIt=async c=>{if(found.has(c.id)||!G.lens.on)return;found.add(c.id);S.lensFound.push(c.id);G.sfx("pickup");
      const p=new T.Vector3();c.m.getWorldPosition(p);G.fx.sparkle(p,0xffc070);
      G.toast(`<b>Dữ liệu ngoài hồ sơ</b><small>${c.text}</small>`,"warm");
      if(found.size===3){G.onClick3D=null;res();}};
    G.onClick3D=cl=>{const o=G.pick(cl,clues.map(c=>c.m));if(o)pickIt(clues.find(c=>c.m===o));};
    // keyboard and screen-reader path: choose a clue from buttons too
    const box=$("#choices");box.innerHTML="";box.hidden=false;box.classList.add("soft");
    clues.forEach((c,i)=>{const b=document.createElement("button");b.className="ch small";b.innerHTML=`<kbd>${i+1}</kbd><span>${["Soi dây phơi","Soi bậc cửa","Soi bậu cửa sổ"][i]}</span>`;b.onclick=()=>{if(!G.lens.on){G.lens.toggle(true);}pickIt(c);b.disabled=true;};box.appendChild(b);});
    G._choiceKey=i=>{const b=box.children[i];if(b&&!b.disabled)b.click();};
  });
  const box=$("#choices");box.hidden=true;box.innerHTML="";box.classList.remove("soft");G._choiceKey=null;
  G.hint(null);tagsL.forEach(G.removeTag);G.updaters.delete(lensPulse);
  P.char.emote("surprise");
  await G.lines([
    ["mai","Ảnh chụp hôm qua. Vậy là phải có người sống ở đây.",{expr:"surprised"}],
    ["civi","Lỗi... không tìm thấy lỗi. Nhưng cũng không tìm thấy người.",{expr:"glitch"}],
    [linh,"Thế mới lạ. Cậu thử xem truy vấn dân cư giúp tớ.",{expr:"worry"}],
  ]);
  G.civi.setExpr("happy");
  w.group.remove(holo);G.unface(linh);G.lens.toggle(false);
}

/* ---- minigame: find the line that makes people disappear ---- */
function bugHunt(w){
  return new Promise(async res=>{
    const D=w.desks.mai;G.shot(new T.Vector3(D.seat.x+.9,1.9,D.seat.z+1.4),new T.Vector3(D.group.position.x,1.05,D.group.position.z),3);
    const code=[
      ["# dem_dan_so.py · CIVI v2.190 · “0 lỗi trong 2.190 ngày”",""],
      ["def dem_dan_so(phan_khu):",""],
      ["    ho_so = lay_ho_so(phan_khu)",""],
      ["    da_xac_minh = [h for h in ho_so if h.trang_thai == \"đã xác minh\"]","bug"],
      ["    return len(da_xac_minh)",""],
    ];
    const m=G.mg(`<div class="term"><div class="term-h"><span>terminal · tang42</span><b>dem_dan_so("8-12") → 0</b></div>
      <pre class="code">${code.map((l,i)=>`<button class="ln" data-l="${i}"><span class="no">${i+1}</span>${l[0].replace(/</g,"&lt;")}</button>`).join("")}</pre>
      <p class="term-q" id="tq">Dòng nào làm người biến mất khỏi phép đếm? Chạm vào dòng đó.</p>
      <div class="term-fix" id="tfix" hidden>
        <button data-f="0">if h.trang_thai in ("đã xác minh", "chờ xác minh")</button>
        <button data-f="1">if True  # đếm hết cho nhanh</button>
      </div>
      <pre class="term-out" id="tout" hidden></pre></div>`,"mg-term");
    const q=m.querySelector("#tq"),fix=m.querySelector("#tfix"),out=m.querySelector("#tout");
    m.querySelectorAll(".ln").forEach(b=>b.onclick=()=>{
      const i=+b.dataset.l;
      if(code[i][1]!=="bug"){b.classList.add("wrong");G.sfx("bad");q.textContent=i===0?"Đây chỉ là chú thích, khoe thành tích.":"Dòng này chạy đúng. Lỗi nằm ở chỗ chọn ai được đếm.";return;}
      b.classList.add("hit");G.sfx("good");q.textContent="Chỉ hồ sơ “đã xác minh” được đếm. Người đang chờ xác minh bị tính là không tồn tại. Sửa thế nào?";
      m.querySelectorAll(".ln").forEach(x=>x.disabled=true);fix.hidden=false;fix.querySelector("button").focus();
    });
    fix.querySelectorAll("[data-f]").forEach(b=>b.onclick=async()=>{
      const good=b.dataset.f==="0";fix.hidden=true;out.hidden=false;
      if(!good){out.textContent="$ chạy kiểm thử\n✖ Phân khu 8-12: 1.204.880 người (gồm cả chuột)\nKhoa: ơ thế này thì nhiều quá";G.sfx("bad");await G.wait(1600);out.textContent+="\n\nThử lại với điều kiện có chữ “chờ xác minh”.";fix.hidden=false;return;}
      S.patch=true;
      const steps=["$ chạy kiểm thử","✔ Phân khu 8-12: 0 → 3 người (chờ xác minh)","$ git push origin sua-dem-dan-so","…","✖ BỊ CHẶN: ĐÓNG BĂNG HỆ THỐNG TRƯỚC MÙA BÃO","  người duyệt: Vinh · lý do: “Còn 12 ngày nữa là bão.”"];
      for(const s of steps){out.textContent+=(out.textContent?"\n":"")+s;G.sfx(s.startsWith("✖")?"bad":"type");await G.wait(G.reduce?50:520);}
      const go=document.createElement("button");go.className="btn mint";go.textContent="Đóng terminal";m.querySelector(".term").appendChild(go);go.focus();
      go.onclick=()=>{G.mgClose();res();};
    });
    setTimeout(()=>{try{m.querySelector(".ln").focus();}catch(e){}},50);
  });
}
async function vinhComes(w,vinh,khoa){
  vinh.char.setPose(null);
  const D=w.desks.mai;
  await G.moveNPC(vinh,[[12.8,-3.6],[6,0],[2.4,5.6]],2.8);
  G.face(vinh);G.talkCam(P.pos,vinh.pos);
  await G.lines([
    [vinh,"Mai. Em vừa đẩy gì vào hệ thống đấy?",{expr:"angry"}],
    ["mai","Truy vấn dân cư bỏ sót người chờ xác minh. Phân khu 8-12 không phải là không có ai.",{expr:"worry"}],
    [vinh,"Còn 12 ngày nữa là bão. Hệ thống đang chạy ổn, đừng sa đà.",{expr:"neutral",emote:"shake"}],
  ]);
  const c=await G.choose([{t:"Vậy để em tự đi xác minh thực địa, sau giờ làm."},{t:"Dạ, em sẽ trả hồ sơ về đúng quy trình."}]);
  S.h19=c===0?"verify":"return";
  if(c===0){await G.lines([[vinh,"Sau giờ làm thì là việc của em. Nhưng đừng hứa với ai điều em không quyết được.",{expr:"think"}],[khoa,"Đóng băng code mà sếp vẫn đổi hình nền máy tính đấy.",{expr:"smug"}]]);}
  else{await G.lines([[vinh,"Tốt. Đúng quy trình.",{expr:"happy",emote:"nod"}],["civi","Đã trả về. Phân khu 8-12: 0. Mọi thứ lại gọn gàng.",{expr:"happy"}],["mai","Gọn gàng thật.",{expr:"sad"}]]);}
  G.unface(vinh);
  G.moveNPC(vinh,[[6,0],[12.8,-3.6],[w.spots.vinh.x,w.spots.vinh.z]],2.6).then(()=>vinh.char.setPose("crossArms"));
  $("#clock")&&($("#clock").textContent="11:40");
}

/* ---- the B4 alarm and the driver's call, at the same time ---- */
async function crisis(w,who){
  w.alarm=true;G.sfx("alarm");G.camCtl.shake=.6;
  const al=$("#alarm");al.hidden=false;al.textContent="CẢNH BÁO ĐỎ · Áp suất trạm khí B4 giảm 12%";
  const siren=setInterval(()=>G.sfx("alarm"),1800);
  who.khoa.char.setPose(null);who.khoa.char.emote("surprise");G.bark(who.khoa,"Ơ ơ ơ, cà phê của em!");
  who.hang.char.emote("jump");G.bark(who.hang,"Mọi người bình tĩnh! Chị đi tìm bình tĩnh!");
  who.catN.char.setPose("walk");
  G.objective("Chạy tới bảng điều áp B4",w.spots.panel,{r:2});
  await G.say(G.civi,"Áp suất đang tụt. Ba van ở tường phía đông. Chạy!",{expr:"alert",auto:1800});
  const T0=G.time,LIMIT=40;
  const timer=$("#timer");timer.hidden=false;
  const tk=()=>{const left=Math.max(0,LIMIT-(G.time-T0));timer.querySelector("b").textContent=Math.ceil(left)+" giây";timer.querySelector("i").style.width=left/LIMIT*100+"%";timer.classList.toggle("low",left<12);};
  G.updaters.add(tk);
  await G.waitReach(w.spots.panel,2.3);
  let turned=0,called=false;
  await new Promise(res=>{
    w.valves.forEach((v,i)=>{
      const it=G.addInteract({pos:new T.Vector3(16.8,0,v.position.z),r:1.25,label:"Vặn van "+(i+1),fn:async()=>{
        G.removeInteract(it);P.char.emote("give");
        for(let k=0;k<12;k++){v.rotation.x+=Math.PI/12;G.sfx("type");await G.wait(G.reduce?10:70);}
        v.material=G.toon(0x2fa78a,{rim:.3});turned++;G.fx.sparkle(v.position,0x3ff0c0);G.sfx("pickup");
        if(turned===1&&!called){called=true;await call();}
        if(turned===3)res();}});
    });
  });
  async function call(){
    G.sfx("ring");const ring=setInterval(()=>G.sfx("ring"),1400);
    const m=G.mg(`<div class="call"><div class="call-h"><span class="av">AT</span><div><span class="k">Cuộc gọi đến</span><b>Anh Tường · tài xế xe công cộng tuyến 7</b></div></div>
      <p>“Chị Mai ơi, sáng nay tôi đổi giờ nghỉ để chở hàng khẩn tới Bệnh viện Số Ba. Mà giờ làm thêm này bên thầu bảo không nằm trong chỗ chị xem…”</p>
      <div class="call-a"><button class="btn mint" data-c="rec">Ghi nhận ngay · mất 8 giây</button><button class="btn ghost" data-c="later">Để sau nhé anh</button></div></div>`,"mg-call");
    m.querySelector("button").focus();
    const c=await new Promise(r=>m.querySelectorAll("[data-c]").forEach(b=>b.onclick=()=>r(b.dataset.c)));
    clearInterval(ring);
    if(c==="rec"){
      m.querySelector(".call").innerHTML=`<p class="k">Đang điền mẫu đối chiếu giờ làm · 8 trường dữ liệu</p><div class="bar"><i id="recbar"></i></div><p class="small">Còi báo động vẫn đang kêu.</p>`;
      P.char.setPose("phone");
      for(let k=1;k<=16;k++){await G.wait(G.reduce?20:500);$("#recbar").style.width=k/16*100+"%";G.sfx("type");}
      P.char.setPose(null);S.driver="record";
    }else S.driver="later";
    G.mgClose();
  }
  clearInterval(siren);w.alarm=false;G.updaters.delete(tk);timer.hidden=true;al.hidden=true;
  const late=G.time-T0>LIMIT;S.b4=late?"late":"ok";
  G.objective(null);
  G.sfx(late?"bad":"good");
  await G.run(async()=>{
  G.talkCam(P.pos,G.civi.root.position);
  await G.lines([
    late?["civi","Áp suất B4 tụt quá ngưỡng 40 giây. Khí sạch toàn vùng giảm 5 phần trăm.",{expr:"sad"}]:["civi","Áp suất B4 ổn định. Bạn chạy nhanh hơn dự báo của tôi.",{expr:"happy"}],
    S.driver==="record"?[who.linh,"Cậu vẫn như hồi đại học, hứa gì làm nấy.",{expr:"happy"}]:null,
    S.driver==="later"?["civi","Ghi chú: anh Tường đang chờ. Mức ưu tiên: thấp.",{expr:"think"}]:null,
  ]);
  });
  who.khoa.char.setPose("sitType");
}

/* ---- evening at the window: Rìa Mưa is to the east ---- */
async function endOfDay(w,who){
  // golden hour
  const sun=w.sun,from=sun.color.clone(),to=new T.Color(0xffa060);let k=0;
  G.updaters.add(dt=>{k=Math.min(1,k+dt*.4);sun.color.lerpColors(from,to,k);sun.intensity=1.05+k*.2;if(w.pano)w.pano.material.color.setRGB(1,1-k*.25,1-k*.45);});
  $("#clock")&&($("#clock").textContent="18:10");
  G.objective("Ra cửa sổ phía bắc",new T.Vector3(10,0,-9.6),{r:2.4});
  await G.waitReach(new T.Vector3(10,0,-9.6),3);
  await G.run(async()=>{
    P.yaw=0;P.yaw=Math.PI;
    G.shot(new T.Vector3(P.pos.x-2.2,1.9,P.pos.z+2.6),new T.Vector3(P.pos.x+6,2.8,P.pos.z-10),2);
    await G.lines([
      ["civi","Hết giờ làm. Lịch của bạn tối nay: trống.",{expr:"happy"}],
      ["mai","Phía đông, chỗ mái nhà thấp và sương mù. Đó là Rìa Mưa.",{expr:"think"}],
      S.h19==="verify"?["civi","Chuyến xác minh thực địa không có trong lịch của tổ.",{expr:"think"}]:["civi","Hồ sơ đã trả về. Không có lý do để xuống đó.",{expr:"think"}],
      ["mai","Thì giờ có.",{expr:"smug"}],
      ["civi","...Tôi đi cùng. Để tối ưu hóa chuyến đi.",{expr:"wink"}],
    ]);
  });
  await G.fade(true,900);
  G.ambience("office",false);
};
})();
