import type { StressSession } from '../types/session';
import type { FrequencyStat, InsightsSummary, TrendPoint } from '../types/insights';
import { getLocalDateString } from './date';

function isCompleted(session: StressSession): boolean {
    return session.stressLevelAfter !== null && session.stressLevelAfter !== undefined;
}

function reductionOf(session: StressSession): number {
    return session.stressLevelBefore - session.stressLevelAfter!;
}

export function computeTrendSeries(sessions: StressSession[]): TrendPoint[] {
    return sessions
        .filter(isCompleted)
        .filter((session) => !!session.createdAt)
        .slice()
        .sort((a, b) => new Date(a.createdAt!).getTime() - new Date(b.createdAt!).getTime())
        .map((session) => ({
            date: session.createdAt!,
            before: session.stressLevelBefore,
            after: session.stressLevelAfter!,
        }));
}

export function computeFrequencyBreakdown(sessions: StressSession[]): FrequencyStat[] {
    const groups = new Map<string, { total: number; count: number }>();

    sessions.filter(isCompleted).forEach((session) => {
        const current = groups.get(session.targetFrequency) ?? { total: 0, count: 0 };
        groups.set(session.targetFrequency, {
            total: current.total + reductionOf(session),
            count: current.count + 1,
        });
    });

    return Array.from(groups.entries())
        .map(([frequency, { total, count }]) => ({
            frequency,
            sessionsCount: count,
            averageReduction: total / count,
        }))
        .sort((a, b) => b.averageReduction - a.averageReduction);
}

export function computeSummary(sessions: StressSession[]): InsightsSummary {
    const completed = sessions.filter(isCompleted);

    const averageReduction =
        completed.length > 0
            ? completed.reduce((sum, session) => sum + reductionOf(session), 0) / completed.length
            : null;

    return {
        totalSessions: sessions.length,
        completedSessions: completed.length,
        averageReduction,
        currentStreakDays: computeCurrentStreakDays(sessions),
    };
}

function computeCurrentStreakDays(sessions: StressSession[]): number {
    const distinctDates = Array.from(
        new Set(sessions.filter((session) => !!session.createdAt).map((session) => getLocalDateString(session.createdAt)))
    ).sort((a, b) => (a < b ? 1 : -1)); // mais recente primeiro

    if (distinctDates.length === 0) return 0;

    const today = getLocalDateString(new Date().toISOString());
    const yesterday = getLocalDateString(new Date(Date.now() - 86_400_000).toISOString());

    if (distinctDates[0] !== today && distinctDates[0] !== yesterday) return 0;

    let streak = 1;
    for (let i = 0; i < distinctDates.length - 1; i++) {
        const current = new Date(distinctDates[i]).getTime();
        const next = new Date(distinctDates[i + 1]).getTime();
        const diffDays = Math.round((current - next) / 86_400_000);

        if (diffDays === 1) {
            streak++;
        } else {
            break;
        }
    }

    return streak;
}