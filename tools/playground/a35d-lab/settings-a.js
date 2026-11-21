/* Theme and UI pages, based on SettingsWindow.cs. */
(()=>{'use strict';
const icon=n=>LAB.glyph(n,20,'currentColor','fluent');
// SettingsParts.cs draws these four icons rather than using a font glyph.
function drawn(kind){
 const ns='http://www.w3.org/2000/svg', svg=document.createElementNS(ns,'svg');
 svg.setAttribute('viewBox','0 0 20 20');svg.setAttribute('width','20');svg.setAttribute('height','20');svg.setAttribute('aria-hidden','true');
 const parts={mode:'<circle cx="10" cy="10" r="7" fill="none" stroke="currentColor" stroke-width="1.4"/><path d="M10 3 A7 7 0 0 1 10 17 Z" fill="currentColor"/>',scale:'<path d="M3 7V5.5A2.5 2.5 0 0 1 5.5 3H7 M13 3h1.5A2.5 2.5 0 0 1 17 5.5V7 M17 13v1.5a2.5 2.5 0 0 1-2.5 2.5H13"/><rect x="3" y="10" width="7" height="7" rx="1.8"/>',toolbar:'<rect x="2.7" y="4.7" width="14.6" height="10.6" rx="2.2"/><path d="M2.7 11.6h14.6"/>',caret:'<rect x="1.5" y="3" width="1.8" height="14" rx=".9" fill="currentColor" stroke="none"/><text x="4.5" y="14" fill="currentColor" stroke="none" font-family="Segoe UI" font-size="12" font-weight="600">Ab</text>',blink:'<rect x="1.5" y="3" width="1.8" height="14" rx=".9" fill="currentColor" stroke="none"/><text x="4.5" y="14" fill="currentColor" stroke="none" font-family="Segoe UI" font-size="12" font-weight="600">A</text><circle cx="15" cy="14.5" r="3.7" fill="var(--surface)" stroke-width="1.1"/><path d="M15 12.6v2l1.3.8" stroke-width="1.1"/>'};
 svg.setAttribute('fill','none');svg.setAttribute('stroke','currentColor');svg.setAttribute('stroke-width','1.4');svg.innerHTML=parts[kind];return svg;
}
const opts=items=>items.map(([value,label])=>[value,label]);
function dropdown(kit,items,value,pick){const d=kit.dropdown(opts(items),value,pick);LAB.mark(d.querySelector('.kit-dropdown-list'),6);return d;}
function expanded(kit,ctx,key,ico,title,value,children,initial){ctx.state.open??={};const open=ctx.state.open[key]??initial;const e=kit.expander(ico,title,null,value,children,open);e.querySelector('button').addEventListener('click',event=>{ctx.state.open[key]=event.currentTarget.getAttribute('aria-expanded')==='true';});return e;}
function chooseTheme(ctx,id){ctx.setTheme(id);}
LAB.settingsPages.Theme=(root,kit,ctx)=>{
 const s=ctx.state;const row=(ico,title,desc,control)=>kit.row(ico,title,desc,control);
 const palette=(dark)=>{const key=dark?'darkPalette':'lightPalette';return kit.subRow(null,kit.paletteChips(dark,s[key],v=>{s[key]=v;if(dark&&s.theme==='light')s.theme='dark';if(!dark&&s.theme==='dark')s.theme='light';chooseTheme(ctx,v);ctx.refresh();}));};
 const colors=kit.card([
  row(drawn('mode'),'Mode','Follow Windows, or stay light or dark',kit.segments([['system','System'],['light','Light'],['dark','Dark']],s.theme,v=>{s.theme=v;chooseTheme(ctx,v==='light'?s.lightPalette:s.darkPalette);ctx.refresh();})),
  expanded(kit,ctx,'light',icon(0xf8a1),'Light mode palette',LAB.palettes[s.lightPalette].name,[palette(false)],true),
  expanded(kit,ctx,'dark',icon(0xf87d),'Dark mode palette',LAB.palettes[s.darkPalette].name,[palette(true)],false),
  row(icon(0xf2f5),'Accent','Caret, selection, headings and switches',kit.accentDots(s.accent,v=>{s.accent=v;document.documentElement.style.setProperty('--accent',v==='default'?LAB.theme.accent:v);}))
 ]);
 const caret=kit.pictureChoice([{value:'bar',label:'Bar',picture:kit.caretPicture(false),width:56},{value:'block',label:'Block',picture:kit.caretPicture(true),width:56}],s.caretStyle,v=>{s.caretStyle=v;});caret.style.gap='var(--snap-5-row-gap)';LAB.mark(caret,5);
 const caretCard=kit.card(row(drawn('caret'),'Caret style',null,caret),row(drawn('blink'),'Blink',null,kit.toggle(s.caretBlink,v=>s.caretBlink=v,'Blink')),row(icon(0xe0cd),'Glide between positions',null,kit.toggle(s.caretGlide,v=>s.caretGlide=v,'Glide between positions')));
 const palettes=Object.values(LAB.palettes).filter(p=>p.dark===LAB.theme.dark).map(p=>[p.id,p.name]);
 const split=kit.card(row(icon(0xe8b0),'Side pane palette','The slot beside your note can look different',dropdown(kit,[['same','Same as main'],...palettes],s.splitPalette,v=>s.splitPalette=v)));
 root.append(kit.group('Colors'),colors,kit.group('Caret'),caretCard,kit.group('Split view'),split);
};
LAB.settingsPages.UI=(root,kit,ctx)=>{
 const s=ctx.state;const row=(ico,title,desc,control)=>kit.row(ico,title,desc,control);
 const materials=kit.pictureChoice(['acrylic','mica','solid'].map(v=>({value:v,label:v[0].toUpperCase()+v.slice(1),picture:kit.materialPicture(v),width:80})),s.material,v=>{s.material=v;ctx.refresh();});
 const hint=s.material==='solid'?'Restart AntiNotepad to save about 80 MB':'Acrylic and Mica use the GPU (~125 MB)\nSolid uses about 80 MB less';
 const windowRows=[row(icon(0xf592),'Background',hint,materials)];
 if(s.material!=='solid')windowRows.push(row(icon(0xf595),'Tint','How much of the desktop shows through',dropdown(kit,[['0.55','Clear'],['0.7','Light'],['0.82','Medium'],['0.92','Dense']],s.tint,v=>{s.tint=v;ctx.refresh();})));
 windowRows.push(row(drawn('scale'),'Scale','Toolbar, panels and text together',dropdown(kit,[['0.9','90%'],['1','100%'],['1.15','115%'],['1.3','130%'],['1.5','150%']],s.scale,v=>s.scale=v)));
 const sub=[kit.subRow('Show',kit.segments([['fade','Fade while typing'],['always','Always visible']],s.chrome,v=>{s.chrome=v;ctx.refresh();}))];
 if(s.chrome==='fade')sub.push(kit.subRow('Fade after',dropdown(kit,[['800','0.8 seconds'],['1600','1.6 seconds'],['3000','3 seconds'],['6000','6 seconds']],s.fadeAfter,v=>s.fadeAfter=v)));
 const descriptions={docked:'A top bar above the note and the pager below it',overlay:'Both bars slide in over the note when the pointer reaches the top or bottom edge',float:'The top bar slides in at the top; the pager floats just below the window from the bottom edge'};
 const toggle=(name,key,desc)=>row(null,name,desc,kit.toggle(s[key],v=>s[key]=v,name));
 const controls=[expanded(kit,ctx,'toolbar',drawn('toolbar'),'Toolbar and pager',s.chrome==='fade'?'Fade while typing':'Always visible',sub,true),
 row(null,'Bar style',descriptions[s.bars],kit.segments([['docked','Docked'],['overlay','Overlay'],['float','Float below']],s.bars,v=>{s.bars=v;ctx.refresh();})),
 toggle('Show Delete button','showDelete','Show the Delete button on the bottom bar'),toggle('Show New note button','showNew','Show the New note button on the bottom bar'),
 row(null,'Slot dots','Choose the appearance of the slot pager',kit.segments([['pill','Pill'],['quiet','Quiet']],s.slotDots,v=>s.slotDots=v)),
 toggle('Edge fade','edgeFade','Text fades into the top and bottom edges where there is more to scroll'),toggle('Menu action row','menuAction','Cut, copy, paste and delete as buttons across the top'),toggle('Colored menu icons','menuColors','Tint a few menu icons with your accent'),toggle('Dim when pinned','dimPinned','A pinned pad fades back while you work elsewhere')];
 root.append(kit.group('Window'),kit.card(windowRows),kit.group('Controls'),kit.card(controls));
};
})();

