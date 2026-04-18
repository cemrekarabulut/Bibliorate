import React, { useState, useEffect } from 'react';
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  LineChart, Line
} from 'recharts';
import { TrendingUp, Users, BookOpen } from 'lucide-react';
import { apiFacade } from '../services/apiFacade';
import './AnalyticsDashboard.css';

const AnalyticsDashboard = () => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAnalytics = async () => {
      setLoading(true);
      const res = await apiFacade.getAnalytics();
      setData(res);
      setLoading(false);
    };
    fetchAnalytics();
  }, []);

  if (loading) {
    return <div className="analytics-page"><div className="spinner-large"></div></div>;
  }

  return (
    <div className="analytics-page">
      <header className="analytics-header">
        <h1>Platform <span className="text-gradient">Analytics</span></h1>
        <p>Real-time insights powered by our Flask microservice simulation.</p>
      </header>

      <div className="kpi-grid">
        <div className="kpi-card glass-panel">
          <div className="kpi-icon-wrap"><BookOpen size={24} className="text-gradient"/></div>
          <div className="kpi-info">
            <h3>Total Books</h3>
            <p className="kpi-val">{data.overview.totalBooks.toLocaleString()}</p>
          </div>
        </div>
        <div className="kpi-card glass-panel">
          <div className="kpi-icon-wrap"><Users size={24} className="text-gradient"/></div>
          <div className="kpi-info">
            <h3>Active Readers</h3>
            <p className="kpi-val">{data.overview.activeUsers.toLocaleString()}</p>
          </div>
        </div>
        <div className="kpi-card glass-panel">
          <div className="kpi-icon-wrap"><TrendingUp size={24} className="text-gradient"/></div>
          <div className="kpi-info">
            <h3>Avg. Daily Reviews</h3>
            <p className="kpi-val">{data.overview.dailyReviews}</p>
          </div>
        </div>
      </div>

      <div className="charts-grid">
        <div className="chart-card glass-panel">
          <h3>Genre Popularity</h3>
          <div className="chart-container">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={data.genrePopularity} margin={{ top: 20, right: 30, left: 0, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#2d3748" vertical={false} />
                <XAxis dataKey="name" stroke="#9ca3af" tick={{fill: '#9ca3af'}} />
                <YAxis stroke="#9ca3af" tick={{fill: '#9ca3af'}} />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#14171f', borderColor: '#2d3748', borderRadius: '8px' }}
                  itemStyle={{ color: '#4ade80' }}
                />
                <Bar dataKey="views" fill="#4ade80" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="chart-card glass-panel">
          <h3>Engagement Over Time</h3>
          <div className="chart-container">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={data.engagementData} margin={{ top: 20, right: 30, left: 0, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#2d3748" vertical={false} />
                <XAxis dataKey="month" stroke="#9ca3af" tick={{fill: '#9ca3af'}} />
                <YAxis stroke="#9ca3af" tick={{fill: '#9ca3af'}} />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#14171f', borderColor: '#2d3748', borderRadius: '8px' }}
                />
                <Line type="monotone" dataKey="activeUsers" stroke="#818cf8" strokeWidth={3} dot={{r: 4, fill: '#818cf8'}} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AnalyticsDashboard;
