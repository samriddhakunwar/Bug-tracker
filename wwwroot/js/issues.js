/* issues.js */
if (!Auth.requireAuth()) throw new Error('Not authenticated');
initSidebar();

let currentPage = 1;
let totalPages  = 1;
let allUsers    = [];

// ── Load Users for assign dropdown ──────────────────────────
async function loadUsers() {
  try {
    if (Auth.isAdmin()) {
      const res = await api.get('/users');
      allUsers = res.data;
    }
  } catch (_) {}
  populateAssignDropdown('issue-assigned', null);
}

function populateAssignDropdown(selectId, selectedId) {
  const sel = document.getElementById(selectId);
  sel.innerHTML = '<option value="">Unassigned</option>' +
    allUsers.map(u => `<option value="${u.id}" ${u.id == selectedId ? 'selected' : ''}>${escHtml(u.username)}</option>`).join('');
}

// ── Load Issues ──────────────────────────────────────────────
async function loadIssues(page = 1) {
  currentPage = page;
  const search   = document.getElementById('filter-search').value.trim();
  const status   = document.getElementById('filter-status').value;
  const priority = document.getElementById('filter-priority').value;

  const params = new URLSearchParams({ page, pageSize: 15 });
  if (search)   params.set('search', search);
  if (status)   params.set('status', status);
  if (priority) params.set('priority', priority);

  const tbody = document.getElementById('issues-body');
  tbody.innerHTML = `<tr class="loading-row"><td colspan="8"><span class="spinner"></span> Loading…</td></tr>`;

  try {
    const res = await api.get(`/issues?${params}`);
    const { issues, totalCount, totalPages: tp } = res.data;
    totalPages = tp;

    if (!issues.length) {
      tbody.innerHTML = `<tr><td colspan="8"><div class="empty-state" style="padding:40px;">
        <i class="fa-solid fa-inbox"></i>No issues found.</div></td></tr>`;
      document.getElementById('pagination-bar').style.display = 'none';
      return;
    }

    tbody.innerHTML = issues.map(i => `
      <tr onclick="location.href='/issue-detail.html?id=${i.id}'">
        <td class="text-muted text-sm">#${i.id}</td>
        <td>
          <div class="fw-600" style="max-width:260px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;">${escHtml(i.title)}</div>
          <div class="text-muted text-sm">${i.commentCount} comment${i.commentCount !== 1 ? 's' : ''}</div>
        </td>
        <td>${statusBadge(i.status)}</td>
        <td>${priorityBadge(i.priority)}</td>
        <td>${severityBadge(i.severity)}</td>
        <td>${i.assignedToUsername ? escHtml(i.assignedToUsername) : '<span class="text-muted">—</span>'}</td>
        <td class="text-muted text-sm">${formatDate(i.createdAt)}</td>
        <td onclick="event.stopPropagation()">
          <div class="d-flex gap-8">
            <button class="btn btn-secondary btn-sm btn-icon" title="Edit" onclick="openEditModal(${i.id})">
              <i class="fa-solid fa-pen"></i>
            </button>
            ${Auth.isAdmin() ? `<button class="btn btn-danger btn-sm btn-icon" title="Delete" onclick="deleteIssue(${i.id}, '${escHtml(i.title)}')">
              <i class="fa-solid fa-trash"></i></button>` : ''}
          </div>
        </td>
      </tr>`).join('');

    renderPagination(page, tp, totalCount);
  } catch (e) {
    tbody.innerHTML = `<tr class="loading-row"><td colspan="8" style="color:var(--danger);">${escHtml(e.message)}</td></tr>`;
  }
}

function renderPagination(page, tp, total) {
  const bar = document.getElementById('pagination-bar');
  if (tp <= 1) { bar.style.display = 'none'; return; }
  bar.style.display = 'flex';

  let html = `<button class="page-btn" ${page===1?'disabled':''} onclick="loadIssues(${page-1})">‹ Prev</button>`;
  for (let p = Math.max(1, page-2); p <= Math.min(tp, page+2); p++) {
    html += `<button class="page-btn ${p===page?'active':''}" onclick="loadIssues(${p})">${p}</button>`;
  }
  html += `<button class="page-btn" ${page===tp?'disabled':''} onclick="loadIssues(${page+1})">Next ›</button>`;
  html += `<span class="page-info">${total} total issues</span>`;
  bar.innerHTML = html;
}

// ── Create/Edit Modal ────────────────────────────────────────
let editingId = null;

function openCreateModal() {
  editingId = null;
  document.getElementById('modal-title').textContent = 'New Issue';
  document.getElementById('issue-form').reset();
  document.getElementById('issue-id').value = '';
  document.getElementById('modal-alert').innerHTML = '';
  populateAssignDropdown('issue-assigned', null);
  document.getElementById('issue-status').closest('.form-row').style.display = 'none';
  openModal('issue-modal');
}

async function openEditModal(id) {
  editingId = id;
  document.getElementById('modal-title').textContent = 'Edit Issue';
  document.getElementById('modal-alert').innerHTML = '';
  openModal('issue-modal');

  try {
    const res = await api.get(`/issues/${id}`);
    const i = res.data;
    document.getElementById('issue-id').value         = i.id;
    document.getElementById('issue-title').value      = i.title;
    document.getElementById('issue-description').value= i.description;
    document.getElementById('issue-priority').value   = ['Low','Medium','High'].indexOf(i.priority);
    document.getElementById('issue-severity').value   = ['Minor','Major','Critical'].indexOf(i.severity);
    document.getElementById('issue-status').value     = ['Open','InProgress','Resolved'].indexOf(i.status);
    document.getElementById('issue-status').closest('.form-row').style.display = '';
    populateAssignDropdown('issue-assigned', i.assignedToUserId);
  } catch (e) {
    showAlert('modal-alert', e.message);
  }
}

document.getElementById('modal-save').addEventListener('click', async () => {
  const title = document.getElementById('issue-title').value.trim();
  if (!title) { showAlert('modal-alert', 'Title is required.'); return; }

  const btn = document.getElementById('modal-save');
  btn.disabled = true; btn.textContent = 'Saving…';

  const payload = {
    title,
    description: document.getElementById('issue-description').value,
    priority:    parseInt(document.getElementById('issue-priority').value),
    severity:    parseInt(document.getElementById('issue-severity').value),
    assignedToUserId: document.getElementById('issue-assigned').value || null
  };

  try {
    if (editingId) {
      payload.status = parseInt(document.getElementById('issue-status').value);
      await api.put(`/issues/${editingId}`, payload);
    } else {
      await api.post('/issues', payload);
    }
    closeModal('issue-modal');
    loadIssues(currentPage);
    showAlert('alert-container', editingId ? 'Issue updated.' : 'Issue created.', 'success');
  } catch (e) {
    showAlert('modal-alert', e.message);
  } finally {
    btn.disabled = false; btn.textContent = 'Save Issue';
  }
});

async function deleteIssue(id, title) {
  if (!confirmDelete(`Delete issue "${title}"? This cannot be undone.`)) return;
  try {
    await api.delete(`/issues/${id}`);
    loadIssues(currentPage);
    showAlert('alert-container', 'Issue deleted.', 'success');
  } catch (e) {
    showAlert('alert-container', e.message);
  }
}

// ── Modal helpers ────────────────────────────────────────────
function openModal(id)  { document.getElementById(id).classList.add('open'); }
function closeModal(id) { document.getElementById(id).classList.remove('open'); }

document.getElementById('btn-new-issue').addEventListener('click', openCreateModal);
document.getElementById('modal-close').addEventListener('click',  () => closeModal('issue-modal'));
document.getElementById('modal-cancel').addEventListener('click', () => closeModal('issue-modal'));
document.getElementById('issue-modal').addEventListener('click', e => { if (e.target.id === 'issue-modal') closeModal('issue-modal'); });

// ── Filters ──────────────────────────────────────────────────
let filterTimer;
['filter-search','filter-status','filter-priority'].forEach(id => {
  document.getElementById(id).addEventListener('input', () => {
    clearTimeout(filterTimer);
    filterTimer = setTimeout(() => loadIssues(1), 350);
  });
});

document.getElementById('btn-clear-filters').addEventListener('click', () => {
  document.getElementById('filter-search').value  = '';
  document.getElementById('filter-status').value  = '';
  document.getElementById('filter-priority').value= '';
  loadIssues(1);
});

// ── Init ─────────────────────────────────────────────────────
loadUsers();
loadIssues(1);
