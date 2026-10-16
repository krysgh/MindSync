import { describe, expect, it } from 'vitest';
import { getLocalDateString } from './date';

describe('getLocalDateString', () => {
    it('returns an empty string when no date is provided', () => {
        expect(getLocalDateString(undefined)).toBe('');
        expect(getLocalDateString('')).toBe('');
    });

    it('keeps dates that are already in YYYY-MM-DD format', () => {
        expect(getLocalDateString('2026-09-10')).toBe('2026-09-10');
    });

    it('converts a UTC instant to the local day', () => {
        expect(getLocalDateString('2026-09-10T02:00:00Z')).toBe('2026-09-09');
        expect(getLocalDateString('2026-09-10T03:00:00Z')).toBe('2026-09-10');
    });

    it('respects an explicit offset in the string', () => {
        expect(getLocalDateString('2026-09-10T23:30:00-03:00')).toBe('2026-09-10');
        expect(getLocalDateString('2026-09-11T01:00:00+00:00')).toBe('2026-09-10');
    });

    it('assumes UTC when the string has no offset', () => {
        expect(getLocalDateString('2026-09-10 02:00:00')).toBe('2026-09-09');
        expect(getLocalDateString('2026-09-10T15:00:00')).toBe('2026-09-10');
    });

    it('returns an empty string for invalid dates', () => {
        expect(getLocalDateString('not a date')).toBe('');
    });
});
