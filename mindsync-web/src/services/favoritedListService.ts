import { apiFetch } from './api';
import type { ApiResponse } from '../types/apiResponse';
import type {
    AddFavoritedSessionPayload,
    CreateFavoritedListPayload,
    FavoritedListDetail,
    FavoritedListSummary,
    UpdateFavoritedListPayload,
    UpdateFavoritedSessionPayload,
} from '../types/favoritedList';

const BASE_PATH = '/favoritedLists';

export const favoritedListService = {
    async listMine(): Promise<FavoritedListSummary[]> {
        const response = await apiFetch<ApiResponse<FavoritedListSummary[]>>(
            `${BASE_PATH}/mine`,
            { method: 'GET' },
            'Falha ao carregar suas listas de favoritos.'
        );
        return response.data ?? [];
    },

    async getById(id: string): Promise<FavoritedListDetail | null> {
        const response = await apiFetch<ApiResponse<FavoritedListDetail>>(
            `${BASE_PATH}/${id}`,
            { method: 'GET' },
            'Falha ao carregar a lista de favoritos.'
        );
        return response.data ?? null;
    },

    async create(payload: CreateFavoritedListPayload): Promise<string> {
        const response = await apiFetch<ApiResponse<string>>(
            BASE_PATH,
            { method: 'POST', body: JSON.stringify(payload) },
            'Falha ao criar a lista de favoritos.'
        );
        if (!response.data) throw new Error('A API não retornou o ID da lista criada.');
        return response.data;
    },

    async rename(payload: UpdateFavoritedListPayload): Promise<void> {
        await apiFetch<ApiResponse<null>>(
            `${BASE_PATH}/${payload.id}`,
            { method: 'PUT', body: JSON.stringify(payload) },
            'Falha ao renomear a lista.'
        );
    },

    async remove(id: string): Promise<void> {
        await apiFetch<ApiResponse<null>>(
            `${BASE_PATH}/${id}`,
            { method: 'DELETE' },
            'Falha ao remover a lista.'
        );
    },

    async addSession(listId: string, payload: AddFavoritedSessionPayload): Promise<string> {
        const response = await apiFetch<ApiResponse<string>>(
            `${BASE_PATH}/${listId}/sessions`,
            { method: 'POST', body: JSON.stringify(payload) },
            'Falha ao favoritar a sessão.'
        );
        if (!response.data) throw new Error('A API não retornou o ID da sessão favoritada.');
        return response.data;
    },

    async renameSession(payload: UpdateFavoritedSessionPayload): Promise<void> {
        await apiFetch<ApiResponse<null>>(
            `${BASE_PATH}/sessions/${payload.id}`,
            { method: 'PUT', body: JSON.stringify(payload) },
            'Falha ao renomear a sessão favoritada.'
        );
    },

    async removeSession(favoritedSessionId: string): Promise<void> {
        await apiFetch<ApiResponse<null>>(
            `${BASE_PATH}/sessions/${favoritedSessionId}`,
            { method: 'DELETE' },
            'Falha ao remover a sessão da lista.'
        );
    },
};
