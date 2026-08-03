import { createTheme } from '@mui/material/styles';

export const appTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#0d6efd',
    },
    secondary: {
      main: '#198754',
    },
  },
  shape: {
    borderRadius: 12,
  },
});
