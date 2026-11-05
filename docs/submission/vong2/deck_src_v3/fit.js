const {chromium}=require('playwright');
(async()=>{const b=await chromium.launch();const p=await b.newPage({viewport:{width:1920,height:1080}});
await p.goto('file://'+__dirname+'/deck2.html');await p.waitForTimeout(800);
await p.evaluate(()=>{document.querySelectorAll('section.s').forEach(s=>{
 if((s.getAttribute('style')||'').includes('padding:0'))return;
 const w=document.createElement('div');w.className='fitw';
 [...s.children].forEach(c=>{if(!c.classList.contains('top')&&!c.classList.contains('foot'))w.appendChild(c)});
 s.insertBefore(w,s.querySelector('.foot'));
 const AW=1700,AH=835;let z=1;
 for(let i=0;i<14;i++){w.style.width=(AW/z)+'px';const h=w.scrollHeight;const nz=Math.min(1.32,Math.max(0.85,AH/h));if(Math.abs(nz-z)<0.005)break;z=(z+nz)/2;}
 w.style.width=(AW/z)+'px';
 while(w.scrollHeight*z>AH&&z>0.8){z-=0.01;w.style.width=(AW/z)+'px';}
 w.style.zoom=z;s.dataset.z=z.toFixed(2);
});});
console.log(await p.evaluate(()=>[...document.querySelectorAll('section.s')].map(s=>s.dataset.z||'-').join(' ')));
await p.pdf({path:__dirname+'/deck3.pdf',width:'1920px',height:'1080px',printBackground:true});
const els=await p.$$('section.s');for(let i=0;i<els.length;i++)await els[i].screenshot({path:__dirname+`/y${i+1}.png`});
await b.close();})();
