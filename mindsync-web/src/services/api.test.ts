import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { apiFetch } from './api';

const fetchMock = vi.fn();

function jsonResponse(body: unknown, init: ResponseInit = {}) {
    return new Response(JSON.stringify(body), { status: 200, ...init });
}

beforeEach(() => {
    fetchMock.mockReset();
    vi.stubGlobal('fetch', fetchMock);
});

afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
});

describe('apiFetch', () => {
    it('sends credentials and a JSON content type to the API base URL', async () => {
        fetchMock.mockResolvedValue(jsonResponse({ data: 1 }));
        await apiFetch('/sessions/me');

        const [url, config] = fetchMock.mock.calls[0];
        expect(url).toMatch(/\/api\/sessions\/me$/);
        expect(config.credentials).toBe('include');
        expect(config.headers['Content-Type']).toBe('application/json');
    });

    it('allows overriding headers while preserving method and body', async () => {
        fetchMock.mockResolvedValue(jsonResponse({}));
        await apiFetch('/x', { method: 'PATCH', body: '{"a":1}', headers: { 'X-Test': '1' } });

        const [, config] = fetchMock.mock.calls[0];
        expect(config.method).toBe('PATCH');
        expect(config.body).toBe('{"a":1}');
        expect(config.headers['X-Test']).toBe('1');
        expect(config.headers['Content-Type']).toBe('application/json');
    });

    it('returns the response JSON on success', async () => {
        fetchMock.mockResolvedValue(jsonResponse({ data: [1, 2] }));
        await expect(apiFetch('/x')).resolves.toEqual({ data: [1, 2] });
    });

    it('returns an empty object for a 204 response', async () => {
        fetchMock.mockResolvedValue(new Response(null, { status: 204 }));
        await expect(apiFetch('/x')).resolves.toEqual({});
    });

    describe('HTTP errors', () => {
        it('joins the validation messages from the errors field', async () => {
            fetchMock.mockResolvedValue(
                jsonResponse({ errors: { Email: ['E-mail inválido.'], Password: ['Senha curta.', 'Falta número.'] } }, { status: 400 })
            );
            await expect(apiFetch('/x')).rejects.toThrow('E-mail inválido. Senha curta. Falta número.');
        });

        it('uses message when there is no errors field', async () => {
            fetchMock.mockResolvedValue(jsonResponse({ message: 'Sessão não encontrada.' }, { status: 404 }));
            await expect(apiFetch('/x')).rejects.toThrow('Sessão não encontrada.');
        });

        it('uses title (ProblemDetails) when there is no message', async () => {
            fetchMock.mockResolvedValue(jsonResponse({ title: 'Unauthorized' }, { status: 401 }));
            await expect(apiFetch('/x')).rejects.toThrow('Unauthorized');
        });

        it('falls back to the provided message when the body is not JSON', async () => {
            fetchMock.mockResolvedValue(new Response('<html>error</html>', { status: 500 }));
            await expect(apiFetch('/x', {}, 'Custom failure.')).rejects.toThrow('Custom failure.');
        });

        it('falls back to the provided message when errors is empty and there is no message', async () => {
            fetchMock.mockResolvedValue(jsonResponse({ errors: {} }, { status: 400 }));
            await expect(apiFetch('/x', {}, 'Fallback.')).rejects.toThrow('Fallback.');
        });
    });

    describe('network failure', () => {
        it('reports server unavailability when the browser is online', async () => {
            vi.spyOn(navigator, 'onLine', 'get').mockReturnValue(true);
            fetchMock.mockRejectedValue(new TypeError('Failed to fetch'));
            await expect(apiFetch('/x')).rejects.toThrow('Não foi possível conectar ao servidor');
        });

        it('reports missing internet when the browser is offline', async () => {
            vi.spyOn(navigator, 'onLine', 'get').mockReturnValue(false);
            fetchMock.mockRejectedValue(new TypeError('Failed to fetch'));
            await expect(apiFetch('/x')).rejects.toThrow('sem conexão com a internet');
        });
    });
});
