import React, { useEffect, useState } from 'react'
import { getDashboardSummary } from './client'
import type { DashboardSummaryDto } from './types'

export default function App() {
  const [data, setData] = useState<DashboardSummaryDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let mounted = true
    setLoading(true)
    getDashboardSummary()
      .then(d => { if (mounted) setData(d) })
      .catch(e => { if (mounted) setError(String(e)) })
      .finally(() => { if (mounted) setLoading(false) })
    return () => { mounted = false }
  }, [])

  return (
    <div className="app">
      <header className="header">Iran Risk Tracker</header>
      <main className="card">
        {loading && <div className="muted">Loading...</div>}
        {error && <div className="error">Error: {error}</div>}
        {data && (
          <div className="summary">
            <div className="score">
              <div className="score-percent">{data.ScorePercent}%</div>
              <div className="score-meta">{data.Level} • {data.ScoreTrend} ({data.ScoreChange >= 0 ? '+' : ''}{data.ScoreChange.toFixed(2)})</div>
              {data.PreviousScore != null && <div className="previous">Previous: {data.PreviousScore}</div>}
              <div className="timestamp">{new Date(data.Timestamp).toLocaleString()}</div>
            </div>
            <div className="top">
              <h3>Top Contributors</h3>
              <ul>
                {data.TopContributors.map(t => (
                  <li key={t.IndicatorKey}>
                    <div className="t-row">
                      <div className="t-left">
                        <div className="t-name">{t.IndicatorName}</div>
                        <div className="t-key">{t.IndicatorKey} • {t.Category}</div>
                      </div>
                      <div className="t-right">
                        <div className="t-weight">{t.WeightedContribution >= 0 ? '+' : ''}{t.WeightedContribution.toFixed(2)}</div>
                      </div>
                    </div>
                    <div className="t-explain">{t.Explanation}</div>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        )}
      </main>
    </div>
  )
}
