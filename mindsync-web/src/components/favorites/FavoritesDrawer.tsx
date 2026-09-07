import { useState, type FormEvent } from 'react';
import type { UseFavoritedListsReturn } from '../../hooks/useFavoritedLists';
import { useDialog } from '../../hooks/useDialog';
import { CloseIcon, StarIcon } from '../icons/Icons';
import { ErrorBanner } from '../common/ErrorBanner';
import { FavoritedListGroup } from './FavoritedListGroup';
import './Favorites.css';

interface FavoritesDrawerProps {
    isOpen: boolean;
    onClose: () => void;
    favorites: UseFavoritedListsReturn;
}

export function FavoritesDrawer({ isOpen, onClose, favorites }: FavoritesDrawerProps) {
    const [newListName, setNewListName] = useState('');
    const [isCreating, setIsCreating] = useState(false);
    const { alertUser } = useDialog();

    if (!isOpen) return null;

    const handleCreate = async (e: FormEvent) => {
        e.preventDefault();
        const trimmed = newListName.trim();
        if (!trimmed) return;

        setIsCreating(true);
        try {
            await favorites.createList(trimmed);
            setNewListName('');
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao criar a lista.');
        } finally {
            setIsCreating(false);
        }
    };

    return (
        <div className="favorites-drawer-overlay" onClick={onClose}>
            <div className="favorites-drawer" onClick={(e) => e.stopPropagation()}>
                <div className="favorites-drawer-header">
                    <h3>
                        <StarIcon className="drawer-title-icon" width={16} height={16} fill="currentColor" /> Minhas
                        Listas de Favoritos
                    </h3>
                    <button className="favorites-drawer-close" onClick={onClose} title="Fechar">
                        <CloseIcon width={16} height={16} fill="currentColor" />
                    </button>
                </div>

                <div className="favorites-drawer-body">
                    <form onSubmit={handleCreate} className="favorites-create-form">
                        <input
                            type="text"
                            value={newListName}
                            onChange={(e) => setNewListName(e.target.value)}
                            placeholder="Nome da nova lista..."
                            maxLength={60}
                        />
                        <button type="submit" disabled={isCreating || !newListName.trim()}>
                            {isCreating ? '...' : '+ Criar'}
                        </button>
                    </form>

                    {favorites.isLoading && <p className="favorites-drawer-status">Carregando listas...</p>}
                    {favorites.error && <ErrorBanner message={favorites.error} />}

                    {!favorites.isLoading && favorites.lists.length === 0 && (
                        <div className="favorites-drawer-empty">
                            Você ainda não tem listas de favoritos. Crie uma acima ou favorite uma sessão pelo ícone de
                            estrela nos cards.
                        </div>
                    )}

                    {favorites.lists.map((list) => (
                        <FavoritedListGroup
                            key={list.id}
                            summary={list}
                            detail={favorites.detailsByListId[list.id]}
                            onRenameList={favorites.renameList}
                            onDeleteList={favorites.deleteList}
                            onRenameSession={favorites.renameFavoritedSession}
                            onRemoveSession={favorites.removeFavoritedSession}
                        />
                    ))}
                </div>
            </div>
        </div>
    );
}