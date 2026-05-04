import React, { createContext, useContext, useState, useCallback } from 'react';

/**
 * AuthContext
 * -----------
 * Provides authentication state (user, token) and actions (login, logout, register)
 * to the entire component tree. Persists the session in localStorage.
 */

const AuthContext = createContext(null);

const STORAGE_KEY = 'bibliorate_auth';

const loadPersistedSession = () => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
};

export const AuthProvider = ({ children }) => {
  const [session, setSession] = useState(loadPersistedSession);

  const persistSession = useCallback((data) => {
    setSession(data);
    if (data) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, []);

  const login = useCallback((responseData) => {
    persistSession({
      token: responseData.token,
      userId: Number(responseData.userId),
      username: responseData.username,
      email: responseData.email,
    });
  }, [persistSession]);

  const logout = useCallback(() => {
    persistSession(null);
  }, [persistSession]);

  const value = {
    user: session,
    token: session?.token ?? null,
    userId: session?.userId ?? null,
    isLoggedIn: Boolean(session?.token),
    login,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an <AuthProvider>');
  return ctx;
};
