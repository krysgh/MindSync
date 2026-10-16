export interface StressSession {
    id: string;
    stressLevelBefore: number;
    stressLevelAfter?: number | null;
    targetFrequency: string;
    createdAt?: string;
}

export interface CreateSessionPayload {
    stressLevelBefore: number;
    targetFrequency: string;
}

export interface CompleteSessionPayload {
    stressLevelAfter: number;
}