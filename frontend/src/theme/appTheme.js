import { createTheme } from '@mui/material/styles';

export const createAppTheme = (mode = 'light') =>
  createTheme({
    palette: {
      mode,
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
