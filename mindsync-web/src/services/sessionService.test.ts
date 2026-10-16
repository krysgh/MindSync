import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { sessionService } from './sessionService';

const fetchMock = vi.fn();
const ok = (body: unknown) => new Response(JSON.stringify(body), { status: 200 });

beforeEach(() => {
    fetchMock.mockReset();
    vi.stubGlobal('fetch', fetchMock);
});

afterEach(() => {
    vi.unstubAllGlobals();
});

describe('sessionService', () => {
    it('listMine requests /sessions/me with GET and no user id in the URL', async () => {
        fetchMock.mockResolvedValue(ok({ data: [{ id: 'a' }] }));
        const result = await sessionService.listMine();

        const [url, config] = fetchMock.mock.calls[0];
        expect(url).toMatch(/\/sessions\/me$/);
        expect(config.method).toBe('GET');
        expect(result).toEqual([{ id: 'a' }]);
    });

    it('listMine returns an empty list when data is null', async () => {
        fetchMock.mockResolvedValue(ok({ data: null }));
        await expect(sessionService.listMine()).resolves.toEqual([]);
    });

    it('create sends a POST with the payload and returns the session id', async () => {
        fetchMock.mockResolvedValue(ok({ data: 'new-id' }));
        const id = await sessionService.create({ stressLevelBefore: 5, targetFrequency: 'Alpha (10 Hz) - Relaxamento' });

        const [url, config] = fetchMock.mock.calls[0];
        expect(url).toMatch(/\/sessions$/);
        expect(config.method).toBe('POST');
        expect(JSON.parse(config.body)).toEqual({ stressLevelBefore: 5, targetFrequency: 'Alpha (10 Hz) - Relaxamento' });
        expect(id).toBe('new-id');
    });

    it('complete sends a PATCH to /sessions/{id}/complete', async () => {
        fetchMock.mockResolvedValue(ok({}));
        await sessionService.complete('abc', { stressLevelAfter: 2 });

        const [url, config] = fetchMock.mock.calls[0];
        expect(url).toMatch(/\/sessions\/abc\/complete$/);
        expect(config.method).toBe('PATCH');
        expect(JSON.parse(config.body)).toEqual({ stressLevelAfter: 2 });
    });

    it('propagates the API error message', async () => {
        fetchMock.mockResolvedValue(new Response(JSON.stringify({ message: 'Já finalizada.' }), { status: 400 }));
        await expect(sessionService.complete('abc', { stressLevelAfter: 1 })).rejects.toThrow('Já finalizada.');
    });
});
