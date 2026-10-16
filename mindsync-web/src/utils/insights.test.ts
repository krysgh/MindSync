import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { computeFrequencyBreakdown, computeSummary, computeTrendSeries } from './insights';
import type { StressSession } from '../types/session';

const ALPHA = 'Alpha (10 Hz) - Relaxamento';
const BETA = 'Beta (20 Hz) - Foco e Atenção';

let sequence = 0;
const make = (before: number, after: number | null | undefined, createdAt: string, frequency = ALPHA): StressSession => ({
    id: `s${++sequence}`,
    stressLevelBefore: before,
    stressLevelAfter: after,
    targetFrequency: frequency,
    createdAt,
});

describe('computeTrendSeries', () => {
    it('ignores pending sessions and sessions without a date', () => {
        const result = computeTrendSeries([
            make(5, null, '2026-09-01T12:00:00Z'),
            { ...make(5, 3, '2026-09-02T12:00:00Z'), createdAt: undefined },
            make(6, 2, '2026-09-03T12:00:00Z'),
        ]);
        expect(result).toEqual([{ date: '2026-09-03T12:00:00Z', before: 6, after: 2 }]);
    });

    it('sorts chronologically without mutating the input', () => {
        const input = [make(5, 4, '2026-09-03T12:00:00Z'), make(6, 3, '2026-09-01T12:00:00Z')];
        const copy = [...input];
        const result = computeTrendSeries(input);
        expect(result.map((point) => point.date)).toEqual(['2026-09-01T12:00:00Z', '2026-09-03T12:00:00Z']);
        expect(input).toEqual(copy);
    });

    it('returns an empty list when there are no sessions', () => {
        expect(computeTrendSeries([])).toEqual([]);
    });
});

describe('computeFrequencyBreakdown', () => {
    it('computes the average reduction per frequency in descending order', () => {
        const result = computeFrequencyBreakdown([
            make(6, 4, '2026-09-01T12:00:00Z', BETA),
            make(5, 5, '2026-09-02T12:00:00Z', BETA),
            make(7, 3, '2026-09-03T12:00:00Z', ALPHA),
        ]);
        expect(result).toEqual([
            { frequency: ALPHA, sessionsCount: 1, averageReduction: 4 },
            { frequency: BETA, sessionsCount: 2, averageReduction: 1 },
        ]);
    });

    it('only considers completed sessions', () => {
        const result = computeFrequencyBreakdown([make(8, null, '2026-09-01T12:00:00Z'), make(5, 4, '2026-09-02T12:00:00Z')]);
        expect(result).toHaveLength(1);
        expect(result[0].sessionsCount).toBe(1);
    });

    it('returns a negative average when stress increases', () => {
        const [stat] = computeFrequencyBreakdown([make(3, 5, '2026-09-01T12:00:00Z')]);
        expect(stat.averageReduction).toBe(-2);
    });
});

describe('computeSummary', () => {
    beforeEach(() => {
        vi.useFakeTimers();
        vi.setSystemTime(new Date('2026-09-10T15:00:00Z'));
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it('returns zeros and a null average when there are no sessions', () => {
        expect(computeSummary([])).toEqual({
            totalSessions: 0,
            completedSessions: 0,
            averageReduction: null,
            currentStreakDays: 0,
        });
    });

    it('counts total and completed sessions and computes the average reduction', () => {
        const summary = computeSummary([
            make(6, 4, '2026-09-10T12:00:00Z'),
            make(5, 4, '2026-09-09T12:00:00Z'),
            make(4, null, '2026-09-08T12:00:00Z'),
        ]);
        expect(summary.totalSessions).toBe(3);
        expect(summary.completedSessions).toBe(2);
        expect(summary.averageReduction).toBe(1.5);
    });

    it('returns a negative average reduction when stress increases on average', () => {
        const summary = computeSummary([
            make(3, 4, '2026-09-01T12:00:00Z'),
            make(3, 5, '2026-09-02T12:00:00Z'),
            make(5, 4, '2026-09-03T12:00:00Z'),
            make(2, 5, '2026-09-04T12:00:00Z'),
        ]);
        expect(summary.averageReduction).toBe(-1.25);
    });

    describe('streak', () => {
        it('counts consecutive days ending today', () => {
            const summary = computeSummary([
                make(5, 4, '2026-09-10T12:00:00Z'),
                make(5, 4, '2026-09-09T12:00:00Z'),
                make(5, 4, '2026-09-08T12:00:00Z'),
            ]);
            expect(summary.currentStreakDays).toBe(3);
        });

        it('keeps the streak when the last session was yesterday', () => {
            expect(computeSummary([make(5, 4, '2026-09-09T12:00:00Z')]).currentStreakDays).toBe(1);
        });

        it('resets when the last session is older than yesterday', () => {
            expect(computeSummary([make(5, 4, '2026-09-07T12:00:00Z')]).currentStreakDays).toBe(0);
        });

        it('stops counting at a gap', () => {
            const summary = computeSummary([
                make(5, 4, '2026-09-10T12:00:00Z'),
                make(5, 4, '2026-09-09T12:00:00Z'),
                make(5, 4, '2026-09-06T12:00:00Z'),
            ]);
            expect(summary.currentStreakDays).toBe(2);
        });

        it('counts several sessions on the same day as a single day', () => {
            const summary = computeSummary([make(5, 4, '2026-09-10T12:00:00Z'), make(6, 3, '2026-09-10T14:00:00Z')]);
            expect(summary.currentStreakDays).toBe(1);
        });

        it('includes pending sessions in the streak', () => {
            expect(computeSummary([make(5, null, '2026-09-10T12:00:00Z')]).currentStreakDays).toBe(1);
        });
    });
});
