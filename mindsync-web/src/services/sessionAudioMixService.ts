import { apiFetch } from './api';
import type { ApiResponse } from '../types/apiResponse';
import type { SessionAudioMix } from '../types/audioMix';

export const sessionAudioMixService = {
    async resolve(stressSessionId: string): Promise<SessionAudioMix> {
        const response = await apiFetch<ApiResponse<SessionAudioMix>>(
            `/mixes/sessions/${stressSessionId}`,
            { method: 'POST' },
            'Falha ao carregar a mixagem de áudio da sessão.'
        );

        if (!response.data) throw new Error('A API não retornou a mixagem de áudio.');
        return response.data;
    },
};