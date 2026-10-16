import { useState, type CSSProperties } from 'react';
import { useNavigate } from 'react-router-dom';
import { STRESS_LEVELS } from '../../constants/stressLevels';
import { getSessionStatus } from '../../utils/sessionStatus';
import type { StressSession } from '../../types/session';
import type { FavoriteEntry, FavoritesControls } from '../../types/favoritedList';
import { FavoriteStarButton } from '../favorites/FavoriteStarButton';
import { ArrowRightIcon, BoltIcon, HourglassIcon, WarningIcon } from '../icons/Icons';

interface SessionCardProps {
    session: StressSession;
    onComplete: (sessionId: string) => void;
    favoriteEntries: FavoriteEntry[];
    favoritesControls: FavoritesControls;
}

const STATUS_ICONS = {
    pending: HourglassIcon,
    improved: BoltIcon,
    worsened: WarningIcon,
    neutral: null,
};

export function SessionCard({ session, onComplete, favoriteEntries, favoritesControls }: SessionCardProps) {
    const navigate = useNavigate();
    const isCompleted = session.stressLevelAfter !== null && session.stressLevelAfter !== undefined;
    const status = getSessionStatus(session);
    const StatusIcon = STATUS_ICONS[status.variant];

    const [isFavoritePopoverOpen, setIsFavoritePopoverOpen] = useState(false);

    const accentColor = STRESS_LEVELS[session.stressLevelBefore]?.color || '#6366f1';
    const cardStyle = { '--session-accent': accentColor } as CSSProperties;

    const beforeStyle = { '--session-value-color': STRESS_LEVELS[session.stressLevelBefore]?.color } as CSSProperties;
    const afterStyle = {
        '--session-value-color': isCompleted ? STRESS_LEVELS[session.stressLevelAfter!]?.color : '#64748b',
    } as CSSProperties;

    const handleOpenPlayer = () => navigate(`/player/${session.id}`);

    return (
        <div
            className={[
                'session-card',
                'session-card--clickable',
                isFavoritePopoverOpen ? 'session-card--popover-open' : '',
                status.variant === 'pending' ? 'session-card--pending' : '',
            ]
                .filter(Boolean)
                .join(' ')}
            style={cardStyle}
            onClick={handleOpenPlayer}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') handleOpenPlayer();
            }}
        >
            <div className="session-card-header">
                <span className="session-card-freq">
                    <strong>Frequência:</strong> {session.targetFrequency}
                </span>

                <div className="session-card-header-right">
                    <div onClick={(e) => e.stopPropagation()} onKeyDown={(e) => e.stopPropagation()}>
                        <FavoriteStarButton
                            session={session}
                            entries={favoriteEntries}
                            controls={favoritesControls}
                            onOpenChange={setIsFavoritePopoverOpen}
                        />
                    </div>

                    <span className={`status-badge status-badge--${status.variant}`}>
                        {StatusIcon && <StatusIcon className="status-badge-icon" width={13} height={13} fill="currentColor" />}
                        {status.label}
                    </span>
                </div>
            </div>

            <div className="session-card-body">
                <div className="session-values">
                    <div>
                        <small className="session-value-label">Antes</small>
                        <p className="session-value" style={beforeStyle}>
                            {STRESS_LEVELS[session.stressLevelBefore]?.label} ({session.stressLevelBefore})
                        </p>
                    </div>

                    <ArrowRightIcon className="session-arrow" width={16} height={16} fill="currentColor" />

                    <div>
                        <small className="session-value-label">Depois</small>
                        <p className="session-value" style={afterStyle}>
                            {isCompleted ? `${STRESS_LEVELS[session.stressLevelAfter!]?.label} (${session.stressLevelAfter})` : 'Pendente...'}
                        </p>
                    </div>
                </div>

                {!isCompleted && (
                    <button
                        onClick={(e) => {
                            e.stopPropagation();
                            onComplete(session.id);
                        }}
                        className="complete-session-btn"
                    >
                        Finalizar & Avaliar
                    </button>
                )}
            </div>
        </div>
    );
}