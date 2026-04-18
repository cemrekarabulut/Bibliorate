import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { apiFacade } from '../../services/apiFacade';
import { useAuth } from '../../context/AuthContext';
import './Auth.css';

/**
 * LoginPage
 * ---------
 * Authenticates the user against the .NET API (/api/auth/login).
 * On success, saves the JWT token and user info via AuthContext,
 * then redirects to the home page.
 */
const LoginPage = () => {
  const { login }          = useAuth();
  const navigate           = useNavigate();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError]       = useState('');
  const [loading, setLoading]   = useState(false);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const response = await apiFacade.login(username, password);
      login(response);       // Persists token + user info in AuthContext
      navigate('/');
    } catch (err) {
      setError(err.message || 'Login failed. Please check your credentials.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card glass-panel">
        <h2 className="auth-title">Welcome Back</h2>
        <p className="auth-subtitle">Login to track your favourite books.</p>

        {error && <div className="auth-error">{error}</div>}

        <form onSubmit={handleLogin} className="auth-form">
          <div className="input-group">
            <label htmlFor="login-username">Username</label>
            <input
              id="login-username"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              autoComplete="username"
            />
          </div>
          <div className="input-group">
            <label htmlFor="login-password">Password</label>
            <input
              id="login-password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              autoComplete="current-password"
            />
          </div>
          <button type="submit" className="auth-btn" disabled={loading}>
            {loading ? 'Logging in…' : 'Log In'}
          </button>
        </form>

        <div className="auth-footer">
          <p>Don't have an account? <Link to="/register" className="text-gradient">Sign up here</Link></p>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
