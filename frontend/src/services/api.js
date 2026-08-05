import { supabase } from './supabaseClient';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api';

async function authHeader() {
  const { data } = await supabase.auth.getSession();
  const token = data?.session?.access_token;
  return token ? { Authorization: `Bearer ${token}` } : {};
}

async function request(path, { method = 'GET', body, isForm = false } = {}) {
  const headers = { ...(await authHeader()) };
  if (!isForm) {
    headers['Content-Type'] = 'application/json';
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: body ? (isForm ? body : JSON.stringify(body)) : undefined,
  });

  if (response.status === 204) {
    return null;
  }

  const result = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(result?.message ?? 'Request failed.');
  }

  return result;
}

// Branches
export const listBranches = () => request('/branches');

// Bookings / Invoices
export const listBookings = () => request('/bookings');
export const listInvoices = () => request('/invoices');
export const generateInvoice = (bookingId) => request('/invoices/generate', { method: 'POST', body: { bookingId } });

// Profile / self-service
export const completeProfile = (payload) => request('/users/complete-profile', { method: 'POST', body: payload });
export const getMyProfile = () => request('/users/me');
export const updateMyProfile = (payload) => request('/users/me', { method: 'PUT', body: payload });
export const logoutAllMyDevices = () => request('/users/me/logout-all', { method: 'POST' });
export const uploadMyAvatar = (file) => {
  const form = new FormData();
  form.append('file', file);
  return request('/users/me/avatar', { method: 'POST', body: form, isForm: true });
};

// User management (admin)
export const listUsers = () => request('/users');
export const getUser = (id) => request(`/users/${id}`);
export const createUser = (payload) => request('/users', { method: 'POST', body: payload });
export const updateUser = (id, payload) => request(`/users/${id}`, { method: 'PUT', body: payload });
export const updateUserStatus = (id, isActive) => request(`/users/${id}/status`, { method: 'PATCH', body: { isActive } });
export const logoutAllForUser = (id) => request(`/users/${id}/logout-all`, { method: 'POST' });
export const uploadUserAvatar = (id, file) => {
  const form = new FormData();
  form.append('file', file);
  return request(`/users/${id}/avatar`, { method: 'POST', body: form, isForm: true });
};

// Roles
export const listRoles = () => request('/roles');
export const createRole = (payload) => request('/roles', { method: 'POST', body: payload });
export const updateRole = (id, payload) => request(`/roles/${id}`, { method: 'PUT', body: payload });
export const deleteRole = (id) => request(`/roles/${id}`, { method: 'DELETE' });

// Departments
export const listDepartments = () => request('/departments');
export const createDepartment = (payload) => request('/departments', { method: 'POST', body: payload });
export const updateDepartment = (id, payload) => request(`/departments/${id}`, { method: 'PUT', body: payload });
export const deleteDepartment = (id) => request(`/departments/${id}`, { method: 'DELETE' });

// Permissions
export const listPermissions = () => request('/permissions');
export const getPermissionMatrix = () => request('/permissions/matrix');
export const updateRolePermissions = (roleId, permissionIds) =>
  request(`/permissions/roles/${roleId}`, { method: 'PUT', body: { permissionIds } });

// Vehicle catalog
export const listVehicleCategories = () => request('/vehicle-categories');
export const createVehicleCategory = (payload) => request('/vehicle-categories', { method: 'POST', body: payload });
export const updateVehicleCategory = (id, payload) => request(`/vehicle-categories/${id}`, { method: 'PUT', body: payload });
export const deleteVehicleCategory = (id) => request(`/vehicle-categories/${id}`, { method: 'DELETE' });

export const listVehicleBrands = () => request('/brands');
export const createVehicleBrand = (payload) => request('/brands', { method: 'POST', body: payload });
export const updateVehicleBrand = (id, payload) => request(`/brands/${id}`, { method: 'PUT', body: payload });
export const deleteVehicleBrand = (id) => request(`/brands/${id}`, { method: 'DELETE' });

export const listVehicleModels = () => request('/models');
export const createVehicleModel = (payload) => request('/models', { method: 'POST', body: payload });
export const updateVehicleModel = (id, payload) => request(`/models/${id}`, { method: 'PUT', body: payload });
export const deleteVehicleModel = (id) => request(`/models/${id}`, { method: 'DELETE' });

// Vehicles
function buildQuery(params) {
  const query = Object.entries(params)
    .filter(([, value]) => value !== undefined && value !== null && value !== '')
    .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
    .join('&');
  return query ? `?${query}` : '';
}

export const listVehicles = (filters = {}) => request(`/vehicles${buildQuery(filters)}`);
export const getVehicle = (id) => request(`/vehicles/${id}`);
export const createVehicle = (payload) => request('/vehicles', { method: 'POST', body: payload });
export const updateVehicle = (id, payload) => request(`/vehicles/${id}`, { method: 'PUT', body: payload });
export const deleteVehicle = (id) => request(`/vehicles/${id}`, { method: 'DELETE' });
export const updateVehicleStatus = (id, status) => request(`/vehicles/${id}/status`, { method: 'PATCH', body: { status } });
export const getVehicleTimeline = (id) => request(`/vehicles/${id}/timeline`);
export const getFleetDashboard = () => request('/vehicles/dashboard');

export const importVehicles = (file) => {
  const form = new FormData();
  form.append('file', file);
  return request('/vehicles/import', { method: 'POST', body: form, isForm: true });
};

export async function exportVehicles(filters = {}) {
  const headers = await authHeader();
  const response = await fetch(`${API_BASE_URL}/vehicles/export${buildQuery(filters)}`, { headers });
  if (!response.ok) {
    throw new Error('Unable to export vehicles.');
  }
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = 'vehicles.csv';
  link.click();
  window.URL.revokeObjectURL(url);
}

// Vehicle documents
export const listVehicleDocuments = (vehicleId) => request(`/vehicles/${vehicleId}/documents`);
export const uploadVehicleDocument = (vehicleId, { documentType, documentNumber, issuedBy, expiryDate, file }) => {
  const form = new FormData();
  form.append('documentType', documentType);
  if (documentNumber) form.append('documentNumber', documentNumber);
  if (issuedBy) form.append('issuedBy', issuedBy);
  if (expiryDate) form.append('expiryDate', expiryDate);
  if (file) form.append('file', file);
  return request(`/vehicles/${vehicleId}/documents`, { method: 'POST', body: form, isForm: true });
};
export const deleteVehicleDocument = (vehicleId, documentId) =>
  request(`/vehicles/${vehicleId}/documents/${documentId}`, { method: 'DELETE' });

// Customers (admin)
export const listCustomers = (filters = {}) => request(`/customers${buildQuery(filters)}`);
export const getCustomer = (id) => request(`/customers/${id}`);
export const createCustomer = (payload) => request('/customers', { method: 'POST', body: payload });
export const updateCustomer = (id, payload) => request(`/customers/${id}`, { method: 'PUT', body: payload });
export const adjustCustomerWallet = (id, payload) => request(`/customers/${id}/wallet/adjust`, { method: 'POST', body: payload });
export const adjustCustomerLoyalty = (id, payload) => request(`/customers/${id}/loyalty/adjust`, { method: 'POST', body: payload });
export const getCustomerBookings = (id) => request(`/customers/${id}/bookings`);
export const getCustomerTimeline = (id) => request(`/customers/${id}/timeline`);
export const getCustomerDashboard = () => request('/customers/dashboard');

// Customers (self-service)
export const getMyCustomerProfile = () => request('/customers/me');
export const updateMyEmergencyContact = (payload) => request('/customers/me', { method: 'PUT', body: payload });
export const getMyRentalHistory = () => request('/customers/me/bookings');
export const getMyFavorites = () => request('/customers/me/favorites');
export const addMyFavorite = (vehicleId) => request('/customers/me/favorites', { method: 'POST', body: { vehicleId } });
export const removeMyFavorite = (vehicleId) => request(`/customers/me/favorites/${vehicleId}`, { method: 'DELETE' });

// Customer documents (works for both admin and self, pass the relevant customerId)
export const listCustomerDocuments = (customerId) => request(`/customers/${customerId}/documents`);
export const uploadCustomerDocument = (customerId, { documentType, documentNumber, expiryDate, file }) => {
  const form = new FormData();
  form.append('documentType', documentType);
  if (documentNumber) form.append('documentNumber', documentNumber);
  if (expiryDate) form.append('expiryDate', expiryDate);
  if (file) form.append('file', file);
  return request(`/customers/${customerId}/documents`, { method: 'POST', body: form, isForm: true });
};
export const verifyCustomerDocument = (customerId, documentId, verificationStatus) =>
  request(`/customers/${customerId}/documents/${documentId}/verify`, { method: 'PATCH', body: { verificationStatus } });
export const deleteCustomerDocument = (customerId, documentId) =>
  request(`/customers/${customerId}/documents/${documentId}`, { method: 'DELETE' });

// Notifications
export const listMyNotifications = () => request('/notifications/me');
export const markNotificationRead = (id) => request(`/notifications/${id}/read`, { method: 'PATCH' });
