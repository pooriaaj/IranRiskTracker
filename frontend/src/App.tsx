import React, { useEffect, useState, useCallback } from 'react'
import { getDashboardSummary, getLiveEvents, postLiveEvent } from './client'
import type { DashboardSummaryDto, LiveEventDto, LiveEventCreateRequest } from './types'

// ── Lookup tables ────────────────────────────────────────────────────────────

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
const URGENCY_LABELS: Record<number, string> = {
  0: 'Low', 1: 'Medium', 2: 'High', 3: 'Critical',
}
const URGENCY_COLORS: Record<number, string> = {
  0: '#6b7280', 1: '#eab308', 2: '#f97316', 3: '#ef4444',
}

// ── Helpers ──────────────────────────────────────────────────────────────────

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
function formatUrgency(urgency: number | string): string {
  if (typeof urgency === 'number') return URGENCY_LABELS[urgency] ?? 'Unknown'
  return urgency
}
function urgencyColor(urgency: number | string): string {
  if (typeof urgency === 'number') return URGENCY_COLORS[urgency] ?? '#6b7280'
  return '#6b7280'
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
function toLocalDateTimeInput(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

// ── Score ring ───────────────────────────────────────────────────────────────

function ScoreRing({ percent, level }: { percent: number; level: number | string }) {
  const r = 66, cx = 88, cy = 88, size = 176
  const circ = 2 * Math.PI * r
  const offset = circ * (1 - Math.min(percent, 100) / 100)
  const color = riskLevelColor(level)
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} aria-label={`Risk score ${percent}%`}>
      <circle cx={cx} cy={cy} r={r} fill="none" stroke="#1b2a42" strokeWidth="13" />
      <circle cx={cx} cy={cy} r={r} fill="none" stroke={color} strokeWidth="13"
        strokeLinecap="round" strokeDasharray={circ} strokeDashoffset={offset}
        transform={`rotate(-90 ${cx} ${cy})`}
        style={{ transition: 'stroke-dashoffset 0.6s ease' }} />
      <text x={cx} y={cy - 8} textAnchor="middle" dominantBaseline="middle"
        fill="#e6eef6" fontSize="30" fontWeight="800" fontFamily="Inter, Segoe UI, sans-serif">
        {percent}%
      </text>
      <text x={cx} y={cy + 20} textAnchor="middle" dominantBaseline="middle"
        fill={color} fontSize="11" fontWeight="700" fontFamily="Inter, Segoe UI, sans-serif" letterSpacing="1.5">
        {formatRiskLevel(level).toUpperCase()}
      </text>
    </svg>
  )
}

// ── Add event form ───────────────────────────────────────────────────────────

const EMPTY_FORM: LiveEventCreateRequest = {
  title: '',
  rawContent: '',
  sourceName: '',
  sourceUrl: '',
  ownerNotes: '',
  occurredAt: toLocalDateTimeInput(new Date()),
  urgency: 1,
  category: 6,
}

interface AddEventFormProps {
  onAdded: () => void
}

function AddEventForm({ onAdded }: AddEventFormProps) {
  const [form, setForm] = useState<LiveEventCreateRequest>(EMPTY_FORM)
  const [submitting, setSubmitting] = useState(false)
  const [err, setErr] = useState<string | null>(null)

  function set(field: keyof LiveEventCreateRequest, value: string | number) {
    setForm(f => ({ ...f, [field]: value }))
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    setSubmitting(true)
    try {
      await postLiveEvent({
        ...form,
        occurredAt: new Date(form.occurredAt).toISOString(),
      })
      setForm({ ...EMPTY_FORM, occurredAt: toLocalDateTimeInput(new Date()) })
      onAdded()
    } catch (e) {
      setErr(String(e))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="add-event-form" onSubmit={handleSubmit}>
      <div className="form-row">
        <input className="form-input" placeholder="Title *" required
          value={form.title} onChange={e => set('title', e.target.value)} />
        <input className="form-input" placeholder="Source name"
          value={form.sourceName} onChange={e => set('sourceName', e.target.value)} />
      </div>
      <div className="form-row">
        <select className="form-select" value={form.category}
          onChange={e => set('category', Number(e.target.value))}>
          {Object.entries(EVENT_CATEGORY_LABELS).filter(([k]) => k !== '0').map(([k, v]) => (
            <option key={k} value={k}>{v}</option>
          ))}
        </select>
        <select className="form-select" value={form.urgency}
          onChange={e => set('urgency', Number(e.target.value))}>
          {Object.entries(URGENCY_LABELS).map(([k, v]) => (
            <option key={k} value={k}>{v}</option>
          ))}
        </select>
        <input className="form-input" type="datetime-local"
          value={form.occurredAt} onChange={e => set('occurredAt', e.target.value)} />
      </div>
      <div className="form-row">
        <input className="form-input" placeholder="Notes (optional)"
          value={form.ownerNotes} onChange={e => set('ownerNotes', e.target.value)} />
      </div>
      {err && <div className="form-error">{err}</div>}
      <button className="form-submit" type="submit" disabled={submitting}>
        {submitting ? 'Adding…' : '+ Add Event'}
      </button>
    </form>
  )
}

// ── Main app ─────────────────────────────────────────────────────────────────

export default function App() {
  const [data, setData] = useState<DashboardSummaryDto | null>(null)
  const [loadingDash, setLoadingDash] = useState(true)
  const [dashErr, setDashErr] = useState<string | null>(null)

  const [events, setEvents] = useState<LiveEventDto[]>([])
  const [loadingEvents, setLoadingEvents] = useState(true)

  const fetchDashboard = useCallback(() => {
    setLoadingDash(true)
    setDashErr(null)
    getDashboardSummary()
      .then(setData)
      .catch(e => setDashErr(String(e)))
      .finally(() => setLoadingDash(false))
  }, [])

  const fetchEvents = useCallback(() => {
    setLoadingEvents(true)
    getLiveEvents()
      .then(setEvents)
      .catch(() => setEvents([]))
      .finally(() => setLoadingEvents(false))
  }, [])

  useEffect(() => {
    fetchDashboard()
    fetchEvents()
  }, [fetchDashboard, fetchEvents])

  function handleEventAdded() {
    fetchEvents()
    fetchDashboard()
  }

  return (
    <div className="app">
      <header className="header">Iran Risk Tracker</header>

      <main className="card">
        {loadingDash && <div className="muted">Loading…</div>}
        {dashErr && <div className="error">Error: {dashErr}</div>}
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
              <button className="refresh-btn" onClick={fetchDashboard} disabled={loadingDash}>
                {loadingDash ? '↻' : '↻ Refresh'}
              </button>
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
                          {t.indicatorKey}
                          <span className="cat-badge">{formatEventCategory(t.category)}</span>
                        </div>
                      </div>
                      <div className="contrib-weight">{formatScoreChange(t.weightedContribution)}</div>
                    </div>
                    {t.explanation && <div className="contrib-explain">{t.explanation}</div>}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        )}
      </main>

      <section className="card live-section">
        <div className="live-header">
          <h2 className="live-title">Live Events</h2>
          <span className="live-count">{loadingEvents ? '…' : `${events.length} active`}</span>
        </div>

        <AddEventForm onAdded={handleEventAdded} />

        {events.length > 0 && (
          <ul className="live-list">
            {events.map(ev => (
              <li key={ev.id} className="live-item">
                <div className="live-item-row">
                  <span className="live-item-title">{ev.title || '(no title)'}</span>
                  <span className="urgency-badge" style={{ color: urgencyColor(ev.urgency) }}>
                    {formatUrgency(ev.urgency)}
                  </span>
                </div>
                <div className="live-item-meta">
                  <span className="cat-badge">{formatEventCategory(ev.category)}</span>
                  {ev.sourceName && <span className="live-source">{ev.sourceName}</span>}
                  <span className="live-time">{formatTimestamp(ev.occurredAt)}</span>
                </div>
              </li>
            ))}
          </ul>
        )}
        {!loadingEvents && events.length === 0 && (
          <div className="muted" style={{ marginTop: 8 }}>No live events yet. Add one above to affect the score.</div>
        )}
      </section>
    </div>
  )
}
