import React, { useState } from 'react';
import { X, Save, Lock, Mail, User as UserIcon } from 'lucide-react';
import { apiFacade } from '../../services/apiFacade';
import { useAuth } from '../../context/AuthContext';
import './SettingsModal.css';

const SettingsModal = ({ isOpen, onClose }) => {
  const { user, userId, token } = useAuth();
  
  const [username, setUsername] = useState(user?.username || '');
  const [email, setEmail]       = useState(user?.email || '');
  const [password, setPassword] = useState('');
  
  const [loading, setLoading]   = useState(false);
  const [message, setMessage]   = useState({ type: '', text: '' });

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage({ type: '', text: '' });
    
    try {
      const dataToUpdate = { username, email };
      if (password) {
        dataToUpdate.password = password;
      }
      
      await apiFacade.updateUser(userId, dataToUpdate, token);
      
      setMessage({ type: 'success', text: 'Settings updated successfully!' });
      setTimeout(() => {
        onClose();
        // optionally trigger a re-fetch of user info or update auth context
      }, 1500);
    } catch (error) {
      setMessage({ type: 'error', text: error.message || 'Failed to update settings.' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content glass-panel">
        <button className="modal-close" onClick={onClose}>
          <X size={20} />
        </button>
        
        <h2>Account Settings</h2>
        
        {message.text && (
          <div className={`modal-msg ${message.type}`}>
            {message.text}
          </div>
        )}
        
        <form onSubmit={handleSubmit} className="settings-form">
          <div className="form-group">
            <label><UserIcon size={16} /> Username</label>
            <input 
              type="text" 
              value={username} 
              onChange={(e) => setUsername(e.target.value)} 
              placeholder="Your username"
            />
          </div>
          
          <div className="form-group">
            <label><Mail size={16} /> Email</label>
            <input 
              type="email" 
              value={email} 
              onChange={(e) => setEmail(e.target.value)} 
              placeholder="Your email"
            />
          </div>
          
          <div className="form-group">
            <label><Lock size={16} /> New Password</label>
            <input 
              type="password" 
              value={password} 
              onChange={(e) => setPassword(e.target.value)} 
              placeholder="Leave blank to keep current"
            />
          </div>
          
          <button type="submit" className="primary-btn submit-btn" disabled={loading}>
            {loading ? 'Saving...' : <><Save size={18} /> Save Changes</>}
          </button>
        </form>
      </div>
    </div>
  );
};

export default SettingsModal;
