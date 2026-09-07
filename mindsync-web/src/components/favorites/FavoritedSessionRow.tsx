import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { STRESS_LEVELS } from '../../constants/stressLevels';
import { useDialog } from '../../hooks/useDialog';
import { CheckIcon, CloseIcon, PencilIcon, ArrowRightIcon } from '../icons/Icons';
import type { FavoritedSessionItem } from '../../types/favoritedList';

interface FavoritedSessionRowProps {
    item: FavoritedSessionItem;
    onRename: (favoritedSessionId: string, customName: string) => Promise<void>;
    onRemove: (favoritedSessionId: string) => Promise<void>;
}

export function FavoritedSessionRow({ item, onRename, onRemove }: FavoritedSessionRowProps) {
    const navigate = useNavigate();
    const [isRenaming, setIsRenaming] = useState(false);
    const [nameDraft, setNameDraft] = useState(item.customName);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { alertUser, confirmUser } = useDialog();

    const beforeLabel = STRESS_LEVELS[item.stressLevelBefore]?.label ?? item.stressLevelBefore;
    const afterLabel =
        item.stressLevelAfter !== null && item.stressLevelAfter !== undefined
            ? STRESS_LEVELS[item.stressLevelAfter]?.label ?? item.stressLevelAfter
            : 'Pendente';

    const handleRenameSubmit = async (e: FormEvent) => {
        e.preventDefault();
        const trimmed = nameDraft.trim();
        if (!trimmed || trimmed === item.customName) {
            setIsRenaming(false);
            return;
        }

        setIsSubmitting(true);
        try {
            await onRename(item.id, trimmed);
            setIsRenaming(false);
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao renomear.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleRemove = async () => {
        const confirmed = await confirmUser(`Remover "${item.customName}" desta lista?`, {
            title: 'Remover da lista',
            confirmLabel: 'Remover',
            variant: 'danger',
        });
        if (!confirmed) return;

        try {
            await onRemove(item.id);
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao remover.');
        }
    };

    if (isRenaming) {
        return (
            <form
                onSubmit={handleRenameSubmit}
                className="favorite-session-rename-form"
                onKeyDown={(e) => e.stopPropagation()}
            >
                <input
                    type="text"
                    value={nameDraft}
                    onChange={(e) => setNameDraft(e.target.value)}
                    maxLength={80}
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
                        setNameDraft(item.customName);
                    }}
                    title="Cancelar"
                >
                    <CloseIcon width={14} height={14} fill="currentColor" />
                </button>
            </form>
        );
    }

    return (
        <div
            className="favorite-session-row favorite-session-row--clickable"
            onClick={() => navigate(`/player/${item.stressSessionId}`)}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') navigate(`/player/${item.stressSessionId}`);
            }}
        >
            <div className="favorite-session-info">
                <div className="favorite-session-name">{item.customName}</div>
                <div className="favorite-session-meta">
                    {item.targetFrequency} · {beforeLabel}{' '}
                    <ArrowRightIcon className="inline-arrow-icon" width={10} height={10} fill="currentColor" />{' '}
                    {afterLabel}
                </div>
            </div>

            <div className="favorite-session-actions" onClick={(e) => e.stopPropagation()} onKeyDown={(e) => e.stopPropagation()}>
                <button
                    type="button"
                    className="favorite-list-icon-btn"
                    onClick={() => setIsRenaming(true)}
                    title="Renomear"
                >
                    <PencilIcon width={14} height={14} fill="currentColor" />
                </button>
                <button
                    type="button"
                    className="favorite-list-icon-btn is-danger"
                    onClick={handleRemove}
                    title="Remover da lista"
                >
                    <CloseIcon width={14} height={14} fill="currentColor" />
                </button>
            </div>
        </div>
    );
}