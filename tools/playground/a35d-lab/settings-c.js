/* A35d lab: System, Shortcuts, Storage and About pages. */
(() => {
'use strict';
const S = LAB.kit.settings;
LAB.settingsPages = LAB.settingsPages || {};
const defaults = () => ({ hotkey:'Ctrl+Alt+Shift+S', newHotkey:'', openAt:'remember', taskbar:'never', fresh:'30', pinned:false, blur:false, startup:false, save:'400', shortcuts:{}, folder:'F:\\LocalVSProj\\Visual Studio Apps\\AntiQuickPad\\.claude\\worktrees\\refactor\\A35d-spacing-tokens\\build\\testdata', pending:'', notice:'' });
let state = LAB.settingsCState = defaults();
const node = (tag, cls, text) => { const e=document.createElement(tag); if(cls)e.className=cls; if(text!=null)e.textContent=text; return e; };
const icon = code => LAB.glyph(code,20,'currentColor','fluent');
const row = (code,title,description,control) => { const r=S.row(code ? icon(code) : null,title,description,control); const desc=r.querySelector('.kit-row-description'); if(desc) desc.style.whiteSpace='pre-line'; return r; };
const pageRoot = () => node('div','settings-c-page');
const add = (p,...children) => p.append(...children);
const markDropdown = el => { const list=el.querySelector('.kit-dropdown-list'); if(list) LAB.mark(list,6); return el; };
const markFluent = root => { root.querySelectorAll('.fl-button,.fl-textbox,.fl-combo').forEach(e=>LAB.mark(e,19)); return root; };
const action = (label, fn, primary=false) => S.textButton(label,fn,primary);
const refresh = root => { const next=root._build(root); root.replaceChildren(...next.childNodes); markFluent(root); };
const mount = build => { const root=pageRoot(); root._build=build; refresh(root); return root; };
const group = (page,title,...rows) => add(page,S.group(title),S.card(...rows));
const stack = (...children) => {const e=node('div');e.style.cssText='display:flex;align-items:center;gap:6px';e.append(...children);return e;};
const cap = (text, live=false) => {const e=node('span','',text);e.style.cssText=`display:inline-block;padding:2px 8px;border:1px solid ${live?'var(--accent)':'color-mix(in srgb,var(--ink) 9%,transparent)'};border-bottom-width:2px;border-radius:4px;background:var(--surface);font-size:12px;white-space:nowrap`;return e;};
const parts = text => text.endsWith('++') ? [...text.slice(0,-2).split('+'),'+'] : text.split('+').filter(Boolean);
function recorder(get, set, allowEmpty, shortcut=false) {
 const host=node('div'); host.tabIndex=-1; let recording=false,candidate=null;
 const draw=()=>{ host.replaceChildren(); const line=stack(); line.style.gap='4px';
  if(recording) {
   if(candidate) parts(candidate).forEach(k=>line.append(cap(k,true))); else line.append(cap('●  Press the new shortcut',true));
   const save=action('Save',()=>apply(candidate),true); save.disabled=!candidate; save.style.marginLeft='8px'; line.append(save);
   if(allowEmpty) line.append(action('Clear',()=>apply(''))); line.append(action('Cancel',()=>{recording=false;draw();}));
   const hint=node('div','', 'Esc or Cancel cancels'+(allowEmpty?' · Clear removes the shortcut':'')); hint.style.cssText='font-size:12px;color:var(--muted);text-align:right;margin-top:4px'; host.append(line,hint);
  } else { const value=get(); if(value) parts(value).forEach(k=>line.append(cap(k))); else {const none=node('span','','None');none.style.color='var(--muted)';line.append(none);} const change=action('Change',()=>{recording=true;candidate=null;draw();host.focus();}); change.style.marginLeft='8px';line.append(change);host.append(line); }
 };
 const apply=value=>{if(value===null)return;if(set(value)!==false){recording=false;draw();}};
 host.addEventListener('keydown',e=>{if(!recording)return;e.preventDefault();e.stopPropagation();
  if(e.key==='Escape'&&!e.ctrlKey&&!e.altKey&&!e.shiftKey){recording=false;draw();return;}
  if(e.key==='Enter'&&!e.ctrlKey&&!e.altKey&&!e.shiftKey){apply(candidate);return;}
  if(e.metaKey)return;
  const mods=[e.ctrlKey&&'Ctrl',e.altKey&&'Alt',e.shiftKey&&'Shift'].filter(Boolean);
  if(['Control','Alt','Shift','Meta'].includes(e.key))return;
  let key=({ ' ':'Space',Escape:'Esc',ArrowLeft:'Left',ArrowRight:'Right',ArrowUp:'Up',ArrowDown:'Down' })[e.key] || e.key;
  if(key.length===1)key=key.toUpperCase();
  const supported=shortcut ? /^(?:[A-Z0-9]|F(?:[1-9]|1[0-9]|2[0-4])|Space|Tab|Enter|Insert|Delete|Backspace|Left|Right|Up|Down|Home|End|PageUp|PageDown|[+\-,./\\\[\]~;'])$/ : /^(?:[A-Z0-9]|F(?:[1-9]|1[0-9]|2[0-4])|Space|Tab|Enter|Insert|Delete|Esc)$/;
  if(!supported.test(key)||(!mods.length&&!(shortcut&&/^F\d+$/.test(key))))return;
  candidate=[...mods,key].join('+');draw();host.focus();
 }); draw(); return host;
}
function systemPage(){ return mount(root=>{
 const p=pageRoot();
 group(p,'Shortcuts',
  row(0xf4b8,'Show or hide AntiNotepad',null,recorder(()=>state.hotkey,v=>(state.hotkey=v,true),false)),
  row(0xf8bd,'Open on a new note',null,recorder(()=>state.newHotkey,v=>(state.newHotkey=v,true),true)));
 const choices=[['remember','Where I left it'],['center','Center'],['mouse','Mouse'],['tray','Above the tray'],['dropdown','Drop down']];
 const tiles=node('div'); tiles.style.cssText='display:grid;grid-template-columns:repeat(5,1fr);width:100%;gap:8px';
 for(const [id,label] of choices){
  const art=node('div'); art.style.cssText='width:100px;height:64px;background:var(--canvas);position:relative;border-radius:4px';
  const mini=node('div'); mini.style.cssText='position:absolute;width:26px;height:20px;border-radius:4px;background:var(--accent);opacity:.8;'+({remember:'left:14px;top:10px',center:'left:37px;top:21px',mouse:'left:46px;top:26px',tray:'right:3px;bottom:11px',dropdown:'left:37px;top:0;border-radius:0'}[id]);art.append(mini);
  const tile=S.tile(art,label,state.openAt===id,()=>{state.openAt=id;refresh(root);},104); tile.classList.remove('lab-mark'); delete tile.dataset.snap; tiles.append(tile);
 }
 const tileRow=S.subRow('',tiles);tileRow.style.paddingLeft='48px';tileRow.firstElementChild.style.display='none';
 const exp=S.expander(icon(0xee77),'Open at',null,choices.find(x=>x[0]===state.openAt)?.[1] || '',[tileRow],true);
 group(p,'Opening',exp,
  row(0xf359,'Taskbar icon','The tray icon stays available while the pad is hidden',S.segments([['visible','While visible'],['always','Always'],['never','Never']],state.taskbar,v=>state.taskbar=v)),
  row(0xf2dd,'Fresh note after','Away this long, the hotkey opens a new page',markDropdown(S.dropdown([['0','Never'],['5','5 minutes'],['15','15 minutes'],['30','30 minutes'],['120','2 hours']],state.fresh,v=>state.fresh=v))));
 group(p,'Window',row(0xf601,'Keep on top',null,S.toggle(state.pinned,v=>state.pinned=v,'Keep on top')),
  row(0xf165,'Hide when I click away','Not while kept on top',S.toggle(state.blur,v=>state.blur=v,'Hide when I click away')),
  row(0xf60e,'Start with Windows',null,S.toggle(state.startup,v=>state.startup=v,'Start with Windows')));
 group(p,'Saving',row(null,'Save after I stop typing','Your note is also saved when you hide, switch notes, or quit.',markDropdown(S.dropdown([['400','0.4 s'],['5000','5 s'],['10000','10 s'],['30000','30 s']],state.save,v=>state.save=v))));
 return p;
 }); }
LAB.settingsPages.System=root=>root.append(systemPage());


const definitions = [
 [
  "new-note",
  "New note",
  "Notes",
  "Pad",
  "Ctrl+N",
  "Create a note.",
  false
 ],
 [
  "next-note",
  "Next note (next slot in slots)",
  "Notes",
  "Pad",
  "Alt+Right",
  "Move to the next note; at the end, create a note. In a slot, move to the next slot.",
  false
 ],
 [
  "previous-note",
  "Previous note (previous slot in slots)",
  "Notes",
  "Pad",
  "Alt+Left",
  "Move to the previous note. In a slot, move to the previous slot.",
  false
 ],
 [
  "next-alias",
  "Next note (alias)",
  "Notes",
  "Pad",
  "",
  "Another key for the next note.",
  false
 ],
 [
  "previous-alias",
  "Previous note (alias)",
  "Notes",
  "Pad",
  "",
  "Another key for the previous note.",
  false
 ],
 [
  "first-note",
  "First note",
  "Notes",
  "Pad",
  "",
  "Open the oldest note in the stack.",
  false
 ],
 [
  "latest-note",
  "Latest note",
  "Notes",
  "Pad",
  "",
  "Open the newest note in the stack.",
  false
 ],
 [
  "move-earlier",
  "Move note earlier",
  "Notes",
  "Pad",
  "",
  "Move the current stack note one place earlier.",
  false
 ],
 [
  "move-later",
  "Move note later",
  "Notes",
  "Pad",
  "",
  "Move the current stack note one place later.",
  false
 ],
 [
  "delete-note",
  "Delete note",
  "Notes",
  "Pad",
  "",
  "Move the current note to the trash.",
  false
 ],
 [
  "search",
  "Search notes",
  "Search",
  "Pad",
  "Ctrl+F",
  "Toggle Search.",
  false
 ],
 [
  "search-alias-p",
  "Search notes (P alias)",
  "Search",
  "Pad",
  "",
  "Another key to toggle Search.",
  false
 ],
 [
  "search-alias-k",
  "Search notes (K alias)",
  "Search",
  "Pad",
  "",
  "Another key to toggle Search.",
  false
 ],
 [
  "settings",
  "Settings",
  "Window",
  "Pad",
  "",
  "Open Settings.",
  false
 ],
 [
  "help",
  "Help",
  "Window",
  "Pad",
  "F1",
  "Show this shortcut list.",
  false
 ],
 [
  "open-slot",
  "Open slot 1–9",
  "Slots",
  "Pad",
  "",
  "Open the numbered slot; use the same key again to return to the stack.",
  true
 ],
 [
  "keep-slot",
  "Keep note in slot 1–9",
  "Slots",
  "Pad",
  "",
  "Move the current note into the numbered slot.",
  true
 ],
 [
  "stack",
  "Back to the stack",
  "Slots",
  "Pad",
  "",
  "Return from a slot to the note stack.",
  false
 ],
 [
  "swap-view",
  "Swap stack and slots",
  "Slots",
  "Pad",
  "",
  "Switch between the stack and slots.",
  false
 ],
 [
  "split",
  "Split view",
  "Slots",
  "Pad",
  "Ctrl+\\",
  "Show a slot beside the current note.",
  false
 ],
 [
  "keep-on-top",
  "Keep on top",
  "Window",
  "Pad",
  "Ctrl+T",
  "Keep the pad above other windows.",
  false
 ],
 [
  "text-size-up",
  "Text size up",
  "Writing",
  "Pad",
  "Ctrl++",
  "Increase note text size.",
  false
 ],
 [
  "text-size-down",
  "Text size down",
  "Writing",
  "Pad",
  "Ctrl+-",
  "Decrease note text size.",
  false
 ],
 [
  "copy-note",
  "Copy the whole note",
  "Writing",
  "Pad",
  "Ctrl+Shift+C",
  "Copy all note text.",
  false
 ],
 [
  "ocr",
  "Read text from a screen snip",
  "Writing",
  "Pad",
  "Ctrl+Shift+O",
  "Read text from a screen snip.",
  false
 ],
 [
  "checkbox-timer",
  "Checkbox or timer",
  "Writing",
  "Pad",
  "",
  "Toggle a checkbox or timer at the caret.",
  false
 ],
 [
  "fold",
  "Fold at caret",
  "Writing",
  "Pad",
  "Ctrl+Shift+[",
  "Fold the block at the caret.",
  false
 ],
 [
  "unfold",
  "Unfold at caret",
  "Writing",
  "Pad",
  "Ctrl+Shift+]",
  "Unfold the block at the caret.",
  false
 ],
 [
  "quit",
  "Quit",
  "Window",
  "Pad",
  "Ctrl+Shift+Q",
  "Quit AntiNotepad.",
  false
 ],
 [
  "grid",
  "Grid view (in search)",
  "Search",
  "Search",
  "",
  "Switch between list and grid results.",
  false
 ],
 [
  "trash",
  "Trash (in search)",
  "Search",
  "Search",
  "",
  "Open the trash or return to notes.",
  false
 ]
];
function shortcutsPage(){return mount(root=>{
 const p=pageRoot();
 group(p,'Shortcut map',row(0xf4b8,'Reset all shortcuts','Restore every action to its default key.',action('Reset all',()=>{state.shortcuts={};refresh(root);}))); 
 for(const section of ['Notes','Search','Window','Slots','Writing']){
  const rows=definitions.filter(d=>d[2]===section).map(d=>{
   const [id,name,,scope,defaultKey,help,family]=d;
   const current=()=>Object.prototype.hasOwnProperty.call(state.shortcuts,id)?state.shortcuts[id]:defaultKey;
   const set=value=>{
    if(family&&value&&!/[1-9]$/.test(value)){window.alert('Use a number key from 1 to 9 for this action.');return false;}
    if(value&&[state.hotkey,state.newHotkey].includes(value)){window.alert('That key is assigned to a global hotkey.');return false;}
    if(value&&/^(Ctrl\+(A|C|X|V|Z|Y|D|I)|Escape|Esc)$/.test(value)){window.alert('That key is reserved for text editing.');return false;}
    const other=definitions.find(x=>x[0]!==id&&x[3]===scope&&(Object.prototype.hasOwnProperty.call(state.shortcuts,x[0])?state.shortcuts[x[0]]:x[4])===value&&value);
    if(other){if(!window.confirm(`${value} is assigned to ${other[1]}. Give it to ${name}? The other action will have no shortcut.`))return false;state.shortcuts[other[0]]='';}
    state.shortcuts[id]=value;queueMicrotask(()=>refresh(root));return true;
   };
   const control=stack(recorder(current,set,true,true),action('Reset',()=>{delete state.shortcuts[id];refresh(root);}));
   control.style.gap='var(--snap-5-row-gap)'; LAB.mark(control,5);
   return row(0xf4b8,name,defaultKey?help:`No default shortcut. ${help}`,control);
  });
  group(p,section,...rows);
 }
 return p;
 });}
LAB.settingsPages.Shortcuts=root=>root.append(shortcutsPage());
function storagePage(){return mount(root=>{
 const p=pageRoot(); const details=node('div');details.style.cssText='display:flex;flex-direction:column;gap:8px;width:100%';
 const path=node('textarea','fl-textbox');path.value=state.folder;path.readOnly=true;path.setAttribute('aria-label','Notes folder');path.style.cssText='width:100%;height:56px;resize:none;overflow:hidden';details.append(path);
 if(state.pending){const target=node('div','',`New location: ${state.pending}`);target.style.cssText='font-size:12px;color:var(--muted);overflow-wrap:anywhere';details.append(target);}
 if(state.notice){const note=node('div','',state.notice);note.style.cssText='font-size:12px;color:var(--muted)';details.append(note);}
 const actions=stack();actions.style.gap='var(--snap-5-row-gap)';LAB.mark(actions,5);
 if(state.pending){actions.style.justifyContent='flex-end';actions.append(action('Cancel',()=>{state.pending='';state.notice='';refresh(root);}),action('Migrate notes',()=>{state.folder=state.pending;state.pending='';state.notice='Notes moved to the new location.';refresh(root);},true));}
 else {actions.append(action('Open',()=>{state.notice=`Opened ${state.folder} (lab preview)`;refresh(root);}),action('Change location…',()=>{const value=window.prompt('Choose notes folder',state.folder);if(value&&value.trim()&&value.trim()!==state.folder){state.pending=value.trim();state.notice='';refresh(root);}}));}
 details.append(actions);
 const sub=S.subRow('',details);sub.style.alignItems='stretch';sub.firstElementChild.style.display='none';
 group(p,'Notes folder',row(0xf418,'Notes folder','Your notes are plain files here, saved as you type.',null),sub);
 const recovered=state.folder+'\\recovered';
 group(p,'Other locations',row(0xf418,'Recovered notes',`${recovered}\nEmpty\nNote files the app couldn't place. Open them in any text editor.`,action('Open',()=>window.alert(`Open folder: ${recovered} (lab preview)`))),
  row(0xf359,'App data',`${defaults().folder}\nLog file and the pointer to your notes folder.`,action('Open',()=>window.alert(`Open folder: ${defaults().folder} (lab preview)`))));
 return p;
 });}
LAB.settingsPages.Storage=root=>root.append(storagePage());
const sharedDefaults={theme:'system',lightPalette:'paper',darkPalette:'ink',material:'acrylic',tint:'0.7',scale:'1',caretStyle:'bar',caretBlink:true,caretGlide:true,splitPalette:'same',chrome:'fade',fadeAfter:'1600',bars:'docked',showDelete:true,showNew:true,slotDots:'pill',edgeFade:true,menuAction:true,menuColors:false,dimPinned:true,accent:'default'};
function showHelp(){
 const cover=node('div');cover.style.cssText='position:fixed;inset:0;z-index:200;background:#0009;display:grid;place-items:center';
 const panel=node('div');panel.style.cssText='width:min(600px,90%);max-height:80%;overflow:auto;padding:24px;background:var(--surface);border:1px solid var(--faint);border-radius:8px';
 const head=stack(node('strong','','Keyboard shortcuts'),action('Close',()=>cover.remove()));head.style.justifyContent='space-between';panel.append(head);
 for(const section of ['Notes','Search','Window','Slots','Writing']){panel.append(S.group(section));for(const d of definitions.filter(x=>x[2]===section)){const line=node('div','',`${d[1]}  ·  ${state.shortcuts[d[0]] ?? d[4] ?? ''}`);line.style.cssText='padding:5px 0;color:var(--muted)';panel.append(line);}}
 cover.append(panel);cover.addEventListener('click',e=>{if(e.target===cover)cover.remove();});document.body.append(cover);
}
function aboutPage(ctx){return mount(root=>{
 const p=pageRoot();
 group(p,'AntiNotepad',row(0xf4a3,'AntiNotepad 0.7.0','A quick scratchpad for Windows\nCopyright © 2026',null),
  row(0xf4b8,'Keyboard shortcuts','Every shortcut on one page',action('Show',()=>{showHelp();})),
  row(0xf19f,'Restore default settings',"Undo it from the pad's notice",action('Restore',()=>{
   const old=structuredClone(state); const oldShared=structuredClone(ctx.state); Object.assign(ctx.state,sharedDefaults); ctx.setTheme('ink'); const fresh=defaults(); fresh.folder=state.folder;state=LAB.settingsCState=fresh;
   const notice=node('div');notice.style.cssText='position:fixed;bottom:28px;right:32px;z-index:100';
   notice.append(LAB.kit.pad.toast('Settings restored','Undo',()=>{state=LAB.settingsCState=old;Object.assign(ctx.state,oldShared);ctx.setTheme(oldShared.theme==='light'?oldShared.lightPalette:oldShared.darkPalette);notice.remove();refresh(root);}));document.body.append(notice);refresh(root);
  })));
 return p;
 });}
LAB.settingsPages.About=(root,kit,ctx)=>root.append(aboutPage(ctx));
})();
