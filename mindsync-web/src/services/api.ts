const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

interface ApiErrorPayload {
    message?: string;
    title?: string;
    errors?: Record<string, string[]>;
}

function extractErrorMessage(payload: ApiErrorPayload | null, fallback: string): string {
    if (!payload) return fallback;

    if (payload.errors) {
        const formatted = Object.values(payload.errors).flat().join(' ');
        if (formatted) return formatted;
    }

    return payload.message || payload.title || fallback;
}

export async function apiFetch<T>(
    endpoint: string,
    options: RequestInit = {},
    fallbackErrorMessage = 'Ocorreu um erro ao processar a requisição.'
): Promise<T> {
    const config: RequestInit = {
        ...options,
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
    };

    let response: Response;
    try {
        response = await fetch(`${API_BASE_URL}${endpoint}`, config);
    } catch {
        throw new Error(
            navigator.onLine
                ? 'Não foi possível conectar ao servidor. Tente novamente em instantes.'
                : 'Você está sem conexão com a internet. Verifique sua rede e tente novamente.'
        );
    }

    if (!response.ok) {
        const errorPayload = await response.json().catch(() => null) as ApiErrorPayload | null;
        throw new Error(extractErrorMessage(errorPayload, fallbackErrorMessage));
    }

    if (response.status === 204) {
        return {} as T;
    }

    return response.json();
}