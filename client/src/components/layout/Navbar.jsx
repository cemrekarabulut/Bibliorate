import React from 'react';
import { BookOpen, Search, BarChart2, User, Menu } from 'lucide-react';
import './Navbar.css';

const Navbar = () => {
  return (
    <nav className="navbar glass-panel">
      <div className="nav-container">
        <div className="nav-logo">
          <BookOpen className="logo-icon text-gradient" size={28} />
          <span className="logo-text">Biblio<span className="text-gradient">Rate</span></span>
        </div>
        
        <div className="nav-links">
          <a href="#" className="nav-link active">
            <Search size={18} />
            <span>Discover</span>
          </a>
          <a href="#" className="nav-link">
            <BarChart2 size={18} />
            <span>Analytics</span>
          </a>
          <a href="#" className="nav-link">
            <User size={18} />
            <span>My Profile</span>
          </a>
        </div>

        <button className="mobile-menu-btn">
          <Menu size={24} />
        </button>
      </div>
    </nav>
  );
};

export default Navbar;
