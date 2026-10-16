import { useCallback, useEffect, useMemo, useState } from 'react';
import { sessionService } from '../services/sessionService';
import type { StressSession } from '../types/session';
import { getLocalDateString } from '../utils/date';

const ITEMS_PER_PAGE = 3;

export function useSessions(userId: string | null) {
    const [sessions, setSessions] = useState<StressSession[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');

    const [selectedDate, setSelectedDate] = useState('');
    const [currentPage, setCurrentPage] = useState(1);

    const [lastSelectedDate, setLastSelectedDate] = useState(selectedDate);
    if (selectedDate !== lastSelectedDate) {
        setLastSelectedDate(selectedDate);
        setCurrentPage(1);
    }

    const fetchSessions = useCallback(async () => {
        if (!userId) return;
        setIsLoading(true);
        try {
            const data = await sessionService.listByUser(userId);
            setSessions(data);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Falha ao carregar as sessões.');
        } finally {
            setIsLoading(false);
        }
    }, [userId]);

    useEffect(() => {
        fetchSessions();
    }, [fetchSessions]);

    const hasPendingSession = useMemo(
        () => sessions.some((session) => session.stressLevelAfter === null || session.stressLevelAfter === undefined),
        [sessions]
    );

    const filteredSessions = useMemo(
        () =>
            sessions.filter((session) => {
                if (!selectedDate || !session.createdAt) return true;
                return getLocalDateString(session.createdAt) === selectedDate;
            }),
        [sessions, selectedDate]
    );

    const totalPages = Math.ceil(filteredSessions.length / ITEMS_PER_PAGE) || 1;
    const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
    const paginatedSessions = filteredSessions.slice(startIndex, startIndex + ITEMS_PER_PAGE);

    return {
        isLoading,
        error,
        selectedDate,
        setSelectedDate,
        currentPage,
        setCurrentPage,
        totalPages,
        paginatedSessions,
        hasPendingSession,
        refresh: fetchSessions,
    };
}
