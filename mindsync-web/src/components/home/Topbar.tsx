import { useNavigate } from 'react-router-dom';
import { ChartIcon, LogoMark, MenuIcon } from '../icons/Icons';
import '../favorites/Favorites.css';

interface TopbarProps {
    onLogout: () => void;
    onOpenFavorites: () => void;
}

export function Topbar({ onLogout, onOpenFavorites }: TopbarProps) {
    const navigate = useNavigate();

    return (
        <header className="home-topbar">
            <div className="home-brand">
                <div className="home-brand-badge">
                    <LogoMark />
                </div>
                <span className="home-brand-name">
                    Mind<span className="home-brand-accent">Sync</span>
                </span>
            </div>

            <div className="home-topbar-right">
                <button
                    onClick={() => navigate('/insights')}
                    className="favorites-menu-btn"
                    title="Meu progresso"
                >
                    <ChartIcon width={18} height={18} fill="currentColor" />
                </button>

                <button
                    onClick={onOpenFavorites}
                    className="favorites-menu-btn"
                    title="Minhas listas de favoritos"
                >
                    <MenuIcon width={18} height={18} fill="currentColor" />
                </button>

                <button onClick={onLogout} className="home-logout-btn">
                    Sair
                </button>
            </div>
        </header>
    );
}