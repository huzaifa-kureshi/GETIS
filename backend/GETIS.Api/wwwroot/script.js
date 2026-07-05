/* =============================================================================
   script.js - Booking + Admin logic
   Talks to the GETIS C# Web API (see /backend/GETIS.Api) instead of
   localStorage. All bookings, packages, and admin auth are handled server-side.
============================================================================= */

// If you ever host the frontend separately from the API (e.g. VS Code Live
// Server on a different port), change this to the full API URL, e.g.
// const API_BASE = 'http://localhost:5232/api';
const API_BASE = '/api';

let PACKAGES = [];

document.addEventListener('DOMContentLoaded', async () => {
  await loadPackagesIntoForm();

  const form = document.getElementById('bookingForm');
  if (form) form.addEventListener('submit', saveBooking);

  setupPackageCardClicks();

  // If we're on the admin page and already have a token, go straight to the dashboard
  if (document.getElementById('recordsBody')) {
    const token = sessionStorage.getItem('getis_admin_token');
    if (token) {
      showAdminDashboard();
      loadAdminTable();
    }
  }
});

/* ===========================
   PACKAGES
   - Loaded from the API and used to populate the booking form's <select>
=========================== */
async function loadPackagesIntoForm() {
  const select = document.getElementById('package');
  if (!select) return;

  try {
    const res = await fetch(`${API_BASE}/packages`);
    if (!res.ok) throw new Error('Failed to load packages');
    PACKAGES = await res.json();

    select.innerHTML = '<option value="" disabled selected>Select package</option>';
    PACKAGES.forEach(p => {
      const opt = document.createElement('option');
      opt.value = p.id;
      opt.textContent = `${p.title} — ${p.durationDays ?? ''} — $${p.price}`;
      select.appendChild(opt);
    });
  } catch (err) {
    console.error('Could not load packages from API:', err);
  }
}

/* ===========================
   SAVE BOOKING
   - Sends the booking to POST /api/bookings
=========================== */
async function saveBooking(e) {
  e.preventDefault();
  const name = document.getElementById('name')?.value?.trim() || '';
  const email = document.getElementById('email')?.value?.trim() || '';
  const date = document.getElementById('date')?.value || '';
  const destination = document.getElementById('destination')?.value || '';
  const packageId = document.getElementById('package')?.value || '';
  const msg = document.getElementById('message');

  if (!name || !email || !date || !destination || !packageId) {
    if (msg) {
      msg.innerText = 'Please complete all fields.';
      msg.style.color = 'red';
      setTimeout(() => { msg.innerText = ''; }, 3000);
    }
    return;
  }

  const payload = {
    fullName: name,
    email,
    travelDate: date,
    destination,
    packageId: Number(packageId)
  };

  try {
    const res = await fetch(`${API_BASE}/bookings`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const errBody = await res.json().catch(() => ({}));
      throw new Error(errBody.error || 'Booking failed. Please try again.');
    }

    if (msg) {
      msg.innerText = 'Booking submitted successfully!';
      msg.style.color = 'green';
    }
    document.getElementById('bookingForm').reset();
  } catch (err) {
    if (msg) {
      msg.innerText = err.message;
      msg.style.color = 'red';
    }
  } finally {
    setTimeout(() => { if (msg) msg.innerText = ''; }, 3000);
  }
}

/* ===========================
   ADMIN LOGIN
   - Calls POST /api/auth/login and stores the returned token in sessionStorage
   - The token is then sent as an "X-Admin-Token" header on every admin request
=========================== */
async function checkLogin() {
  const input = document.getElementById('adminPass');
  const userInput = document.getElementById('adminUser'); // optional field, defaults to "admin"
  const msg = document.getElementById('loginMsg');
  if (!input) return;

  const username = userInput ? userInput.value : 'admin';
  const password = input.value;

  try {
    const res = await fetch(`${API_BASE}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    if (!res.ok) {
      if (msg) {
        msg.innerText = 'Incorrect password. Please try again.';
        msg.style.color = 'red';
      }
      return;
    }

    const data = await res.json();
    sessionStorage.setItem('getis_admin_token', data.token);

    showAdminDashboard();
    if (msg) msg.innerText = '';
    await loadAdminTable();
  } catch (err) {
    if (msg) {
      msg.innerText = 'Unable to reach the server. Is the API running?';
      msg.style.color = 'red';
    }
  }
}

function showAdminDashboard() {
  const loginScreen = document.getElementById('loginScreen');
  if (loginScreen) loginScreen.style.display = 'none';

  const sidebar = document.querySelector('.sidebar');
  const main = document.querySelector('.main-content');
  if (sidebar) sidebar.classList.remove('hidden');
  if (main) main.classList.remove('hidden');
}

function logout() {
  sessionStorage.removeItem('getis_admin_token');
  location.reload();
}

function getAdminHeaders() {
  const token = sessionStorage.getItem('getis_admin_token');
  return { 'X-Admin-Token': token || '' };
}

function handleUnauthorized(res) {
  if (res.status === 401) {
    sessionStorage.removeItem('getis_admin_token');
    location.reload();
    return true;
  }
  return false;
}

/* ===========================
   LOAD ADMIN TABLE
   - Fetches bookings from GET /api/bookings (admin-protected)
=========================== */
async function loadAdminTable() {
  const tableBody = document.getElementById('recordsBody');
  if (!tableBody) return;

  try {
    const res = await fetch(`${API_BASE}/bookings`, { headers: getAdminHeaders() });
    if (handleUnauthorized(res)) return;

    const bookings = await res.json();
    tableBody.innerHTML = '';

    bookings.forEach((b, i) => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td class="px-4 py-3">${i + 1}</td>
        <td class="px-4 py-3">${escapeHtml(b.travelerName)}</td>
        <td class="px-4 py-3">${escapeHtml(b.travelerEmail)}</td>
        <td class="px-4 py-3">${escapeHtml((b.travelDate || '').substring(0, 10))}</td>
        <td class="px-4 py-3">${escapeHtml(b.destination)}</td>
        <td class="px-4 py-3">${escapeHtml(b.packageTitle)}</td>
        <td class="px-4 py-3"><button class="delete-btn" onclick="deleteBooking(${b.id})">Delete</button></td>
      `;
      tableBody.appendChild(tr);
    });
  } catch (err) {
    console.error('Could not load bookings from API:', err);
  }
}

/* ===========================
   DELETE BOOKING
   - Calls DELETE /api/bookings/{id}
=========================== */
async function deleteBooking(id) {
  try {
    const res = await fetch(`${API_BASE}/bookings/${id}`, {
      method: 'DELETE',
      headers: getAdminHeaders()
    });
    if (handleUnauthorized(res)) return;
    await loadAdminTable();
  } catch (err) {
    console.error('Could not delete booking:', err);
  }
}

/* ===========================
   DOWNLOAD CSV
   - Calls GET /api/bookings/export/csv (admin-protected)
=========================== */
async function downloadCSV() {
  try {
    const res = await fetch(`${API_BASE}/bookings/export/csv`, { headers: getAdminHeaders() });
    if (handleUnauthorized(res)) return;
    if (!res.ok) return alert('Unable to export CSV.');

    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'bookings.csv';
    a.click();
    URL.revokeObjectURL(url);
  } catch (err) {
    console.error(err);
  }
}

/* ===========================
   DOWNLOAD JSON
   - Reuses GET /api/bookings and downloads it as a JSON file
=========================== */
async function downloadJSON() {
  try {
    const res = await fetch(`${API_BASE}/bookings`, { headers: getAdminHeaders() });
    if (handleUnauthorized(res)) return;

    const bookings = await res.json();
    if (!bookings.length) return alert('No bookings available to download.');

    const blob = new Blob([JSON.stringify(bookings, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'bookings.json';
    a.click();
    URL.revokeObjectURL(url);
  } catch (err) {
    console.error(err);
  }
}

/* ===========================
   PACKAGE CARD AUTO-FILL
   - When a user clicks a package card, auto-select the matching package in the form
=========================== */
function setupPackageCardClicks() {
  const selectButtons = document.querySelectorAll('.select-pkg-btn');
  const pkgSelect = document.getElementById('package');

  if (!selectButtons.length || !pkgSelect) return;

  selectButtons.forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();

      const card = btn.closest('.pkg-card');
      const name = card.getAttribute('data-package-name');

      let found = false;
      for (const opt of pkgSelect.options) {
        if (opt.textContent.trim().startsWith(name)) {
          pkgSelect.value = opt.value;
          found = true;
          break;
        }
      }

      if (!found) {
        console.warn(`No matching package found in the API for "${name}". Has it loaded yet?`);
      }

      const form = document.getElementById('bookingForm');
      if (form) {
        form.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    });
  });
}

/* ===========================
   Helper functions
=========================== */
function escapeHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/"/g, '&quot;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
}

// ================ background slideshow ==========================================
const slides = document.querySelectorAll('.bg-slide');
let currentSlide = 0;
if (slides.length > 0) {
  slides[0].classList.add('active');
  setInterval(() => {
    slides[currentSlide].classList.remove('active');
    currentSlide = (currentSlide + 1) % slides.length;
    slides[currentSlide].classList.add('active');
  }, 5000);
}
// ====================================================================================
