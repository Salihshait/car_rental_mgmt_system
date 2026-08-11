import { useEffect, useMemo, useState } from 'react';
import {
  AppBar,
  Box,
  Collapse,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Tooltip,
  Typography,
  useMediaQuery,
} from '@mui/material';
import { useTheme } from '@mui/material/styles';
import { Outlet, useLocation, useNavigate, Link as RouterLink } from 'react-router-dom';
import MenuIcon from '@mui/icons-material/Menu';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import LogoutIcon from '@mui/icons-material/Logout';
import DashboardIcon from '@mui/icons-material/Dashboard';
import EventNoteIcon from '@mui/icons-material/EventNote';
import DirectionsCarIcon from '@mui/icons-material/DirectionsCar';
import BadgeIcon from '@mui/icons-material/Badge';
import PeopleIcon from '@mui/icons-material/People';
import SupportAgentIcon from '@mui/icons-material/SupportAgent';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import AssessmentIcon from '@mui/icons-material/Assessment';
import CloudIcon from '@mui/icons-material/Cloud';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings';
import PersonIcon from '@mui/icons-material/Person';
import { supabase } from '../services/supabaseClient';
import { useThemeMode } from '../theme/ThemeModeContext';
import ChatbotWidget from './ChatbotWidget';

const DRAWER_WIDTH = 260;

const navGroups = [
  {
    label: 'Overview',
    icon: DashboardIcon,
    items: [{ label: 'Dashboard', to: '/dashboard' }],
  },
  {
    label: 'Bookings & Billing',
    icon: EventNoteIcon,
    items: [
      { label: 'Bookings', to: '/booking' },
      { label: 'Billing', to: '/invoices' },
    ],
  },
  {
    label: 'Fleet',
    icon: DirectionsCarIcon,
    items: [
      { label: 'Vehicles', to: '/vehicles' },
      { label: 'Vehicle Catalog', to: '/vehicle-catalog' },
      { label: 'Fleet Dashboard', to: '/fleet/dashboard' },
      { label: 'Live Map', to: '/fleet/live-map' },
      { label: 'Trip History', to: '/fleet/trips' },
      { label: 'Fuel Monitoring', to: '/fleet/fuel' },
      { label: 'Maintenance', to: '/fleet/maintenance' },
      { label: 'Transfers', to: '/fleet/transfers' },
    ],
  },
  {
    label: 'Drivers',
    icon: BadgeIcon,
    items: [
      { label: 'Driver Assignments', to: '/fleet/drivers' },
      { label: 'Driver Roster', to: '/drivers' },
      { label: 'Driver App', to: '/driver-app' },
    ],
  },
  {
    label: 'Customers',
    icon: PeopleIcon,
    items: [{ label: 'Customers', to: '/customers' }],
  },
  {
    label: 'CRM',
    icon: SupportAgentIcon,
    items: [
      { label: 'CRM Hub', to: '/crm' },
      { label: 'My Tickets', to: '/my-tickets' },
    ],
  },
  {
    label: 'Finance',
    icon: AccountBalanceIcon,
    items: [{ label: 'Finance Hub', to: '/finance' }],
  },
  {
    label: 'Reports',
    icon: AssessmentIcon,
    items: [{ label: 'Reports Hub', to: '/reports' }],
  },
  {
    label: 'SaaS',
    icon: CloudIcon,
    items: [{ label: 'SaaS Hub', to: '/saas' }],
  },
  {
    label: 'AI',
    icon: AutoAwesomeIcon,
    items: [{ label: 'AI Hub', to: '/ai' }],
  },
  {
    label: 'Administration',
    icon: AdminPanelSettingsIcon,
    items: [
      { label: 'Users', to: '/users' },
      { label: 'Roles', to: '/roles' },
      { label: 'Permissions', to: '/permissions' },
    ],
  },
];

const accountItems = [
  { label: 'My Account', to: '/my-account' },
  { label: 'Notifications', to: '/notifications' },
  { label: 'Profile', to: '/profile' },
];

export default function AppShell() {
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();
  const { mode, toggleMode } = useThemeMode();
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));

  const activeGroupLabel = useMemo(
    () => navGroups.find((group) => group.items.some((item) => location.pathname.startsWith(item.to)))?.label,
    [location.pathname],
  );

  const [openGroups, setOpenGroups] = useState(() => new Set(activeGroupLabel ? [activeGroupLabel] : []));
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    if (activeGroupLabel) {
      setOpenGroups((prev) => (prev.has(activeGroupLabel) ? prev : new Set(prev).add(activeGroupLabel)));
    }
  }, [activeGroupLabel]);

  const toggleGroup = (label) => {
    setOpenGroups((prev) => {
      const next = new Set(prev);
      if (next.has(label)) {
        next.delete(label);
      } else {
        next.add(label);
      }
      return next;
    });
  };

  const handleLogout = async () => {
    await supabase.auth.signOut();
    navigate('/login');
  };

  const closeOnMobile = () => {
    if (!isDesktop) setMobileOpen(false);
  };

  const drawerContent = (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Toolbar>
        <Typography variant="h6" fontWeight={700}>Car Rent</Typography>
      </Toolbar>
      <Divider />
      <List sx={{ flexGrow: 1, overflowY: 'auto' }} component="nav">
        {navGroups.map((group) => {
          const GroupIcon = group.icon;
          const isOpen = openGroups.has(group.label);
          return (
            <Box key={group.label}>
              <ListItemButton onClick={() => toggleGroup(group.label)}>
                <ListItemIcon sx={{ minWidth: 36 }}><GroupIcon fontSize="small" /></ListItemIcon>
                <ListItemText primary={group.label} />
                {isOpen ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
              </ListItemButton>
              <Collapse in={isOpen} timeout="auto" unmountOnExit>
                <List component="div" disablePadding>
                  {group.items.map((item) => (
                    <ListItemButton
                      key={item.to}
                      component={RouterLink}
                      to={item.to}
                      selected={location.pathname === item.to}
                      sx={{ pl: 6 }}
                      onClick={closeOnMobile}
                    >
                      <ListItemText primary={item.label} slotProps={{ primary: { variant: 'body2' } }} />
                    </ListItemButton>
                  ))}
                </List>
              </Collapse>
            </Box>
          );
        })}
      </List>
      <Divider />
      <List component="nav">
        {accountItems.map((item) => (
          <ListItemButton
            key={item.to}
            component={RouterLink}
            to={item.to}
            selected={location.pathname === item.to}
            onClick={closeOnMobile}
          >
            <ListItemIcon sx={{ minWidth: 36 }}><PersonIcon fontSize="small" /></ListItemIcon>
            <ListItemText primary={item.label} slotProps={{ primary: { variant: 'body2' } }} />
          </ListItemButton>
        ))}
      </List>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <AppBar
        position="fixed"
        color="default"
        elevation={1}
        sx={{ zIndex: (t) => t.zIndex.drawer + 1, width: { md: `calc(100% - ${DRAWER_WIDTH}px)` }, ml: { md: `${DRAWER_WIDTH}px` } }}
      >
        <Toolbar sx={{ gap: 1 }}>
          <IconButton edge="start" onClick={() => setMobileOpen(true)} sx={{ display: { md: 'none' } }}>
            <MenuIcon />
          </IconButton>
          <Box sx={{ flexGrow: 1 }} />
          <Tooltip title={mode === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}>
            <IconButton onClick={toggleMode} color="inherit">
              {mode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
            </IconButton>
          </Tooltip>
          <Tooltip title="Logout">
            <IconButton onClick={handleLogout} color="inherit">
              <LogoutIcon />
            </IconButton>
          </Tooltip>
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { md: DRAWER_WIDTH }, flexShrink: { md: 0 } }}>
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{ display: { xs: 'block', md: 'none' }, '& .MuiDrawer-paper': { width: DRAWER_WIDTH } }}
        >
          {drawerContent}
        </Drawer>
        <Drawer
          variant="permanent"
          open
          sx={{ display: { xs: 'none', md: 'block' }, '& .MuiDrawer-paper': { width: DRAWER_WIDTH, boxSizing: 'border-box' } }}
        >
          {drawerContent}
        </Drawer>
      </Box>

      <Box component="main" sx={{ flexGrow: 1, width: { md: `calc(100% - ${DRAWER_WIDTH}px)` }, bgcolor: 'background.default', minHeight: '100vh' }}>
        <Toolbar />
        <Outlet />
      </Box>
      <ChatbotWidget />
    </Box>
  );
}
