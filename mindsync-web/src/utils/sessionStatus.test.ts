import { describe, expect, it } from 'vitest';
import { getSessionStatus } from './sessionStatus';
import type { StressSession } from '../types/session';

const make = (before: number, after?: number | null): StressSession => ({
    id: 's1',
    stressLevelBefore: before,
    stressLevelAfter: after,
    targetFrequency: 'Alpha (10 Hz) - Relaxamento',
});

describe('getSessionStatus', () => {
    it.each([[undefined], [null]])('marks the session as pending when stressLevelAfter is %s', (after) => {
        expect(getSessionStatus(make(5, after))).toEqual({ variant: 'pending', label: 'Em Andamento' });
    });

    it('treats a stressLevelAfter of 0 as completed, not pending', () => {
        expect(getSessionStatus(make(3, 0)).variant).toBe('improved');
    });

    it('uses singular for a 1 point reduction and plural for more', () => {
        expect(getSessionStatus(make(5, 4))).toEqual({ variant: 'improved', label: 'Redução de 1 pt' });
        expect(getSessionStatus(make(7, 4))).toEqual({ variant: 'improved', label: 'Redução de 3 pts' });
    });

    it('reports an increase using the absolute value and correct plural', () => {
        expect(getSessionStatus(make(3, 4))).toEqual({ variant: 'worsened', label: 'Aumento de 1 pt' });
        expect(getSessionStatus(make(2, 6))).toEqual({ variant: 'worsened', label: 'Aumento de 4 pts' });
    });

    it('reports neutral when nothing changed', () => {
        expect(getSessionStatus(make(4, 4))).toEqual({ variant: 'neutral', label: 'Sem alteração' });
    });
});
