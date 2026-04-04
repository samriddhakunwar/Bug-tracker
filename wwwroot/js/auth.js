/* ============================================================
   auth.js – JWT token helpers shared across all pages
   ============================================================ */

const API_BASE = '/api';

const Auth = {
    getToken: () => localStorage.getItem('bt_token'),
    getUser:  () => JSON.parse(localStorage.getItem('bt_user') || 'null'),
    isLoggedIn: () => !!localStorage.getItem('bt_token'),
    isAdmin: () => {
        const u = Auth.getUser();
        return u && u.role === 'Admin';
    },
    save: (data) => {
        localStorage.setItem('bt_token', data.token);
        localStorage.setItem('bt_user', JSON.stringify({
            id: data.userId,
            username: data.username,
            email: data.email,
            role: data.role
        }));
    },
    logout: () => {
        localStorage.removeItem('bt_token');
        localStorage.removeItem('bt_user');
        window.location.href = '/login.html';
    },
    requireAuth: () => {
        if (!Auth.isLoggedIn()) {
            window.location.href = '/login.html';
            return false;
        }
        return true;
    },
    requireGuest: () => {
        if (Auth.isLoggedIn()) {
            window.location.href = '/dashboard.html';
        }
    }
};

/* ── API helpers ─────────────────────────────────────────── */
async function apiRequest(method, path, body = null, isForm = false) {
    const headers = {};
    if (Auth.getToken()) headers['Authorization'] = `Bearer ${Auth.getToken()}`;
    if (body && !isForm) headers['Content-Type'] = 'application/json';

    const opts = { method, headers };
    if (body) opts.body = isForm ? body : JSON.stringify(body);

    const res = await fetch(`${API_BASE}${path}`, opts);
    const json = await res.json().catch(() => ({}));

    if (!res.ok) {
        throw new Error(json.message || `HTTP ${res.status}`);
    }
    return json;
}

const api = {
    get:    (path)         => apiRequest('GET',    path),
    post:   (path, body)   => apiRequest('POST',   path, body),
    put:    (path, body)   => apiRequest('PUT',    path, body),
    delete: (path)         => apiRequest('DELETE', path),
    upload: (path, formData) => apiRequest('POST', path, formData, true)
};

/* ── UI helpers ──────────────────────────────────────────── */
function showAlert(containerId, message, type = 'error') {
    const icons = { error: '⚠', success: '✓', info: 'ℹ' };
    const el = document.getElementById(containerId);
    if (!el) return;
    el.innerHTML = `<div class="alert alert-${type}"><span>${icons[type]}</span>${escHtml(message)}</div>`;
    el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    if (type === 'success') setTimeout(() => { el.innerHTML = ''; }, 3000);
}

function clearAlert(containerId) {
    const el = document.getElementById(containerId);
    if (el) el.innerHTML = '';
}

function escHtml(str) {
    return String(str)
        .replace(/&/g,'&amp;').replace(/</g,'&lt;')
        .replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function statusBadge(status) {
    const map = {
        'Open':       'badge-open',
        'InProgress': 'badge-inprogress',
        'Resolved':   'badge-resolved'
    };
    const label = status === 'InProgress' ? 'In Progress' : status;
    return `<span class="badge ${map[status] || ''}">${label}</span>`;
}

function priorityBadge(p) {
    return `<span class="badge badge-${p.toLowerCase()}">${p}</span>`;
}

function severityBadge(s) {
    return `<span class="badge badge-${s.toLowerCase()}">${s}</span>`;
}

function roleBadge(r) {
    return `<span class="badge badge-${r.toLowerCase()}">${r}</span>`;
}

function formatDate(d) {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-US', {
        year: 'numeric', month: 'short', day: 'numeric'
    });
}

function formatDateTime(d) {
    if (!d) return '—';
    return new Date(d).toLocaleString('en-US', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });
}

function timeAgo(d) {
    const diff = Math.floor((Date.now() - new Date(d)) / 1000);
    if (diff < 60)   return 'just now';
    if (diff < 3600) return `${Math.floor(diff/60)}m ago`;
    if (diff < 86400)return `${Math.floor(diff/3600)}h ago`;
    return `${Math.floor(diff/86400)}d ago`;
}

function initSidebar() {
    const user = Auth.getUser();
    if (!user) return;

    const nameEl = document.getElementById('sb-username');
    const roleEl = document.getElementById('sb-role');
    const avatarEl = document.getElementById('sb-avatar');
    if (nameEl) nameEl.textContent = user.username;
    if (roleEl) roleEl.textContent = user.role;
    if (avatarEl) avatarEl.textContent = user.username.charAt(0).toUpperCase();

    // Hide admin links for non-admins
    if (!Auth.isAdmin()) {
        document.querySelectorAll('.admin-only').forEach(el => el.style.display = 'none');
    }

    // Active link highlight
    const current = window.location.pathname.split('/').pop() || 'index.html';
    document.querySelectorAll('.sidebar-nav a').forEach(a => {
        const href = a.getAttribute('href');
        if (href === current || (current === '' && href === 'dashboard.html')) {
            a.classList.add('active');
        }
    });
}

function confirmDelete(message) {
    return window.confirm(message || 'Are you sure you want to delete this?');
}
