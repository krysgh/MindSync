import type { FrequencyStat } from '../../types/insights';

interface FrequencyBreakdownProps {
    stats: FrequencyStat[];
}

export function FrequencyBreakdown({ stats }: FrequencyBreakdownProps) {
    if (stats.length === 0) {
        return <p className="insights-empty">Complete pelo menos uma sessão pra ver esse ranking.</p>;
    }

    const maxAbsReduction = Math.max(...stats.map((stat) => Math.abs(stat.averageReduction)), 1);

    return (
        <div className="frequency-breakdown">
            {stats.map((stat, index) => {
                const isBest = index === 0 && stat.averageReduction > 0;
                const widthPercent = Math.min(100, (Math.abs(stat.averageReduction) / maxAbsReduction) * 100);

                return (
                    <div key={stat.frequency} className={`frequency-row ${isBest ? 'frequency-row--best' : ''}`}>
                        <div className="frequency-row-header">
                            <span className="frequency-row-name">
                                {isBest && <span className="frequency-row-badge">Melhor pra você</span>}
                                {stat.frequency}
                            </span>
                            <span className="frequency-row-value">
                                {formatSigned(stat.averageReduction)} pts · {stat.sessionsCount}{' '}
                                {stat.sessionsCount === 1 ? 'sessão' : 'sessões'}
                            </span>
                        </div>
                        <div className="frequency-row-track">
                            <div
                                className={`frequency-row-fill ${stat.averageReduction < 0 ? 'frequency-row-fill--negative' : ''}`}
                                style={{ width: `${widthPercent}%` }}
                            />
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

function formatSigned(value: number): string {
    if (value > 0) return `-${value.toFixed(1)}`;
    if (value < 0) return `+${Math.abs(value).toFixed(1)}`;
    return '0.0';
}