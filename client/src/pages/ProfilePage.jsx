import React, { useState, useEffect } from 'react';
import { User, Library, Settings, LogOut } from 'lucide-react';
import { Link } from 'react-router-dom';
import { apiFacade } from '../services/apiFacade';
import { useAuth } from '../context/AuthContext';
import BookCard from '../components/discovery/BookCard';
import './ProfilePage.css';

/**
 * ProfilePage
 * -----------
 * Shows the authenticated user's profile, favourites list, and account settings.
 * Redirects unauthenticated visitors to the login prompt.
 */
const ProfilePage = () => {
  const { isLoggedIn, user, userId, token, logout, login } = useAuth();

  const [activeTab, setActiveTab]   = useState('lists'); // 'lists' | 'settings'
  const [favourites, setFavourites] = useState([]);
  const [loading, setLoading]       = useState(true);

  // Settings form state
  const [uname, setUname]                     = useState('');
  const [email, setEmail]                     = useState('');
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword]         = useState('');
  
  const [settingsLoading, setSettingsLoading] = useState(false);
  const [settingsError, setSettingsError]     = useState('');
  const [settingsSuccess, setSettingsSuccess] = useState('');

  // ── Fetch Favourites ──────────────────────────────────────────────────
  useEffect(() => {
    if (!isLoggedIn) {
      setLoading(false);
      return;
    }

    const fetchFavourites = async () => {
      setLoading(true);
      try {
        const data = await apiFacade.getFavorites(userId, token);
        setFavourites(data);
      } catch (err) {
        console.error('Failed to load favourites:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchFavourites();
  }, [isLoggedIn, userId, token]);

  // ── Sync user data to form ───────────────────────────────────────────
  useEffect(() => {
    if (user) {
      setUname(user.username || '');
      setEmail(user.email || '');
    }
  }, [user]);

  // ── Settings Submit ──────────────────────────────────────────────────
  const handleUpdateProfile = async (e) => {
    e.preventDefault();
    setSettingsError('');
    setSettingsSuccess('');

    // If changing password, must provide current
    if (newPassword && !currentPassword) {
      setSettingsError('Please provide your current password to set a new one.');
      return;
    }

    setSettingsLoading(true);
    try {
      const payload = {
        username: uname !== user.username ? uname : undefined,
        email: email !== user.email ? email : undefined,
        currentPassword: currentPassword || undefined,
        newPassword: newPassword || undefined,
      };

      // Strip undefined
      Object.keys(payload).forEach(key => payload[key] === undefined && delete payload[key]);

      if (Object.keys(payload).length === 0) {
        setSettingsError('No changes detected.');
        setSettingsLoading(false);
        return;
      }

      const responseData = await apiFacade.updateProfile(userId, payload, token);
      
      // Update global context with new user info & new token
      login(responseData);
      
      setSettingsSuccess('Profile updated successfully!');
      setCurrentPassword('');
      setNewPassword('');

      setTimeout(() => setSettingsSuccess(''), 3000);
    } catch (err) {
      setSettingsError(err.message || 'Failed to update profile.');
    } finally {
      setSettingsLoading(false);
    }
  };


  // ── Not logged in ───────────────────────────────────────────────────────
  if (!isLoggedIn) {
    return (
      <div className="profile-page">
        <div className="profile-locked glass-panel">
          <h2>Please Log In</h2>
          <p>You need to be logged in to view your profile and lists.</p>
          <Link to="/login" className="primary-btn" style={{ marginTop: '1rem', display: 'inline-flex' }}>
            Go to Login
          </Link>
        </div>
      </div>
    );
  }

  // ── Render ──────────────────────────────────────────────────────────────
  return (
    <div className="profile-page">
      <header className="profile-header glass-panel">
        <div className="profile-avatar">
          <User size={48} className="text-gradient" />
        </div>
        <div className="profile-info">
          <h1>
            Welcome, <span className="text-gradient">{user?.username ?? 'Reader'}</span>
          </h1>
          <p>{user?.email}</p>
        </div>
        <div className="profile-actions">
          <button 
            className={`settings-btn ${activeTab === 'lists' ? 'active-tab-btn' : ''}`} 
            onClick={() => setActiveTab('lists')}
          >
            <Library size={20} /> My Lists
          </button>
          <button 
            className={`settings-btn ${activeTab === 'settings' ? 'active-tab-btn' : ''}`} 
            onClick={() => setActiveTab('settings')} 
            style={{ marginLeft: '0.5rem' }}
          >
            <Settings size={20} /> Settings
          </button>
          <button className="settings-btn" onClick={logout} style={{ marginLeft: '0.5rem' }}>
            <LogOut size={20} /> Logout
          </button>
        </div>
      </header>

      <section className="profile-content">
        
        {/* TAB: My Lists */}
        {activeTab === 'lists' && (
          <div className="fade-in">
            <div className="section-header">
              <Library className="text-gradient" size={24} />
              <h2>My Favourites List</h2>
            </div>

            {loading ? (
              <div className="spinner-large" style={{ margin: '4rem auto' }} />
            ) : favourites.length > 0 ? (
              <div className="favorites-grid">
                {favourites.map(book => (
                  <BookCard key={book.id} book={book} />
                ))}
              </div>
            ) : (
              <div className="empty-favorites">
                <p>You haven't added any books to your favourites yet.</p>
                <Link to="/" className="primary-btn" style={{ marginTop: '1rem', display: 'inline-flex' }}>
                  Discover Books
                </Link>
              </div>
            )}
          </div>
        )}

        {/* TAB: Settings */}
        {activeTab === 'settings' && (
          <div className="fade-in settings-container glass-panel">
            <div className="section-header">
              <Settings className="text-gradient" size={24} />
              <h2>Account Settings</h2>
            </div>

            {settingsError && <div className="auth-error" style={{marginBottom: '1.5rem'}}>{settingsError}</div>}
            {settingsSuccess && <div className="auth-success" style={{marginBottom: '1.5rem'}}>{settingsSuccess}</div>}

            <form onSubmit={handleUpdateProfile} className="settings-form">
              <div className="input-group">
                <label>Username</label>
                <input 
                  type="text" 
                  value={uname} 
                  onChange={e => setUname(e.target.value)} 
                  required
                />
              </div>

              <div className="input-group">
                <label>Email Address</label>
                <input 
                  type="email" 
                  value={email} 
                  onChange={e => setEmail(e.target.value)}
                  required
                />
              </div>

              <hr className="settings-divider" />
              <h3>Change Password</h3>
              <p className="settings-help">Leave blank if you do not want to change your password.</p>

              <div className="input-group">
                <label>Current Password</label>
                <input 
                  type="password" 
                  value={currentPassword} 
                  onChange={e => setCurrentPassword(e.target.value)}
                  autoComplete="current-password"
                />
              </div>

              <div className="input-group">
                <label>New Password</label>
                <input 
                  type="password" 
                  value={newPassword} 
                  onChange={e => setNewPassword(e.target.value)}
                  autoComplete="new-password"
                />
              </div>

              <button type="submit" className="primary-btn settings-save-btn" disabled={settingsLoading}>
                {settingsLoading ? 'Saving…' : 'Save Changes'}
              </button>
            </form>
          </div>
        )}

      </section>
    </div>
  );
};

export default ProfilePage;
