const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api';

function authHeader() {
  const token = localStorage.getItem('accessToken');
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export async function login(payload) {
  const response = await fetch(`${API_BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result?.message ?? 'Login failed.');
  }

  return result;
}

export async function listBookings() {
  const response = await fetch(`${API_BASE_URL}/bookings`, {
    headers: { ...authHeader() },
  });

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result?.message ?? 'Unable to load bookings.');
  }

  return result;
}

export async function listInvoices() {
  const response = await fetch(`${API_BASE_URL}/invoices`, {
    headers: { ...authHeader() },
  });

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result?.message ?? 'Unable to load invoices.');
  }

  return result;
}

export async function generateInvoice(bookingId) {
  const response = await fetch(`${API_BASE_URL}/invoices/generate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeader() },
    body: JSON.stringify({ bookingId }),
  });

  const result = await response.json();

  if (!response.ok) {
    throw new Error(result?.message ?? 'Unable to generate invoice.');
  }

  return result;
}
