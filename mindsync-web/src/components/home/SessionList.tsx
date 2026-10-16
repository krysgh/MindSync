import type { StressSession } from '../../types/session';
import type { FavoriteEntry, FavoritesControls } from '../../types/favoritedList';
import { ErrorBanner } from '../common/ErrorBanner';
import { SessionCard } from './SessionCard';

interface SessionListProps {
    sessions: StressSession[];
    isLoading: boolean;
    error: string;
    hasDateFilter: boolean;
    onCompleteSession: (sessionId: string) => void;
    getFavoriteEntries: (stressSessionId: string) => FavoriteEntry[];
    favoritesControls: FavoritesControls;
}

export function SessionList({
    sessions,
    isLoading,
    error,
    hasDateFilter,
    onCompleteSession,
    getFavoriteEntries,
    favoritesControls,
}: SessionListProps) {
    return (
        <>
            {isLoading && <p className="home-status-text">Carregando histórico...</p>}
            {error && <ErrorBanner message={error} />}

            {!isLoading && sessions.length === 0 && (
                <div className="empty-state">
                    <p>{hasDateFilter ? 'Nenhuma sessão encontrada nesta data.' : 'Nenhuma sessão cadastrada ainda.'}</p>
                </div>
            )}

            <div className="session-list">
                {sessions.map((session) => (
                    <SessionCard
                        key={session.id}
                        session={session}
                        onComplete={onCompleteSession}
                        favoriteEntries={getFavoriteEntries(session.id)}
                        favoritesControls={favoritesControls}
                    />
                ))}
            </div>
        </>
    );
}