import { afterEach, describe, expect, it } from 'vitest';
import { createPlaybackTimer, publishPlaybackTiming } from './playbackTiming';

function clock(values: number[]) {
    let index = 0;
    return () => values[Math.min(index++, values.length - 1)];
}

describe('createPlaybackTimer', () => {
    it('returns null until every stage has been marked', () => {
        const timer = createPlaybackTimer(clock([0, 10, 20]));
        timer.mark('resolveEnd');
        timer.mark('fetchStart');

        expect(timer.report()).toBeNull();
    });

    it('computes the duration of each stage from its marks', () => {
        const timer = createPlaybackTimer(clock([0, 40, 50, 450, 520, 530]));
        timer.mark('resolveEnd');
        timer.mark('fetchStart');
        timer.mark('downloadEnd');
        timer.mark('decodeEnd');
        timer.mark('firstSound');

        expect(timer.report()).toEqual({
            resolveMs: 40,
            downloadMs: 400,
            decodeMs: 70,
            otherMs: 20,
            startupMs: 530,
            audioLatencyMs: 0,
        });
    });

    it('keeps the first value when a stage is marked more than once', () => {
        const timer = createPlaybackTimer(clock([0, 10, 99, 20, 30, 40, 50]));
        timer.mark('resolveEnd');
        timer.mark('resolveEnd');
        timer.mark('fetchStart');
        timer.mark('downloadEnd');
        timer.mark('decodeEnd');
        timer.mark('firstSound');

        expect(timer.report()?.resolveMs).toBe(10);
    });

    it('rounds durations to one decimal place', () => {
        const timer = createPlaybackTimer(clock([0, 10.04, 10.04, 20.06, 30.01, 40.049]));
        timer.mark('resolveEnd');
        timer.mark('fetchStart');
        timer.mark('downloadEnd');
        timer.mark('decodeEnd');
        timer.mark('firstSound');

        const report = timer.report();
        expect(report?.resolveMs).toBe(10);
        expect(report?.downloadMs).toBe(10);
        expect(report?.startupMs).toBe(40);
    });

    it('stores the audio latency and ignores invalid values', () => {
        const timer = createPlaybackTimer(clock([0, 1, 2, 3, 4, 5]));
        timer.mark('resolveEnd');
        timer.mark('fetchStart');
        timer.mark('downloadEnd');
        timer.mark('decodeEnd');
        timer.mark('firstSound');

        timer.setAudioLatency(23.46);
        expect(timer.report()?.audioLatencyMs).toBe(23.5);

        timer.setAudioLatency(Number.NaN);
        expect(timer.report()?.audioLatencyMs).toBe(0);

        timer.setAudioLatency(-5);
        expect(timer.report()?.audioLatencyMs).toBe(0);
    });

    it('registers a performance mark for each new stage', () => {
        const before = performance.getEntriesByName('mindsync:fetchStart').length;
        const timer = createPlaybackTimer(clock([0, 1]));
        timer.mark('fetchStart');
        timer.mark('fetchStart');

        expect(performance.getEntriesByName('mindsync:fetchStart').length).toBe(before + 1);
    });
});

describe('publishPlaybackTiming', () => {
    afterEach(() => {
        delete (window as unknown as { __mindsyncTiming?: unknown }).__mindsyncTiming;
    });

    it('appends each report to the global list', () => {
        const base = { resolveMs: 1, downloadMs: 2, decodeMs: 3, otherMs: 4, startupMs: 10, audioLatencyMs: 0 };

        publishPlaybackTiming(base);
        publishPlaybackTiming({ ...base, startupMs: 20 });

        const stored = (window as unknown as { __mindsyncTiming: { startupMs: number }[] }).__mindsyncTiming;
        expect(stored.map((r) => r.startupMs)).toEqual([10, 20]);
    });
});
