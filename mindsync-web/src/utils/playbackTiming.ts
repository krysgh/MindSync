export type TimingStage = 'mount' | 'resolveEnd' | 'fetchStart' | 'downloadEnd' | 'decodeEnd' | 'firstSound';

export interface PlaybackTimingReport {
    resolveMs: number;
    downloadMs: number;
    decodeMs: number;
    otherMs: number;
    startupMs: number;
    audioLatencyMs: number;
}

export interface PlaybackTimer {
    mark: (stage: TimingStage) => void;
    setAudioLatency: (milliseconds: number) => void;
    report: () => PlaybackTimingReport | null;
}

const round = (value: number) => Math.round(value * 10) / 10;

export function createPlaybackTimer(now: () => number = () => performance.now()): PlaybackTimer {
    const marks = new Map<TimingStage, number>();
    let audioLatencyMs = 0;

    const mark = (stage: TimingStage) => {
        if (marks.has(stage)) return;
        marks.set(stage, now());
        if (typeof performance !== 'undefined' && typeof performance.mark === 'function') {
            performance.mark(`mindsync:${stage}`);
        }
    };

    const setAudioLatency = (milliseconds: number) => {
        audioLatencyMs = Number.isFinite(milliseconds) && milliseconds > 0 ? milliseconds : 0;
    };

    const report = (): PlaybackTimingReport | null => {
        const mount = marks.get('mount');
        const resolveEnd = marks.get('resolveEnd');
        const fetchStart = marks.get('fetchStart');
        const downloadEnd = marks.get('downloadEnd');
        const decodeEnd = marks.get('decodeEnd');
        const firstSound = marks.get('firstSound');

        if (
            mount === undefined ||
            resolveEnd === undefined ||
            fetchStart === undefined ||
            downloadEnd === undefined ||
            decodeEnd === undefined ||
            firstSound === undefined
        ) {
            return null;
        }

        const resolveMs = resolveEnd - mount;
        const downloadMs = downloadEnd - fetchStart;
        const decodeMs = decodeEnd - downloadEnd;
        const startupMs = firstSound - mount;
        const otherMs = startupMs - resolveMs - downloadMs - decodeMs;

        return {
            resolveMs: round(resolveMs),
            downloadMs: round(downloadMs),
            decodeMs: round(decodeMs),
            otherMs: round(otherMs),
            startupMs: round(startupMs),
            audioLatencyMs: round(audioLatencyMs),
        };
    };

    mark('mount');

    return { mark, setAudioLatency, report };
}

export function publishPlaybackTiming(report: PlaybackTimingReport): void {
    const target = window as unknown as { __mindsyncTiming?: PlaybackTimingReport[] };
    target.__mindsyncTiming = [...(target.__mindsyncTiming ?? []), report];
    if (import.meta.env.DEV) {
        console.table(report);
    }
}
