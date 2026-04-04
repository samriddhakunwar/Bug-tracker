/* users.js */
(async () => {
  if (!Auth.requireAuth()) return;
  initSidebar();

  if (!Auth.isAdmin()) {
    document.getElementById('users-card').style.display = 'none';
    document.getElementById('access-denied').style.display = 'block';
    return;
  }

  await loadUsers();
})();

async function loadUsers() {
  const tbody = document.getElementById('users-body');
  try {
    const res = await api.get('/users');
    const users = res.data;
    document.getElementById('user-count').textContent = `${users.length} user${users.length !== 1 ? 's' : ''}`;

    if (!users.length) {
      tbody.innerHTML = `<tr><td colspan="7"><div class="empty-state" style="padding:30px;">No users found.</div></td></tr>`;
      return;
    }

    const me = Auth.getUser();
    tbody.innerHTML = users.map(u => `
      <tr>
        <td class="text-muted text-sm">${u.id}</td>
        <td>
          <div class="d-flex align-center gap-8">
            <div class="user-avatar" style="width:30px;height:30px;font-size:12px;">${u.username.charAt(0).toUpperCase()}</div>
            <span class="fw-600">${escHtml(u.username)}</span>
            ${u.id === me.id ? '<span class="badge" style="background:var(--primary-light);color:var(--primary);font-size:10px;">You</span>' : ''}
          </div>
        </td>
        <td class="text-muted">${escHtml(u.email)}</td>
        <td>${roleBadge(u.role)}</td>
        <td>
          ${u.isActive
            ? '<span class="badge" style="background:#dcfce7;color:#166534;">Active</span>'
            : '<span class="badge" style="background:#fee2e2;color:#991b1b;">Inactive</span>'}
        </td>
        <td class="text-muted text-sm">${formatDate(u.createdAt)}</td>
        <td>
          ${u.id !== me.id && u.isActive
            ? `<button class="btn btn-danger btn-sm" onclick="deactivateUser(${u.id}, '${escHtml(u.username)}')">
                <i class="fa-solid fa-user-slash"></i> Deactivate
               </button>`
            : '<span class="text-muted text-sm">—</span>'}
        </td>
      </tr>`).join('');
  } catch (e) {
    tbody.innerHTML = `<tr class="loading-row"><td colspan="7" style="color:var(--danger);">${escHtml(e.message)}</td></tr>`;
  }
}

async function deactivateUser(id, username) {
  if (!confirmDelete(`Deactivate user "${username}"? They will no longer be able to log in.`)) return;
  try {
    await api.delete(`/users/${id}`);
    showAlert('alert-container', `User "${username}" deactivated.`, 'success');
    await loadUsers();
  } catch (e) {
    showAlert('alert-container', e.message);
  }
}
