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

// Reports
async function downloadReportFile(path, filters, filename) {
  const headers = await authHeader();
  const response = await fetch(`${API_BASE_URL}${path}${buildQuery(filters)}`, { headers });
  if (!response.ok) {
    throw new Error('Unable to export report.');
  }
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  link.click();
  window.URL.revokeObjectURL(url);
}

export const getRevenueDashboard = (filters = {}) => request(`/reports/revenue/dashboard${buildQuery(filters)}`);
export const exportRevenueReport = (format, filters = {}) =>
  downloadReportFile('/reports/revenue/export', { ...filters, format }, `revenue-report.${format}`);

export const getBookingsDashboard = (filters = {}) => request(`/reports/bookings/dashboard${buildQuery(filters)}`);
export const exportBookingsReport = (format, filters = {}) =>
  downloadReportFile('/reports/bookings/export', { ...filters, format }, `bookings-report.${format}`);

export const getFleetReportDashboard = (filters = {}) => request(`/reports/fleet/dashboard${buildQuery(filters)}`);
export const exportFleetReport = (format, filters = {}) =>
  downloadReportFile('/reports/fleet/export', { ...filters, format }, `fleet-report.${format}`);

export const getMaintenanceReportDashboard = (filters = {}) => request(`/reports/maintenance/dashboard${buildQuery(filters)}`);
export const exportMaintenanceReport = (format, filters = {}) =>
  downloadReportFile('/reports/maintenance/export', { ...filters, format }, `maintenance-report.${format}`);

export const getCustomerReportDashboard = (filters = {}) => request(`/reports/customers/dashboard${buildQuery(filters)}`);
export const exportCustomerReport = (format, filters = {}) =>
  downloadReportFile('/reports/customers/export', { ...filters, format }, `customer-report.${format}`);

export const getDriverReportDashboard = (filters = {}) => request(`/reports/drivers/dashboard${buildQuery(filters)}`);
export const exportDriverReport = (format, filters = {}) =>
  downloadReportFile('/reports/drivers/export', { ...filters, format }, `driver-report.${format}`);

export const getFinanceReportDashboard = (filters = {}) => request(`/reports/finance/dashboard${buildQuery(filters)}`);
export const exportFinanceReport = (format, filters = {}) =>
  downloadReportFile('/reports/finance/export', { ...filters, format }, `finance-report.${format}`);

// CRM: Support tickets
export const createTicket = (payload) => request('/support-tickets', { method: 'POST', body: payload });
export const listMyTickets = () => request('/support-tickets/me');
export const getMyTicketDetail = (id) => request(`/support-tickets/me/${id}`);
export const replyToTicketAsCustomer = (id, payload) => request(`/support-tickets/me/${id}/messages`, { method: 'POST', body: payload });
export const listTickets = (filters = {}) => request(`/support-tickets${buildQuery(filters)}`);
export const getTicketDetail = (id) => request(`/support-tickets/${id}`);
export const replyToTicketAsStaff = (id, payload) => request(`/support-tickets/${id}/messages`, { method: 'POST', body: payload });
export const assignTicket = (id, assignedToUserId) => request(`/support-tickets/${id}/assign`, { method: 'PATCH', body: { assignedToUserId } });
export const updateTicketStatus = (id, status) => request(`/support-tickets/${id}/status`, { method: 'PATCH', body: { status } });

// CRM: Complaints
export const createComplaint = (payload) => request('/complaints', { method: 'POST', body: payload });
export const listMyComplaints = () => request('/complaints/me');
export const listComplaints = (filters = {}) => request(`/complaints${buildQuery(filters)}`);
export const getComplaint = (id) => request(`/complaints/${id}`);
export const resolveComplaint = (id, payload) => request(`/complaints/${id}/status`, { method: 'PATCH', body: payload });

// CRM: Feedback
export const createFeedback = (payload) => request('/feedback', { method: 'POST', body: payload });
export const listMyFeedback = () => request('/feedback/me');
export const listFeedback = (filters = {}) => request(`/feedback${buildQuery(filters)}`);
export const setFeedbackPublished = (id, isPublished) => request(`/feedback/${id}/publish`, { method: 'PATCH', body: { isPublished } });

// CRM: Message templates
export const listTemplates = (filters = {}) => request(`/crm/templates${buildQuery(filters)}`);
export const getTemplate = (id) => request(`/crm/templates/${id}`);
export const createTemplate = (payload) => request('/crm/templates', { method: 'POST', body: payload });
export const updateTemplate = (id, payload) => request(`/crm/templates/${id}`, { method: 'PUT', body: payload });
export const deleteTemplate = (id) => request(`/crm/templates/${id}`, { method: 'DELETE' });
export const previewTemplate = (id, sampleValues) => request(`/crm/templates/${id}/preview`, { method: 'POST', body: { sampleValues } });

// CRM: Campaigns
export const listCampaigns = (filters = {}) => request(`/crm/campaigns${buildQuery(filters)}`);
export const getCampaign = (id) => request(`/crm/campaigns/${id}`);
export const createCampaign = (payload) => request('/crm/campaigns', { method: 'POST', body: payload });
export const scheduleCampaign = (id, scheduledAt) => request(`/crm/campaigns/${id}/schedule`, { method: 'POST', body: { scheduledAt } });
export const sendCampaignNow = (id) => request(`/crm/campaigns/${id}/send-now`, { method: 'POST' });
export const cancelCampaign = (id) => request(`/crm/campaigns/${id}/cancel`, { method: 'POST' });
export const deleteCampaign = (id) => request(`/crm/campaigns/${id}`, { method: 'DELETE' });
export const getCampaignLogs = (id) => request(`/crm/campaigns/${id}/logs`);

// CRM: Message logs (Email / SMS / WhatsApp / Push activity)
export const listMessageLogs = (filters = {}) => request(`/crm/message-logs${buildQuery(filters)}`);
export const sendAdHocMessage = (payload) => request('/crm/message-logs/send', { method: 'POST', body: payload });

// Finance: Income / Expenses
export const getIncomeDashboard = (filters = {}) => request(`/finance/income/dashboard${buildQuery(filters)}`);
export const exportIncomeReport = (format, filters = {}) =>
  downloadReportFile('/finance/income/export', { ...filters, format }, `income-report.${format}`);
export const getExpensesDashboard = (filters = {}) => request(`/finance/expenses/dashboard${buildQuery(filters)}`);
export const exportExpensesReport = (format, filters = {}) =>
  downloadReportFile('/finance/expenses/export', { ...filters, format }, `expense-report.${format}`);

// Finance: Cashbook
export const getCashbook = (filters = {}) => request(`/finance/cashbook${buildQuery(filters)}`);
export const exportCashbook = (format, filters = {}) =>
  downloadReportFile('/finance/cashbook/export', { ...filters, format }, `cashbook.${format}`);

// Finance: Ledger
export const getLedger = (filters = {}) => request(`/finance/ledger${buildQuery(filters)}`);
export const exportLedger = (format, filters = {}) =>
  downloadReportFile('/finance/ledger/export', { ...filters, format }, `ledger.${format}`);

// Finance: Profit & Loss
export const getProfitLoss = (filters = {}) => request(`/finance/profit-loss${buildQuery(filters)}`);
export const exportProfitLoss = (format, filters = {}) =>
  downloadReportFile('/finance/profit-loss/export', { ...filters, format }, `profit-loss.${format}`);

// Finance: Balance Sheet
export const getBalanceSheet = (filters = {}) => request(`/finance/balance-sheet${buildQuery(filters)}`);
export const exportBalanceSheet = (format, filters = {}) =>
  downloadReportFile('/finance/balance-sheet/export', { ...filters, format }, `balance-sheet.${format}`);

// Finance: GST Reports
export const getGstReport = (filters = {}) => request(`/finance/gst${buildQuery(filters)}`);
export const exportGstReport = (format, filters = {}) =>
  downloadReportFile('/finance/gst/export', { ...filters, format }, `gst-report.${format}`);

// Finance: Bank accounts
export const listBankAccounts = () => request('/finance/bank-accounts');
export const getBankAccount = (id) => request(`/finance/bank-accounts/${id}`);
export const createBankAccount = (payload) => request('/finance/bank-accounts', { method: 'POST', body: payload });
export const updateBankAccount = (id, payload) => request(`/finance/bank-accounts/${id}`, { method: 'PUT', body: payload });
export const listBankAccountTransactions = (id) => request(`/finance/bank-accounts/${id}/transactions`);
export const addBankAccountTransaction = (id, payload) => request(`/finance/bank-accounts/${id}/transactions`, { method: 'POST', body: payload });

// Finance: Journal
export const listJournalEntries = (filters = {}) => request(`/finance/journal${buildQuery(filters)}`);
export const createJournalEntry = (payload) => request('/finance/journal', { method: 'POST', body: payload });
export const updateJournalEntry = (id, payload) => request(`/finance/journal/${id}`, { method: 'PUT', body: payload });
export const deleteJournalEntry = (id) => request(`/finance/journal/${id}`, { method: 'DELETE' });

// SaaS: Tenants (Company Registration)
export const registerTenant = (payload) => request('/saas/tenants/register', { method: 'POST', body: payload });
export const listTenants = () => request('/saas/tenants');
export const getTenant = (id) => request(`/saas/tenants/${id}`);
export const updateTenant = (id, payload) => request(`/saas/tenants/${id}`, { method: 'PUT', body: payload });
export const getTenantDatabaseInfo = (id) => request(`/saas/tenants/${id}/database-info`);

// SaaS: Subscription Plans
export const listSubscriptionPlans = (filters = {}) => request(`/saas/plans${buildQuery(filters)}`);
export const getSubscriptionPlan = (id) => request(`/saas/plans/${id}`);
export const createSubscriptionPlan = (payload) => request('/saas/plans', { method: 'POST', body: payload });
export const updateSubscriptionPlan = (id, payload) => request(`/saas/plans/${id}`, { method: 'PUT', body: payload });

// SaaS: Subscriptions
export const listTenantSubscriptions = (tenantId) => request(`/saas/tenants/${tenantId}/subscriptions`);
export const createTenantSubscription = (tenantId, payload) => request(`/saas/tenants/${tenantId}/subscriptions`, { method: 'POST', body: payload });
export const cancelTenantSubscription = (tenantId, subscriptionId) =>
  request(`/saas/tenants/${tenantId}/subscriptions/${subscriptionId}/cancel`, { method: 'POST' });

// SaaS: Billing
export const listSubscriptionInvoices = (filters = {}) => request(`/saas/billing${buildQuery(filters)}`);
export const generateSubscriptionInvoice = (subscriptionId) => request('/saas/billing/generate', { method: 'POST', body: { subscriptionId } });
export const markSubscriptionInvoicePaid = (invoiceId, gateway) => request(`/saas/billing/${invoiceId}/mark-paid`, { method: 'POST', body: { gateway } });

// SaaS: Plan Limits
export const getTenantEffectiveLimits = (tenantId) => request(`/saas/tenants/${tenantId}/plan-limits`);

// SaaS: Feature Toggles
export const listTenantFeatureOverrides = (tenantId) => request(`/saas/tenants/${tenantId}/feature-toggles/overrides`);
export const upsertTenantFeatureOverride = (tenantId, payload) =>
  request(`/saas/tenants/${tenantId}/feature-toggles/overrides`, { method: 'PUT', body: payload });
export const resolveTenantFeature = (tenantId, featureKey) => request(`/saas/tenants/${tenantId}/feature-toggles/resolve/${featureKey}`);

// SaaS: White Label branding
export const getTenantBranding = (tenantId) => request(`/saas/tenants/${tenantId}/branding`);
export const updateTenantBranding = (tenantId, payload) => request(`/saas/tenants/${tenantId}/branding`, { method: 'PUT', body: payload });

// SaaS: Custom Domain
export const listTenantDomains = (tenantId) => request(`/saas/tenants/${tenantId}/domains`);
export const createTenantDomain = (tenantId, payload) => request(`/saas/tenants/${tenantId}/domains`, { method: 'POST', body: payload });
export const verifyTenantDomain = (tenantId, domainId) => request(`/saas/tenants/${tenantId}/domains/${domainId}/verify`, { method: 'POST' });

// SaaS: Usage Monitoring
export const getPlatformUsageOverview = (filters = {}) => request(`/saas/usage/overview${buildQuery(filters)}`);
export const getTenantUsage = (tenantId, filters = {}) => request(`/saas/usage/tenants/${tenantId}${buildQuery(filters)}`);

// AI: Dynamic Pricing
export const getSuggestedPrice = (filters = {}) => request(`/ai/pricing/suggest${buildQuery(filters)}`);

// AI: Demand Prediction
export const getDemandForecast = (filters = {}) => request(`/ai/demand/forecast${buildQuery(filters)}`);

// AI: Predictive Maintenance
export const listMaintenancePredictions = (filters = {}) => request(`/ai/maintenance-predictions${buildQuery(filters)}`);
export const generateMaintenancePredictions = () => request('/ai/maintenance-predictions/generate', { method: 'POST' });
export const updateMaintenancePredictionStatus = (id, status) => request(`/ai/maintenance-predictions/${id}/status`, { method: 'PATCH', body: { status } });

// AI: Fraud Detection
export const evaluateBookingForFraud = (bookingId) => request(`/ai/fraud/evaluate/${bookingId}`, { method: 'POST' });
export const listFraudAlerts = (filters = {}) => request(`/ai/fraud${buildQuery(filters)}`);
export const reviewFraudAlert = (id, status) => request(`/ai/fraud/${id}/review`, { method: 'PATCH', body: { status } });

// AI: Revenue Prediction
export const getRevenueForecast = (filters = {}) => request(`/ai/revenue-forecast${buildQuery(filters)}`);

// AI: Recommendation Engine
export const getMyRecommendations = () => request('/ai/recommendations/me');

// AI: Vehicle Damage Detection
export async function analyzeDamage({ vehicleId, rentalId, image }) {
  const form = new FormData();
  if (vehicleId) form.append('vehicleId', vehicleId);
  if (rentalId) form.append('rentalId', rentalId);
  form.append('image', image);
  return request('/ai/damage-detection/analyze', { method: 'POST', body: form, isForm: true });
}
export const listDamageDetectionHistory = (filters = {}) => request(`/ai/damage-detection/history${buildQuery(filters)}`);

// AI: OCR (Driving License / RC Book)
export async function extractDrivingLicense(image) {
  const form = new FormData();
  form.append('image', image);
  return request('/ai/ocr/driving-license', { method: 'POST', body: form, isForm: true });
}
export async function extractRcBook(image) {
  const form = new FormData();
  form.append('image', image);
  return request('/ai/ocr/rc-book', { method: 'POST', body: form, isForm: true });
}
export const listOcrHistory = (filters = {}) => request(`/ai/ocr/history${buildQuery(filters)}`);

// AI: Chatbot
export const startChatSession = () => request('/ai/chatbot/sessions', { method: 'POST' });
export const sendChatMessage = (sessionId, message) => request(`/ai/chatbot/sessions/${sessionId}/messages`, { method: 'POST', body: { message } });
export const getChatHistory = (sessionId) => request(`/ai/chatbot/sessions/${sessionId}/messages`);

// AI: Voice Booking
export async function submitVoiceBooking(audio) {
  const form = new FormData();
  form.append('audio', audio);
  return request('/ai/voice-booking/submit', { method: 'POST', body: form, isForm: true });
}
export const listMyVoiceBookings = () => request('/ai/voice-booking/me');
