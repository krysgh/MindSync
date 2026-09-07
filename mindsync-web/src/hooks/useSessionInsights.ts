import { useCallback, useEffect, useMemo, useState } from 'react';
import { sessionService } from '../services/sessionService';
import type { StressSession } from '../types/session';
import { computeFrequencyBreakdown, computeSummary, computeTrendSeries } from '../utils/insights';

export function useSessionInsights(userId: string | null) {
    const [sessions, setSessions] = useState<StressSession[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');

    const fetchSessions = useCallback(async () => {
        if (!userId) return;
        setIsLoading(true);
        try {
            const data = await sessionService.listByUser(userId);
            setSessions(data);
            setError('');
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Falha ao carregar suas métricas.');
        } finally {
            setIsLoading(false);
        }
    }, [userId]);

    useEffect(() => {
        fetchSessions();
    }, [fetchSessions]);

    const summary = useMemo(() => computeSummary(sessions), [sessions]);
    const trendSeries = useMemo(() => computeTrendSeries(sessions), [sessions]);
    const frequencyBreakdown = useMemo(() => computeFrequencyBreakdown(sessions), [sessions]);

    return { isLoading, error, summary, trendSeries, frequencyBreakdown, refresh: fetchSessions };
}