import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { BookOpen, Search, BarChart2, User, Menu } from 'lucide-react';
import './Navbar.css';

const Navbar = () => {
  const location = useLocation();

  const isActive = (path) => {
    return location.pathname === path ? 'active' : '';
  };

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
            <span>Profile</span>
          </Link>
          <Link to="/login" className={`nav-link ${isActive('/login')}`}>
            <span>Sign In</span>
          </Link>
        </div>

        <button className="mobile-menu-btn">
          <Menu size={24} />
        </button>
      </div>
    </nav>
  );
};

export default Navbar;
