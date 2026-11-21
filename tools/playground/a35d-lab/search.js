// SearchOverlay.cs lab rendering. The host supplies notes and overlay callbacks.
(() => {
  const LAB = window.LAB;
  LAB.overlays = LAB.overlays || {};
  LAB.overlays.search = { mount(host, ctx = {}) {
    const el = (tag, cls, text) => {
      const node = document.createElement(tag);
      if (cls) node.className = cls;
      if (text != null) node.textContent = text;
      return node;
    };
    const style = el('style');
    style.textContent = `
      .search-shell { position:absolute; inset:40px 0 0; background:linear-gradient(130deg,var(--canvas),color-mix(in srgb,var(--canvas) 92%,#563225)); }
      .search-overlay { position:absolute; inset:var(--snap-9-overlay-margin) var(--snap-9-overlay-margin) 46px; display:flex; flex-direction:column; min-height:0; color:var(--ink); font-family:'Segoe UI Variable Text','Segoe UI',sans-serif; }
      .search-header { display:flex; align-items:center; margin-bottom:8px; flex:none; }
      .search-query { width:0; flex:1; min-width:0; height:40px; border:1px solid color-mix(in srgb,var(--faint) 50%,transparent); border-radius:8px; padding:8px 12px; background:var(--surface); color:var(--ink); caret-color:var(--caret); font-size:16px; outline:none; }
      .search-query:focus { border:2px solid color-mix(in srgb,var(--accent) 90%,transparent); padding:7px 11px; }
      .search-query::placeholder { color:var(--faint); }
      .search-tools { display:flex; align-items:center; gap:2px; margin-left:var(--snap-10-field-margin); }
      .search-results { min-height:0; flex:1; overflow-y:auto; overflow-x:hidden; scrollbar-width:thin; scrollbar-color:color-mix(in srgb,var(--faint) 50%,transparent) transparent; }
      .search-results.list { display:flex; flex-direction:column; gap:2px; }
      .search-results.grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(min(192px, calc((100% - 8px) / 2)),1fr)); align-content:start; gap:8px; }
      .search-card { display:block; width:100%; text-align:left; min-width:0; padding:8px 12px; border:1px solid color-mix(in srgb,var(--faint) 30%,transparent); border-radius:8px; background:color-mix(in srgb,var(--surface) 50%,transparent); color:var(--ink); cursor:pointer; }
      .search-card.current { border:2px solid color-mix(in srgb,var(--accent) 90%,transparent); padding:7px 11px; background:var(--surface); }
      .search-card.selected { background:var(--selection); }
      .search-card:active { filter:brightness(.94); }
      .search-results.grid .search-card { height:152px; }
      .search-body { display:flex; flex-direction:column; gap:var(--snap-11-line-gap); }
      .search-title { font-size:14px; font-weight:600; overflow:hidden; white-space:nowrap; text-overflow:ellipsis; line-height:17px; }
      .search-snippet { font:12px/15px 'Cascadia Mono',Consolas,monospace; color:var(--muted); overflow:hidden; display:-webkit-box; -webkit-box-orient:vertical; -webkit-line-clamp:2; overflow-wrap:anywhere; }
      .search-results.grid .search-snippet { -webkit-line-clamp:5; }
      .search-snippet mark { color:var(--ink); background:var(--selection); font-weight:600; }
      .search-meta { display:flex; gap:8px; font-size:12px; line-height:15px; color:var(--faint); }
      .search-slot { color:var(--accent); font-weight:600; } .search-mode { color:var(--keyword); font-weight:600; }
      .search-empty { margin:16px var(--snap-10-row-margin); color:var(--faint); }
      .search-status { flex:none; margin:8px 0 0 4px; color:var(--faint); font-size:12px; line-height:16px; }
    `;
    const shell = el('div', 'search-shell');
    const root = el('div', 'search-overlay');
    LAB.mark(root, 9);
    const header = el('div', 'search-header');
    const query = el('input', 'search-query');
    query.type = 'text'; query.placeholder = 'Search notes';
    query.setAttribute('aria-label', 'Search notes');
    const tools = el('div', 'search-tools'); LAB.mark(tools, 10);
    const results = el('div', 'search-results list');
    const status = el('div', 'search-status');
    header.append(query, tools); root.append(header, results, status);
    shell.append(root); host.replaceChildren(style, shell);
    let grid = !!ctx.grid, trash = ctx.trash === true, cursor = 0, matches = [];
    const selected = new Set();
    const getText = note => typeof note === 'string' ? note : String(note.text ?? note.body ?? note.content ?? '');
    const parts = note => {
      const text = getText(note).replace(/\r/g, '');
      const lines = text.split('\n');
      const first = lines[0].trim().replace(/:$/, '').toLowerCase();
      const keyword = /^(math|list|todo|checklist|bullet|bulletlist|numberlist|timer|clipboard|code)(?:[ :].+)?$/.test(first) ? first.split(/[ :]/)[0] : '';
      const detected = /^(todo|checklist|bullet|bulletlist|numberlist)$/.test(keyword) ? 'list' : keyword;
      const mode = typeof note === 'object' && note.mode ? String(note.mode) : detected;
      const skip = keyword ? 1 : 0;
      const fallback = lines.slice(skip, skip + 40).map(s => s.trim().replace(/^[#\-* >]+/, '').replace(/\[[ x]\]/g, '').trim()).find(Boolean) || 'Empty note';
      const title = typeof note === 'object' && note.title && note.title !== lines[0] ? String(note.title) : (fallback.length > 60 ? fallback.slice(0, 60) + '…' : fallback);
      return { text, lines, mode, skip, title };
    };
    const ago = value => {
      if (!value) return 'just now';
      const date = new Date(value), mins = Math.floor((Date.now() - date.getTime()) / 60000);
      if (!Number.isFinite(mins)) return String(value);
      if (mins < 1) return 'just now';
      if (mins < 60) return `${mins}m ago`;
      if (date.toDateString() === new Date().toDateString()) return date.toLocaleTimeString([], { hour:'numeric', minute:'2-digit' });
      if (mins < 2880) return 'yesterday';
      if (mins < 10080) return date.toLocaleDateString([], { weekday:'short' });
      return date.toLocaleDateString([], { month:'short', day:'numeric' });
    };
    const markMatches = (node, text, terms) => {
      const lower = text.toLocaleLowerCase(); let at = 0;
      while (at < text.length) {
        let found = -1, term = '';
        for (const candidate of terms) {
          const index = lower.indexOf(candidate.toLocaleLowerCase(), at);
          if (index >= 0 && (found < 0 || index < found)) { found = index; term = candidate; }
        }
        if (found < 0) { node.append(document.createTextNode(text.slice(at))); break; }
        if (found > at) node.append(document.createTextNode(text.slice(at, found)));
        node.append(el('mark', '', text.slice(found, found + term.length)));
        at = found + term.length;
      }
    };
    const source = () => (trash ? (ctx.trashNotes || (Array.isArray(ctx.trash) ? ctx.trash : [])) : (ctx.notes || []));
    const refreshMatches = () => {
      const terms = query.value.split(' ').filter(Boolean);
      matches = source().map((note, index) => ({ note, index, ...parts(note) })).filter(item => {
        const haystack = (item.title + '\n' + item.text).toLocaleLowerCase();
        return (trash || item.text.trim().length > 0) && terms.every(term => haystack.includes(term.toLocaleLowerCase()));
      });
      cursor = Math.min(cursor, Math.max(0, matches.length - 1));
      return terms;
    };
    const updateStatus = () => {
      const count = matches.length;
      const hint = trash ? 'Enter restores · Delete removes forever' : 'Enter opens · Ctrl+Delete moves to trash · Ctrl+click selects several';
      status.textContent = `${count} ${trash ? 'in trash' : count === 1 ? 'note' : 'notes'}${selected.size ? ` · ${selected.size} selected` : ''} · ${hint}`;
    };
    const close = () => { if (typeof ctx.close === 'function') ctx.close(); else if (typeof ctx.closeOverlay === 'function') ctx.closeOverlay(); else host.replaceChildren(); };
    const open = () => { if (matches[cursor] && typeof ctx.openNote === 'function') ctx.openNote(matches[cursor].index); };
    const updateCurrent = () => {
      Array.from(results.querySelectorAll('.search-card')).forEach((card, i) => {
        card.classList.toggle('current', i === cursor);
        card.classList.toggle('selected', selected.has(matches[i]?.note));
        card.setAttribute('aria-selected', String(i === cursor));
      });
      updateStatus();
    };
    const render = () => {
      const terms = refreshMatches();
      results.className = 'search-results ' + (grid ? 'grid' : 'list');
      results.replaceChildren();
      if (!matches.length) {
        const empty = el('div', 'search-empty', trash ? 'The trash is empty.' : query.value.length ? 'No notes match.' : 'No notes yet.');
        LAB.mark(empty, 10); results.append(empty);
      }
      matches.forEach((item, index) => {
        const card = el('button', 'search-card'); card.type = 'button';
        card.setAttribute('aria-label', item.title);
        const body = el('div', 'search-body'); LAB.mark(body, 11);
        const title = el('div', 'search-title', item.title);
        let lines = item.lines.slice(item.skip ? 2 : 1, 400).filter(line => line.trim());
        if (terms.length) {
          const hit = lines.findIndex(line => line.toLocaleLowerCase().includes(terms[0].toLocaleLowerCase()));
          if (hit > 0) lines = lines.slice(hit);
        }
        const snippetText = (typeof item.note === 'object' && item.note.snippet != null ? String(item.note.snippet) : lines.slice(0, 6).map(line => line.trim().replace(/\[ \]/g, '☐').replace(/\[x\]/g, '☑')).join('  ·  '));
        const snippet = el('div', 'search-snippet'); markMatches(snippet, snippetText, terms);
        const meta = el('div', 'search-meta');
        if (item.note.slot > 0) meta.append(el('span', 'search-slot', `slot ${item.note.slot}`));
        if (item.mode && item.mode.toLowerCase() !== 'plain') meta.append(el('span', 'search-mode', item.mode));
        const timestamp = trash && item.note.deleted ? 'deleted ' + ago(item.note.deleted) : ago(item.note.modified);
        meta.append(el('span', '', timestamp));
        body.append(title, snippet, meta); card.append(body);
        card.addEventListener('mouseenter', () => { cursor = index; updateCurrent(); });
        card.addEventListener('click', event => {
          cursor = index;
          if (event.ctrlKey) { selected.has(item.note) ? selected.delete(item.note) : selected.add(item.note); updateCurrent(); }
          else open();
        });
        results.append(card);
      });
      updateCurrent();
    };
    const tool = (glyph, label, click) => LAB.kit.pad.iconButton(glyph, label, click, 32);
    const renderTools = () => {
      tools.replaceChildren(
        tool(grid ? '\uEA37' : '\uF0E2', grid ? 'List view' : 'Card grid', () => { grid = !grid; renderTools(); render(); query.focus(); }),
        tool(trash ? '\uE72B' : '\uE74D', trash ? 'Back to notes' : `Trash · ${(ctx.trashNotes || (Array.isArray(ctx.trash) ? ctx.trash : [])).length}`, () => {
          trash = !trash; query.value = ''; query.placeholder = trash ? 'Search the trash' : 'Search notes'; cursor = 0; selected.clear(); renderTools(); render(); query.focus();
        }),
        tool('\uE711', 'Close (Esc)', close)
      );
    };
    const onKey = event => {
      if (event.key === 'Escape') { event.preventDefault(); close(); return; }
      if (event.key === 'Enter' && !event.ctrlKey && !event.altKey && !event.metaKey) { event.preventDefault(); open(); return; }
      if (event.ctrlKey || event.altKey || event.metaKey) return;
      let step = 0;
      const cards = results.querySelectorAll('.search-card');
      const columns = grid && cards.length > 1 ? Math.max(1, Math.round(results.clientWidth / (cards[0].offsetWidth + 8))) : 1;
      if (event.key === 'ArrowDown') step = columns;
      if (event.key === 'ArrowUp') step = -columns;
      if (grid && event.key === 'ArrowRight' && query.selectionStart === query.value.length) step = 1;
      if (grid && event.key === 'ArrowLeft' && query.selectionStart === 0) step = -1;
      if (!step || !matches.length) return;
      event.preventDefault(); cursor = Math.max(0, Math.min(matches.length - 1, cursor + step)); updateCurrent();
      cards[cursor]?.scrollIntoView({ block:'nearest' });
    };
    query.addEventListener('input', () => { cursor = 0; selected.clear(); render(); });
    root.addEventListener('keydown', onKey);
    renderTools(); render();
    query.focus();
    return { unmount() { root.removeEventListener('keydown', onKey); if (shell.parentNode === host) host.replaceChildren(); } };
  }};
})();
