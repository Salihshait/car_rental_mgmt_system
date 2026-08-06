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

// Fleet: drivers
export const listDrivers = () => request('/drivers');
export const getDriver = (id) => request(`/drivers/${id}`);
export const createDriver = (payload) => request('/drivers', { method: 'POST', body: payload });
export const updateDriver = (id, payload) => request(`/drivers/${id}`, { method: 'PUT', body: payload });
export const getDriverDashboard = () => request('/drivers/dashboard');
export const getDriverPerformance = (id) => request(`/drivers/${id}/performance`);

// Driver: self-service ("Driver App Dashboard")
export const getMyDriverProfile = () => request('/drivers/me');
export const updateMyDriverProfile = (payload) => request('/drivers/me', { method: 'PUT', body: payload });
export const getMyDriverAssignment = () => request('/drivers/me/assignment');
export const getMyDriverTrips = () => request('/drivers/me/trips');
export const getMyDriverAttendance = (filters = {}) => request(`/drivers/me/attendance${buildQuery(filters)}`);
export const checkInDriver = () => request('/drivers/me/attendance/check-in', { method: 'POST' });
export const checkOutDriver = () => request('/drivers/me/attendance/check-out', { method: 'POST' });
export const getMyDriverSalary = () => request('/drivers/me/salary');
export const getMyDriverRatings = () => request('/drivers/me/ratings');
export const getMyDriverPerformance = () => request('/drivers/me/performance');

// Driver documents (works for both admin and self, pass the relevant driverId)
export const listDriverDocuments = (driverId) => request(`/drivers/${driverId}/documents`);
export const uploadDriverDocument = (driverId, { documentType, documentNumber, expiryDate, file }) => {
  const form = new FormData();
  form.append('documentType', documentType);
  if (documentNumber) form.append('documentNumber', documentNumber);
  if (expiryDate) form.append('expiryDate', expiryDate);
  if (file) form.append('file', file);
  return request(`/drivers/${driverId}/documents`, { method: 'POST', body: form, isForm: true });
};
export const verifyDriverDocument = (driverId, documentId, verificationStatus) =>
  request(`/drivers/${driverId}/documents/${documentId}/verify`, { method: 'PATCH', body: { verificationStatus } });
export const deleteDriverDocument = (driverId, documentId) =>
  request(`/drivers/${driverId}/documents/${documentId}`, { method: 'DELETE' });

// Driver attendance (admin)
export const getDriverAttendance = (driverId, filters = {}) => request(`/driver-attendance/${driverId}${buildQuery(filters)}`);
export const markDriverAttendance = (driverId, payload) => request(`/driver-attendance/${driverId}/mark`, { method: 'POST', body: payload });

// Driver salary
export const listDriverSalary = (driverId) => request(`/driver-salary${buildQuery({ driverId })}`);
export const generateDriverSalary = (payload) => request('/driver-salary', { method: 'POST', body: payload });
export const markDriverSalaryPaid = (id) => request(`/driver-salary/${id}/mark-paid`, { method: 'POST' });

// Driver ratings
export const listDriverRatings = (driverId) => request(`/driver-ratings${buildQuery({ driverId })}`);
export const addDriverRating = (payload) => request('/driver-ratings', { method: 'POST', body: payload });

// Fleet: dashboard / availability
export const getFleetDashboardSummary = () => request('/fleet/dashboard');
export const getFleetAvailability = () => request('/fleet/availability');

// Fleet: GPS tracking / live map / trips
export const recordVehicleLocation = (payload) => request('/fleet/tracking/locations', { method: 'POST', body: payload });
export const getLatestVehicleLocations = () => request('/fleet/tracking/locations/latest');
export const startTrip = (payload) => request('/fleet/tracking/trips/start', { method: 'POST', body: payload });
export const endTrip = (tripId, payload = {}) => request(`/fleet/tracking/trips/${tripId}/end`, { method: 'POST', body: payload });
export const listTrips = (filters = {}) => request(`/fleet/tracking/trips${buildQuery(filters)}`);
export const getTripLocations = (tripId) => request(`/fleet/tracking/trips/${tripId}/locations`);
export const simulateTrip = (vehicleId) => request(`/fleet/tracking/vehicles/${vehicleId}/simulate`, { method: 'POST' });

// Fleet: fuel monitoring
export const listFuelLogs = (vehicleId) => request(`/fuel-logs${buildQuery({ vehicleId })}`);
export const createFuelLog = (payload) => request('/fuel-logs', { method: 'POST', body: payload });
export const getFuelConsumptionSummary = (vehicleId) => request(`/fuel-logs/vehicles/${vehicleId}/summary`);

// Fleet: maintenance schedule
export const listMaintenance = (filters = {}) => request(`/vehicle-maintenance${buildQuery(filters)}`);
export const scheduleMaintenance = (payload) => request('/vehicle-maintenance', { method: 'POST', body: payload });
export const startMaintenance = (id) => request(`/vehicle-maintenance/${id}/start`, { method: 'POST' });
export const completeMaintenance = (id, payload = {}) => request(`/vehicle-maintenance/${id}/complete`, { method: 'POST', body: payload });
export const cancelMaintenance = (id) => request(`/vehicle-maintenance/${id}/cancel`, { method: 'POST' });

// Fleet: driver assignments
export const listDriverAssignments = (filters = {}) => request(`/driver-assignments${buildQuery(filters)}`);
export const assignDriver = (payload) => request('/driver-assignments', { method: 'POST', body: payload });
export const unassignDriver = (id) => request(`/driver-assignments/${id}/unassign`, { method: 'POST' });

// Fleet: vehicle transfers
export const listVehicleTransfers = (filters = {}) => request(`/vehicle-transfers${buildQuery(filters)}`);
export const createVehicleTransfer = (payload) => request('/vehicle-transfers', { method: 'POST', body: payload });
export const completeVehicleTransfer = (id) => request(`/vehicle-transfers/${id}/complete`, { method: 'POST' });
export const cancelVehicleTransfer = (id) => request(`/vehicle-transfers/${id}/cancel`, { method: 'POST' });
