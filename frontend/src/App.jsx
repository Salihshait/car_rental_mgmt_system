import { Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import ResetPasswordPage from './pages/ResetPasswordPage';
import CompleteProfilePage from './pages/CompleteProfilePage';
import DashboardPage from './pages/DashboardPage';
import BookingPage from './pages/BookingPage';
import InvoicesPage from './pages/InvoicesPage';
import VehiclesListPage from './pages/VehiclesListPage';
import VehicleDetailsPage from './pages/VehicleDetailsPage';
import VehicleCatalogPage from './pages/VehicleCatalogPage';
import UsersListPage from './pages/UsersListPage';
import UserDetailsPage from './pages/UserDetailsPage';
import RoleManagementPage from './pages/RoleManagementPage';
import PermissionMatrixPage from './pages/PermissionMatrixPage';
import ProfilePage from './pages/ProfilePage';
import CustomersListPage from './pages/CustomersListPage';
import CustomerDetailsPage from './pages/CustomerDetailsPage';
import CustomerPortalPage from './pages/CustomerPortalPage';
import NotificationsPage from './pages/NotificationsPage';
import FleetDashboardPage from './pages/FleetDashboardPage';
import LiveMapPage from './pages/LiveMapPage';
import TripHistoryPage from './pages/TripHistoryPage';
import FuelMonitoringPage from './pages/FuelMonitoringPage';
import MaintenanceSchedulePage from './pages/MaintenanceSchedulePage';
import VehicleTransfersPage from './pages/VehicleTransfersPage';
import DriverAssignmentsPage from './pages/DriverAssignmentsPage';
import DriversListPage from './pages/DriversListPage';
import DriverDetailsPage from './pages/DriverDetailsPage';
import DriverAppDashboardPage from './pages/DriverAppDashboardPage';
import ReportsHubPage from './pages/reports/ReportsHubPage';
import RevenueReportPage from './pages/reports/RevenueReportPage';
import BookingsReportPage from './pages/reports/BookingsReportPage';
import FleetReportPage from './pages/reports/FleetReportPage';
import MaintenanceReportPage from './pages/reports/MaintenanceReportPage';
import CustomerReportPage from './pages/reports/CustomerReportPage';
import DriverReportPage from './pages/reports/DriverReportPage';
import FinanceReportPage from './pages/reports/FinanceReportPage';
import CrmHubPage from './pages/crm/CrmHubPage';
import SupportTicketsPage from './pages/crm/SupportTicketsPage';
import ComplaintsPage from './pages/crm/ComplaintsPage';
import FeedbackPage from './pages/crm/FeedbackPage';
import MessageTemplatesPage from './pages/crm/MessageTemplatesPage';
import CampaignsPage from './pages/crm/CampaignsPage';
import MessageLogsPage from './pages/crm/MessageLogsPage';
import MyTicketsPage from './pages/MyTicketsPage';
import FinanceHubPage from './pages/finance/FinanceHubPage';
import IncomePage from './pages/finance/IncomePage';
import ExpensesPage from './pages/finance/ExpensesPage';
import CashbookPage from './pages/finance/CashbookPage';
import BankPage from './pages/finance/BankPage';
import JournalPage from './pages/finance/JournalPage';
import LedgerPage from './pages/finance/LedgerPage';
import ProfitLossPage from './pages/finance/ProfitLossPage';
import BalanceSheetPage from './pages/finance/BalanceSheetPage';
import GstReportsPage from './pages/finance/GstReportsPage';
import SaasHubPage from './pages/saas/SaasHubPage';
import TenantsPage from './pages/saas/TenantsPage';
import TenantDetailPage from './pages/saas/TenantDetailPage';
import SubscriptionPlansPage from './pages/saas/SubscriptionPlansPage';
import BillingPage from './pages/saas/BillingPage';
import UsageMonitoringPage from './pages/saas/UsageMonitoringPage';
import AiHubPage from './pages/ai/AiHubPage';
import DynamicPricingPage from './pages/ai/DynamicPricingPage';
import DemandForecastPage from './pages/ai/DemandForecastPage';
import PredictiveMaintenancePage from './pages/ai/PredictiveMaintenancePage';
import FraudAlertsPage from './pages/ai/FraudAlertsPage';
import DamageDetectionPage from './pages/ai/DamageDetectionPage';
import DocumentOcrPage from './pages/ai/DocumentOcrPage';
import RevenueForecastPage from './pages/ai/RevenueForecastPage';
import RecommendationsPage from './pages/ai/RecommendationsPage';
import VoiceBookingPage from './pages/ai/VoiceBookingPage';
import AppShell from './components/AppShell';
import RequireAuth from './components/RequireAuth';

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />
      <Route path="/complete-profile" element={<RequireAuth><CompleteProfilePage /></RequireAuth>} />

      <Route element={<RequireAuth><AppShell /></RequireAuth>}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/booking" element={<BookingPage />} />
        <Route path="/invoices" element={<InvoicesPage />} />
        <Route path="/vehicles" element={<VehiclesListPage />} />
        <Route path="/vehicles/new" element={<VehicleDetailsPage />} />
        <Route path="/vehicles/:id" element={<VehicleDetailsPage />} />
        <Route path="/vehicle-catalog" element={<VehicleCatalogPage />} />
        <Route path="/users" element={<UsersListPage />} />
        <Route path="/users/:id" element={<UserDetailsPage />} />
        <Route path="/roles" element={<RoleManagementPage />} />
        <Route path="/permissions" element={<PermissionMatrixPage />} />
        <Route path="/profile" element={<ProfilePage />} />
        <Route path="/customers" element={<CustomersListPage />} />
        <Route path="/customers/:id" element={<CustomerDetailsPage />} />
        <Route path="/my-account" element={<CustomerPortalPage />} />
        <Route path="/notifications" element={<NotificationsPage />} />
        <Route path="/fleet/dashboard" element={<FleetDashboardPage />} />
        <Route path="/fleet/live-map" element={<LiveMapPage />} />
        <Route path="/fleet/trips" element={<TripHistoryPage />} />
        <Route path="/fleet/fuel" element={<FuelMonitoringPage />} />
        <Route path="/fleet/maintenance" element={<MaintenanceSchedulePage />} />
        <Route path="/fleet/transfers" element={<VehicleTransfersPage />} />
        <Route path="/fleet/drivers" element={<DriverAssignmentsPage />} />
        <Route path="/drivers" element={<DriversListPage />} />
        <Route path="/drivers/:id" element={<DriverDetailsPage />} />
        <Route path="/driver-app" element={<DriverAppDashboardPage />} />
        <Route path="/reports" element={<ReportsHubPage />} />
        <Route path="/reports/revenue" element={<RevenueReportPage />} />
        <Route path="/reports/bookings" element={<BookingsReportPage />} />
        <Route path="/reports/fleet" element={<FleetReportPage />} />
        <Route path="/reports/maintenance" element={<MaintenanceReportPage />} />
        <Route path="/reports/customers" element={<CustomerReportPage />} />
        <Route path="/reports/drivers" element={<DriverReportPage />} />
        <Route path="/reports/finance" element={<FinanceReportPage />} />
        <Route path="/crm" element={<CrmHubPage />} />
        <Route path="/crm/support" element={<SupportTicketsPage />} />
        <Route path="/crm/complaints" element={<ComplaintsPage />} />
        <Route path="/crm/feedback" element={<FeedbackPage />} />
        <Route path="/crm/templates" element={<MessageTemplatesPage />} />
        <Route path="/crm/campaigns" element={<CampaignsPage />} />
        <Route path="/crm/messages" element={<MessageLogsPage />} />
        <Route path="/my-tickets" element={<MyTicketsPage />} />
        <Route path="/finance" element={<FinanceHubPage />} />
        <Route path="/finance/income" element={<IncomePage />} />
        <Route path="/finance/expenses" element={<ExpensesPage />} />
        <Route path="/finance/cashbook" element={<CashbookPage />} />
        <Route path="/finance/bank" element={<BankPage />} />
        <Route path="/finance/journal" element={<JournalPage />} />
        <Route path="/finance/ledger" element={<LedgerPage />} />
        <Route path="/finance/profit-loss" element={<ProfitLossPage />} />
        <Route path="/finance/balance-sheet" element={<BalanceSheetPage />} />
        <Route path="/finance/gst" element={<GstReportsPage />} />
        <Route path="/saas" element={<SaasHubPage />} />
        <Route path="/saas/tenants" element={<TenantsPage />} />
        <Route path="/saas/tenants/:id" element={<TenantDetailPage />} />
        <Route path="/saas/plans" element={<SubscriptionPlansPage />} />
        <Route path="/saas/billing" element={<BillingPage />} />
        <Route path="/saas/usage" element={<UsageMonitoringPage />} />
        <Route path="/ai" element={<AiHubPage />} />
        <Route path="/ai/pricing" element={<DynamicPricingPage />} />
        <Route path="/ai/demand" element={<DemandForecastPage />} />
        <Route path="/ai/maintenance-predictions" element={<PredictiveMaintenancePage />} />
        <Route path="/ai/fraud" element={<FraudAlertsPage />} />
        <Route path="/ai/damage-detection" element={<DamageDetectionPage />} />
        <Route path="/ai/ocr" element={<DocumentOcrPage />} />
        <Route path="/ai/revenue-forecast" element={<RevenueForecastPage />} />
        <Route path="/ai/recommendations" element={<RecommendationsPage />} />
        <Route path="/ai/voice-booking" element={<VoiceBookingPage />} />
      </Route>

      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
