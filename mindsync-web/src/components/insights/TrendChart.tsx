import { STRESS_LEVELS } from '../../constants/stressLevels';
import type { TrendPoint } from '../../types/insights';

interface TrendChartProps {
    points: TrendPoint[];
}

const CHART_WIDTH = 600;
const CHART_HEIGHT = 220;
const PADDING = { top: 16, right: 16, bottom: 28, left: 12 };
const MAX_LEVEL = STRESS_LEVELS.length - 1; // 8

export function TrendChart({ points }: TrendChartProps) {
    if (points.length === 0) {
        return (
            <p className="insights-empty">
                Ainda não há sessões avaliadas suficientes pra montar esse gráfico. Finalize algumas sessões e volte aqui.
            </p>
        );
    }

    const innerWidth = CHART_WIDTH - PADDING.left - PADDING.right;
    const innerHeight = CHART_HEIGHT - PADDING.top - PADDING.bottom;

    const xFor = (index: number) =>
        points.length === 1
            ? PADDING.left + innerWidth / 2
            : PADDING.left + (index / (points.length - 1)) * innerWidth;
    const yFor = (level: number) => PADDING.top + innerHeight - (level / MAX_LEVEL) * innerHeight;

    const buildPath = (getValue: (point: TrendPoint) => number) =>
        points.map((point, index) => `${index === 0 ? 'M' : 'L'} ${xFor(index)} ${yFor(getValue(point))}`).join(' ');

    const labelStep = Math.max(1, Math.ceil(points.length / 5));

    return (
        <div className="insights-chart-wrapper">
            <svg viewBox={`0 0 ${CHART_WIDTH} ${CHART_HEIGHT}`} className="insights-chart" preserveAspectRatio="none">
                {[0, 2, 4, 6, 8].map((level) => (
                    <line
                        key={level}
                        x1={PADDING.left}
                        x2={CHART_WIDTH - PADDING.right}
                        y1={yFor(level)}
                        y2={yFor(level)}
                        className="insights-chart-gridline"
                    />
                ))}

                <path d={buildPath((p) => p.after)} className="insights-chart-line insights-chart-line--after" />
                <path d={buildPath((p) => p.before)} className="insights-chart-line insights-chart-line--before" />

                {points.map((point, index) => (
                    <circle
                        key={`before-${index}`}
                        cx={xFor(index)}
                        cy={yFor(point.before)}
                        r={3}
                        className="insights-chart-dot insights-chart-dot--before"
                    />
                ))}
                {points.map((point, index) => (
                    <circle
                        key={`after-${index}`}
                        cx={xFor(index)}
                        cy={yFor(point.after)}
                        r={3}
                        className="insights-chart-dot insights-chart-dot--after"
                    />
                ))}

                {points.map((point, index) =>
                    index % labelStep === 0 ? (
                        <text
                            key={`label-${index}`}
                            x={xFor(index)}
                            y={CHART_HEIGHT - 8}
                            className="insights-chart-axis-label"
                            textAnchor="middle"
                        >
                            {formatShortDate(point.date)}
                        </text>
                    ) : null
                )}
            </svg>

            <div className="insights-chart-legend">
                <span className="insights-chart-legend-item">
                    <span className="insights-chart-legend-dot insights-chart-legend-dot--before" /> Antes
                </span>
                <span className="insights-chart-legend-item">
                    <span className="insights-chart-legend-dot insights-chart-legend-dot--after" /> Depois
                </span>
            </div>
        </div>
    );
}

function formatShortDate(iso: string): string {
    return new Date(iso).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
}