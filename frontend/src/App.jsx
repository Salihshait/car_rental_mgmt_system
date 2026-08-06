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
      </Route>

      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
