async function apiGet(url) {
  const r = await fetch(url, { credentials: 'same-origin' });
  if (r.ok) return r.json();
  throw new Error(await r.text());
}

async function apiPost(url, body) {
  const r = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'same-origin',
    body: JSON.stringify(body || {})
  });
  let data = null;
  try { data = await r.json(); } catch { data = null; }
  return { ok: r.ok, status: r.status, ...(data || {}) };
}

const esc = s => (s ?? '').replace(/[&<>"]/g, m => (
  { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[m]
));

function getQueryParam(name) {
  try {
    const url = new URL(window.location.href);
    return url.searchParams.get(name);
  } catch { return null; }
}


(function () {
  const html = document.documentElement;
  const key = 'theme'; // 'light' | 'dark'
  function setTheme(th) {
    html.setAttribute('data-bs-theme', th);
    try { localStorage.setItem(key, th); } catch {}
    const btn = document.getElementById('btnTheme');
    if (btn) {
      btn.title = th === 'dark' ? 'Switch to light' : 'Switch to dark';
      btn.setAttribute('aria-label', btn.title);
    }
  }
  function getTheme() {
    try { return localStorage.getItem(key); } catch { return null; }
  }
  (function ensureApplied() {
    const stored = getTheme();
    if (stored === 'light' || stored === 'dark') setTheme(stored);
  })();
  document.getElementById('btnTheme')?.addEventListener('click', () => {
    const current = html.getAttribute('data-bs-theme') === 'dark' ? 'dark' : 'light';
    setTheme(current === 'dark' ? 'light' : 'dark');
  });
})();

document.getElementById('global-search')?.addEventListener('submit',(e)=>{
});

document.querySelector('form[action="/auth/logout"]')?.addEventListener('submit', async (e)=>{
  e.preventDefault();
  const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
  await fetch('/auth/logout', {
    method: 'POST',
    headers: { 'RequestVerificationToken': token }
  });
  location.href = '/auth/login';
});

async function initHome(){
  const elLatest = document.querySelector('#tblLatest tbody');
  const tblLatest = document.getElementById('tblLatest');
  const latestEmpty = tblLatest?.dataset.empty ?? 'Empty';
  const latestError = tblLatest?.dataset.error ?? 'Error loading data';

  const elTop = document.getElementById('listTop');
  const topEmpty = elTop?.dataset.empty ?? 'Empty';
  const topError = elTop?.dataset.error ?? 'Error loading';

  const elCloud = document.getElementById('tagCloud');
  const cloudEmpty = elCloud?.dataset.empty ?? 'No tags';

  if (!elLatest) return;

  async function loadLatest(){
    try {
      const data = await apiGet('/api/home/latest?take=12');
      elLatest.innerHTML = data.length ? data.map(row=>`
        <tr onclick="location.href='/Inventories/Details/${row.id}'">
          <td class="fw-semibold">${esc(row.title)}</td>
          <td class="text-truncate" style="max-width:360px;">${esc(row.descriptionMarkdown ?? '')}</td>
          <td class="text-nowrap">${new Date(row.createdAt).toLocaleString()}</td>
        </tr>`).join('') : `<tr><td colspan="3" class="text-muted">${latestEmpty}</td></tr>`;
    } catch {
      elLatest.innerHTML = `<tr><td colspan="3" class="text-danger">${latestError}</td></tr>`;
    }
  }

  async function loadTop(){
    try {
      const data = await apiGet('/api/home/top?take=5');
      elTop.innerHTML = data.length ? data.map(x=>`
        <li class="list-group-item d-flex justify-content-between align-items-center"
            onclick="location.href='/Inventories/Details/${x.id}'">
          <span class="text-truncate">${esc(x.title)}</span>
          <span class="badge bg-primary rounded-pill">${new Date(x.updatedAt).toLocaleDateString()}</span>
        </li>`).join('') : `<li class="list-group-item text-muted">${topEmpty}</li>`;
    } catch {
      elTop.innerHTML = `<li class="list-group-item text-danger">${topError}</li>`;
    }
  }

async function loadCloud(){
  try{
    const raw = await apiGet('/api/tags/cloud?take=40');

    const list = Array.isArray(raw) ? raw.map(it => {
      if (it && typeof it === 'object' && !Array.isArray(it)) {
        return { tag: it.tag ?? it.Tag ?? "", count: it.count ?? it.Count ?? 0 };
      }
      if (Array.isArray(it)) {
        const [tag, count] = it;
        return { tag: String(tag ?? ""), count: Number(count ?? 0) };
      }
      return { tag: "", count: 0 };
    }).filter(x => x.tag) : [];

    elCloud.innerHTML = list.length
      ? list.map(({tag, count}) => {
          const size = Math.min(32, 12 + (Number(count)||0)*2);
          return `<a href="/search?tag=${encodeURIComponent(tag)}" class="me-2 mb-2 d-inline-block"
                     style="font-size:${size}px;text-decoration:none">${esc(tag)}</a>`;
        }).join('')
      : `<span class="text-muted">${cloudEmpty}</span>`;
  } catch {
    elCloud.innerHTML = `<span class="text-muted">${cloudEmpty}</span>`;
  }
}


  await Promise.all([loadLatest(), loadTop(), loadCloud()]);
  document.getElementById('btnLatestRefresh')?.addEventListener('click', loadLatest);
}

function initLogin(){}
function initRegister(){}

async function initSearch(){
  const invTbody = document.querySelector('#tblInv tbody');
  const itTbody  = document.querySelector('#tblItems tbody');
  if (!invTbody && !itTbody) return;

  const invLoading = document.getElementById('invLoading');
  const itLoading  = document.getElementById('itLoading');
  const invEmpty   = document.getElementById('invEmpty');
  const itEmpty    = document.getElementById('itEmpty');
  const invError   = document.getElementById('invError');
  const itError    = document.getElementById('itError');

  const q   = (getQueryParam('q')   || '').trim();
  const tag = (getQueryParam('tag') || '').trim();

  if (!q && !tag) {
    invLoading?.classList.add('d-none'); itLoading?.classList.add('d-none');
    invEmpty?.classList.remove('d-none'); itEmpty?.classList.remove('d-none');
    return;
  }

  try {
    if (tag && !q) {
      const data = await apiGet(`/api/search/by-tag?tag=${encodeURIComponent(tag)}&page=1&pageSize=50`);
      invLoading?.classList.add('d-none');
      if (data.items?.length) {
        invTbody && (invTbody.innerHTML = data.items.map(x => `
          <tr>
            <td>${esc(x.title)}</td>
            <td class="text-truncate" style="max-width:320px;">${esc(x.descriptionMarkdown ?? '')}</td>
            <td class="text-nowrap">${new Date(x.createdAt).toLocaleString()}</td>
            <td class="text-end">
              <a class="btn btn-sm btn-outline-primary" href="/Inventories/Details/${x.id}">Open</a>
            </td>
          </tr>`).join(''));
      } else {
        invEmpty?.classList.remove('d-none');
      }
      itLoading?.classList.add('d-none');
      itEmpty?.classList.remove('d-none');
      return;
    }

    const data = await apiGet(`/api/search?q=${encodeURIComponent(q)}`);

    invLoading?.classList.add('d-none');
    if (data.inventories?.length) {
      invTbody && (invTbody.innerHTML = data.inventories.map(x => `
        <tr>
          <td>${esc(x.title)}</td>
          <td class="text-truncate" style="max-width:320px;">${esc(x.descriptionMarkdown ?? '')}</td>
          <td class="text-nowrap">${new Date(x.createdAt).toLocaleString()}</td>
          <td class="text-end">
            <a class="btn btn-sm btn-outline-primary" href="/Inventories/Details/${x.id}">Open</a>
          </td>
        </tr>`).join(''));
    } else {
      invEmpty?.classList.remove('d-none');
    }

    itLoading?.classList.add('d-none');
    if (data.items?.length) {
      itTbody && (itTbody.innerHTML = data.items.map(x => `
        <tr>
          <td>${esc(x.customId ?? '')}</td>
          <td class="text-truncate" style="max-width:320px;">
            ${esc((x.string1 ?? x.text1 ?? '').toString())}
          </td>
          <td class="text-nowrap">${new Date(x.createdAt ?? x.updatedAt ?? Date.now()).toLocaleString()}</td>
          <td class="text-end">
            <a class="btn btn-sm btn-outline-secondary" href="/Inventories/Details/${x.inventoryId ?? x.inventoryID ?? ''}">Open</a>
          </td>
        </tr>`).join(''));
    } else {
      itEmpty?.classList.remove('d-none');
    }
  } catch {
    invLoading?.classList.add('d-none'); itLoading?.classList.add('d-none');
    invError?.classList.remove('d-none'); itError?.classList.remove('d-none');
  }
}

document.addEventListener('DOMContentLoaded', ()=>{
  const marker = document.querySelector('[data-page]');
  const page = marker?.getAttribute('data-page');
  switch(page){
    case 'home':     initHome();     break;
    case 'login':    initLogin();    break;
    case 'register': initRegister(); break;
    case 'search':   initSearch();   break;
    default: break;
  }
});
