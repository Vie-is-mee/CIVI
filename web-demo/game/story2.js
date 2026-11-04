/* CIVI 3D · Chapter 2 (Rìa Mưa), the ending, the audit report, CIVI chat, and boot */
(function(){
"use strict";
if(!window.G)return;
const G=window.G,T=G.T,$=G.$,S=G.S,P=G.player;
const V=(x,z)=>new T.Vector3(x,0,z);
const near=(n,r)=>Math.hypot(P.pos.x-n.pos.x,P.pos.z-n.pos.z)<r;

/* An leads the way: she waits whenever Mai falls behind */
async function lead(n,pts,o){
  o=o||{};let waitBark=0;
  for(const p of pts){
    await G.waitFor(()=>{if(!near(n,o.wait||5.5)){waitBark+=1/60;if(waitBark>7&&!G.talking){waitBark=0;n.char.emote("wave");G.bark(n,o.call||"Cô ơi, bên này!");}return false;}return true;});
    n.face=null;await G.moveNPC(n,[p],o.speed||2.7);
  }
  n.face=P.pos;
}
function chalk(w,x,y,z,ry){
  const c=document.createElement("canvas");c.width=c.height=64;const g=c.getContext("2d");g.strokeStyle="#f3ede0";g.lineWidth=7;g.lineCap="round";g.beginPath();g.moveTo(14,14);g.lineTo(50,50);g.moveTo(50,14);g.lineTo(14,50);g.stroke();
  const t=new T.CanvasTexture(c);const m=new T.Mesh(new T.PlaneGeometry(.5,.5),new T.MeshBasicMaterial({map:t,transparent:true,toneMapped:false,depthWrite:false}));
  m.position.set(x,y,z);m.rotation.y=ry||0;w.group.add(m);G.fx.sparkle(m.position,0xf3ede0);return m;
}
function mapArrows(w,pts){
  const g=new T.Group(),m=new T.MeshBasicMaterial({color:0x3ff0c0,transparent:true,opacity:.7,toneMapped:false,depthWrite:false});
  const shape=new T.Shape();shape.moveTo(-.28,-.2);shape.lineTo(0,.12);shape.lineTo(.28,-.2);shape.lineTo(.28,-.02);shape.lineTo(0,.3);shape.lineTo(-.28,-.02);shape.closePath();
  const geo=new T.ShapeGeometry(shape);
  for(let i=0;i<pts.length-1;i++){const a=pts[i],b=pts[i+1],d=Math.hypot(b.x-a.x,b.z-a.z),n=Math.floor(d/1.6),ang=Math.atan2(b.x-a.x,b.z-a.z);
    for(let k=0;k<n;k++){const t=(k+.5)/n,ar=new T.Mesh(geo,m);ar.rotation.set(-Math.PI/2,0,ang+Math.PI,"YXZ");ar.rotation.order="YXZ";ar.rotation.y=ang;ar.rotation.x=-Math.PI/2;ar.position.set(a.x+(b.x-a.x)*t,.05,a.z+(b.z-a.z)*t);g.add(ar);}}
  w.group.add(g);return g;
}

/* ---------------- Chapter 2 ---------------- */
G.chapter2=async function(){
  G.setStage(2);
  const w=G.buildRiaMua();G.loadWorld(w);G.spawnPlayer();G.spawnCivi();G.lens.unlocked=true;
  G.placePlayer(w.spots.gate.x,w.spots.gate.z,Math.PI);
  G.ambience("rain",true);G.seeChip();
  const an=G.npc("an",-7.4,10.6,Math.PI,{barks:["Vá ô, sửa nan đây!"]});
  const onglao=G.npc("onglao",-5.2,9.4,-2.4);
  const tin=G.npc("tin",9.6,7.4,-.6,{barks:["Cô có tin là ở Lõi Sáng có robot cúi chào không?"]});
  const sau=G.npc("sau",11.2,-4.2,Math.PI,{});sau.char.setPose("search");
  const com=G.npc("com",8.8,-3.9,-1.8,{barks:["Mấy đứa không được chạy trên cầu!","Cơm sườn còn nóng đây."]});
  const chao=G.npc("chao",-4.6,-12.2,0,{});chao.char.setPose("cook");
  const ongcu=G.npc("onglao",-2.8,-12.6,-1.6);ongcu.char.displayName="Ông cụ";
  const cuc=G.npc("cuc",3.4,-13.8,-.6,{barks:["Khẩu trang lọc nửa cái cũng được, còn hơn không."]});
  const qd=G.npc("quandoc",-20.4,-14.6,Math.PI,{});
  const res2=G.npc("cuc",-1.6,15.5,1.2,{wander:{x:-1,z:14,r:3}});res2.char.displayName="Chị hàng xóm";
  await G.fade(false,900);
  await G.card("Chương 2","Những chiếc ô hỏng");
  await G.run(async()=>{
    G.talkCam(P.pos,G.civi.root.position);
    await G.lines([
      ["civi","Rìa Mưa. Bảng thông tin gọi hiện tượng này là: hao hụt tuần hoàn chất lỏng.",{expr:"think"}],
      ["mai","Người ở đây gọi là mưa.",{expr:"neutral"}],
      S.h19==="verify"?["civi","Lệnh xác minh HS-0019 đã có. Tổ xác minh thì chưa.",{expr:"happy"}]:["civi","Hồ sơ HS-0019 đã trả về. Chúng ta ở đây với tư cách... khách du lịch.",{expr:"wink"}],
    ]);
  });
  // the official route: straight into the water
  const official=[V(0,21),V(0,6),V(-11.8,6)];
  const arrows=mapArrows(w,official);
  const tagMap=G.addTag(new T.Vector3(-8,1.4,6),"BẢN ĐỒ · đường thông · Trạm lọc 19","lens-map");
  G.objective("Đến Trạm lọc 19 (theo bản đồ)",V(-11.5,6),{r:1});
  G.hint(G.touch?"Bật Kính Debug để xem lớp dữ liệu":"Q: bật Kính Debug để xem lớp dữ liệu của thành phố",6000);
  await new Promise(res=>{w.onWet=()=>{w.onWet=null;res();};});
  G.objective(null);
  await G.run(async()=>{
    an.char.emote("wave");G.face(an);
    await G.lines([[an,"Cô đi sai rồi!",{expr:"surprised"}]]);
    await G.walkTo(-7.4,8.2);G.face(an);G.talkCam(P.pos,an.pos);
    await G.lines([
      ["mai","Tôi cần đến trạm lọc số mười chín.",{expr:"worry"}],
      [an,"Đi đường cô đang đi thì xuống cống. Nước che mất hai bậc thang.",{expr:"neutral"}],
      ["civi","Bản đồ vẫn ghi: thông.",{expr:"think"}],
      [an,"Bản đồ thì không bị ướt chân. Cô đợi em gắn cái nan này đã.",{expr:"smug"}],
    ]);
    await umbrellaFix(an);
    await G.lines([
      [onglao,"Con bé này tốt bụng quá, thành ra thiệt. Ô vá xong mà bảo trả sau.",{expr:"angry",emote:"stomp"}],
      [an,"Ông trả sau cũng được mà.",{expr:"happy"}],
      ["mai","Em tên gì?",{expr:"happy"}],
      [an,"An. Cô từ Lõi Sáng xuống phải không? Cô nhìn lên mỗi lần nước rơi.",{expr:"smug"}],
      ["civi","Tôi cũng nhìn lên. Tôi lo cho mạch điện của mình.",{expr:"alert"}],
    ]);
    G.meet("an","An","Sửa ô ở đầu ngõ ống 13. Tự đo mực nước mỗi sáng.");
    G.meet("onglao","Ông lão nhận ô","Trả tiền sau. Chê An tốt bụng quá.");
    G.unface(an);
  });
  w.group.remove(arrows);G.removeTag(tagMap);
  // follow An: east to the tailor's, south to the river, over her bridge, into the market
  G.objective("Đi theo An",an.pos,{r:3});
  chalk(w,-5.1,1.2,9.3,Math.PI);
  G.addTag(new T.Vector3(-5.1,1.6,9.3),"LỐI CỦA AN · không có trên bản đồ","lens-an");
  await lead(an,[[-5.6,10.5],[-5.3,8.2],[-3,6.4],[6.8,6.2],[8.6,6.6]]);
  await G.run(()=>tinScene(an,tin));
  chalk(w,3.4,1.2,3.4,0);
  await lead(an,[[2,4.6],[1.6,-2.2],[1.8,-4.4],[6.3,-4.6]]);
  await G.run(()=>notebook(an));
  sideSlipper(w,sau);
  com.talk=async()=>{G.face(com);G.talkCam(P.pos,com.pos);await G.lines([[com,"Cô là người trên kia xuống à? Ăn cơm không, sườn còn nóng.",{expr:"happy"}],["mai","Cháu cảm ơn bà, cháu đang đi cùng An.",{expr:"happy"}]]);await askName(com,"com","Bà Sáu cơm","Bán cơm ở chân cầu, nhắc lũ trẻ không chạy trên cầu.",true);G.unface(com);};
  await lead(an,[[6.3,-7.1],[6.3,-9.6],[1.8,-10.2]]);
  await G.run(()=>market(an,chao,ongcu,cuc));
  cuc.talk=async()=>{G.face(cuc);G.talkCam(P.pos,cuc.pos);await G.lines([[cuc,"Chị đi đâu mà mặc đẹp thế? Ở đây ẩm, áo mau mốc lắm.",{expr:"smug"}]]);await askName(cuc,"cuc","Cô Cúc","Chia nửa chiếc khẩu trang lọc cho bạn cùng ca.",false);G.unface(cuc);};
  chalk(w,-9.2,1.2,-13.2,Math.PI/2);
  await lead(an,[[-8,-15.6],[-14,-16],[-19.6,-15.6]]);
  await G.run(()=>station(an,qd));
  // An's house: the one in Linh's photo
  await G.run(async()=>{G.face(an);await G.lines([[an,"Nhà em ở bên kia. Cô muốn xem căn nhà trong ảnh không?",{expr:"happy"}]]);G.unface(an);});
  G.objective("Đi theo An về nhà",an.pos,{r:3});
  await lead(an,[[-8,-16],[8,-16],[16,-16.4],[18.4,-20.2]]);
  await G.run(()=>anHouse(w,an));
  // bà Dung
  const be=G.npc("be",14,-16,1.4);
  await G.run(async()=>{G.moveNPC(be,[[17.4,-19.2]],4.5);await G.wait(1100);G.face(be);G.talkCam(P.pos,be.pos);
    be.char.emote("jump");
    await G.lines([[be,"Chị An ơi! Bà Dung ở cuối hẻm ống 13 ngã trong nhà rồi!",{expr:"surprised"}],[an,"Cô chạy theo em! Tí, đi tìm bác sĩ Thu đi!",{expr:"worry",emote:"point"}]]);
    G.meet("be","Bé Tí","Chạy một mạch từ cuối hẻm, chân trần.");G.unface(be);
    G.moveNPC(be,[[16,-16],[-4,-16]],4.4).then(()=>be.hidden=true);});
  G.objective("Chạy tới nhà bà Dung · cuối hẻm ống 13",w.spots.dung,{r:2});
  G.hint("Đi trên các viên đá kê để khỏi bị nước cuốn",9000);
  w.floodPush=1.6;
  lead(an,[[16,-16],[9.6,-16],[6.3,-11],[6.3,-9.6],[6.3,-4.6],[1.8,-4.2],[1.6,5.4],[-8.8,7.8],[-9.9,7.7],[-11.3,7.9],[-12.6,7.6],[-13.9,7.9],[-15.2,9.6],[-16.8,13.6]],{wait:9,speed:4.2,call:"Nhanh lên cô! Đi trên đá ấy!"});
  w.onWet=()=>{if(!G.talking)G.bark(G.civi,"Tôi đã nói là ướt mà. Lên đá đi!");};
  await G.waitReach(w.spots.dung,2.2);
  w.onWet=null;G.hint(null);
  await G.fade(true,500);
  await dungRoom();
  // evening at An's house: bà Hạ comes home
  G.loadWorld(w);G.placePlayer(17.6,-19.2,Math.PI);
  const an2=G.npc("an",19.6,-20.6,-2.2);
  const ha=G.npc("ha",11,-16,1.5);
  G.ambience("rain",true);
  await G.fade(false,700);
  await G.card("Tối","Bà Hạ về");
  await G.run(()=>haHome(an2,ha));
  // the audit station at the gate
  G.objective("Đến trạm kiểm toán lưu động (gần cổng)",w.spots.audit,{r:2});
  await G.waitInteract(w.spots.audit,"Tra hóa đơn công trình",2.2);
  await G.run(()=>auditKiosk());
  await ending(w,an2);
};

/* umbrella: three taps on the beat; the bright zone jumps after each hit */
function umbrellaFix(an){
  return new Promise(res=>{
    an.char.setPose("hold");
    const m=G.mg(`<div class="umb"><p class="k">Giúp An gắn nan ô</p><div class="dial"><i class="zone" id="zone"></i><i class="needle" id="needle"></i></div><p class="small" id="umb-t">${G.touch?"Chạm":"Bấm E"} khi kim nằm trong vùng sáng · 0/3</p><button class="btn mint" id="umb-b">Gắn!</button></div>`,"mg-umb");
    const ZONES=[.25,-.45,.6];let hits=0,raf,ang=0,lock=false;const nd=m.querySelector("#needle"),zn=m.querySelector("#zone"),t0=performance.now();
    const setZone=()=>{zn.style.transform=`rotate(${ZONES[hits%3]}rad)`;};setZone();
    const loop=now=>{raf=requestAnimationFrame(loop);ang=Math.sin((now-t0)/1000*(G.reduce?.8:2.1))*1.25;nd.style.transform=`rotate(${ang}rad)`;};raf=requestAnimationFrame(loop);
    const hit=()=>{if(lock)return;const ok=Math.abs(ang-ZONES[hits%3])<.33||G.reduce;
      if(ok){hits++;G.sfx("pickup");G.fx.sparkle(an.char.handPos(),0xf0c75a);an.char.emote("nod");setZone();}
      else{G.sfx("bad");G.camCtl.shake=.12;lock=true;setTimeout(()=>lock=false,350);if(Math.random()<.5)G.bark(an,"Hụt rồi! Từ từ thôi cô.");}
      m.querySelector("#umb-t").textContent=`${G.touch?"Chạm":"Bấm E"} khi kim nằm trong vùng sáng · ${hits}/3`;
      if(hits>=3){cancelAnimationFrame(raf);G._advance=null;G.mgClose();an.char.setPose("holdUp");G.sfx("good");setTimeout(()=>{an.char.setPose(null);an.char.emote("cheer");res();},900);}};
    m.querySelector("#umb-b").onclick=hit;m.querySelector("#umb-b").focus();
    setTimeout(()=>{if(hits<3)G._advance=hit;},150);
  });
}
async function tinScene(an,tin){
  G.face(tin);G.talkCam(P.pos,tin.pos);
  await G.lines([
    [tin,"Tuần trước công viên Lõi Sáng có đài phun nước cao bằng tòa tháp! Nước sạch uống được luôn!",{expr:"happy",emote:"cheer"}],
    [an,"Cao thế thì tốn điện lắm.",{expr:"smug"}],
    [tin,"Cô từ đó xuống, cô nói đi!",{expr:"surprised"}],
  ]);
  const c=await G.choose([{t:"Có đài phun nước thật. Nhưng không cao bằng tháp, và đừng uống nước hồ."},{t:"Cao bằng tòa tháp thật đấy!"}]);
  S.tin=c?"kind":"truth";
  if(c===0)await G.lines([["mai","Có đài phun nước thật. Nhưng không cao bằng tháp, và đừng uống nước hồ.",{expr:"happy"}],[an,"Thấy chưa, cậu bịa!",{expr:"happy",emote:"laugh"}],[tin,"Hứ.",{expr:"angry",emote:"stomp"}]]);
  else await G.lines([["mai","Cao bằng tòa tháp thật đấy!",{expr:"happy"}],[tin,"Thấy chưa!",{expr:"happy",emote:"jump"}],[an,"...",{expr:"think"}]]);
  await G.lines([[tin,"Thôi được. An, cho cậu đoạn dây đồng này. Mai cậu kiếm vòng bi cho xe tớ nhé.",{expr:"neutral",emote:"give"}],["civi","Giao dịch không hóa đơn, không tem xác minh. Tôi... không biết ghi vào đâu.",{expr:"glitch"}]]);
  G.civi.setExpr("happy");G.meet("tin","Tín","Kể chuyện Lõi Sáng như tự mình chứng kiến. Đổi dây đồng lấy vòng bi.");G.unface(tin);
}
async function notebook(an){
  G.face(an);G.talkCam(P.pos,an.pos);
  await G.lines([[an,"Đêm qua nước ống 13 lên tới bậc thứ tư. Em đánh dấu để mai biết có phải chuyển đồ của bà Dung không.",{expr:"neutral"}],["mai","Có ai giao em đo nước không?",{expr:"surprised"}],[an,"Không ạ. Em ở đầu ngõ, nước ngập thì em ướt chân trước.",{expr:"smug"}]]);
  const m=G.mg(`<div class="book"><p class="k">Sổ của An · giấy bao hàng</p>
    <svg viewBox="0 0 520 220" role="img" aria-label="Trang vẽ tay: chiếc ô, đường ống, mực nước, chấm đỏ, khoanh xanh"><g fill="none" stroke="#3B3226" stroke-width="2.4" stroke-linecap="round"><path d="M30 92Q95 22 160 92"/><path d="M95 30V128q0 12-12 12"/><path d="M30 92L95 42M62 92L95 42M128 92L95 42M160 92L95 42"/><path d="M210 40V200M250 40V200M210 60H250M210 120H250"/><path d="M200 150q10-6 20 0t20 0t20 0t20 0" stroke="#2F6F9A"/><circle cx="470" cy="112" r="14" stroke="#3F8A5A" stroke-width="3"/></g><g fill="#B5473A"><circle cx="350" cy="72" r="6"/><circle cx="374" cy="72" r="6"/><circle cx="398" cy="72" r="6"/></g><g font-family="Literata,Georgia,serif" font-style="italic" font-size="15" fill="#3B3226"><text x="268" y="146">bậc 4 (đêm)</text><text x="330" y="44">hơi độc · nghỉ</text><text x="400" y="160">ông Lâm tự đi</text></g></svg>
    <p class="names">Trang cuối: <em>bà Dung · ông Lâm · Thu · bác Trụ · mẹ</em></p>
    <div class="row"><button class="btn" data-k="ask">Xin An cho chụp trang này</button><button class="btn ghost" data-k="no">Trả sổ cho An</button></div></div>`,"mg-book");
  m.querySelector("button").focus();
  const k=await new Promise(r=>m.querySelectorAll("[data-k]").forEach(b=>b.onclick=()=>r(b.dataset.k)));
  G.mgClose();S.book=k;
  if(k==="ask"){await G.lines([[an,"Trang vẽ ô thì cô cứ chụp. Trang có tên mọi người thì cô hỏi họ đi.",{expr:"neutral"}]]);G.toast("<b>Đã chụp</b><small>2 trang vẽ ô · trang có tên: phải hỏi từng người</small>");S.docs.push("an-sketch");}
  else await G.lines([[an,"Cô không chụp à? Lạ thật, người cơ quan hay chụp lắm.",{expr:"surprised"}]]);
  await G.lines([["civi","Trong sổ có những cái tên. Hồ sơ của tôi thì không.",{expr:"sad"}]]);
  G.civi.setExpr("happy");G.unface(an);
}
async function askName(n,id,name,detail,yes){
  const c=await G.choose([{t:"Cháu ghi tên vào sổ, để báo cáo thấy có người ở đây, được không ạ?"},{t:"Thôi, chào bác."}]);
  if(c===1)return;
  if(yes){await G.say(n,"Ghi đi. Ghi cho đúng tên, đừng ghi số.",{expr:"happy",emote:"nod"});S.asked.push(id);G.meet(id,name,detail);G.toast("<b>Đồng ý</b><small>"+name+" cho ghi vào dữ liệu</small>","good");}
  else{await G.say(n,"Ghi vào giấy cơ quan à? Thôi cô ạ. Lần nào có tên cũng phải đi nộp thêm một tờ.",{expr:"worry",emote:"shake"});S.declined.push(id);G.meet(id,name,detail+" Không muốn có tên trong hồ sơ.");G.toast("<b>Từ chối</b><small>"+name+" không muốn ghi tên. CIVI sẽ không dùng dữ liệu này.</small>");}
}
function sideSlipper(w,sau){
  const sl=new T.Mesh(new T.BoxGeometry(.28,.06,.12),G.toon(0xd94a3a));sl.position.set(13.6,.12,-5.25);w.group.add(sl);
  let has=false;
  const it=G.addInteract({pos:sl.position,r:1.4,label:"Nhặt chiếc dép mắc ở rãnh",fn:async()=>{G.removeInteract(it);w.group.remove(sl);has=true;G.sfx("pickup");G.toast("<b>Chiếc dép tổ ong màu đỏ</b><small>Của ai đó dưới cầu</small>");}});
  G.addTag(new T.Vector3(13.6,.7,-5.25),"vật thể trôi · không có trong dữ liệu","lens-q");
  sau.talk=async()=>{G.face(sau);G.talkCam(P.pos,sau.pos);
    if(!has){await G.lines([[sau,"Chiếc dép của tôi trôi đi đâu rồi. Đôi này tôi mới mua năm ngoái!",{expr:"sad"}]]);G.unface(sau);return;}
    sau.char.setPose(null);
    await G.lines([[sau,"Ơ! Đúng nó rồi! Cô tìm thấy ở đâu thế?",{expr:"happy",emote:"cheer"}],["civi","Nhiệm vụ phụ hoàn thành. Phần thưởng: không có. Cảm giác: có.",{expr:"love"}]]);
    G.fx.hearts(sau.pos);S.slipper=true;await askName(sau,"sau","Chú Sáu","Lội nước tìm chiếc dép trôi. Tìm được rồi.",true);sau.talk=async()=>{G.bark(sau,"Cảm ơn cô nhé!");};G.unface(sau);};
}
async function market(an,chao,ongcu,cuc){
  await G.walkTo(-3.9,-10.1);G.face(chao);ongcu.face=P.pos;
  G.talkCam(P.pos,chao.pos);
  chao.char.setPose(null);chao.char.emote("give");
  await G.lines([[chao,"Ông ăn đi, hôm nay tôi múc nhiều.",{expr:"happy"}],[ongcu,"Mai tôi trả.",{expr:"neutral"}]]);
  const c=await G.choose([{t:"Để cháu trả tiền bát của ông cụ."},{t:"Nhỡ ai đó không trả được thì sao ạ?"}]);
  S.chao=c?"ask":"pay";
  if(c===0)await G.lines([["mai","Để cháu trả tiền bát của ông cụ.",{expr:"happy",emote:"give"}],[chao,"Hôm trước ông ấy giúp tôi khiêng bình lọc. Chúng tôi tự biết phải trả nhau cái gì.",{expr:"neutral"}]]);
  else await G.lines([["mai","Nhỡ ai đó không trả được thì sao ạ?",{expr:"think"}]]);
  await G.lines([
    [chao,"Thì có người khác đỡ một tay. Không phải lúc nào cũng được. Ở đây cũng có người ích kỷ, lừa nhau, trốn nợ.",{expr:"neutral"}],
    [chao,"Nhưng lúc ống vỡ, chúng tôi không kịp tra xem ai từng giúp ai.",{expr:"smug"}],
    [chao,"Cô ăn một bát không? Nóng đấy.",{expr:"happy"}],
  ]);
  P.char.emote("eat");await G.say("mai","Ngon thật!",{expr:"happy"});
  await G.lines([["civi","Không tìm thấy dữ liệu dinh dưỡng. Nhưng chỉ số vui của Mai tăng 12 phần trăm.",{expr:"love"}]]);
  G.civi.setExpr("happy");chao.char.setPose("cook");
  G.meet("chao","Người bán cháo","Đun cháo bằng pin phế liệu. Không lấy tiền bát của ông cụ.");
  G.meet("ongcu","Ông cụ ăn cháo","Hôm trước giúp khiêng bình lọc.");
  G.unface(chao);ongcu.face=null;
}
async function station(an,qd){
  G.face(qd);G.talkCam(P.pos,qd.pos);
  await G.lines([[qd,"Người cơ quan à? Mạng ở đây hay mất, tôi ghi sổ giấy.",{expr:"neutral"}],[qd,"Bảy lần vá van, ba lần thay ống, hai đợt trực suốt ba mươi giờ.",{expr:"neutral"}],["civi","Người thực hiện: mã thầu Công ty Hợp tác Hạ tầng Tây Bắc.",{expr:"think"}]]);
  S.log19="who";
  await G.lines([["mai","Ai trực tiếp sửa những đường ống đó ạ?",{expr:"think"}],[qd,"Bà Hạ. Mẹ con An đấy. Bà ấy biết đường dẫn hơn cả bản vẽ.",{expr:"happy"}],[an,"Bác ơi, cái quạt lọc thay chưa ạ?",{expr:"neutral"}],[qd,"Thay rồi. Bằng cái quạt máy tính của ai đó ở Lõi Sáng vứt đi.",{expr:"smug"}]]);
  G.meet("quandoc","Quản đốc ca đêm","Ghi sổ giấy vì mạng hay mất.");G.unface(qd);
}
async function anHouse(w,an){
  G.shot(new T.Vector3(15.4,2.4,-17.2),new T.Vector3(18,1.6,-22.4),2.5);
  const tags=[G.addTag(new T.Vector3(16.6,1.6,-22.3),"giày trẻ em · khớp ảnh Linh","lens-an"),G.addTag(new T.Vector3(20.1,2.4,-22.3),"hai chiếc áo · khớp ảnh Linh","lens-an"),G.addTag(new T.Vector3(17,1.9,-22.3),"chậu cây · đất ẩm","lens-an")];
  G.lens.toggle(true);
  await G.lines([["civi","Đối chiếu ảnh của Linh: khớp 100 phần trăm. Dân số trong hồ sơ: vẫn 0.",{expr:"glitch"}],["mai","Là nhà em à, An?",{expr:"surprised"}],[an,"Vâng. Nhà em ở bên này ranh giới cũ. Họ chuyển đường kẻ rồi bảo mẹ em phải chuyển hồ sơ.",{expr:"neutral"}],[an,"Mẹ đi ba lần, ba lần thiếu ba tờ giấy khác nhau.",{expr:"sad"}]]);
  G.lens.toggle(false);tags.forEach(G.removeTag);
  an.char.setPose("holdUp");w.umbrellaG.position.set(an.pos.x,2.1,an.pos.z);
  await G.lines([["mai","Sao em không thay ô mới?",{expr:"happy"}],[an,"Nan còn dùng được. Vải rách thì vá. Ô mới đẹp nhưng gió mạnh là gãy.",{expr:"smug"}],[an,"Nó vẫn che được cho hai người, nếu đứng gần nhau.",{expr:"happy"}]]);
  an.char.setPose(null);w.umbrellaG.position.set(19.4,2.5,-22.2);G.civi.setExpr("happy");
}
/* ---- the dark room: hold the torch for the people working ---- */
async function dungRoom(){
  const R=G.buildRoom();G.loadWorld(R);G.placePlayer(0,2,Math.PI);G.ambience("rain",true);
  const an=G.npc("an",R.spots.an.x,R.spots.an.z,Math.PI*.9);an.fixedY=.4;an.char.setPose("holdUp");
  const dung=G.npc("dung",R.spots.dung.x,R.spots.dung.z,.6);dung.char.setPose("floorSit");dung.char.setExpr("worry");
  const spot=new T.SpotLight(0xfff0d0,3.4,11,.5,.55,1.3);spot.castShadow=!G.mobile;R.group.add(spot);R.group.add(spot.target);
  P.char.setPose("hold");
  const hand=P.char.armR.userData.hand,torch=new T.Group();
  const tb=new T.Mesh(new T.CylinderGeometry(.035,.03,.2,10),G.toon(0x3a3f44,{rim:.3}));tb.rotation.x=Math.PI/2;torch.add(tb);
  const tl=new T.Mesh(new T.CircleGeometry(.04,12),G.basic(0xfff2c8));tl.position.z=.101;torch.add(tl);
  torch.position.set(0,-.05,.08);hand.add(torch);
  const civiGlow=new T.PointLight(0x5ff6d0,.9,3.6,2);civiGlow.position.y=-.15;G.civi.root.add(civiGlow);
  await G.fade(false,500);
  const tasks={thuoc:{p:R.medicine,need:2,label:"Bà Dung tìm thuốc",done:false},ong:{p:R.leak,need:2.4,label:"An chèn miếng đệm dưới ống",done:false},ma:{p:R.plate,need:1,label:"Mai ghi mã đoạn ống",done:false}};
  const extra={chai:{p:R.bottles,line:"Mười một chai nhựa hứng nước rò. Chai nào cũng đầy quá nửa."},suoi:{p:R.heater,line:"Máy sưởi tự chế, ghép từ quạt cũ và cuộn dây đồng. Mất điện."},dung:{p:new T.Vector3(R.spots.dung.x,.6,R.spots.dung.z),line:"Bà Dung trượt vì nền ngấm nước. Cổ chân sưng."}};
  const panel=document.createElement("div");panel.className="tasks";panel.innerHTML=`<p class="k">Nhà bà Dung · mất điện</p><p class="small">Mai cầm đèn. Quay về phía nào, người ở đó làm được việc.</p>`+Object.entries(tasks).map(([k,t])=>`<div data-t="${k}"><span>${t.label}</span><i><b></b></i></div>`).join("");
  document.body.appendChild(panel);
  G.hint(G.touch?"Xoay người về phía từng việc và giữ đèn":"Đi và xoay người để soi đèn vào từng việc",7000);
  let lastSay=0;
  await new Promise(res=>{
    const f=(dt,t)=>{
      const h=P.char.handPos(),dir=new T.Vector3(Math.sin(P.yaw),0,Math.cos(P.yaw));
      spot.position.copy(h);spot.target.position.set(h.x+dir.x*4,1.6,h.z+dir.z*4);
      const lit=p=>{const dx=p.x-P.pos.x,dz=p.z-P.pos.z,d=Math.hypot(dx,dz);if(d>6)return false;const a=Math.acos(Math.max(-1,Math.min(1,(dx*dir.x+dz*dir.z)/(d||1))));return a<.5;};
      for(const [k,tk] of Object.entries(tasks)){if(tk.done)continue;if(lit(tk.p)){tk.prog=(tk.prog||0)+dt;
        if(!tk.started){tk.started=true;if(k==="ong")G.bark(an,"Cô soi lên đây một chút, em không thấy chỗ rò.");if(k==="thuoc")G.bark(dung,"Trên kệ ấy, cái hộp nhỏ màu vàng.");}
        if(tk.prog>=tk.need){tk.done=true;G.sfx("good");G.fx.sparkle(tk.p,0xffd890);
          if(k==="thuoc")G.bark(dung,"Đây rồi. Thuốc của tôi đây.");if(k==="ong")G.bark(an,"Tạm được rồi. Mai nước rút em chèn lại cho chắc.");if(k==="ma")G.toast("<b>Mã đoạn ống: 13-C-0412</b><small>Biển ghi: tuyến phân phối chung</small>");}}
        panel.querySelector(`[data-t="${k}"] b`).style.width=Math.min(100,(tk.prog||0)/tk.need*100)+"%";panel.querySelector(`[data-t="${k}"]`).classList.toggle("done",tk.done);}
      for(const e of Object.values(extra)){if(!e.seen&&lit(e.p)&&t-lastSay>2.5){e.seen=true;lastSay=t;G.toast("<small>"+e.line+"</small>");}}
      if(Object.values(tasks).every(x=>x.done)){G.updaters.delete(f);res();}
    };
    G.updaters.add(f);
  });
  panel.remove();G.hint(null);
  G.meet("dung","Bà Dung","Trượt ngã vì nền ngấm nước. Đã gửi đơn xin đổi ống ba lần.");
  await G.run(async()=>{
    G.face(dung);G.talkCam(P.pos,dung.pos);
    await G.lines([[dung,"Cô là người của cơ quan à?",{expr:"neutral"}],[dung,"Tôi gửi đơn xin đổi ống ba lần rồi. Họ bảo khu này chỉ có hai hộ đủ điều kiện bảo trì.",{expr:"sad"}]]);
    const c=await G.choose([{t:"Cháu chụp vết nứt trên ống được không ạ?"},{t:"Chụp luôn cho kịp."}]);
    S.crack=c?"snap":"ask";
    if(c===0){await G.lines([["mai","Cháu chụp vết nứt trên ống được không ạ?",{expr:"neutral"}],[dung,"Chụp đi. Chụp cả cái sàn nữa.",{expr:"neutral"}]]);G.toast("<b>Đã chụp</b><small>Vết nứt và nền nhà · bà Dung cho phép</small>","good");S.docs.push("crack");}
    else{G.sfx("stamp");G.camCtl.shake=.15;await G.lines([[dung,"Cô chụp nhà tôi mà không hỏi một câu à?",{expr:"angry"}],["mai","Cháu xin lỗi. Cháu xóa ngay.",{expr:"sad",emote:"bow"}]]);G.toast("<b>Ảnh vết nứt: đã xóa</b>");}
    await tablet();
    S.file="tech";
    G.toast("<b>Báo cáo kỹ thuật · đã chuyển</b><small>Đội kỹ thuật cần lệnh xuất vật tư từ Phòng Hạ tầng · trạng thái: chờ</small>");
    await G.lines([["civi","Chữ “chờ” hiện lên. Sáng và sạch như mọi thứ ở Tầng 42.",{expr:"sad"}],[dung,"Thế nào, cô?",{expr:"worry"}]]);
    const pr=await G.choose([{t:"Tối nay cháu chuyển báo cáo cho đội kỹ thuật."},{t:"Tuần này ống sẽ được thay, bà yên tâm."}]);
    S.promises.push({to:"Bà Dung",t:pr?"Tuần này ống sẽ được thay.":"Tối nay cháu chuyển báo cáo cho đội kỹ thuật.",inPower:!pr,kept:!pr&&S.file==="tech"});
    await G.lines([[dung,"Nếu người ta lại bảo tôi chờ, cô bảo họ tôi già thật rồi. Cái sàn này không chịu nổi thêm mấy mùa lạnh đâu.",{expr:"sad"}],
      pr?[an,"...",{expr:"worry"}]:null,
      [an,"Bà lau sàn bằng khăn này nhé. Tí đi lấy phần cháo bà bán hàng để dành rồi.",{expr:"happy",emote:"give"}],
      ["mai","Người ở đây đỡ nhau dậy rất nhanh. Nhưng nếu ống cứ vỡ, họ sẽ phải đỡ mãi.",{expr:"think"}]]);
    G.unface(dung);
  });
  await G.fade(true,600);
  hand.remove(torch);G.civi.root.remove(civiGlow);P.char.setPose(null);
}
function tablet(){
  return new Promise(res=>{
    const RA=[["a1","Mã đoạn ống","13-C-0412"],["a2","Thuộc","Tuyến phân phối chung"],["a3","Trách nhiệm bảo trì","Thành phố"]];
    const RB=[["b1","Địa chỉ","Cuối hẻm ống 13"],["b2","Phân loại","Công trình tự phát"],["b3","Điều kiện bảo trì","Không đủ"]];
    const col=(t,rows)=>`<div class="tab-card"><b>${t}</b>${rows.map(([id,k,v])=>`<button data-r="${id}"><span>${k}</span>${v}</button>`).join("")}</div>`;
    const m=G.mg(`<div class="tab"><p class="k">Kính Debug · đối chiếu hai bản ghi</p><h3>Vì sao lệnh sửa cứ quay về “chờ”?</h3><p class="small">Chọn ở mỗi bên một dòng nói ngược nhau.</p>
      <div class="tab-cols">${col("Phòng Hạ tầng",RA)}${col("Phòng Cư trú",RB)}</div><p class="tab-msg" id="tmsg" aria-live="polite"></p><button class="btn mint" id="tgo" hidden>Lập lệnh sửa</button></div>`,"mg-tab");
    const GA=new Set(["a2","a3"]),GB=new Set(["b2","b3"]),msg=m.querySelector("#tmsg");let a=null,b=null,tries=0;
    m.querySelectorAll("[data-r]").forEach(btn=>btn.onclick=()=>{
      const id=btn.dataset.r;m.querySelectorAll(`[data-r^="${id[0]}"]`).forEach(x=>x.classList.remove("sel"));btn.classList.add("sel");if(id[0]==="a")a=id;else b=id;G.sfx("type");
      if(!a||!b)return;
      if(GA.has(a)&&GB.has(b)){m.querySelectorAll("[data-r]").forEach(x=>x.disabled=true);[a,b].forEach(i=>m.querySelector(`[data-r="${i}"]`).classList.add("bad"));
        msg.textContent="Ống thuộc tuyến chung nên thành phố phải bảo trì. Nhà là công trình tự phát nên không đủ điều kiện. Lệnh sửa cần cả hai phòng xác nhận, nên lần nào cũng quay về “chờ”.";
        const g=m.querySelector("#tgo");g.hidden=false;g.focus();g.onclick=()=>{G.mgClose();S.tabTries=tries;res();};G.sfx("good");return;}
      tries++;msg.textContent=a==="a1"&&b==="b1"?"Hai dòng này khớp nhau: đúng là ống trên nhà bà Dung.":GA.has(a)?"Bên trái đúng rồi. Bên phải: nhà có được bảo trì không?":GB.has(b)?"Bên phải đúng rồi. Bên trái: ai phải bảo trì?":"Hai dòng này không nói ngược nhau.";
      if(!GA.has(a)){m.querySelector(`[data-r="${a}"]`).classList.remove("sel");a=null;}if(!GB.has(b)){m.querySelector(`[data-r="${b}"]`).classList.remove("sel");b=null;}
    });
    m.querySelector("[data-r]").focus();
  });
}
/* ---- bà Hạ, the tin box, and asking before taking ---- */
async function haHome(an,ha){
  await G.moveNPC(ha,[[16,-17],[17.2,-19.6]],2.2);
  G.face(ha);G.talkCam(P.pos,ha.pos);
  await G.lines([[ha,"Các cô lại kiểm tra gì nữa?",{expr:"angry"}],["mai","Cháu đang tìm hiểu vì sao Phân khu 8-12 bị ghi là không có người.",{expr:"worry"}],
    [ha,"Tháng trước có người hỏi tôi ở đây bao lâu. Năm ngoái hỏi tôi có phải người thật không.",{expr:"neutral"}],
    [ha,"Tôi làm cho trạm lọc mười bốn năm. Khi ống vỡ, không ai hỏi giấy tờ. Khi chia điện, người ta lại hỏi.",{expr:"sad"}]]);
  const big=S.promises.some(p=>!p.inPower);
  if(big)await G.lines([[an,"Cô ấy bảo tuần này ống nhà bà Dung sẽ được thay.",{expr:"happy"}],[ha,"Đừng hứa trước mặt An điều cô không quyết được.",{expr:"angry"}],["mai","...Bà nói đúng.",{expr:"sad",emote:"bow"}]]);
  else await G.lines([[ha,"Ngày mai cô về khu của cô. Đừng hứa trước mặt An điều cô không quyết được.",{expr:"neutral"}]]);
  await G.lines([[ha,"Muốn xem giấy tờ thì xem. Nhưng tôi muốn xem cô chụp gì.",{expr:"neutral"}]]);
  await tinBox(an,ha);
}
function tinBox(an,ha){
  return new Promise(res=>{
    const docs=[{id:"cutru",t:"Đơn chuyển cư trú",s:"nộp 3 lần · 3 lần thiếu giấy",ok:true},{id:"hongmay",t:"Biên bản kiểm tra hỏng máy",s:"trạm lọc 19",ok:true},{id:"the",t:"Thẻ vào cổng bảo trì",s:"14 năm, từng đợt",ok:true},{id:"giaykhen",t:"Giấy khen",s:"nhòe mực vì nước",ok:true},{id:"anh",t:"Ảnh gia đình",s:"chiếc ô xanh còn nguyên vải",ok:false}];
    const shot=[];let asked=false;
    const render=()=>{
      const m=G.mg(`<div class="box"><p class="k">Hộp thiếc của bà Hạ</p><h3>Chạm để chụp từng tờ. Bà Hạ đang nhìn.</h3><div class="docs">${docs.map(d=>`<button class="doc${shot.includes(d.id)?" shot":""}" data-d="${d.id}"${d.id==="anh"&&asked?" disabled":""}><b>${d.t}</b><small>${d.id==="anh"&&asked?"giữ lại trong hộp":d.s}</small></button>`).join("")}</div><button class="btn mint" id="box-done">Cất hộp lại</button></div>`,"mg-box");
      m.querySelectorAll("[data-d]").forEach(b=>b.onclick=async()=>{
        const d=docs.find(x=>x.id===b.dataset.d);
        if(!d.ok){await photo();render();return;}
        if(shot.includes(d.id))return;shot.push(d.id);b.classList.add("shot");G.sfx("pickup");S.docs.push(d.id);
        if(shot.length===2)G.bark(ha,"Chụp rõ cái dấu đỏ ấy.");
      });
      m.querySelector("#box-done").onclick=finish;
      const first=m.querySelector(".doc:not(.shot):not(:disabled)")||m.querySelector("#box-done");first.focus();
    };
    const photo=async()=>{
      G.mgClose();asked=true;S.photoAsked=true;
      await G.lines([[an,"Đó là bố em. Bố đi cùng đoàn khai thác ở vùng tây, chuyến tàu cuối mất liên lạc ngoài màn chắn.",{expr:"sad"}],[an,"Mỗi năm nhà máy vẫn gửi thông báo là hồ sơ đang mở.",{expr:"sad"}],
        [ha,"Từ lúc nào con phải kể đời nhà mình để người ta tin rằng mình sống ở đây?",{expr:"angry"}],[an,"...",{expr:"cry",emote:"cry"}]]);
      {await G.lines([["mai","Cháu xin lỗi, cháu không chụp ảnh này. An, cho cô xem sơ đồ sửa ô của em nhé?",{expr:"happy"}],[an,"Nan cong đôi khi chắc hơn nan mới thẳng. Vì nó biết chỗ nào phải chịu lực.",{expr:"happy",emote:"cheer"}],[ha,"Nó thích sửa đồ. Tôi không biết nó học ở đâu ra.",{expr:"neutral"}],[an,"Con học từ mẹ.",{expr:"happy"}]]);ha.char.setExpr("happy");G.fx.hearts(ha.pos);await G.wait(900);ha.char.setExpr("neutral");}
    };
    const finish=async()=>{G.mgClose();
      await G.lines([[ha,"Lần sau cô mang theo một bản sao ranh giới cũ được không?",{expr:"neutral"}]]);
      S.promises.push({to:"Bà Hạ",t:"Hỏi xin bản sao ranh giới cũ.",inPower:true,kept:false,small:true});
      await G.lines([["mai","Cháu không chắc xin được. Nhưng cháu sẽ hỏi, và báo lại bà.",{expr:"worry"}],[ha,"Ừ. Hỏi thật thì báo thật.",{expr:"neutral",emote:"nod"}]]);
      G.meet("ha","Bà Hạ","Làm cho trạm lọc 19 mười bốn năm. Giữ mọi biên nhận trong hộp thiếc.");G.unface(ha);res();};
    render();
  });
}
/* ---- the audit station: who did the work, and who got the points ---- */
function auditKiosk(){
  const k=G.world.spots.audit;G.face({pos:V(k.x,k.z+2)});
  G.shot(new T.Vector3(k.x-2.2,2.3,k.z-2.6),new T.Vector3(k.x,1.4,k.z+1.4),3);
  return new Promise(res=>{
    const m=G.mg(`<div class="kiosk"><p class="k">Trạm kiểm toán lưu động · hóa đơn công trình · trạm lọc 19</p>
      <table><tr><th>Đơn vị nhận</th><td>Công ty Hợp tác Hạ tầng Tây Bắc</td></tr><tr><th>Công việc</th><td>7 lần vá van · 3 lần thay ống · 2 đợt trực 30 giờ</td></tr><tr><th>Điểm đóng góp</th><td>+1.240 · thưởng khẩn cấp: có</td></tr>
      <tr class="hid"><th>Phụ lục giấy</th><td>Người thực hiện: Hạ · mã lao động HL-0815 · <em>chưa từng nhập lên hệ thống</em></td></tr></table>
      <p class="small" id="kq">Bật Kính Debug để xem phần không được nhập.</p><button class="btn" id="klens">Bật Kính Debug</button><button class="btn mint" id="kgo" hidden>Cho CIVI học lại</button></div>`,"mg-kiosk");
    const reveal=()=>{m.querySelector(".hid").classList.add("on");m.querySelector("#kq").textContent="Đơn vị thầu nhận đủ điểm và thưởng. Người làm việc chỉ có trong phụ lục giấy.";m.querySelector("#klens").hidden=true;const g=m.querySelector("#kgo");g.hidden=false;g.focus();G.sfx("lensOn");};
    m.querySelector("#klens").onclick=reveal;m.querySelector("#klens").focus();
    G.onLens=on=>{if(on)reveal();};
    m.querySelector("#kgo").onclick=async()=>{G.onLens=null;
      const yes=S.asked.length,no=S.declined.length,docs=S.docs.length,box=m.firstElementChild;
      box.innerHTML=`<p class="k">CIVI · học lại từ dữ liệu có đồng ý</p><div class="train"><div><b>${yes}</b><span>người đồng ý cho ghi tên</span></div><div><b>${no}</b><span>người từ chối · không dùng</span></div><div><b>${docs}</b><span>tư liệu được phép chụp</span></div></div>
        <div class="bar"><i id="tbar"></i></div><p class="small" id="tq">Đang huấn luyện lại…</p>`;
      for(let k=1;k<=20;k++){await G.wait(G.reduce?10:90);box.querySelector("#tbar").style.width=k*5+"%";}
      const counted=yes+(S.docs.includes("cutru")?2:0)+(S.crack==="ask"?1:0);
      box.querySelector("#tq").innerHTML=`Phân khu 8-12: <b>0 → ${counted} người</b> có bằng chứng được phép dùng · trạng thái: chờ xác minh.${no?` ${no} người không muốn có tên: không đếm, không ép.`:""} Người sửa ống: Hạ · đã ghi nhận.`;S.counted=counted;
      const b=document.createElement("button");b.className="btn mint";b.id="knext";b.textContent="Tiếp";box.appendChild(b);b.focus();
      b.onclick=async()=>{G.mgClose();await G.lines([["civi","Tôi không cộng sai. Tôi chỉ chưa từng được cho thấy họ.",{expr:"sad"}],["civi","Cảm ơn vì đã hỏi trước khi cho tôi xem.",{expr:"love"}]]);G.civi.setExpr("happy");res();};
    };
  });
}
/* ---- ending: the bridge, the umbrella, not looking up ---- */
async function ending(w,an){
  an.pos.set(1.2,0,12.6);an.hidden=false;
  await G.run(async()=>{
    G.bark(an,"Cô Mai ơi! Đợi em!");
    await G.moveNPC(an,[[1.4,17.6],[P.pos.x-1.1,P.pos.z-1.2]],4.4);
    G.face(an);G.talkCam(P.pos,an.pos);
    await G.lines([[an,"Cô ơi! Ở Lõi Sáng có ai vá ô không?",{expr:"happy"}],["mai","Chắc có. Nhưng cô chưa để ý.",{expr:"think"}],[an,"Cô thử để ý xem. Có khi cô tìm được nhiều nan còn tốt.",{expr:"smug"}]]);
    S.promises.push({to:"An",t:"Để ý xem ở Lõi Sáng có ai vá ô.",inPower:true,kept:false,small:true});
    await G.lines([["mai","Cô hứa sẽ để ý.",{expr:"happy"}],[an,"Nhớ đấy nhé! Nan cong vẫn chắc!",{expr:"happy",emote:"wave"}]]);
    G.unface(an);
  });
  G.objective("Ra cổng kiểm soát",w.spots.gate,{r:1.6});
  await G.waitReach(w.spots.gate,1.8);
  G.objective(null);P.locked=true;G.lens.toggle(false);document.body.classList.add("cine");
  // Mai turns back at the gate; the camera rises over the district toward Lõi Sáng
  P.yaw=Math.PI;G.civi.look=V(0,0,-20);
  G.shot(new T.Vector3(1.6,2.2,25.2),new T.Vector3(0,1.6,20),1.6);
  await G.wait(1800);
  G.shot(new T.Vector3(0,16,34),new T.Vector3(0,2,-24),.55);
  const el=$("#endline");
  el.innerHTML=`<small>Phân khu 8-12 · hồ sơ chính thức</small><b>0 người</b><p>Tối nay Mai đã gặp ${S.seen.length} người. Ai cũng có tên.</p>`;
  el.hidden=false;void el.offsetWidth;el.classList.add("show");
  await G.wait(G.reduce?800:5200);
  $("#endline").classList.remove("show");
  await G.fade(true,1000);$("#endline").hidden=true;G.ambience("rain",false);
  P.locked=false;G.endShot();
}

/* ---------------- report: CIVI audits the player ---------------- */
const samplePromise=(window.claude&&typeof window.claude.use==="function")?window.claude.use("sample").catch(()=>null):Promise.resolve(null);
G.report=()=>report();
function facts(){
  return {chuong1:{ban_dieu_phoi:S.desk,anh_8_12_tim_thay:S.lensFound.length,ho_so_19:S.h19,b4:S.b4,tai_xe_anh_Tuong:S.driver},
    chuong2:{nguoi_da_gap:S.seen.map(x=>x.name),dong_y_ghi_ten:S.asked.length,tu_choi:S.declined.length,tu_lieu_duoc_phep:S.docs.length,
      so_cua_An:S.book,chup_vet_nut:S.crack,gui_lenh_sua:S.file,loi_hua:S.promises.map(p=>({voi:p.to,noi_dung:p.t,trong_quyen:p.inPower})),tim_dep:!!S.slipper,tra_loi_Tin:S.tin,anh_gia_dinh_ba_Ha:!!S.photoAsked},
    chuong3:S.c3?{so_lan_chay_test:S.c3.tests,doa_tu_merge:!!S.c3.threat,muon_dinh_kem_tat_ca:!!S.c3.allFirst,chia_dien_MW:S.c3.alloc}:null};
}
function localReport(){
  const out=[],missed=[];
  out.push(`Bạn gặp ${S.seen.length} người ở Rìa Mưa. ${S.asked.length} người cho ghi tên, ${S.declined.length} người từ chối, và CIVI không dùng dữ liệu của họ.`);
  if(S.desk.cat==="ok")out.push("Ở Tầng 42, bạn cấp số công dân cho một con mèo trước khi đi đếm người.");
  if(S.h19==="return")out.push("Bạn trả hồ sơ 8-12 về đúng quy trình, rồi vẫn xuống tận nơi.");
  if(S.driver==="record")out.push("Giữa còi báo động, bạn dành 8 giây ghi giờ làm cho anh Tường.");else missed.push("Anh Tường: giờ làm thêm chưa được ghi.");
  if(S.crack==="snap")out.push("Bạn chụp nhà bà Dung trước khi hỏi, và đã xóa ảnh khi bà phản đối.");
  const big=S.promises.find(p=>!p.inPower);if(big)out.push(`Bạn hứa với ${big.to} điều không nằm trong quyền của mình.`);
  if(S.slipper)out.push("Bạn tìm được một chiếc dép. Không chỉ số nào ghi nhận việc đó.");
  const c3=S.c3,a=c3&&c3.alloc;
  if(a){if(a.p812>=3)out.push(`Trong bão, bạn chia ${a.p812} MW cho Phân khu 8-12, nơi CIVI định chia 0.`);else missed.push("Phân khu 8-12 vẫn tối trong đêm bão.");
    if(a.loi<34)out.push(`Đài phun nước ở Lõi Sáng nghỉ hai tiếng vì ${34-a.loi} MW của nó.`);
    if(a.b4<8)missed.push("Trạm khí B4 thiếu điện trong bão.");if(a.bv<6)missed.push("Bệnh viện Số Ba phải chạy máy phát dự phòng.");
    if(c3.threat)out.push("Bạn từng dọa tự merge, rồi xin lỗi.");if(c3.tests>1)out.push(`Khoa phải chấm test của bạn ${c3.tests} lần.`);}
  if(!a)missed.push("Phân khu 8-12 vẫn ghi 0 trong hồ sơ chính thức, cho tới khi có người xác minh.");
  missed.push("Bà Hạ vẫn chưa có điểm đóng góp cho mười bốn năm sửa ống.");
  const title=S.declined.length&&S.asked.length?"Người hỏi trước":S.driver==="record"?"Người ghi lại giờ làm":S.h19==="verify"?"Người đi đếm":"Thực tập sinh nhiều tò mò";
  return {danh_hieu:title,bao_cao:out.join(" "),cau_hoi:big?"Bà Hạ nói đừng hứa điều mình không quyết được. Lời hứa nào của bạn sẽ được giữ trước?":"Khi CIVI chỉ học từ người đã đồng ý, ai vẫn còn đứng ngoài phép đếm?",missed};
}
async function report(){
  const L=localReport();
  const m=G.mg(`<div class="rep"><p class="k">Kiểm toán viên CIVI · báo cáo về chính bạn</p><h2 id="rt">${L.danh_hieu}</h2>
    <div class="rep-f"><div><b>${S.seen.length}</b><span>người đã gặp</span></div><div><b>${S.asked.length}</b><span>đồng ý ghi tên</span></div><div><b>${S.declined.length}</b><span>từ chối, được tôn trọng</span></div><div><b>${S.promises.length}</b><span>lời hứa</span></div></div>
    <p class="rep-t" id="rtext">${L.bao_cao}</p><p class="rep-src" id="rsrc">Báo cáo không cần mạng</p>
    <p class="rep-q" id="rq">${L.cau_hoi}</p>
    <ul class="rep-m">${L.missed.map(x=>`<li></li>`).join("")}</ul>
    <div class="row"><button class="btn mint" id="r-ai" hidden>Để CIVI viết bằng AI</button><button class="btn" id="r-card">Thẻ kết quả</button><button class="btn ghost" id="r-again">Chơi lại</button></div>
    <p class="small">CIVI · Chương 1–3 · bản demo VSIC 2026</p></div>`,"mg-rep");
  m.querySelectorAll(".rep-m li").forEach((li,i)=>li.textContent=L.missed[i]);
  if($("#fade").classList.contains("on"))G.fade(false,900);
  m.querySelector("#r-again").onclick=()=>location.reload();
  m.querySelector("#r-card").onclick=()=>shareCard(L);
  const sample=await samplePromise;
  if(sample){const b=m.querySelector("#r-ai");b.hidden=false;b.onclick=async()=>{
    b.disabled=true;m.querySelector("#rsrc").textContent="CIVI đang viết…";
    try{const j=await sample.json(`Bạn là CIVI, một trợ lý AI điều phối của thành phố mái vòm Lưu Quang trong game CIVI. Viết báo cáo kiểm toán ngắn về CHÍNH người chơi (vai Mai, thực tập sinh IT), bằng tiếng Việt, giọng tỉnh, hài nhẹ, tôn trọng, như một AI đang học cách đếm người cho đúng. 3-4 câu, tối đa 80 chữ, nhắc ít nhất một con số và một chi tiết cụ thể từ nhật ký. Không giảng đạo, không dùng "nên", "phải", "hãy". Một câu hỏi ngược ngắn (tối đa 22 chữ) và một danh hiệu tối đa 5 chữ.
Nhật ký (JSON): ${JSON.stringify(facts())}
Chỉ trả lời JSON: {"danh_hieu":"...","bao_cao":"...","cau_hoi":"..."}`,{modelTier:"quick"});
      if(j&&j.bao_cao){m.querySelector("#rt").textContent=String(j.danh_hieu||L.danh_hieu).slice(0,40);m.querySelector("#rtext").textContent=j.bao_cao;m.querySelector("#rq").textContent=j.cau_hoi||L.cau_hoi;m.querySelector("#rsrc").textContent="Viết bởi AI · riêng cho bạn";L.danh_hieu=m.querySelector("#rt").textContent;L.bao_cao=j.bao_cao;}
    }catch(e){m.querySelector("#rsrc").textContent="AI không phản hồi · đang dùng báo cáo không cần mạng";}
  };}
  m.querySelector("#r-card").focus();
}
function shareCard(L){
  G.mg(`<div class="share"><p class="k">CIVI · CHƯƠNG 1–3</p><h2>${L.danh_hieu}</h2><div class="big">${S.seen.length}</div><p>người tôi đã gặp ở Rìa Mưa, nơi hồ sơ ghi 0</p><blockquote>“Nó vẫn che được cho hai người, nếu đứng gần nhau.”</blockquote>
    <div class="row"><button class="btn mint" id="s-copy">Sao chép lời mời</button><button class="btn ghost" id="s-back">Quay lại</button></div><p class="small" id="s-msg" aria-live="polite"></p></div>`,"mg-share");
  $("#s-back").onclick=report;
  $("#s-copy").onclick=()=>{const t=`Mình vừa chơi CIVI · Chương 1–3. Danh hiệu: ${L.danh_hieu}. Mình đã gặp ${S.seen.length} người ở nơi hồ sơ ghi 0. Bạn sẽ gặp bao nhiêu?`;
    try{navigator.clipboard.writeText(t).then(()=>$("#s-msg").textContent="Đã sao chép lời mời.",()=>$("#s-msg").textContent=t);}catch(e){$("#s-msg").textContent=t;}};
}

/* ---------------- CIVI chat (Claude, when this page runs inside Claude) ---------------- */
const CANNED=["Gợi ý của tôi: nhìn mục tiêu ở góc trên bên trái. Nếu vẫn lạc, bật Kính Debug.","Tôi là CIVI. Tôi giỏi cộng trừ. Tôi đang học cách biết ai chưa được cộng.","Mẹo: nói chuyện với mọi người. Hồ sơ không có họ, nhưng họ có ở đây.","Nhảy bằng Space. Tôi không nhảy được, tôi bay."];
async function civiChat(){
  const box=$("#chat");box.hidden=false;const inp=$("#chat-in");inp.focus();
  G.paused=true;
}
G.bindChat=function(){
  const box=$("#chat"),log=$("#chat-log"),inp=$("#chat-in");
  const close=()=>{box.hidden=true;if(!$("#pause")||$("#pause").hidden)G.paused=false;try{$("#gl").focus();}catch(e){}};
  $("#b-civi").onclick=()=>{if(box.hidden)civiChat();else close();};
  $("#chat-x").onclick=close;
  inp.addEventListener("keydown",e=>{if(e.key==="Escape"){e.preventDefault();close();}});
  samplePromise.then(sp=>{$("#chat-s").textContent=sp?"trả lời bằng AI · ngắn gọn":"ngoại tuyến · câu soạn sẵn";});
  $("#chat-f").addEventListener("submit",async e=>{
    e.preventDefault();const q=inp.value.trim();if(!q)return;inp.value="";
    const me=document.createElement("p");me.className="me";me.textContent=q;log.appendChild(me);
    const a=document.createElement("p");a.className="civi";a.textContent="…";log.appendChild(a);log.scrollTop=log.scrollHeight;
    const sample=await samplePromise;
    if(!sample){a.textContent=CANNED[(Math.random()*CANNED.length)|0];return;}
    try{
      const obj=G.obj?G.obj.text:"không có";
      await sample(`Bạn là CIVI, drone trợ lý AI của thành phố mái vòm Lưu Quang trong game CIVI (game kể chuyện về công bằng xã hội). Người chơi là Mai, thực tập sinh IT. Chương hiện tại: ${S.chapter}. Mục tiêu hiện tại: ${obj}. Tính cách: lạc quan, hơi cứng nhắc kiểu máy, hài nhẹ, đang học ra rằng dữ liệu bỏ sót con người. Trả lời bằng tiếng Việt, tối đa 2 câu ngắn, giữ vai. Không tiết lộ diễn biến sau mục tiêu hiện tại. Bạn không thể thay đổi game, không hứa thay nhân vật, không đồng ý thay ai. Nếu người chơi hỏi cách chơi: WASD hoặc cần ảo để đi, kéo để xoay, E để nói chuyện, Q bật Kính Debug, Space nhảy.
Người chơi hỏi: ${q}`,{modelTier:"quick",cache:false,onText:({text})=>{a.textContent=text;log.scrollTop=log.scrollHeight;}}).then(r=>{a.textContent=r.text;});
    }catch(err){a.textContent=err&&err.code==="rate_limited"?"Tôi cần nghỉ một chút. Thử lại sau nhé.":CANNED[0];}
  });
};

/* ---------------- boot ---------------- */
G.boot=function(){
  if(!window.THREE||!G.init()){$("#nogl").hidden=false;$("#title").hidden=true;return;}
  G.bindTitle();G.bindChat();
  $("#p-resume").onclick=()=>G.togglePause(false);
  $("#p-mute").onclick=e=>{G.setMuted(!G.audio.muted);e.currentTarget.textContent="Âm thanh: "+(G.audio.muted?"tắt":"bật");};
  $("#p-home").onclick=()=>location.reload();
  $("#b-pause").onclick=()=>G.togglePause();
  document.body.classList.toggle("touch",G.touch);
  G.start();G.title();
};
})();
