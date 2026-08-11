import { createContext, useContext, useMemo, useState } from 'react';

const STORAGE_KEY = 'car-rent-theme-mode';

const ThemeModeContext = createContext({ mode: 'light', toggleMode: () => {} });

function readStoredMode() {
  if (typeof window === 'undefined') return 'light';
  return window.localStorage.getItem(STORAGE_KEY) === 'dark' ? 'dark' : 'light';
}

export function ThemeModeProvider({ children }) {
  const [mode, setMode] = useState(readStoredMode);

  const toggleMode = () => {
    setMode((previous) => {
      const next = previous === 'light' ? 'dark' : 'light';
      window.localStorage.setItem(STORAGE_KEY, next);
      return next;
    });
  };

  const value = useMemo(() => ({ mode, toggleMode }), [mode]);

  return <ThemeModeContext.Provider value={value}>{children}</ThemeModeContext.Provider>;
}

export function useThemeMode() {
  return useContext(ThemeModeContext);
}
