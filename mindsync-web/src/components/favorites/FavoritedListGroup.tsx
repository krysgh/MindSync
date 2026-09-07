import { useState, type FormEvent } from 'react';
import { useDialog } from '../../hooks/useDialog';
import { CheckIcon, ChevronDownIcon, ChevronRightIcon, CloseIcon, PencilIcon, TrashIcon } from '../icons/Icons';
import type { FavoritedListDetail, FavoritedListSummary } from '../../types/favoritedList';
import { FavoritedSessionRow } from './FavoritedSessionRow';

interface FavoritedListGroupProps {
    summary: FavoritedListSummary;
    detail?: FavoritedListDetail;
    onRenameList: (id: string, name: string) => Promise<void>;
    onDeleteList: (id: string) => Promise<void>;
    onRenameSession: (favoritedSessionId: string, customName: string) => Promise<void>;
    onRemoveSession: (favoritedSessionId: string) => Promise<void>;
}

export function FavoritedListGroup({
    summary,
    detail,
    onRenameList,
    onDeleteList,
    onRenameSession,
    onRemoveSession,
}: FavoritedListGroupProps) {
    const [isExpanded, setIsExpanded] = useState(false);
    const [isRenaming, setIsRenaming] = useState(false);
    const [nameDraft, setNameDraft] = useState(summary.name);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { alertUser, confirmUser } = useDialog();

    const handleRenameSubmit = async (e: FormEvent) => {
        e.preventDefault();
        const trimmed = nameDraft.trim();
        if (!trimmed || trimmed === summary.name) {
            setIsRenaming(false);
            return;
        }

        setIsSubmitting(true);
        try {
            await onRenameList(summary.id, trimmed);
            setIsRenaming(false);
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao renomear a lista.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async () => {
        const confirmed = await confirmUser(`Excluir a lista "${summary.name}" e todos os seus itens?`, {
            title: 'Excluir lista',
            confirmLabel: 'Excluir',
            variant: 'danger',
        });
        if (!confirmed) return;

        try {
            await onDeleteList(summary.id);
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao excluir a lista.');
        }
    };

    return (
        <div className="favorite-list-group">
            <div className="favorite-list-group-header" onClick={() => !isRenaming && setIsExpanded((prev) => !prev)}>
                <button type="button" className="favorite-list-group-toggle" aria-label="Expandir">
                    {isExpanded ? (
                        <ChevronDownIcon width={14} height={14} fill="currentColor" />
                    ) : (
                        <ChevronRightIcon width={14} height={14} fill="currentColor" />
                    )}
                </button>
                <span className="favorite-list-group-name">{summary.name}</span>
                <span className="favorite-list-group-count">{summary.totalItems}</span>

                <div className="favorite-list-group-actions" onClick={(e) => e.stopPropagation()}>
                    <button
                        type="button"
                        className="favorite-list-icon-btn"
                        onClick={() => setIsRenaming(true)}
                        title="Renomear lista"
                    >
                        <PencilIcon width={14} height={14} fill="currentColor" />
                    </button>
                    <button
                        type="button"
                        className="favorite-list-icon-btn is-danger"
                        onClick={handleDelete}
                        title="Excluir lista"
                    >
                        <TrashIcon width={14} height={14} fill="currentColor" />
                    </button>
                </div>
            </div>

            {isRenaming && (
                <form
                    onSubmit={handleRenameSubmit}
                    className="favorite-list-rename-form"
                    onKeyDown={(e) => e.stopPropagation()}
                >
                    <input
                        type="text"
                        value={nameDraft}
                        onChange={(e) => setNameDraft(e.target.value)}
                        maxLength={60}
                        autoFocus
                    />
                    <button type="submit" className="favorite-list-icon-btn" disabled={isSubmitting} title="Salvar">
                        <CheckIcon width={14} height={14} fill="currentColor" />
                    </button>
                    <button
                        type="button"
                        className="favorite-list-icon-btn"
                        onClick={() => {
                            setIsRenaming(false);
                            setNameDraft(summary.name);
                        }}
                        title="Cancelar"
                    >
                        <CloseIcon width={14} height={14} fill="currentColor" />
                    </button>
                </form>
            )}

            {isExpanded && (
                <div className="favorite-list-group-items">
                    {!detail || detail.items.length === 0 ? (
                        <div className="favorite-session-row">
                            <span className="favorite-session-meta">Nenhuma sessão nesta lista ainda.</span>
                        </div>
                    ) : (
                        detail.items.map((item) => (
                            <FavoritedSessionRow
                                key={item.id}
                                item={item}
                                onRename={onRenameSession}
                                onRemove={onRemoveSession}
                            />
                        ))
                    )}
                </div>
            )}
        </div>
    );
}