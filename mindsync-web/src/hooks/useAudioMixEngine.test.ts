import { act, renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useAudioMixEngine } from './useAudioMixEngine';
import { createPlaybackTimer } from '../utils/playbackTiming';
import type { AudioMixLayer } from '../types/audioMix';

const layers: AudioMixLayer[] = [
    { audioTrackId: 't1', name: 'Binaural', role: 'binaural', audioUrl: 'http://x/a.mp3', volume: 0.8, durationSeconds: 600 },
    { audioTrackId: 't2', name: 'Rain', role: 'texture', audioUrl: 'http://x/b.mp3', volume: 0.5, durationSeconds: 300 },
];

function streamResponse(bytes: number) {
    const body = new ReadableStream<Uint8Array>({
        start(controller) {
            controller.enqueue(new Uint8Array(bytes / 2));
            controller.enqueue(new Uint8Array(bytes / 2));
            controller.close();
        },
    });
    return new Response(body, { status: 200, headers: { 'content-length': String(bytes) } });
}

let started: number;
let created: number;

class FakeAudioContext {
    state = 'running';
    destination = {};
    baseLatency = 0.01;
    outputLatency = 0.02;
    createAnalyser() {
        return { fftSize: 0, connect: vi.fn() };
    }
    createGain() {
        return { gain: { value: 1 }, connect: vi.fn() };
    }
    createBufferSource() {
        created += 1;
        return { buffer: null, loop: false, connect: vi.fn(), start: vi.fn(() => { started += 1; }), stop: vi.fn() };
    }
    decodeAudioData = vi.fn(async () => ({}) as AudioBuffer);
    resume = vi.fn(async () => undefined);
    suspend = vi.fn(async () => undefined);
    close = vi.fn(async () => undefined);
}

describe('useAudioMixEngine', () => {
    beforeEach(() => {
        started = 0;
        created = 0;
        vi.stubGlobal('AudioContext', FakeAudioContext);
        vi.stubGlobal('fetch', vi.fn(async () => streamResponse(1000)));
    });

    afterEach(() => {
        vi.unstubAllGlobals();
    });

    it('downloads and decodes every layer, then stops loading', async () => {
        const { result } = renderHook(() => useAudioMixEngine(layers));

        await waitFor(() => expect(result.current.isLoading).toBe(false));

        expect(fetch).toHaveBeenCalledTimes(2);
        expect(result.current.loadingProgress).toBe(100);
        expect(result.current.volumes).toEqual([0.8, 0.5]);
        expect(result.current.error).toBe('');
    });

    it('starts one looping source per layer when play is called', async () => {
        const { result } = renderHook(() => useAudioMixEngine(layers));
        await waitFor(() => expect(result.current.isLoading).toBe(false));

        await act(async () => {
            await result.current.play();
        });

        expect(created).toBe(2);
        expect(started).toBe(2);
        expect(result.current.isPlaying).toBe(true);
    });

    it('does not rebuild the sources when play is called again after a pause', async () => {
        const { result } = renderHook(() => useAudioMixEngine(layers));
        await waitFor(() => expect(result.current.isLoading).toBe(false));

        await act(async () => {
            await result.current.play();
            await result.current.pause();
            await result.current.play();
        });

        expect(created).toBe(2);
        expect(result.current.isPlaying).toBe(true);
    });

    it('reports an error and stops loading when a download fails', async () => {
        vi.stubGlobal('fetch', vi.fn(async () => new Response(null, { status: 500 })));
        const { result } = renderHook(() => useAudioMixEngine(layers));

        await waitFor(() => expect(result.current.isLoading).toBe(false));

        expect(result.current.error).toContain('Falha ao baixar a faixa');
    });

    it('updates the stored volume of a layer', async () => {
        const { result } = renderHook(() => useAudioMixEngine(layers));
        await waitFor(() => expect(result.current.isLoading).toBe(false));

        act(() => result.current.setVolume(1, 0.2));

        expect(result.current.volumes).toEqual([0.8, 0.2]);
    });

    it('completes the timing report and publishes it once playback starts', async () => {
        const timer = createPlaybackTimer();
        timer.mark('resolveEnd');
        const { result } = renderHook(() => useAudioMixEngine(layers, timer));
        await waitFor(() => expect(result.current.isLoading).toBe(false));

        expect(timer.report()).toBeNull();

        await act(async () => {
            await result.current.play();
        });

        const report = timer.report();
        expect(report).not.toBeNull();
        expect(report!.startupMs).toBeGreaterThanOrEqual(report!.downloadMs);
        expect(report!.audioLatencyMs).toBe(30);
        const published = (window as unknown as { __mindsyncTiming: unknown[] }).__mindsyncTiming;
        expect(published).toHaveLength(1);
    });
});
