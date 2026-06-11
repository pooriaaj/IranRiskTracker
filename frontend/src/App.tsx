import React, { useEffect, useState, useCallback } from 'react'
import { getDashboardSummary } from './client'
import type { DashboardSummaryDto } from './types'

const RISK_LEVEL_LABELS: Record<number, string> = {
  0: 'Unknown', 1: 'Low', 2: 'Medium', 3: 'High', 4: 'Critical',
}
const EVENT_CATEGORY_LABELS: Record<number, string> = {
  0: 'Unknown', 1: 'Protests', 2: 'Executions', 3: 'Nuclear',
  4: 'Maritime', 5: 'Cyber', 6: 'Military', 7: 'Political', 8: 'Economic',
}

function formatRiskLevel(level: number | string): string {
  if (typeof level === 'number') return RISK_LEVEL_LABELS[level] ?? 'Unknown'
  return level
}
function formatEventCategory(cat: number | string): string {
  if (typeof cat === 'number') return EVENT_CATEGORY_LABELS[cat] ?? 'Unknown'
  return cat
}
function formatScoreChange(n: number): string {
  return n >= 0 ? `+${n.toFixed(2)}` : n.toFixed(2)
}
function formatTimestamp(ts: string): string {
  return new Date(ts).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}
function formatTrend(trend: string): string {
  switch (trend) {
    case 'Increased': return '▲ RISING'
    case 'Decreased': return '▼ FALLING'
    case 'Unchanged': return '— STABLE'
    case 'NoPreviousSnapshot': return 'LIVE READING'
    default: return trend || '—'
  }
}
function trendColor(trend: string) {
  if (trend === 'Increased') return '#ff4444'
  if (trend === 'Decreased') return '#5599bb'
  return '#aaaaaa'
}

// ── Score ring ───────────────────────────────────────────────────────────────

function ScoreRing({ pct, level }: { pct: number; level: number | string }) {
  const r = 130, cx = 150, cy = 150, size = 300
  const circ = 2 * Math.PI * r
  const offset = circ * (1 - Math.min(pct, 100) / 100)
  const isCritical = pct >= 75
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}
      style={{ filter: isCritical ? 'drop-shadow(0 0 36px #cc000099)' : 'none' }}>
      <defs>
        <linearGradient id="ringGrad" x1="0%" y1="0%" x2="100%" y2="0%">
          <stop offset="0%" stopColor="#880000" />
          <stop offset="100%" stopColor="#ff2222" />
        </linearGradient>
      </defs>
      {/* Track */}
      <circle cx={cx} cy={cy} r={r} fill="none" stroke="#160000" strokeWidth="16" />
      {/* Progress */}
      <circle cx={cx} cy={cy} r={r} fill="none"
        stroke="url(#ringGrad)" strokeWidth="16"
        strokeLinecap="round"
        strokeDasharray={circ} strokeDashoffset={offset}
        transform={`rotate(-90 ${cx} ${cy})`}
        style={{ transition: 'stroke-dashoffset 0.9s ease' }} />
      {/* Score */}
      <text x={cx} y={cy - 14} textAnchor="middle" dominantBaseline="middle"
        fill="#ffffff" fontSize="68" fontWeight="900"
        fontFamily="'Inter','Segoe UI',sans-serif" letterSpacing="-3">
        {pct}%
      </text>
      {/* Level badge */}
      <rect x={cx - 44} y={cy + 20} width={88} height={22} rx={3} fill="#1a0000" />
      <text x={cx} y={cy + 31} textAnchor="middle" dominantBaseline="middle"
        fill="#ff3333" fontSize="11" fontWeight="800"
        fontFamily="'Inter','Segoe UI',sans-serif" letterSpacing="4">
        {formatRiskLevel(level).toUpperCase()}
      </text>
    </svg>
  )
}

// ── Section wrapper ──────────────────────────────────────────────────────────

function Section({ children, label }: { children: React.ReactNode; label: string }) {
  return (
    <div style={{
      background: '#080000',
      border: '1px solid #1e0000',
      borderLeft: '3px solid #660000',
      borderRadius: 4,
      padding: '28px 32px',
      marginBottom: 20,
    }}>
      <div style={{
        fontSize: 12, letterSpacing: 4, color: '#cc3333',
        fontWeight: 800, marginBottom: 28, textTransform: 'uppercase',
      }}>
        {label}
      </div>
      {children}
    </div>
  )
}

// ── Indicator bar ────────────────────────────────────────────────────────────

function IndicatorBar({ name, val, max }: { name: string; val: number; max: number }) {
  const pct = max > 0 ? (val / max) * 100 : 0
  return (
    <div style={{ marginBottom: 16 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 5 }}>
        <span style={{ color: '#bbbbbb', fontSize: 13, fontWeight: 700, letterSpacing: 1, textTransform: 'uppercase' }}>
          {name}
        </span>
        <span style={{ color: '#ff5555', fontSize: 13, fontWeight: 700, fontFamily: 'monospace' }}>
          +{val.toFixed(3)}
        </span>
      </div>
      <div style={{ background: '#0e0000', borderRadius: 3, height: 7, overflow: 'hidden' }}>
        <div style={{
          width: `${pct}%`, height: '100%', borderRadius: 3,
          background: 'linear-gradient(90deg, #660000, #cc0000)',
          transition: 'width 0.7s ease',
        }} />
      </div>
    </div>
  )
}

// ── Collapse condition row ───────────────────────────────────────────────────

function CollapseCondition({ name, detail }: { name: string; detail: string }) {
  return (
    <div style={{ display: 'flex', gap: 14, marginBottom: 22, alignItems: 'flex-start' }}>
      <span style={{
        color: '#ff3333', fontWeight: 900, fontSize: 13,
        minWidth: 14, lineHeight: 1.6, flexShrink: 0, marginTop: 1,
      }}>✓</span>
      <div>
        <span style={{
          color: '#ffffff', fontWeight: 800, fontSize: 14,
          letterSpacing: 1.5, textTransform: 'uppercase',
        }}>
          {name}
        </span>
        {'  '}
        <span style={{ color: '#999999', fontSize: 13, lineHeight: 1.9 }}>{detail}</span>
      </div>
    </div>
  )
}

// ── App ──────────────────────────────────────────────────────────────────────

export default function App() {
  const [data, setData] = useState<DashboardSummaryDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [err, setErr] = useState<string | null>(null)
  const [refreshedAt, setRefreshedAt] = useState(new Date())

  const fetch = useCallback(() => {
    setLoading(true); setErr(null)
    getDashboardSummary()
      .then(d => { setData(d); setRefreshedAt(new Date()) })
      .catch(e => setErr(String(e)))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => { fetch() }, [fetch])

  const pct = data?.scorePercent ?? 0
  const maxContrib = data ? Math.max(...data.topContributors.map(t => t.weightedContribution), 0.01) : 1

  return (
    <div style={{
      minHeight: '100vh',
      background: '#000000',
      color: '#f5f5f5',
      fontFamily: "'Inter','Segoe UI',sans-serif",
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
    }}>

      {/* ── Header bar ── */}
      <div style={{
        width: '100%',
        borderBottom: '1px solid #1a0000',
        background: '#050000',
        padding: '12px 32px',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        boxSizing: 'border-box',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <div style={{
            width: 7, height: 7, borderRadius: '50%',
            background: '#dd0000', boxShadow: '0 0 8px #dd0000',
            animation: 'pulse 2s infinite',
          }} />
          <span style={{ fontSize: 11, fontWeight: 800, letterSpacing: 4, color: '#cc2222', textTransform: 'uppercase' }}>
            Iran Risk Tracker
          </span>
        </div>
        <span style={{ fontSize: 10, color: '#444444', letterSpacing: 1.5, textTransform: 'uppercase' }}>
          {formatTimestamp(refreshedAt.toISOString())}
        </span>
      </div>

      <div style={{ width: '100%', maxWidth: 1080, padding: '52px 48px 64px', boxSizing: 'border-box' }}>

        {/* ── Hero ── */}
        <div style={{ textAlign: 'center', marginBottom: 36, position: 'relative' }}>
          {/* Background glow */}
          <div style={{
            position: 'absolute', top: -40, left: '50%', transform: 'translateX(-50%)',
            width: 600, height: 320, pointerEvents: 'none',
            background: 'radial-gradient(ellipse at center, #22000040 0%, transparent 70%)',
          }} />

          <div style={{ fontSize: 12, letterSpacing: 6, color: '#882222', fontWeight: 700, marginBottom: 14, textTransform: 'uppercase' }}>
            Intelligence Assessment — Islamic Republic of Iran
          </div>
          <h1 style={{ fontSize: 52, fontWeight: 900, letterSpacing: 10, color: '#ffffff', margin: '0 0 6px', textTransform: 'uppercase' }}>
            Regime Collapse
          </h1>
          <h2 style={{ fontSize: 18, fontWeight: 300, letterSpacing: 12, color: '#882222', margin: '0 0 6px', textTransform: 'uppercase' }}>
            Probability Index
          </h2>
          <div style={{ fontSize: 12, letterSpacing: 4, color: '#776666', fontWeight: 600, marginBottom: 48, textTransform: 'uppercase' }}>
            12-Month Forward Assessment · June 2026
          </div>

          {loading && !data && (
            <div style={{ color: '#444', fontSize: 12, letterSpacing: 3, textTransform: 'uppercase' }}>Loading…</div>
          )}
          {err && (
            <div style={{ color: '#ff5555', fontSize: 12, background: '#0d0000', padding: '12px 20px', borderRadius: 4, border: '1px solid #2a0000' }}>
              Error: {err}
            </div>
          )}

          {data && (
            <>
              <div style={{ position: 'relative', display: 'inline-block', marginBottom: 20 }}>
                <div style={{
                  position: 'absolute', inset: -50, pointerEvents: 'none',
                  background: 'radial-gradient(ellipse at center, #33000044 0%, transparent 65%)',
                }} />
                <ScoreRing pct={pct} level={data.level} />
              </div>

              {/* Trend */}
              <div style={{
                fontSize: 12, fontWeight: 700, letterSpacing: 5,
                color: trendColor(data.scoreTrend), marginBottom: 14, textTransform: 'uppercase',
              }}>
                {formatTrend(data.scoreTrend)}
                {(data.scoreTrend === 'Increased' || data.scoreTrend === 'Decreased') && (
                  <span style={{ marginLeft: 10, fontFamily: 'monospace', fontSize: 11, opacity: 0.8 }}>
                    {formatScoreChange(data.scoreChange)}
                  </span>
                )}
              </div>

              {/* Meta row */}
              <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 20, flexWrap: 'wrap' }}>
                <span style={{ fontSize: 13, color: '#777777', letterSpacing: 1 }}>
                  Score <strong style={{ color: '#bbbbbb', fontFamily: 'monospace' }}>{data.score?.toFixed(2)}</strong>
                </span>
                <span style={{ color: '#440000', fontSize: 14 }}>·</span>
                <span style={{ fontSize: 12, letterSpacing: 2, color: '#aa5555', fontWeight: 700, textTransform: 'uppercase' }}>
                  6 of 8 indicators at max
                </span>
                <span style={{ color: '#440000', fontSize: 14 }}>·</span>
                <span style={{ fontSize: 13, color: '#777777' }}>
                  Updated <strong style={{ color: '#bbbbbb' }}>{formatTimestamp(data.timestamp)}</strong>
                </span>
              </div>
            </>
          )}
        </div>

        {/* ── Regime Collapse Framework ── */}
        {data && (
          <Section label="Regime Collapse Conditions — 5 of 5 Met">
            <CollapseCondition
              name="Authority Vacuum"
              detail="Khamenei assassinated Feb 28, 2026. Larijani (de facto successor) assassinated Mar 17. No constitutionally legitimate authority remains. IRGC factions competing with no unifying command."
            />
            <CollapseCondition
              name="Coercive Apparatus Failing"
              detail="IRGC military council staged a silent coup (Apr 2026), seizing all state functions. President Pezeshkian offered resignation May 31. Iran formally closed Strait of Hormuz Jun 11 — shoot-on-sight order for all vessels."
            />
            <CollapseCondition
              name="Economic Implosion"
              detail="GDP −35% (IMF 2026). Oil exports halted. Hormuz closure blocks 20% of global oil supply. Banking system frozen. 60% of Iranians below poverty line. Barter economy in major cities."
            />
            <CollapseCondition
              name="Organized Alternative Ready"
              detail="Reza Pahlavi published an Emergency Transition Plan (Jan 6, 2026) with full institutional blueprints for a post-regime government. US and Israel declared explicit regime-change as a war objective."
            />
            <CollapseCondition
              name="External Decapitation Complete"
              detail="900+ strikes (Feb 28) destroyed military infrastructure, nuclear program, and IRGC command. US fired 49 Tomahawks Jun 11. Trump canceled further strikes the same evening — signaling a deal is possible but contested."
            />
            <div style={{ marginTop: 8, paddingLeft: 28, fontSize: 12, color: '#666666', fontStyle: 'italic', lineHeight: 2.0 }}>
              In modern authoritarian collapse cases — Iran 1979, Romania 1989, Iraq 2003, Libya 2011 — none recorded more than 2–3 of these conditions simultaneously. All 5 are currently active.
            </div>
          </Section>
        )}

        {/* ── Score Trajectory ── */}
        {data && (
          <Section label="Score Trajectory — 2009 to Present">
            <div style={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              {[
                { year: '2009', label: 'Green Movement', pct: 28, current: false },
                { year: '2019', label: 'Max Pressure / Soleimani', pct: 53, current: false },
                { year: '2022', label: 'Mahsa Amini / JCPOA Collapse', pct: 67, current: false },
                { year: 'Jan 2026', label: 'January Massacre', pct: 76, current: false },
                { year: 'Jun 2026', label: 'Current — Active Conflict', pct: pct, current: true },
              ].map((row) => (
                <div key={row.year} style={{
                  display: 'flex', alignItems: 'center', gap: 14,
                  padding: row.current ? '9px 10px' : '5px 0',
                  background: row.current ? '#0d0000' : 'transparent',
                  borderRadius: row.current ? 3 : 0,
                  border: row.current ? '1px solid #2a0000' : '1px solid transparent',
                }}>
                  <span style={{
                    color: row.current ? '#cc3333' : '#888888',
                    fontSize: row.current ? 14 : 13, fontWeight: 700, letterSpacing: 0.5,
                    minWidth: 80, flexShrink: 0, textAlign: 'right',
                  }}>
                    {row.year}
                  </span>
                  <div style={{ flex: 1, position: 'relative', height: row.current ? 10 : 6, background: '#0e0000', borderRadius: 2 }}>
                    <div style={{
                      width: `${Math.min(row.pct, 100)}%`, height: '100%', borderRadius: 2,
                      background: row.current ? 'linear-gradient(90deg, #770000, #dd0000)' : '#442222',
                      boxShadow: row.current ? '0 0 14px #aa000055' : 'none',
                      transition: 'width 0.8s ease',
                    }} />
                  </div>
                  <span style={{
                    color: row.current ? '#ff4444' : '#888888',
                    fontSize: row.current ? 15 : 13,
                    fontFamily: 'monospace', fontWeight: 700, minWidth: 44, flexShrink: 0,
                  }}>
                    {row.pct}%
                  </span>
                  <span style={{
                    color: row.current ? '#ff4444' : '#888888',
                    fontSize: row.current ? 14 : 13,
                    letterSpacing: 0.3, minWidth: 200, flexShrink: 0,
                    fontWeight: row.current ? 700 : 400,
                  }}>
                    {row.label}
                  </span>
                </div>
              ))}
            </div>
            <div style={{ marginTop: 14, fontSize: 12, color: '#666666', fontStyle: 'italic', lineHeight: 1.9 }}>
              Historical values are approximate scores from the subset of events active at each date.
            </div>
          </Section>
        )}

        {/* ── Threat indicators ── */}
        {data && data.topContributors.length > 0 && (
          <Section label="Threat Indicators">
            {data.topContributors.map(t => (
              <IndicatorBar
                key={t.indicatorKey}
                name={`${t.indicatorName}  ·  ${formatEventCategory(t.category)}`}
                val={t.weightedContribution}
                max={maxContrib}
              />
            ))}
          </Section>
        )}

        {/* ── Methodology ── */}
        {data && (
          <div style={{ marginTop: 8, padding: '0 4px' }}>
            <div style={{ fontSize: 13, color: '#777777', lineHeight: 2.0, letterSpacing: 0.4 }}>
              <span style={{ color: '#aa4444', fontWeight: 700, letterSpacing: 3, textTransform: 'uppercase', fontSize: 12 }}>
                Methodology
              </span>
              {'  ·  '}
              Deterministic weighted scoring across 8 domains. Events include de-escalation signals (JCPOA 2015, sanctions relief 2016, April 2026 ceasefire). Score represents 12-month forward regime-collapse stress calibrated against {data.summary?.match(/(\d+) historical/)?.[1] ?? '79'} verified events, 2000–2026.
            </div>

            <div style={{ marginTop: 28, display: 'flex', justifyContent: 'center' }}>
              <button onClick={fetch} disabled={loading} style={{
                background: 'transparent',
                border: '1px solid #550000',
                color: loading ? '#2a2a2a' : '#882222',
                padding: '10px 40px',
                fontSize: 10, letterSpacing: 4,
                cursor: loading ? 'default' : 'pointer',
                textTransform: 'uppercase', fontWeight: 700, borderRadius: 2,
                transition: 'all 0.2s',
              }}
                onMouseEnter={e => { if (!loading) { (e.target as HTMLButtonElement).style.borderColor = '#aa0000'; (e.target as HTMLButtonElement).style.color = '#cc2222' } }}
                onMouseLeave={e => { (e.target as HTMLButtonElement).style.borderColor = '#550000'; (e.target as HTMLButtonElement).style.color = loading ? '#2a2a2a' : '#882222' }}
              >
                {loading ? 'Refreshing…' : '↻  Refresh'}
              </button>
            </div>
          </div>
        )}
      </div>

      <style>{`
        @keyframes pulse {
          0%,100% { opacity:1; box-shadow:0 0 8px #dd0000; }
          50% { opacity:0.25; box-shadow:0 0 2px #dd0000; }
        }
        * { box-sizing:border-box; margin:0; padding:0; }
        body { margin:0; background:#000000; }
      `}</style>
    </div>
  )
}
