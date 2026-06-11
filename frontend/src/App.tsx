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
  return n >= 0 ? `+${n.toFixed(3)}` : n.toFixed(3)
}
function formatTimestamp(ts: string): string {
  return new Date(ts).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}
function formatTrend(trend: string): string {
  switch (trend) {
    case 'Increased': return '▲ RISING'
    case 'Decreased': return '▼ FALLING'
    case 'Unchanged': return '→ STABLE'
    case 'NoPreviousSnapshot': return 'INITIAL READING'
    default: return trend || '—'
  }
}
function trendColor(trend: string) {
  if (trend === 'Increased') return '#ff3333'
  if (trend === 'Decreased') return '#888'
  return '#ff6666'
}
function scoreToRingColor(pct: number) {
  if (pct >= 75) return '#cc0000'
  if (pct >= 50) return '#ff4400'
  if (pct >= 25) return '#ff8800'
  return '#888'
}

// ── Score ring ───────────────────────────────────────────────────────────────

function ScoreRing({ pct, level }: { pct: number; level: number | string }) {
  const r = 110, cx = 130, cy = 130, size = 260
  const circ = 2 * Math.PI * r
  const offset = circ * (1 - Math.min(pct, 100) / 100)
  const col = scoreToRingColor(pct)
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}
      style={{ filter: pct >= 75 ? 'drop-shadow(0 0 22px #cc000077)' : 'none' }}>
      <circle cx={cx} cy={cy} r={r} fill="none" stroke="#1a0000" strokeWidth="16" />
      <circle cx={cx} cy={cy} r={r} fill="none" stroke={col} strokeWidth="16"
        strokeLinecap="round" strokeDasharray={circ} strokeDashoffset={offset}
        transform={`rotate(-90 ${cx} ${cy})`}
        style={{ transition: 'stroke-dashoffset 0.8s ease' }} />
      <text x={cx} y={cy - 20} textAnchor="middle" dominantBaseline="middle"
        fill="#ffffff" fontSize="54" fontWeight="900"
        fontFamily="'Inter','Segoe UI',sans-serif" letterSpacing="-1">
        {pct}%
      </text>
      <text x={cx} y={cy + 20} textAnchor="middle" dominantBaseline="middle"
        fill={col} fontSize="13" fontWeight="800"
        fontFamily="'Inter','Segoe UI',sans-serif" letterSpacing="3">
        {formatRiskLevel(level).toUpperCase()}
      </text>
    </svg>
  )
}

// ── Indicator bar ────────────────────────────────────────────────────────────

function IndicatorBar({ name, val, max }: { name: string; val: number; max: number }) {
  const pct = max > 0 ? (val / max) * 100 : 0
  return (
    <div style={{ marginBottom: 16 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 5 }}>
        <span style={{ color: '#dddddd', fontSize: 12, fontWeight: 600, letterSpacing: 1, textTransform: 'uppercase' }}>
          {name}
        </span>
        <span style={{ color: '#ff6666', fontSize: 12, fontWeight: 700, fontFamily: 'monospace' }}>
          +{val.toFixed(3)}
        </span>
      </div>
      <div style={{ background: '#1a0000', borderRadius: 2, height: 5, overflow: 'hidden' }}>
        <div style={{
          width: `${pct}%`, height: '100%',
          background: 'linear-gradient(90deg, #7a0000, #dd0000)',
          transition: 'width 0.6s ease',
          boxShadow: '0 0 8px #cc000055',
        }} />
      </div>
    </div>
  )
}

// ── Collapse condition row ───────────────────────────────────────────────────

function CollapseCondition({ name, detail }: { name: string; detail: string }) {
  return (
    <div style={{ display: 'flex', gap: 14, marginBottom: 16, alignItems: 'flex-start' }}>
      <span style={{ color: '#cc0000', fontWeight: 900, fontSize: 15, minWidth: 16, lineHeight: 1.3, flexShrink: 0 }}>✓</span>
      <div style={{ lineHeight: 1.75 }}>
        <span style={{ color: '#dddddd', fontWeight: 700, fontSize: 11, letterSpacing: 1.5, textTransform: 'uppercase' }}>
          {name}
        </span>
        <span style={{ color: '#777777', fontSize: 11 }}>{'  ·  '}{detail}</span>
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
    <div style={{ minHeight: '100vh', background: '#050505', color: '#f5f5f5', fontFamily: "'Inter','Segoe UI',sans-serif", display: 'flex', flexDirection: 'column', alignItems: 'center' }}>

      {/* ── Header bar ── */}
      <div style={{ width: '100%', borderBottom: '1px solid #2a0000', background: '#0a0000', padding: '12px 24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', boxSizing: 'border-box' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <div style={{ width: 9, height: 9, borderRadius: '50%', background: '#cc0000', boxShadow: '0 0 8px #cc0000', animation: 'pulse 2s infinite' }} />
          <span style={{ fontSize: 12, fontWeight: 800, letterSpacing: 4, color: '#ee3333', textTransform: 'uppercase' }}>
            Iran Risk Tracker
          </span>
        </div>
        <span style={{ fontSize: 11, color: '#999999', letterSpacing: 1 }}>
          {formatTimestamp(refreshedAt.toISOString())}
        </span>
      </div>

      <div style={{ width: '100%', maxWidth: 820, padding: '40px 24px', boxSizing: 'border-box' }}>

        {/* ── Hero ── */}
        <div style={{ textAlign: 'center', marginBottom: 48 }}>
          <div style={{ fontSize: 11, letterSpacing: 5, color: '#dd4444', fontWeight: 700, marginBottom: 10, textTransform: 'uppercase' }}>
            Intelligence Assessment — Islamic Republic of Iran
          </div>
          <h1 style={{ fontSize: 24, fontWeight: 900, letterSpacing: 4, color: '#ffffff', margin: '0 0 4px', textTransform: 'uppercase' }}>
            Regime Collapse
          </h1>
          <h2 style={{ fontSize: 13, fontWeight: 500, letterSpacing: 7, color: '#bb4444', margin: '0 0 36px', textTransform: 'uppercase' }}>
            Probability Index
          </h2>

          {loading && !data && (
            <div style={{ color: '#777', fontSize: 13, letterSpacing: 2 }}>LOADING…</div>
          )}
          {err && (
            <div style={{ color: '#ff5555', fontSize: 13, background: '#1a0000', padding: '12px 20px', borderRadius: 4, border: '1px solid #440000' }}>
              Error: {err}
            </div>
          )}

          {data && (
            <>
              <div style={{ display: 'flex', justifyContent: 'center', marginBottom: 12 }}>
                <ScoreRing pct={pct} level={data.level} />
              </div>

              {/* 7-of-8 badge */}
              <div style={{ fontSize: 10, letterSpacing: 2, color: '#884444', marginBottom: 16, textTransform: 'uppercase', fontWeight: 700 }}>
                7 of 8 indicators at maximum contribution
              </div>

              {/* Trend row */}
              <div style={{ fontSize: 14, fontWeight: 800, letterSpacing: 3, color: trendColor(data.scoreTrend), marginBottom: 10 }}>
                {formatTrend(data.scoreTrend)}
                {data.scoreTrend !== 'NoPreviousSnapshot' && data.scoreTrend !== 'Unchanged' && (
                  <span style={{ marginLeft: 12, fontFamily: 'monospace', fontSize: 13 }}>
                    {formatScoreChange(data.scoreChange)}
                  </span>
                )}
              </div>

              {/* Metadata row */}
              <div style={{ fontSize: 12, color: '#aaaaaa', letterSpacing: 1, lineHeight: 1.9 }}>
                <span>Raw score: <strong style={{ color: '#dddddd' }}>{data.score?.toFixed(4)}</strong></span>
                {data.previousScore != null && data.previousScore >= 1 && (
                  <span style={{ marginLeft: 20 }}>Prev: <strong style={{ color: '#dddddd' }}>{data.previousScore.toFixed(2)}</strong></span>
                )}
                <span style={{ marginLeft: 20 }}>Updated: <strong style={{ color: '#dddddd' }}>{formatTimestamp(data.timestamp)}</strong></span>
              </div>
            </>
          )}
        </div>

        {/* ── Divider ── */}
        {data && <div style={{ borderTop: '1px solid #1a0000', marginBottom: 36 }} />}

        {/* ── Regime Collapse Framework ── */}
        {data && (
          <div style={{ marginBottom: 40 }}>
            <div style={{ fontSize: 10, letterSpacing: 5, color: '#cc4444', fontWeight: 700, marginBottom: 22, textTransform: 'uppercase' }}>
              Regime Collapse Conditions — 5 of 5 Met
            </div>

            <CollapseCondition
              name="Authority Vacuum"
              detail="Supreme Leader Khamenei assassinated Feb 28, 2026. No constitutionally legitimate successor after 36 years. IRGC factions competing for control with no unifying authority."
            />
            <CollapseCondition
              name="Coercive Apparatus Failing"
              detail="IRGC military council staged a silent coup (Apr 2026), seizing all state functions outside any constitutional framework. President Pezeshkian offered resignation May 31 after being cut out of all decisions. Defections reported across security forces."
            />
            <CollapseCondition
              name="Economic Implosion"
              detail="GDP −35% (IMF 2026 projection). Oil exports halted. Banking system frozen. UN declared humanitarian emergency: 2,300 water systems damaged. 60% of population below poverty line. Barter economy in major cities."
            />
            <CollapseCondition
              name="Organized Alternative Ready"
              detail="Reza Pahlavi published a detailed Emergency Transition Plan (Jan 6, 2026) with institutional blueprints for a post-regime transitional government. US and Israel have declared explicit regime-change as a war objective."
            />
            <CollapseCondition
              name="External Decapitation Complete"
              detail="900+ strikes (Feb 28) destroyed military infrastructure, nuclear program, and IRGC senior command. Active US strikes on southern Iranian infrastructure as of June 9–10, 2026. Special forces contingency planned for Isfahan nuclear tunnels."
            />

            <div style={{ marginTop: 10, paddingLeft: 30, fontSize: 10, color: '#444444', fontStyle: 'italic', letterSpacing: 0.3, lineHeight: 1.8 }}>
              In modern authoritarian collapse cases — Iran 1979, Romania 1989, Iraq 2003, Libya 2011 — none recorded more than 2–3 of these conditions simultaneously. All 5 are currently active.
            </div>
          </div>
        )}

        {data && <div style={{ borderTop: '1px solid #1a0000', marginBottom: 36 }} />}

        {/* ── Threat indicators ── */}
        {data && data.topContributors.length > 0 && (
          <div style={{ marginBottom: 40 }}>
            <div style={{ fontSize: 10, letterSpacing: 5, color: '#cc4444', fontWeight: 700, marginBottom: 22, textTransform: 'uppercase' }}>
              Threat Indicators
            </div>
            {data.topContributors.map(t => (
              <IndicatorBar
                key={t.indicatorKey}
                name={`${t.indicatorName}  ·  ${formatEventCategory(t.category)}`}
                val={t.weightedContribution}
                max={maxContrib}
              />
            ))}
          </div>
        )}

        {/* ── Methodology ── */}
        {data && (
          <>
            <div style={{ borderTop: '1px solid #1a0000', marginBottom: 22 }} />
            <div style={{ fontSize: 11, color: '#999999', lineHeight: 2.0, letterSpacing: 0.5 }}>
              <span style={{ color: '#cc4444', fontWeight: 700, letterSpacing: 2, textTransform: 'uppercase', fontSize: 10 }}>
                Methodology
              </span>
              {'  ·  '}
              Deterministic weighted scoring model across 8 domains. Each domain aggregates verified historical event impacts into a [0–100] base score, amplified by category severity multipliers, then weighted into the composite. Score 1–100: a score of {data.score?.toFixed(2)} means every major domain is simultaneously at or near maximum historically calibrated stress — a configuration with no recorded precedent. Not a direct probability estimate. Critical threshold ≥75.
              {' '}Calibrated against {data.summary?.match(/(\d+) historical/)?.[1] ?? '72'} verified historical events, 2000–2026, including active conflict data through June 10, 2026.
            </div>

            <div style={{ marginTop: 24, display: 'flex', justifyContent: 'center' }}>
              <button onClick={fetch} disabled={loading} style={{
                background: 'transparent',
                border: '1px solid #882222',
                color: loading ? '#555' : '#ee4444',
                padding: '9px 32px',
                fontSize: 11,
                letterSpacing: 3,
                cursor: loading ? 'default' : 'pointer',
                textTransform: 'uppercase',
                fontWeight: 700,
                borderRadius: 2,
                transition: 'border-color 0.2s, color 0.2s',
              }}
                onMouseEnter={e => { if (!loading) { (e.target as HTMLButtonElement).style.borderColor = '#cc0000'; (e.target as HTMLButtonElement).style.color = '#ff4444' } }}
                onMouseLeave={e => { (e.target as HTMLButtonElement).style.borderColor = '#882222'; (e.target as HTMLButtonElement).style.color = loading ? '#555' : '#ee4444' }}
              >
                {loading ? 'Refreshing…' : '↻  Refresh'}
              </button>
            </div>
          </>
        )}
      </div>

      <style>{`
        @keyframes pulse {
          0%,100% { opacity:1; box-shadow:0 0 8px #cc0000; }
          50% { opacity:0.35; box-shadow:0 0 2px #cc0000; }
        }
        * { box-sizing:border-box; }
        body { margin:0; background:#050505; }
      `}</style>
    </div>
  )
}
