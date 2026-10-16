import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useSessions } from '../hooks/useSessions';
import { useFavoritedLists } from '../hooks/useFavoritedLists';
import { CreateSessionModal } from '../components/session/CreateSessionModal';
import { CompleteSessionModal } from '../components/session/CompleteSessionModal';
import { Topbar } from '../components/home/Topbar';
import { SessionToolbar } from '../components/home/SessionToolbar';
import { PendingBanner } from '../components/home/PendingBanner';
import { SessionList } from '../components/home/SessionList';
import { Pagination } from '../components/home/Pagination';
import { FavoritesDrawer } from '../components/favorites/FavoritesDrawer';
import type { FavoritesControls } from '../types/favoritedList';
import './Home.css';

export function Home() {
    const { userId, logout, isLoading: isAuthLoading } = useAuth();
    const navigate = useNavigate();

    const {
        isLoading,
        error,
        selectedDate,
        setSelectedDate,
        currentPage,
        setCurrentPage,
        totalPages,
        paginatedSessions,
        hasPendingSession,
        refresh,
    } = useSessions(userId);

    const favorites = useFavoritedLists(userId);

    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [selectedSessionId, setSelectedSessionId] = useState<string | null>(null);
    const [isCompleteModalOpen, setIsCompleteModalOpen] = useState(false);
    const [isFavoritesDrawerOpen, setIsFavoritesDrawerOpen] = useState(false);

    useEffect(() => {
        if (isAuthLoading) return;
        if (!userId) navigate('/');
    }, [userId, isAuthLoading, navigate]);

    const favoritesControls: FavoritesControls = useMemo(
        () => ({
            lists: favorites.lists,
            onCreateList: favorites.createList,
            onAddToList: favorites.addSessionToList,
            onRemoveFromList: favorites.removeFavoritedSession,
        }),
        [favorites.lists, favorites.createList, favorites.addSessionToList, favorites.removeFavoritedSession]
    );

    if (isAuthLoading) {
        return <div className="home-loading-screen">Carregando sessão...</div>;
    }

    const handleOpenCompleteModal = (sessionId: string) => {
        setSelectedSessionId(sessionId);
        setIsCompleteModalOpen(true);
    };

    const handleLogout = () => {
        logout();
        navigate('/');
    };

    return (
        <div className="app-page home-page">
            <div className="app-container home-container">
                <Topbar onLogout={handleLogout} onOpenFavorites={() => setIsFavoritesDrawerOpen(true)} />

                <main className="home-main">
                    <SessionToolbar
                        selectedDate={selectedDate}
                        onDateChange={setSelectedDate}
                        onCreateSession={() => setIsCreateModalOpen(true)}
                        createDisabled={hasPendingSession}
                    />

                    <PendingBanner visible={hasPendingSession} />

                    <SessionList
                        sessions={paginatedSessions}
                        isLoading={isLoading}
                        error={error}
                        hasDateFilter={!!selectedDate}
                        onCompleteSession={handleOpenCompleteModal}
                        getFavoriteEntries={(stressSessionId) => favorites.favoritesBySessionId.get(stressSessionId) ?? []}
                        favoritesControls={favoritesControls}
                    />

                    <Pagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
                </main>
            </div>

            <CreateSessionModal
                isOpen={isCreateModalOpen}
                onClose={() => setIsCreateModalOpen(false)}
                onSuccess={refresh}
            />

            <CompleteSessionModal
                sessionId={selectedSessionId}
                isOpen={isCompleteModalOpen}
                onClose={() => {
                    setIsCompleteModalOpen(false);
                    setSelectedSessionId(null);
                }}
                onSuccess={refresh}
            />

            <FavoritesDrawer
                isOpen={isFavoritesDrawerOpen}
                onClose={() => setIsFavoritesDrawerOpen(false)}
                favorites={favorites}
            />
        </div>
    );
}