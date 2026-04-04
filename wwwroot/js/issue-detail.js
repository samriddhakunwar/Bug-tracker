/* issue-detail.js */
if (!Auth.requireAuth()) throw new Error();
initSidebar();

const params  = new URLSearchParams(location.search);
const issueId = parseInt(params.get('id'));

if (!issueId) { location.href = '/issues.html'; throw new Error(); }

let currentIssue = null;
let allUsers     = [];

// ── Load all data ────────────────────────────────────────────
async function loadPage() {
  try {
    // Fetch issue, activity, users in parallel
    const [issueRes, logsRes] = await Promise.all([
      api.get(`/issues/${issueId}`),
      api.get(`/activity-logs/${issueId}`)
    ]);

    if (Auth.isAdmin()) {
      try { const ur = await api.get('/users'); allUsers = ur.data; } catch (_) {}
    }

    currentIssue = issueRes.data;
    renderIssue(currentIssue);
    renderActivity(logsRes.data);

    document.getElementById('page-loading').style.display = 'none';
    document.getElementById('page-content').style.display = 'block';
  } catch (e) {
    document.getElementById('page-loading').innerHTML =
      `<div class="alert alert-error"><span>⚠</span>${escHtml(e.message)}</div>`;
  }
}

function renderIssue(i) {
  document.title = `#${i.id} ${i.title} – BugTracker`;
  document.getElementById('topbar-issue-id').textContent = `#${i.id}`;
  document.getElementById('issue-title').textContent = i.title;
  document.getElementById('issue-description').textContent = i.description || 'No description provided.';

  document.getElementById('meta-status').innerHTML   = statusBadge(i.status);
  document.getElementById('meta-priority').innerHTML = priorityBadge(i.priority);
  document.getElementById('meta-severity').innerHTML = severityBadge(i.severity);
  document.getElementById('meta-assignee').textContent = i.assignedToUsername || 'Unassigned';
  document.getElementById('meta-created').textContent  = formatDateTime(i.createdAt);

  document.getElementById('det-reporter').textContent = i.createdByUsername;
  document.getElementById('det-created').textContent  = formatDateTime(i.createdAt);
  document.getElementById('det-updated').textContent  = formatDateTime(i.updatedAt);
  document.getElementById('det-comments').textContent = i.commentCount;

  // Topbar actions
  const me = Auth.getUser();
  const canEdit = Auth.isAdmin() || i.createdByUserId === me.id;
  const actions = document.getElementById('topbar-actions');
  actions.innerHTML = '';
  if (canEdit) {
    const editBtn = document.createElement('button');
    editBtn.className = 'btn btn-secondary btn-sm';
    editBtn.innerHTML = '<i class="fa-solid fa-pen"></i> Edit';
    editBtn.onclick = openEditModal;
    actions.appendChild(editBtn);
  }
  if (Auth.isAdmin()) {
    const delBtn = document.createElement('button');
    delBtn.className = 'btn btn-danger btn-sm';
    delBtn.innerHTML = '<i class="fa-solid fa-trash"></i> Delete';
    delBtn.onclick = deleteIssue;
    actions.appendChild(delBtn);
  }

  renderComments(i);
  renderAttachments(i);
}

// ── Comments ─────────────────────────────────────────────────
function renderComments(issue) {
  const list = document.getElementById('comments-list');
  if (!issue.comments || !issue.comments.length) {
    list.innerHTML = `<div class="empty-state" style="padding:20px 0;"><i class="fa-regular fa-comment"></i>No comments yet. Be the first!</div>`;
    return;
  }
  const me = Auth.getUser();
  list.innerHTML = issue.comments.map(c => `
    <div class="comment-item">
      <div>
        <span class="comment-author">${escHtml(c.user?.username || 'Unknown')}</span>
        <span class="comment-time">${timeAgo(c.createdAt)}</span>
        ${(c.userId === me.id || Auth.isAdmin())
          ? `<button class="btn btn-sm" style="float:right;color:var(--gray-400);background:none;border:none;cursor:pointer;"
               onclick="deleteComment(${c.id})"><i class="fa-solid fa-times"></i></button>` : ''}
      </div>
      <div class="comment-body">${escHtml(c.content)}</div>
    </div>`).join('');
}

document.getElementById('btn-add-comment').addEventListener('click', async () => {
  const txt = document.getElementById('comment-input').value.trim();
  if (!txt) return;
  const btn = document.getElementById('btn-add-comment');
  btn.disabled = true; btn.textContent = 'Posting…';

  try {
    await api.post(`/issues/${issueId}/comments`, { content: txt });
    document.getElementById('comment-input').value = '';
    await refreshIssue();
  } catch (e) {
    showAlert('comment-alert', e.message);
  } finally {
    btn.disabled = false;
    btn.innerHTML = '<i class="fa-solid fa-paper-plane"></i> Post Comment';
  }
});

async function deleteComment(commentId) {
  if (!confirmDelete('Delete this comment?')) return;
  try {
    await api.delete(`/issues/${issueId}/comments/${commentId}`);
    await refreshIssue();
  } catch (e) {
    showAlert('alert-container', e.message);
  }
}

// ── Attachments ───────────────────────────────────────────────
function renderAttachments(issue) {
  const list = document.getElementById('attachments-list');
  if (!issue.attachments || !issue.attachments.length) {
    list.innerHTML = `<div class="empty-state" style="padding:20px 0;"><i class="fa-regular fa-file"></i>No attachments yet.</div>`;
    return;
  }

  const extIcon = (name) => {
    const ext = name.split('.').pop().toLowerCase();
    if (['jpg','jpeg','png','gif'].includes(ext)) return 'fa-image';
    if (ext === 'pdf') return 'fa-file-pdf';
    if (['txt','log'].includes(ext)) return 'fa-file-lines';
    if (ext === 'zip') return 'fa-file-zipper';
    return 'fa-file';
  };

  const fmtSize = (bytes) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes/1024).toFixed(1)} KB`;
    return `${(bytes/1048576).toFixed(1)} MB`;
  };

  list.innerHTML = issue.attachments.map(a => `
    <div class="attachment-item">
      <div class="attachment-icon"><i class="fa-solid ${extIcon(a.fileName)}"></i></div>
      <div style="flex:1;overflow:hidden;">
        <a href="${a.filePath}" target="_blank" class="attachment-name" style="color:var(--primary);text-decoration:none;">${escHtml(a.fileName)}</a>
        <div class="attachment-size">${fmtSize(a.fileSize)} · ${timeAgo(a.uploadedAt)}</div>
      </div>
      ${Auth.isAdmin()
        ? `<button class="btn btn-secondary btn-sm btn-icon" onclick="deleteAttachment(${a.id})" title="Delete">
             <i class="fa-solid fa-trash" style="color:var(--danger);"></i>
           </button>` : ''}
    </div>`).join('');
}

document.getElementById('attachment-input').addEventListener('change', async (e) => {
  const file = e.target.files[0];
  if (!file) return;
  const formData = new FormData();
  formData.append('file', file);
  try {
    await api.upload(`/attachments/${issueId}`, formData);
    await refreshIssue();
    showAlert('alert-container', 'Attachment uploaded.', 'success');
  } catch (e) {
    showAlert('alert-container', e.message);
  }
  e.target.value = '';
});

async function deleteAttachment(attachmentId) {
  if (!confirmDelete('Delete this attachment?')) return;
  try {
    await api.delete(`/attachments/${attachmentId}`);
    await refreshIssue();
    showAlert('alert-container', 'Attachment deleted.', 'success');
  } catch (e) {
    showAlert('alert-container', e.message);
  }
}

// ── Activity Log ─────────────────────────────────────────────
function renderActivity(logs) {
  const list = document.getElementById('activity-list');
  if (!logs.length) {
    list.innerHTML = `<li style="color:var(--gray-400);font-size:13px;">No activity yet.</li>`;
    return;
  }

  const actionIcon = (action) => ({
    'Created':  'fa-plus',
    'Updated':  'fa-pen',
    'Commented':'fa-comment',
    'Deleted':  'fa-trash'
  }[action] || 'fa-circle-dot');

  list.innerHTML = logs.map(l => `
    <li class="timeline-item">
      <div class="timeline-dot"><i class="fa-solid ${actionIcon(l.action)}"></i></div>
      <div class="timeline-content">
        <div class="timeline-action">${escHtml(l.action)}</div>
        ${l.newValue ? `<div class="timeline-detail">${escHtml(l.newValue)}</div>` : ''}
        <div class="timeline-meta">${escHtml(l.username)} · ${timeAgo(l.createdAt)}</div>
      </div>
    </li>`).join('');
}

// ── Edit Modal ────────────────────────────────────────────────
function openEditModal() {
  const i = currentIssue;
  document.getElementById('edit-title').value       = i.title;
  document.getElementById('edit-description').value = i.description;
  document.getElementById('edit-status').value      = ['Open','InProgress','Resolved'].indexOf(i.status);
  document.getElementById('edit-priority').value    = ['Low','Medium','High'].indexOf(i.priority);
  document.getElementById('edit-severity').value    = ['Minor','Major','Critical'].indexOf(i.severity);

  const sel = document.getElementById('edit-assigned');
  sel.innerHTML = '<option value="">Unassigned</option>' +
    allUsers.map(u => `<option value="${u.id}" ${u.id === i.assignedToUserId ? 'selected' : ''}>${escHtml(u.username)}</option>`).join('');

  document.getElementById('edit-modal').classList.add('open');
}

document.getElementById('edit-modal-save').addEventListener('click', async () => {
  const title = document.getElementById('edit-title').value.trim();
  if (!title) { showAlert('edit-modal-alert', 'Title is required.'); return; }

  const btn = document.getElementById('edit-modal-save');
  btn.disabled = true; btn.textContent = 'Saving…';

  try {
    await api.put(`/issues/${issueId}`, {
      title,
      description:      document.getElementById('edit-description').value,
      status:           parseInt(document.getElementById('edit-status').value),
      priority:         parseInt(document.getElementById('edit-priority').value),
      severity:         parseInt(document.getElementById('edit-severity').value),
      assignedToUserId: document.getElementById('edit-assigned').value || null
    });
    document.getElementById('edit-modal').classList.remove('open');
    await refreshIssue();
    showAlert('alert-container', 'Issue updated.', 'success');
  } catch (e) {
    showAlert('edit-modal-alert', e.message);
  } finally {
    btn.disabled = false; btn.textContent = 'Save Changes';
  }
});

document.getElementById('edit-modal-close').addEventListener('click',  () => document.getElementById('edit-modal').classList.remove('open'));
document.getElementById('edit-modal-cancel').addEventListener('click', () => document.getElementById('edit-modal').classList.remove('open'));
document.getElementById('edit-modal').addEventListener('click', e => { if (e.target.id === 'edit-modal') document.getElementById('edit-modal').classList.remove('open'); });

// ── Delete Issue ─────────────────────────────────────────────
async function deleteIssue() {
  if (!confirmDelete(`Delete issue #${issueId}? This cannot be undone.`)) return;
  try {
    await api.delete(`/issues/${issueId}`);
    location.href = '/issues.html';
  } catch (e) {
    showAlert('alert-container', e.message);
  }
}

// ── Refresh helper ────────────────────────────────────────────
async function refreshIssue() {
  const [issueRes, logsRes] = await Promise.all([
    api.get(`/issues/${issueId}`),
    api.get(`/activity-logs/${issueId}`)
  ]);
  currentIssue = issueRes.data;
  renderIssue(currentIssue);
  renderActivity(logsRes.data);
}

// ── Boot ──────────────────────────────────────────────────────
loadPage();
