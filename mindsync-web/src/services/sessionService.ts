import { apiFetch } from './api';
import type { ApiResponse } from '../types/apiResponse';
import type { CompleteSessionPayload, CreateSessionPayload, StressSession } from '../types/session';

export const sessionService = {
    async listByUser(userId: string): Promise<StressSession[]> {
        const response = await apiFetch<ApiResponse<StressSession[]>>(
            `/sessions/users/${userId}`,
            { method: 'GET' },
            'Falha ao carregar as sessões.'
        );
        return response.data ?? [];
    },

    async create(payload: CreateSessionPayload): Promise<string> {
        const response = await apiFetch<ApiResponse<string>>(
            '/sessions',
            { method: 'POST', body: JSON.stringify(payload) },
            'Falha ao criar sessão.'
        );
        return response.data!;
    },

    async complete(sessionId: string, payload: CompleteSessionPayload): Promise<void> {
        await apiFetch<ApiResponse<null>>(
            `/sessions/${sessionId}/complete`,
            { method: 'PATCH', body: JSON.stringify(payload) },
            'Falha ao finalizar a sessão.'
        );
    },
};