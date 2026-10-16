import { useState, type FormEvent } from 'react';
import { useDialog } from '../../hooks/useDialog';
import type { FavoriteEntry, FavoritesControls } from '../../types/favoritedList';
import type { StressSession } from '../../types/session';

interface FavoritePopoverProps {
    session: StressSession;
    entries: FavoriteEntry[];
    controls: FavoritesControls;
}

export function FavoritePopover({ session, entries, controls }: FavoritePopoverProps) {
    const [newListName, setNewListName] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [pendingListId, setPendingListId] = useState<string | null>(null);
    const { alertUser } = useDialog();

    const entryByListId = new Map(entries.map((entry) => [entry.listId, entry]));

    const handleToggleList = async (listId: string) => {
        setPendingListId(listId);
        try {
            const existingEntry = entryByListId.get(listId);
            if (existingEntry) {
                await controls.onRemoveFromList(existingEntry.favoritedSessionId);
            } else {
                await controls.onAddToList(listId, session.id, session.targetFrequency);
            }
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao atualizar a lista de favoritos.');
        } finally {
            setPendingListId(null);
        }
    };

    const handleCreateAndAdd = async (e: FormEvent) => {
        e.preventDefault();
        const trimmedName = newListName.trim();
        if (!trimmedName) return;

        setIsSubmitting(true);
        try {
            const listId = await controls.onCreateList(trimmedName);
            await controls.onAddToList(listId, session.id, session.targetFrequency);
            setNewListName('');
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao criar a lista.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="favorite-popover">
            <p className="favorite-popover-title">Salvar em uma lista</p>

            {controls.lists.length === 0 ? (
                <p className="favorite-popover-empty">Você ainda não tem listas. Crie uma abaixo.</p>
            ) : (
                <div className="favorite-popover-list">
                    {controls.lists.map((list) => (
                        <label key={list.id} className="favorite-popover-item">
                            <input
                                type="checkbox"
                                checked={entryByListId.has(list.id)}
                                disabled={pendingListId === list.id}
                                onChange={() => handleToggleList(list.id)}
                            />
                            {list.name}
                        </label>
                    ))}
                </div>
            )}

            <form onSubmit={handleCreateAndAdd} className="favorite-popover-new">
                <input
                    type="text"
                    value={newListName}
                    onChange={(e) => setNewListName(e.target.value)}
                    placeholder="Nova lista..."
                    maxLength={60}
                />
                <button type="submit" disabled={isSubmitting || !newListName.trim()}>
                    {isSubmitting ? '...' : 'Criar'}
                </button>
            </form>
        </div>
    );
}