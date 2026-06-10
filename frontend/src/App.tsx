import React, { useEffect, useState, useCallback } from 'react'
import { getDashboardSummary } from './client'
import type { DashboardSummaryDto } from './types'

// ── Lookup tables ────────────────────────────────────────────────────────────

const RISK_LEVEL_LABELS: Record<number, string> = {
  0: 'Unknown', 1: 'Low', 2: 'Medium', 3: 'High', 4: 'Critical',
}
const EVENT_CATEGORY_LABELS: Record<number, string> = {
  0: 'Unknown', 1: 'Protests', 2: 'Executions', 3: 'Nuclear',
  4: 'Maritime', 5: 'Cyber', 6: 'Military', 7: 'Political', 8: 'Economic',
}

// ── Helpers ──────────────────────────────────────────────────────────────────

function formatRiskLevel(level: number | string): string {
  if (typeof level === 'number') return RISK_LEVEL_LABELS[level] ?? 'Unknown'
  return level
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
    case 'Increased': return '▲ RISING'
    case 'Decreased': return '▼ FALLING'
    case 'Unchanged': return '→ STABLE'
    case 'NoPreviousSnapshot': return 'INITIAL READING'
    default: return trend || '—'
  }
}
function trendColor(trend: string): string {
  if (trend === 'Increased') return '#ff2222'
  if (trend === 'Decreased') return '#888'
  return '#cc0000'
}
function scoreToColor(pct: number): string {
  if (pct >= 75) return '#cc0000'
  if (pct >= 50) return '#ff4400'
  if (pct >= 25) return '#ff8800'
  return '#888888'
}

// ── Score ring ───────────────────────────────────────────────────────────────

function ScoreRing({ percent, level }: { percent: number; level: number | string }) {
  const r = 110, cx = 130, cy = 130, size = 260
  const circ = 2 * Math.PI * r
  const offset = circ * (1 - Math.min(percent, 100) / 100)
  const color = scoreToColor(percent)
  const levelLabel = formatRiskLevel(level).toUpperCase()

  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} aria-label={`Regime collapse probability ${percent}%`}
      style={{ filter: percent >= 75 ? 'drop-shadow(0 0 18px #cc000088)' : 'none' }}>
      <circle cx={cx} cy={cy} r={r} fill="none" stroke="#1a0000" strokeWidth="16" />
      <circle cx={cx} cy={cy} r={r} fill="none" stroke={color} strokeWidth="16"
        strokeLinecap="round" strokeDasharray={circ} strokeDashoffset={offset}
        transform={`rotate(-90 ${cx} ${cy})`}
        style={{ transition: 'stroke-dashoffset 0.8s ease' }} />
      <text x={cx} y={cy - 18} textAnchor="middle" dominantBaseline="middle"
        fill="#ffffff" fontSize="52" fontWeight="900" fontFamily="'Inter', 'Segoe UI', sans-serif"
        letterSpacing="-1">
        {percent}%
      </text>
      <text x={cx} y={cy + 20} textAnchor="middle" dominantBaseline="middle"
        fill={color} fontSize="13" fontWeight="800" fontFamily="'Inter', 'Segoe UI', sans-serif"
        letterSpacing="3">
        {levelLabel}
      </text>
    </svg>
  )
}

// ── Indicator bar ────────────────────────────────────────────────────────────

function IndicatorBar({ name, contribution, maxContribution }: {
  name: string
  contribution: number
  maxContribution: number
}) {
  const pct = maxContribution > 0 ? (contribution / maxContribution) * 100 : 0
  return (
    <div style={{ marginBottom: 14 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
        <span style={{ color: '#cccccc', fontSize: 12, fontWeight: 600, letterSpacing: 1, textTransform: 'uppercase' }}>{name}</span>
        <span style={{ color: '#ff4444', fontSize: 12, fontWeight: 700, fontFamily: 'monospace' }}>+{contribution.toFixed(3)}</span>
      </div>
      <div style={{ background: '#1a0000', borderRadius: 2, height: 4, overflow: 'hidden' }}>
        <div style={{
          width: `${pct}%`, height: '100%',
          background: 'linear-gradient(90deg, #8b0000, #cc0000)',
          transition: 'width 0.6s ease',
          boxShadow: '0 0 6px #cc000066',
        }} />
      </div>
    </div>
  )
}

// ── Main app ─────────────────────────────────────────────────────────────────

export default function App() {
  const [data, setData] = useState<DashboardSummaryDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [err, setErr] = useState<string | null>(null)
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date())

  const fetchDashboard = useCallback(() => {
    setLoading(true)
    setErr(null)
    getDashboardSummary()
      .then(d => { setData(d); setLastRefresh(new Date()) })
      .catch(e => setErr(String(e)))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    fetchDashboard()
  }, [fetchDashboard])

  const pct = data?.scorePercent ?? 0
  const ringColor = scoreToColor(pct)
  const maxContrib = data ? Math.max(...data.topContributors.map(t => t.weightedContribution), 0.01) : 1

  return (
    <div style={{
      minHeight: '100vh',
      background: '#050505',
      color: '#f5f5f5',
      fontFamily: "'Inter', 'Segoe UI', sans-serif",
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
    }}>
      {/* Header */}
      <div style={{
        width: '100%',
        borderBottom: '1px solid #2a0000',
        background: '#0a0000',
        padding: '14px 24px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        boxSizing: 'border-box',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <div style={{
            width: 10, height: 10, borderRadius: '50%',
            background: '#cc0000',
            boxShadow: '0 0 8px #cc0000',
            animation: 'pulse 2s infinite',
          }} />
          <span style={{ fontSize: 13, fontWeight: 700, letterSpacing: 3, color: '#cc0000', textTransform: 'uppercase' }}>
            Iran Risk Tracker
          </span>
        </div>
        <span style={{ fontSize: 11, color: '#555', letterSpacing: 1 }}>
          {formatTimestamp(lastRefresh.toISOString())}
        </span>
      </div>

      {/* Main content */}
      <div style={{ width: '100%', maxWidth: 820, padding: '40px 24px', boxSizing: 'border-box' }}>

        {/* Hero section */}
        <div style={{ textAlign: 'center', marginBottom: 48 }}>
          <div style={{ fontSize: 11, letterSpacing: 6, color: '#660000', fontWeight: 700, marginBottom: 12, textTransform: 'uppercase' }}>
            Intelligence Assessment — Islamic Republic of Iran
          </div>
          <h1 style={{
            fontSize: 22,
            fontWeight: 900,
            letterSpacing: 4,
            color: '#ffffff',
            margin: '0 0 4px',
            textTransform: 'uppercase',
          }}>
            Regime Collapse
          </h1>
          <h2 style={{
            fontSize: 14,
            fontWeight: 400,
            letterSpacing: 6,
            color: '#660000',
            margin: '0 0 40px',
            textTransform: 'uppercase',
          }}>
            Probability Index
          </h2>

          {loading && !data && (
            <div style={{ color: '#555', fontSize: 13, letterSpacing: 2 }}>LOADING INTELLIGENCE DATA…</div>
          )}
          {err && (
            <div style={{ color: '#cc0000', fontSize: 13, background: '#1a0000', padding: '12px 20px', borderRadius: 4, border: '1px solid #3a0000' }}>
              ERROR: {err}
            </div>
          )}

          {data && (
            <>
              <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 24 }}>
                <ScoreRing percent={pct} level={data.level} />
              </div>

              <div style={{ marginBottom: 8, fontSize: 13, fontWeight: 700, letterSpacing: 3, color: trendColor(data.scoreTrend) }}>
                {formatScoreTrend(data.scoreTrend)}
                {data.scoreTrend !== 'NoPreviousSnapshot' && data.scoreTrend !== 'Unchanged' && (
                  <span style={{ marginLeft: 10, fontFamily: 'monospace', color: '#ff4444' }}>
                    {formatScoreChange(data.scoreChange)}
                  </span>
                )}
              </div>

              <div style={{ fontSize: 12, color: '#444', letterSpacing: 1 }}>
                Raw score: {data.score?.toFixed(4)}
                {data.previousScore != null && (
                  <span style={{ marginLeft: 16 }}>prev: {data.previousScore.toFixed(4)}</span>
                )}
              </div>
            </>
          )}
        </div>

        {/* Divider */}
        {data && (
          <div style={{ borderTop: '1px solid #1a0000', marginBottom: 36 }} />
        )}

        {/* Indicators */}
        {data && data.topContributors.length > 0 && (
          <div>
            <div style={{ fontSize: 10, letterSpacing: 5, color: '#440000', fontWeight: 700, marginBottom: 24, textTransform: 'uppercase' }}>
              Threat Indicators
            </div>
            {data.topContributors.map(t => (
              <IndicatorBar
                key={t.indicatorKey}
                name={`${t.indicatorName} · ${formatEventCategory(t.category)}`}
                contribution={t.weightedContribution}
                maxContribution={maxContrib}
              />
            ))}
          </div>
        )}

        {/* Methodology note */}
        {data && (
          <>
            <div style={{ borderTop: '1px solid #1a0000', margin: '36px 0 24px' }} />
            <div style={{ fontSize: 10, color: '#333', lineHeight: 1.8, letterSpacing: 0.5 }}>
              <span style={{ color: '#440000', fontWeight: 700, letterSpacing: 2, textTransform: 'uppercase' }}>Methodology</span>
              &nbsp;·&nbsp;
              Deterministic weighted indicator model. 8 indicators across military, nuclear, economic, civil, and cyber domains.
              Baseline calibrated against {data.summary?.match(/(\d+) historical/)?.[1] ?? '53'} verified historical events (2000–2026).
              Score range 1–100. Critical threshold: ≥75.
            </div>
            <div style={{ marginTop: 20, display: 'flex', justifyContent: 'center' }}>
              <button onClick={fetchDashboard} disabled={loading} style={{
                background: 'transparent',
                border: '1px solid #2a0000',
                color: loading ? '#333' : '#660000',
                padding: '8px 24px',
                fontSize: 10,
                letterSpacing: 3,
                cursor: loading ? 'default' : 'pointer',
                textTransform: 'uppercase',
                fontWeight: 700,
                borderRadius: 2,
                transition: 'all 0.2s',
              }}>
                {loading ? 'REFRESHING…' : '↻ REFRESH'}
              </button>
            </div>
          </>
        )}
      </div>

      <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 1; box-shadow: 0 0 8px #cc0000; }
          50% { opacity: 0.4; box-shadow: 0 0 3px #cc0000; }
        }
        * { box-sizing: border-box; }
        body { margin: 0; background: #050505; }
      `}</style>
    </div>
  )
}
