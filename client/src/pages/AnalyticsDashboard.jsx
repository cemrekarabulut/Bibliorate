import React, { useState, useEffect } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  LineChart, Line,
} from 'recharts';
import { TrendingUp, BookOpen, Eye } from 'lucide-react';
import { apiFacade } from '../services/apiFacade';
import './AnalyticsDashboard.css';

/**
 * AnalyticsDashboard
 * ------------------
 * Displays platform analytics fetched through the .NET API,
 * which proxies the calls to the Flask microservice.
 *
 * Data shape returned by apiFacade.getAnalytics():
 *   { genrePopularity: [{ name, views }], mostViewed: [{ title, views }] }
 */
const AnalyticsDashboard = () => {
  const [data, setData]     = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError]   = useState('');

  useEffect(() => {
    const fetchAnalytics = async () => {
      setLoading(true);
      setError('');
      try {
        const result = await apiFacade.getAnalytics();
        setData(result);
      } catch (err) {
        console.error('Failed to load analytics:', err);
        setError('Could not load analytics data. Make sure the API is running.');
      } finally {
        setLoading(false);
      }
    };
    fetchAnalytics();
  }, []);

  // ── Loading / Error states ──────────────────────────────────────────────

  if (loading) {
    return (
      <div className="analytics-page">
        <div className="spinner-large" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="analytics-page">
        <div className="glass-panel" style={{ padding: '2rem', textAlign: 'center', color: 'var(--color-error, #f87171)' }}>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  const topGenre = data.genrePopularity?.[0]?.name ?? '—';
  const topViews = data.mostViewed?.[0]?.views     ?? 0;

  // ── Render ──────────────────────────────────────────────────────────────

  return (
    <div className="analytics-page">
      <header className="analytics-header">
        <h1>Platform <span className="text-gradient">Analytics</span></h1>
        <p>Real-time insights powered by the Flask microservice.</p>
      </header>

      <div className="kpi-grid">
        <div className="kpi-card glass-panel">
          <div className="kpi-icon-wrap">
            <BookOpen size={24} className="text-gradient" />
          </div>
          <div className="kpi-info">
            <h3>Top Genre</h3>
            <p className="kpi-val">{topGenre}</p>
          </div>
        </div>

        <div className="kpi-card glass-panel">
          <div className="kpi-icon-wrap">
            <Eye size={24} className="text-gradient" />
          </div>
          <div className="kpi-info">
            <h3>Most Viewed (Views)</h3>
            <p className="kpi-val">{topViews.toLocaleString()}</p>
          </div>
        </div>

        <div className="kpi-card glass-panel">
          <div className="kpi-icon-wrap">
            <TrendingUp size={24} className="text-gradient" />
          </div>
          <div className="kpi-info">
            <h3>Genres Tracked</h3>
            <p className="kpi-val">{data.genrePopularity?.length ?? 0}</p>
          </div>
        </div>
      </div>

      <div className="charts-grid">
        {/* Genre Popularity Bar Chart */}
        <div className="chart-card glass-panel">
          <h3>Genre Popularity</h3>
          <div className="chart-container">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={data.genrePopularity}
                margin={{ top: 20, right: 30, left: 0, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#2d3748" vertical={false} />
                <XAxis dataKey="name" stroke="#9ca3af" tick={{ fill: '#9ca3af' }} />
                <YAxis stroke="#9ca3af" tick={{ fill: '#9ca3af' }} />
                <Tooltip
                  contentStyle={{ backgroundColor: '#14171f', borderColor: '#2d3748', borderRadius: '8px' }}
                  itemStyle={{ color: '#4ade80' }}
                />
                <Bar dataKey="views" fill="#4ade80" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Most Viewed Books Line Chart */}
        <div className="chart-card glass-panel">
          <h3>Most Viewed Books</h3>
          <div className="chart-container">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart
                data={data.mostViewed}
                margin={{ top: 20, right: 30, left: 0, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#2d3748" vertical={false} />
                <XAxis dataKey="title" stroke="#9ca3af" tick={{ fill: '#9ca3af', fontSize: 11 }} />
                <YAxis stroke="#9ca3af" tick={{ fill: '#9ca3af' }} />
                <Tooltip
                  contentStyle={{ backgroundColor: '#14171f', borderColor: '#2d3748', borderRadius: '8px' }}
                />
                <Line
                  type="monotone"
                  dataKey="views"
                  stroke="#818cf8"
                  strokeWidth={3}
                  dot={{ r: 4, fill: '#818cf8' }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AnalyticsDashboard;
