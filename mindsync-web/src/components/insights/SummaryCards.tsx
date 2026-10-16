import type { InsightsSummary } from '../../types/insights';

interface SummaryCardsProps {
    summary: InsightsSummary;
}

export function SummaryCards({ summary }: SummaryCardsProps) {
    const reductionLabel =
        summary.averageReduction === null
            ? '—'
            : `${formatSigned(summary.averageReduction)} pts`;

    return (
        <div className="insights-summary-grid">
            <div className="insights-summary-card">
                <span className="insights-summary-value">{summary.totalSessions}</span>
                <span className="insights-summary-label">Sessões criadas</span>
            </div>

            <div className="insights-summary-card">
                <span className="insights-summary-value">{summary.completedSessions}</span>
                <span className="insights-summary-label">Sessões avaliadas</span>
            </div>

            <div className="insights-summary-card">
                <span
                    className={`insights-summary-value ${summary.averageReduction !== null
                            ? summary.averageReduction > 0
                                ? 'insights-summary-value--positive'
                                : summary.averageReduction < 0
                                    ? 'insights-summary-value--negative'
                                    : ''
                            : ''
                        }`}
                >
                    {reductionLabel}
                </span>
                <span className="insights-summary-label">Redução média de estresse</span>
            </div>

            <div className="insights-summary-card">
                <span className="insights-summary-value">{summary.currentStreakDays}</span>
                <span className="insights-summary-label">
                    {summary.currentStreakDays === 1 ? 'Dia seguido usando' : 'Dias seguidos usando'}
                </span>
            </div>
        </div>
    );
}

function formatSigned(value: number): string {
    if (value > 0) return `-${value.toFixed(1)}`;
    if (value < 0) return `+${Math.abs(value).toFixed(1)}`;
    return '0.0';
}