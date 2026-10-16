import { apiFetch } from './api';
import type { ApiResponse } from '../types/apiResponse';
import type { LoginPayload, RegisterPayload } from '../types/auth';

export const authService = {
    async me(): Promise<string> {
        const response = await apiFetch<ApiResponse<string>>('/auth/me', { method: 'GET' });
        return response.data!;
    },

    async login(payload: LoginPayload): Promise<string> {
        const response = await apiFetch<ApiResponse<string>>(
            '/auth/login',
            { method: 'POST', body: JSON.stringify(payload) },
            'E-mail ou senha incorretos.'
        );
        return response.data!;
    },

    register(payload: RegisterPayload): Promise<void> {
        return apiFetch<void>(
            '/auth/register',
            { method: 'POST', body: JSON.stringify(payload) },
            'Erro ao realizar o cadastro.'
        );
    },

    logout(): Promise<void> {
        return apiFetch<void>('/auth/logout', { method: 'POST' });
    },
};