import { useEffect, useRef, useState } from 'react';

export function useIdleControls(timeoutMs = 3000) {
    const [isVisible, setIsVisible] = useState(true);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    useEffect(() => {
        const resetTimer = () => {
            setIsVisible(true);
            if (timerRef.current) clearTimeout(timerRef.current);
            timerRef.current = setTimeout(() => setIsVisible(false), timeoutMs);
        };

        resetTimer();
        window.addEventListener('pointerdown', resetTimer);
        window.addEventListener('pointermove', resetTimer);

        return () => {
            window.removeEventListener('pointerdown', resetTimer);
            window.removeEventListener('pointermove', resetTimer);
            if (timerRef.current) clearTimeout(timerRef.current);
        };
    }, [timeoutMs]);

    return isVisible;
}