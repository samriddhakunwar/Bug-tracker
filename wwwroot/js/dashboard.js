/* dashboard.js */
(async () => {
  if (!Auth.requireAuth()) return;
  initSidebar();

  // Load stats
  try {
    const res = await api.get('/issues/stats');
    const s = res.data;
    document.getElementById('stat-open').textContent       = s.openCount;
    document.getElementById('stat-inprogress').textContent = s.inProgressCount;
    document.getElementById('stat-resolved').textContent   = s.resolvedCount;
    document.getElementById('stat-total').textContent      = s.totalCount;
  } catch (e) {
    console.error('Stats error', e);
  }

  // Load recent issues (first 8)
  try {
    const res = await api.get('/issues?page=1&pageSize=8');
    const issues = res.data.issues;
    const tbody = document.getElementById('recent-issues-body');

    if (!issues.length) {
      tbody.innerHTML = `<tr><td colspan="6" class="empty-state" style="padding:30px;">
        <i class="fa-solid fa-inbox" style="font-size:28px;display:block;margin-bottom:8px;"></i>
        No issues yet. <a href="/issues.html">Create the first one!</a></td></tr>`;
      return;
    }

    tbody.innerHTML = issues.map(i => `
      <tr onclick="location.href='/issue-detail.html?id=${i.id}'">
        <td class="text-muted text-sm">#${i.id}</td>
        <td>
          <div class="fw-600" style="max-width:280px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;">${escHtml(i.title)}</div>
          <div class="text-muted text-sm">${escHtml(i.createdByUsername)}</div>
        </td>
        <td>${statusBadge(i.status)}</td>
        <td>${priorityBadge(i.priority)}</td>
        <td>${i.assignedToUsername ? escHtml(i.assignedToUsername) : '<span class="text-muted">—</span>'}</td>
        <td class="text-muted text-sm">${formatDate(i.createdAt)}</td>
      </tr>`).join('');
  } catch (e) {
    document.getElementById('recent-issues-body').innerHTML =
      `<tr class="loading-row"><td colspan="6" style="color:var(--danger);">Failed to load issues.</td></tr>`;
  }
})();
