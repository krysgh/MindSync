import { useEffect, useRef, useState } from 'react';
import { StarIcon } from '../icons/Icons';
import { useClickOutside } from '../../hooks/useClickOutside';
import { FavoritePopover } from './FavoritePopover';
import type { FavoriteEntry, FavoritesControls } from '../../types/favoritedList';
import type { StressSession } from '../../types/session';
import './Favorites.css';

interface FavoriteStarButtonProps {
    session: StressSession;
    entries: FavoriteEntry[];
    controls: FavoritesControls;
    onOpenChange?: (isOpen: boolean) => void;
}

export function FavoriteStarButton({ session, entries, controls, onOpenChange }: FavoriteStarButtonProps) {
    const [isOpen, setIsOpen] = useState(false);
    const anchorRef = useRef<HTMLDivElement>(null);

    useClickOutside(anchorRef, () => setIsOpen(false), isOpen);

    useEffect(() => {
        onOpenChange?.(isOpen);
    }, [isOpen, onOpenChange]);

    const isFavorited = entries.length > 0;

    return (
        <div className="favorite-popover-anchor" ref={anchorRef}>
            <button
                type="button"
                className={`favorite-star-btn ${isFavorited ? 'is-active' : ''}`}
                onClick={() => setIsOpen((prev) => !prev)}
                title={isFavorited ? 'Gerenciar listas de favoritos' : 'Favoritar sessão'}
            >
                <StarIcon width={18} height={18} fill="currentColor" />
            </button>

            {isOpen && <FavoritePopover session={session} entries={entries} controls={controls} />}
        </div>
    );
}