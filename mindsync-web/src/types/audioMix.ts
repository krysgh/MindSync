export interface AudioMixLayer {
    audioTrackId: string;
    name: string;
    role: string;
    audioUrl: string;
    volume: number;
    durationSeconds: number;
}

export interface SessionAudioMix {
    id: string;
    stressSessionId: string;
    createdAt: string;
    layers: AudioMixLayer[];
}