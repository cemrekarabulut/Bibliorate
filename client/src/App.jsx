import React from 'react';
import Navbar from './components/layout/Navbar';
import DiscoveryPage from './pages/DiscoveryPage';
import './App.css';

function App() {
  return (
    <div className="app-container">
      <Navbar />
      <main className="main-content">
        <DiscoveryPage />
      </main>
    </div>
  );
}

export default App;
