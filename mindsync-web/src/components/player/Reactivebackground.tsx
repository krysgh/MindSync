import { useEffect, useRef } from 'react';
import './ReactiveBackground.css';

interface ReactiveBackgroundProps {
    getAnalyser: () => AnalyserNode | null;
}

const ENTRANCE_START_OPACITY = 0.95;
const RESTING_OPACITY = 0.42;
const ENTRANCE_DURATION_MS = 2000;

export function ReactiveBackground({ getAnalyser }: ReactiveBackgroundProps) {
    const redRef = useRef<HTMLDivElement>(null);
    const blueRef = useRef<HTMLDivElement>(null);
    const purpleRef = useRef<HTMLDivElement>(null);
    const overlayRef = useRef<HTMLDivElement>(null);
    const frameIdRef = useRef<number>(0);
    const mountedAtRef = useRef<number | null>(null);

    useEffect(() => {
        mountedAtRef.current = performance.now();
        const dataArray = new Uint8Array(128);

        const tick = () => {
            const elapsed = performance.now() - (mountedAtRef.current ?? performance.now());
            const entranceProgress = Math.min(1, elapsed / ENTRANCE_DURATION_MS);
            const eased = 1 - Math.pow(1 - entranceProgress, 3);
            const baseOpacity = ENTRANCE_START_OPACITY + (RESTING_OPACITY - ENTRANCE_START_OPACITY) * eased;

            const analyser = getAnalyser();
            let overall = 0;

            if (analyser) {
                analyser.getByteFrequencyData(dataArray);

                const low = boost(averageRange(dataArray, 0, 20));
                const mid = boost(averageRange(dataArray, 20, 60));
                const high = boost(averageRange(dataArray, 60, 120));
                overall = (low + mid + high) / 3;

                applyPulse(redRef.current, low);
                applyPulse(blueRef.current, mid);
                applyPulse(purpleRef.current, high);
            }

            if (overlayRef.current) {
                const opacity = Math.max(0.08, baseOpacity - overall * 0.4);
                overlayRef.current.style.opacity = `${opacity}`;
            }

            frameIdRef.current = requestAnimationFrame(tick);
        };

        frameIdRef.current = requestAnimationFrame(tick);

        return () => cancelAnimationFrame(frameIdRef.current);
    }, [getAnalyser]);

    return (
        <div className="reactive-background">
            <div className="reactive-orbit reactive-orbit-red">
                <div ref={redRef} className="reactive-blob reactive-blob-red" />
            </div>
            <div className="reactive-orbit reactive-orbit-blue">
                <div ref={blueRef} className="reactive-blob reactive-blob-blue" />
            </div>
            <div className="reactive-orbit reactive-orbit-purple">
                <div ref={purpleRef} className="reactive-blob reactive-blob-purple" />
            </div>
            <div ref={overlayRef} className="reactive-darkness-overlay" />
        </div>
    );
}

function averageRange(data: Uint8Array, start: number, end: number): number {
    let sum = 0;
    for (let i = start; i < end; i++) sum += data[i];
    return sum / (end - start) / 255;
}

function boost(intensity: number): number {
    return Math.sqrt(intensity);
}

function applyPulse(el: HTMLDivElement | null, intensity: number) {
    if (!el) return;
    const scale = 0.85 + intensity * 0.5;
    const opacity = 0.5 + intensity * 0.5;
    el.style.transform = `scale(${scale})`;
    el.style.opacity = `${opacity}`;
}