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
    case 'Unchanged': return '→ STABLE'
    case 'NoPreviousSnapshot': return 'LIVE READING'
    default: return trend || '—'
  }
}
function trendColor(trend: string) {
  if (trend === 'Increased') return '#ff3333'
  if (trend === 'Decreased') return '#66aacc'
  if (trend === 'NoPreviousSnapshot') return '#cc4444'
  return '#888888'
}
function scoreToRingColor(pct: number) {
  if (pct >= 75) return '#cc0000'
  if (pct >= 50) return '#ff4400'
  if (pct >= 25) return '#ff8800'
  return '#888'
}

// ── Score ring ───────────────────────────────────────────────────────────────

function ScoreRing({ pct, level }: { pct: number; level: number | string }) {
  const r = 118, cx = 140, cy = 140, size = 280
  const circ = 2 * Math.PI * r
  const offset = circ * (1 - Math.min(pct, 100) / 100)
  const col = scoreToRingColor(pct)
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}
      style={{ filter: pct >= 75 ? 'drop-shadow(0 0 28px #cc000088)' : 'none' }}>
      <circle cx={cx} cy={cy} r={r} fill="none" stroke="#1a0000" strokeWidth="14" />
      <circle cx={cx} cy={cy} r={r} fill="none" stroke={col} strokeWidth="14"
        strokeLinecap="round" strokeDasharray={circ} strokeDashoffset={offset}
        transform={`rotate(-90 ${cx} ${cy})`}
        style={{ transition: 'stroke-dashoffset 0.8s ease' }} />
      <text x={cx} y={cy - 18} textAnchor="middle" dominantBaseline="middle"
        fill="#ffffff" fontSize="60" fontWeight="900"
        fontFamily="'Inter','Segoe UI',sans-serif" letterSpacing="-2">
        {pct}%
      </text>
      <text x={cx} y={cy + 22} textAnchor="middle" dominantBaseline="middle"
        fill={col} fontSize="13" fontWeight="800"
        fontFamily="'Inter','Segoe UI',sans-serif" letterSpacing="4">
        {formatRiskLevel(level).toUpperCase()}
      </text>
    </svg>
  )
}

// ── Indicator bar ────────────────────────────────────────────────────────────

function IndicatorBar({ name, val, max }: { name: string; val: number; max: number }) {
  const pct = max > 0 ? (val / max) * 100 : 0
  return (
    <div style={{ marginBottom: 18 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
        <span style={{ color: '#cccccc', fontSize: 11, fontWeight: 700, letterSpacing: 1.5, textTransform: 'uppercase' }}>
          {name}
        </span>
        <span style={{ color: '#ff5555', fontSize: 11, fontWeight: 700, fontFamily: 'monospace' }}>
          +{val.toFixed(3)}
        </span>
      </div>
      <div style={{ background: '#150000', borderRadius: 3, height: 6, overflow: 'hidden' }}>
        <div style={{
          width: `${pct}%`, height: '100%', borderRadius: 3,
          background: 'linear-gradient(90deg, #880000, #dd0000)',
          transition: 'width 0.6s ease',
          boxShadow: '0 0 10px #cc000066',
        }} />
      </div>
    </div>
  )
}

// ── Collapse condition row ───────────────────────────────────────────────────

function CollapseCondition({ name, detail }: { name: string; detail: string }) {
  return (
    <div style={{ display: 'flex', gap: 14, marginBottom: 20, alignItems: 'flex-start' }}>
      <span style={{ color: '#cc0000', fontWeight: 900, fontSize: 14, minWidth: 16, lineHeight: 1.4, flexShrink: 0, marginTop: 1 }}>✓</span>
      <div style={{ lineHeight: 1.8 }}>
        <span style={{ color: '#eeeeee', fontWeight: 700, fontSize: 11, letterSpacing: 1.5, textTransform: 'uppercase' }}>
          {name}
        </span>
        <span style={{ color: '#888888', fontSize: 11 }}>{'  ·  '}{detail}</span>
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
      <div style={{ width: '100%', borderBottom: '1px solid #220000', background: '#080000', padding: '13px 28px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', boxSizing: 'border-box' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <div style={{ width: 8, height: 8, borderRadius: '50%', background: '#cc0000', boxShadow: '0 0 10px #cc0000', animation: 'pulse 2s infinite' }} />
          <span style={{ fontSize: 11, fontWeight: 800, letterSpacing: 5, color: '#dd3333', textTransform: 'uppercase' }}>
            Iran Risk Tracker
          </span>
        </div>
        <span style={{ fontSize: 10, color: '#555555', letterSpacing: 1.5, textTransform: 'uppercase' }}>
          {formatTimestamp(refreshedAt.toISOString())}
        </span>
      </div>

      <div style={{ width: '100%', maxWidth: 840, padding: '48px 28px 60px', boxSizing: 'border-box' }}>

        {/* ── Hero ── */}
        <div style={{ textAlign: 'center', marginBottom: 56 }}>
          <div style={{ fontSize: 10, letterSpacing: 6, color: '#cc3333', fontWeight: 700, marginBottom: 14, textTransform: 'uppercase' }}>
            Intelligence Assessment — Islamic Republic of Iran
          </div>
          <h1 style={{ fontSize: 30, fontWeight: 900, letterSpacing: 6, color: '#ffffff', margin: '0 0 6px', textTransform: 'uppercase' }}>
            Regime Collapse
          </h1>
          <h2 style={{ fontSize: 14, fontWeight: 400, letterSpacing: 8, color: '#aa3333', margin: '0 0 6px', textTransform: 'uppercase' }}>
            Probability Index
          </h2>
          <div style={{ fontSize: 10, letterSpacing: 4, color: '#553333', fontWeight: 600, marginBottom: 40, textTransform: 'uppercase' }}>
            12-Month Forward Assessment · June 2026
          </div>

          {loading && !data && (
            <div style={{ color: '#555', fontSize: 12, letterSpacing: 3, textTransform: 'uppercase' }}>Loading…</div>
          )}
          {err && (
            <div style={{ color: '#ff5555', fontSize: 12, background: '#110000', padding: '12px 20px', borderRadius: 4, border: '1px solid #330000' }}>
              Error: {err}
            </div>
          )}

          {data && (
            <>
              {/* Radial glow behind ring */}
              <div style={{ position: 'relative', display: 'inline-block', marginBottom: 16 }}>
                <div style={{
                  position: 'absolute', inset: -40,
                  background: 'radial-gradient(ellipse at center, #33000033 0%, transparent 70%)',
                  pointerEvents: 'none',
                }} />
                <ScoreRing pct={pct} level={data.level} />
              </div>

              {/* Trend */}
              <div style={{ fontSize: 13, fontWeight: 800, letterSpacing: 4, color: trendColor(data.scoreTrend), marginBottom: 10, textTransform: 'uppercase' }}>
                {formatTrend(data.scoreTrend)}
                {data.scoreTrend === 'Increased' && (
                  <span style={{ marginLeft: 10, fontFamily: 'monospace', fontSize: 12, opacity: 0.8 }}>
                    {formatScoreChange(data.scoreChange)}
                  </span>
                )}
                {data.scoreTrend === 'Decreased' && (
                  <span style={{ marginLeft: 10, fontFamily: 'monospace', fontSize: 12, opacity: 0.8 }}>
                    {formatScoreChange(data.scoreChange)}
                  </span>
                )}
              </div>

              {/* Score + badge row */}
              <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 24, marginBottom: 8 }}>
                <span style={{ fontSize: 11, color: '#666666', letterSpacing: 1.5, textTransform: 'uppercase' }}>
                  Score <strong style={{ color: '#cccccc', fontFamily: 'monospace' }}>{data.score?.toFixed(2)}</strong>
                </span>
                <span style={{ color: '#330000', fontSize: 11 }}>·</span>
                <span style={{ fontSize: 10, letterSpacing: 2, color: '#773333', fontWeight: 700, textTransform: 'uppercase' }}>
                  6 of 8 indicators at max
                </span>
                <span style={{ color: '#330000', fontSize: 11 }}>·</span>
                <span style={{ fontSize: 11, color: '#666666', letterSpacing: 1, textTransform: 'uppercase' }}>
                  Updated <strong style={{ color: '#aaaaaa' }}>{formatTimestamp(data.timestamp)}</strong>
                </span>
              </div>
            </>
          )}
        </div>

        {/* ── Divider ── */}
        {data && <div style={{ borderTop: '1px solid #220000', marginBottom: 40 }} />}

        {/* ── Regime Collapse Framework ── */}
        {data && (
          <div style={{ marginBottom: 44 }}>
            <div style={{ fontSize: 10, letterSpacing: 5, color: '#cc3333', fontWeight: 700, marginBottom: 26, textTransform: 'uppercase' }}>
              Regime Collapse Conditions — 5 of 5 Met
            </div>

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

            <div style={{ marginTop: 14, paddingLeft: 30, fontSize: 10, color: '#3a3a3a', fontStyle: 'italic', letterSpacing: 0.3, lineHeight: 2.0 }}>
              In modern authoritarian collapse cases — Iran 1979, Romania 1989, Iraq 2003, Libya 2011 — none recorded more than 2–3 of these conditions simultaneously. All 5 are currently active.
            </div>
          </div>
        )}

        {data && <div style={{ borderTop: '1px solid #220000', marginBottom: 40 }} />}

        {/* ── Score Trajectory ── */}
        {data && (
          <div style={{ marginBottom: 44 }}>
            <div style={{ fontSize: 10, letterSpacing: 5, color: '#cc3333', fontWeight: 700, marginBottom: 26, textTransform: 'uppercase' }}>
              Score Trajectory — 2009 to Present
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {[
                { year: '2009', label: 'Green Movement', pct: 28, level: 'MEDIUM', current: false },
                { year: '2019', label: 'Max Pressure / Soleimani', pct: 53, level: 'HIGH', current: false },
                { year: '2022', label: 'Mahsa Amini / JCPOA Collapse', pct: 67, level: 'HIGH', current: false },
                { year: 'Jan 2026', label: 'January Massacre', pct: 76, level: 'CRITICAL', current: false },
                { year: 'Jun 2026', label: 'Current — Active Conflict', pct: pct, level: 'CRITICAL', current: true },
              ].map((row) => (
                <div key={row.year} style={{
                  display: 'flex', alignItems: 'center', gap: 14,
                  padding: row.current ? '10px 12px' : '6px 0',
                  marginBottom: row.current ? 4 : 0,
                  background: row.current ? '#0d0000' : 'transparent',
                  borderRadius: row.current ? 4 : 0,
                  border: row.current ? '1px solid #2a0000' : '1px solid transparent',
                }}>
                  <span style={{
                    color: row.current ? '#cc4444' : '#444444',
                    fontSize: row.current ? 11 : 10,
                    fontWeight: 700, letterSpacing: 1,
                    minWidth: 68, flexShrink: 0, textAlign: 'right',
                  }}>
                    {row.year}
                  </span>
                  <div style={{ flex: 1, position: 'relative', height: row.current ? 8 : 5, background: '#0e0000', borderRadius: 3 }}>
                    <div style={{
                      width: `${Math.min(row.pct, 100)}%`, height: '100%', borderRadius: 3,
                      background: row.current ? 'linear-gradient(90deg, #880000, #dd0000)' : '#2d0f0f',
                      boxShadow: row.current ? '0 0 12px #cc000055' : 'none',
                      transition: 'width 0.8s ease',
                    }} />
                  </div>
                  <span style={{
                    color: row.current ? '#ff5555' : '#555555',
                    fontSize: row.current ? 12 : 10,
                    fontFamily: 'monospace', fontWeight: 700, minWidth: 38, flexShrink: 0,
                  }}>
                    {row.pct}%
                  </span>
                  <span style={{
                    color: row.current ? '#cc4444' : '#3a3a3a',
                    fontSize: row.current ? 11 : 10,
                    letterSpacing: 0.5, minWidth: 140, flexShrink: 0,
                    fontWeight: row.current ? 600 : 400,
                  }}>
                    {row.label}
                  </span>
                  <span style={{
                    color: row.current ? '#882222' : '#2a0a0a',
                    fontSize: 9, letterSpacing: 2, fontWeight: 700, textTransform: 'uppercase',
                    minWidth: 54, flexShrink: 0,
                  }}>
                    {row.level}
                  </span>
                </div>
              ))}
            </div>
            <div style={{ marginTop: 12, fontSize: 10, color: '#333333', fontStyle: 'italic', letterSpacing: 0.3, lineHeight: 1.9 }}>
              Historical values are approximate scores from the subset of events active at each date. Current score is live from the model.
            </div>
          </div>
        )}

        {data && <div style={{ borderTop: '1px solid #220000', marginBottom: 40 }} />}

        {/* ── Threat indicators ── */}
        {data && data.topContributors.length > 0 && (
          <div style={{ marginBottom: 44 }}>
            <div style={{ fontSize: 10, letterSpacing: 5, color: '#cc3333', fontWeight: 700, marginBottom: 26, textTransform: 'uppercase' }}>
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
            <div style={{ borderTop: '1px solid #220000', marginBottom: 24 }} />
            <div style={{ fontSize: 11, color: '#555555', lineHeight: 2.0, letterSpacing: 0.5 }}>
              <span style={{ color: '#cc3333', fontWeight: 700, letterSpacing: 3, textTransform: 'uppercase', fontSize: 10 }}>
                Methodology
              </span>
              {'  ·  '}
              Deterministic weighted scoring model across 8 domains. Each domain aggregates verified historical event impacts — including de-escalation events (JCPOA 2015, sanctions relief 2016, April 2026 ceasefire) — into a [0–100] base score, amplified by category severity multipliers, then weighted into the composite. Score represents 12-month forward regime-collapse stress. A score of {data.score?.toFixed(2)} means every major domain is simultaneously at or near maximum historically calibrated stress — a configuration with no recorded precedent. Not a direct probability estimate. Critical threshold ≥75.
              {' '}Calibrated against {data.summary?.match(/(\d+) historical/)?.[1] ?? '79'} verified events, 2000–2026, through June 11, 2026.
            </div>

            <div style={{ marginTop: 28, display: 'flex', justifyContent: 'center' }}>
              <button onClick={fetch} disabled={loading} style={{
                background: 'transparent',
                border: '1px solid #660000',
                color: loading ? '#333' : '#cc3333',
                padding: '10px 36px',
                fontSize: 10,
                letterSpacing: 4,
                cursor: loading ? 'default' : 'pointer',
                textTransform: 'uppercase',
                fontWeight: 700,
                borderRadius: 2,
                transition: 'border-color 0.2s, color 0.2s',
              }}
                onMouseEnter={e => { if (!loading) { (e.target as HTMLButtonElement).style.borderColor = '#cc0000'; (e.target as HTMLButtonElement).style.color = '#ff4444' } }}
                onMouseLeave={e => { (e.target as HTMLButtonElement).style.borderColor = '#660000'; (e.target as HTMLButtonElement).style.color = loading ? '#333' : '#cc3333' }}
              >
                {loading ? 'Refreshing…' : '↻  Refresh'}
              </button>
            </div>
          </>
        )}
      </div>

      <style>{`
        @keyframes pulse {
          0%,100% { opacity:1; box-shadow:0 0 10px #cc0000; }
          50% { opacity:0.3; box-shadow:0 0 3px #cc0000; }
        }
        * { box-sizing:border-box; }
        body { margin:0; background:#050505; }
      `}</style>
    </div>
  )
}
