import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { BookOpen, Search, BarChart2, User, LogIn, Menu } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import './Navbar.css';

/**
 * Navbar
 * ------
 * Top navigation bar. Shows "Sign In" when logged out,
 * or the user's username with a profile link when logged in.
 */
const Navbar = () => {
  const location            = useLocation();
  const { isLoggedIn, user } = useAuth();

  const isActive = (path) => location.pathname === path ? 'active' : '';

  return (
    <nav className="navbar glass-panel">
      <div className="nav-container">
        <Link to="/" className="nav-logo">
          <BookOpen className="logo-icon text-gradient" size={28} />
          <span className="logo-text">Biblio<span className="text-gradient">Rate</span></span>
        </Link>

        <div className="nav-links">
          <Link to="/" className={`nav-link ${isActive('/')}`}>
            <Search size={18} />
            <span>Discover</span>
          </Link>
          <Link to="/analytics" className={`nav-link ${isActive('/analytics')}`}>
            <BarChart2 size={18} />
            <span>Analytics</span>
          </Link>
          <Link to="/profile" className={`nav-link ${isActive('/profile')}`}>
            <User size={18} />
            <span>{isLoggedIn ? (user?.username ?? 'Profile') : 'Profile'}</span>
          </Link>
          {!isLoggedIn && (
            <Link to="/login" className={`nav-link ${isActive('/login')}`}>
              <LogIn size={18} />
              <span>Sign In</span>
            </Link>
          )}
        </div>

        <button className="mobile-menu-btn" aria-label="Open menu">
          <Menu size={24} />
        </button>
      </div>
    </nav>
  );
};

export default Navbar;
