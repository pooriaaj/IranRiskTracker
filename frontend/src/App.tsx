import React, { useEffect, useState } from 'react'
import { getDashboardSummary } from './client'
import type { DashboardSummaryDto } from './types'

const RISK_LEVEL_LABELS: Record<number, string> = {
  0: 'Unknown', 1: 'Low', 2: 'Medium', 3: 'High', 4: 'Critical',
}

const RISK_LEVEL_COLORS: Record<number, string> = {
  0: '#6b7280', 1: '#22c55e', 2: '#eab308', 3: '#f97316', 4: '#ef4444',
}

const EVENT_CATEGORY_LABELS: Record<number, string> = {
  0: 'Unknown', 1: 'Protests', 2: 'Executions', 3: 'Nuclear',
  4: 'Maritime', 5: 'Cyber', 6: 'Military', 7: 'Political', 8: 'Economic',
}

function formatRiskLevel(level: number | string): string {
  if (typeof level === 'number') return RISK_LEVEL_LABELS[level] ?? 'Unknown'
  return level
}

function riskLevelColor(level: number | string): string {
  if (typeof level === 'number') return RISK_LEVEL_COLORS[level] ?? '#6b7280'
  const idx = Object.values(RISK_LEVEL_LABELS).indexOf(level)
  return idx >= 0 ? (RISK_LEVEL_COLORS[idx] ?? '#6b7280') : '#6b7280'
}

function formatEventCategory(category: number | string): string {
  if (typeof category === 'number') return EVENT_CATEGORY_LABELS[category] ?? 'Unknown'
  return category
}

function formatScoreChange(change: number): string {
  const fixed = change.toFixed(3)
  return change >= 0 ? `+${fixed}` : fixed
}

function formatTimestamp(ts: string): string {
  return new Date(ts).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

function formatScoreTrend(trend: string): string {
  switch (trend) {
    case 'Increased': return '▲ Increasing'
    case 'Decreased': return '▼ Decreasing'
    case 'Unchanged': return '→ Unchanged'
    case 'NoPreviousSnapshot': return 'First reading'
    default: return trend || '—'
  }
}

function trendClass(trend: string): string {
  if (trend === 'Increased') return 'trend-up'
  if (trend === 'Decreased') return 'trend-down'
  return 'trend-stable'
}

interface ScoreRingProps {
  percent: number
  level: number | string
}

function ScoreRing({ percent, level }: ScoreRingProps) {
  const r = 66
  const cx = 88
  const cy = 88
  const size = 176
  const circumference = 2 * Math.PI * r
  const offset = circumference * (1 - Math.min(percent, 100) / 100)
  const color = riskLevelColor(level)
  const label = formatRiskLevel(level).toUpperCase()

  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} aria-label={`Risk score ${percent}%`}>
      {/* track */}
      <circle cx={cx} cy={cy} r={r} fill="none" stroke="#1b2a42" strokeWidth="13" />
      {/* progress arc */}
      <circle
        cx={cx} cy={cy} r={r}
        fill="none"
        stroke={color}
        strokeWidth="13"
        strokeLinecap="round"
        strokeDasharray={circumference}
        strokeDashoffset={offset}
        transform={`rotate(-90 ${cx} ${cy})`}
        style={{ transition: 'stroke-dashoffset 0.6s ease' }}
      />
      {/* score */}
      <text
        x={cx} y={cx - 8}
        textAnchor="middle" dominantBaseline="middle"
        fill="#e6eef6" fontSize="30" fontWeight="800"
        fontFamily="Inter, Segoe UI, sans-serif"
      >
        {percent}%
      </text>
      {/* level label */}
      <text
        x={cx} y={cx + 20}
        textAnchor="middle" dominantBaseline="middle"
        fill={color} fontSize="11" fontWeight="700"
        fontFamily="Inter, Segoe UI, sans-serif"
        letterSpacing="1.5"
      >
        {label}
      </text>
    </svg>
  )
}

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
        {loading && <div className="muted">Loading…</div>}
        {error && <div className="error">Error: {error}</div>}
        {data && (
          <div className="summary">

            <div className="score-panel">
              <ScoreRing percent={data.scorePercent} level={data.level} />
              <div className={`trend-label ${trendClass(data.scoreTrend)}`}>
                {formatScoreTrend(data.scoreTrend)}
              </div>
              {data.scoreTrend !== 'NoPreviousSnapshot' && (
                <div className="score-change">{formatScoreChange(data.scoreChange)}</div>
              )}
              {data.previousScore != null && (
                <div className="prev-score">Prev: {data.previousScore.toFixed(4)}</div>
              )}
              <div className="timestamp">{formatTimestamp(data.timestamp)}</div>
              {data.summary && <div className="summary-text">{data.summary}</div>}
            </div>

            <div className="contributors">
              <h3 className="contributors-heading">Top Contributors</h3>
              <ul>
                {data.topContributors.map(t => (
                  <li key={t.indicatorKey}>
                    <div className="contrib-row">
                      <div className="contrib-left">
                        <div className="contrib-name">{t.indicatorName}</div>
                        <div className="contrib-meta">
                          {t.indicatorKey} &bull; {formatEventCategory(t.category)}
                        </div>
                      </div>
                      <div className="contrib-weight">
                        {formatScoreChange(t.weightedContribution)}
                      </div>
                    </div>
                    {t.explanation && (
                      <div className="contrib-explain">{t.explanation}</div>
                    )}
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
