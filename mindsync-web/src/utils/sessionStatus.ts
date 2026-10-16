import type { StressSession } from '../types/session';

export type SessionStatusVariant = 'pending' | 'improved' | 'worsened' | 'neutral';

export interface SessionStatus {
    variant: SessionStatusVariant;
    label: string;
}

export function getSessionStatus(session: StressSession): SessionStatus {
    const isCompleted = session.stressLevelAfter !== null && session.stressLevelAfter !== undefined;

    if (!isCompleted) {
        return { variant: 'pending', label: 'Em Andamento' };
    }

    const delta = session.stressLevelBefore - session.stressLevelAfter!;

    if (delta > 0) {
        return { variant: 'improved', label: `Redução de ${delta} pt${delta > 1 ? 's' : ''}` };
    }

    if (delta < 0) {
        const increase = Math.abs(delta);
        return { variant: 'worsened', label: `Aumento de ${increase} pt${increase > 1 ? 's' : ''}` };
    }

    return { variant: 'neutral', label: 'Sem alteração' };
}