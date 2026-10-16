import { useCallback, useEffect, useRef, useState } from 'react';
import type { AudioMixLayer } from '../types/audioMix';

interface LayerNodes {
    source: AudioBufferSourceNode;
    gain: GainNode;
}

function concatChunks(chunks: Uint8Array[], totalLength: number): ArrayBuffer {
    const result = new Uint8Array(totalLength);
    let offset = 0;
    for (const chunk of chunks) {
        result.set(chunk, offset);
        offset += chunk.length;
    }
    return result.buffer;
}

export function useAudioMixEngine(layers: AudioMixLayer[]) {
    const [isLoading, setIsLoading] = useState(true);
    const [loadingProgress, setLoadingProgress] = useState(0);
    const [error, setError] = useState('');
    const [isPlaying, setIsPlaying] = useState(false);
    const [volumes, setVolumes] = useState<number[]>([]);

    const audioContextRef = useRef<AudioContext | null>(null);
    const analyserRef = useRef<AnalyserNode | null>(null);
    const buffersRef = useRef<AudioBuffer[]>([]);
    const layerNodesRef = useRef<LayerNodes[]>([]);
    const hasStartedRef = useRef(false);
    const volumesRef = useRef<number[]>([]);

    useEffect(() => {
        if (layers.length === 0) return;
        let cancelled = false;

        const load = async () => {
            setIsLoading(true);
            setLoadingProgress(0);
            setError('');
            hasStartedRef.current = false;

            const bytesLoaded = new Array<number>(layers.length).fill(0);
            const bytesTotal = new Array<number>(layers.length).fill(0);

            const reportProgress = () => {
                const totalKnown = bytesTotal.reduce((sum, t) => sum + t, 0);
                if (totalKnown <= 0) return;
                const loaded = bytesLoaded.reduce((sum, l) => sum + l, 0);
                setLoadingProgress(Math.min(100, Math.round((loaded / totalKnown) * 100)));
            };

            try {
                const context = new AudioContext();
                audioContextRef.current = context;

                const analyser = context.createAnalyser();
                analyser.fftSize = 256;
                analyser.connect(context.destination);
                analyserRef.current = analyser;

                const buffers = await Promise.all(
                    layers.map(async (layer, index) => {
                        let response: Response;
                        try {
                            response = await fetch(layer.audioUrl, { mode: 'cors' });
                        } catch {
                            throw new Error(
                                navigator.onLine
                                    ? `Não foi possível baixar a faixa "${layer.name}". Tente novamente em instantes.`
                                    : 'Você está sem conexão com a internet. Verifique sua rede e tente novamente.'
                            );
                        }

                        if (!response.ok || !response.body) {
                            throw new Error(`Falha ao baixar a faixa "${layer.name}".`);
                        }

                        bytesTotal[index] = Number(response.headers.get('content-length')) || 0;

                        const reader = response.body.getReader();
                        const chunks: Uint8Array[] = [];
                        let received = 0;

                        for (; ;) {
                            const { done, value } = await reader.read();
                            if (done) break;
                            if (value) {
                                chunks.push(value);
                                received += value.length;
                                bytesLoaded[index] = received;
                                reportProgress();
                            }
                        }

                        const arrayBuffer = concatChunks(chunks, received);
                        return context.decodeAudioData(arrayBuffer);
                    })
                );

                if (cancelled) return;
                buffersRef.current = buffers;

                const initialVolumes = layers.map((layer) => layer.volume);
                volumesRef.current = initialVolumes;
                setVolumes(initialVolumes);
                setLoadingProgress(100);
            } catch (err) {
                if (!cancelled) setError(err instanceof Error ? err.message : 'Falha ao preparar o áudio da sessão.');
            } finally {
                if (!cancelled) setIsLoading(false);
            }
        };

        load();

        return () => {
            cancelled = true;
            layerNodesRef.current.forEach(({ source }) => {
                try {
                    source.stop();
                } catch {
                }
            });
            layerNodesRef.current = [];
            audioContextRef.current?.close().catch(() => { });
            audioContextRef.current = null;
        };
    }, [layers]);

    const buildAndStartSources = useCallback(() => {
        const context = audioContextRef.current;
        const analyser = analyserRef.current;
        if (!context || !analyser) return;

        layerNodesRef.current = buffersRef.current.map((buffer, index) => {
            const source = context.createBufferSource();
            source.buffer = buffer;
            source.loop = true;

            const gain = context.createGain();
            gain.gain.value = volumesRef.current[index] ?? layers[index]?.volume ?? 1;

            source.connect(gain);
            gain.connect(analyser);
            source.start(0);

            return { source, gain };
        });
    }, [layers]);

    const play = useCallback(async () => {
        const context = audioContextRef.current;
        if (!context || buffersRef.current.length === 0) return;

        if (context.state === 'suspended') {
            await context.resume();
        }

        if (!hasStartedRef.current) {
            buildAndStartSources();
            hasStartedRef.current = true;
        }

        setIsPlaying(true);
    }, [buildAndStartSources]);

    const pause = useCallback(async () => {
        const context = audioContextRef.current;
        if (!context) return;
        await context.suspend();
        setIsPlaying(false);
    }, []);

    const togglePlayPause = useCallback(() => {
        if (isPlaying) {
            pause();
        } else {
            play();
        }
    }, [isPlaying, pause, play]);

    const setVolume = useCallback((index: number, value: number) => {
        volumesRef.current[index] = value;
        setVolumes((prev) => {
            const next = [...prev];
            next[index] = value;
            return next;
        });

        const node = layerNodesRef.current[index];
        if (node) {
            node.gain.gain.value = value;
        }
    }, []);

    const getAnalyser = useCallback(() => analyserRef.current, []);

    return {
        isLoading,
        loadingProgress,
        error,
        isPlaying,
        play,
        pause,
        togglePlayPause,
        getAnalyser,
        volumes,
        setVolume,
    };
}