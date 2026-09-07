export interface TrendPoint {
    date: string;
    before: number;
    after: number;
}

export interface FrequencyStat {
    frequency: string;
    sessionsCount: number;
    averageReduction: number;
}

export interface InsightsSummary {
    totalSessions: number;
    completedSessions: number;
    averageReduction: number | null;
    currentStreakDays: number;
}