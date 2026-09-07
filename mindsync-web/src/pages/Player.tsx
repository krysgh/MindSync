import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { sessionAudioMixService } from '../services/sessionAudioMixService';
import { useAudioMixEngine } from '../hooks/useAudioMixEngine';
import { useIdleControls } from '../hooks/useIdleControls';
import { ReactiveBackground } from '../components/player/ReactiveBackground';
import { MixerPanel } from '../components/player/MixerPanel';
import { CloseIcon, PauseIcon, PlayIcon, SlidersIcon } from '../components/icons/Icons';
import type { AudioMixLayer } from '../types/audioMix';
import './Player.css';

export function Player() {
    const { sessionId } = useParams<{ sessionId: string }>();
    const navigate = useNavigate();

    const [layers, setLayers] = useState<AudioMixLayer[]>([]);
    const [isResolving, setIsResolving] = useState(true);
    const [resolveError, setResolveError] = useState('');

    useEffect(() => {
        if (!sessionId) return;
        let cancelled = false;

        sessionAudioMixService
            .resolve(sessionId)
            .then((mix) => {
                if (!cancelled) setLayers(mix.layers);
            })
            .catch((err) => {
                if (!cancelled) setResolveError(err instanceof Error ? err.message : 'Falha ao carregar a sessão.');
            })
            .finally(() => {
                if (!cancelled) setIsResolving(false);
            });

        return () => {
            cancelled = true;
        };
    }, [sessionId]);

    const engine = useAudioMixEngine(layers);
    const isControlsVisible = useIdleControls(3000);
    const [isMixerOpen, setIsMixerOpen] = useState(false);

    useEffect(() => {
        if (!engine.isLoading && !engine.error && layers.length > 0 && !engine.isPlaying) {
            engine.play();
        }
    }, [engine.isLoading, engine.error, layers.length]);

    const handleExit = () => navigate('/home');

    if (isResolving) {
        return <div className="player-status-screen">Preparando sua sessão...</div>;
    }

    if (resolveError) {
        return (
            <div className="player-status-screen">
                <p>{resolveError}</p>
                <button className="player-back-btn" onClick={handleExit}>
                    Voltar
                </button>
            </div>
        );
    }

    return (
        <div className="player-page">
            <ReactiveBackground getAnalyser={engine.getAnalyser} />

            <div className="player-content">
                {engine.isLoading && (
                    <>
                        <p className="player-loading-text">Preparando sua sessão...</p>
                        <div className="player-progress-track">
                            <div className="player-progress-fill" style={{ width: `${engine.loadingProgress}%` }} />
                        </div>
                        <span className="player-progress-label">{engine.loadingProgress}%</span>
                    </>
                )}

                {engine.error && <p className="player-error-text">{engine.error}</p>}
            </div>

            <div className={`player-controls ${isControlsVisible || isMixerOpen ? 'is-visible' : ''}`}>
                {!engine.isLoading && !engine.error && (
                    <>
                        <button
                            className="player-icon-btn"
                            onClick={engine.togglePlayPause}
                            title={engine.isPlaying ? 'Pausar' : 'Tocar'}
                        >
                            {engine.isPlaying ? (
                                <PauseIcon width={18} height={18} fill="currentColor" />
                            ) : (
                                <PlayIcon width={18} height={18} fill="currentColor" />
                            )}
                        </button>

                        <button
                            className="player-icon-btn"
                            onClick={() => setIsMixerOpen((prev) => !prev)}
                            title="Ajustar volume das camadas"
                        >
                            <SlidersIcon width={18} height={18} fill="currentColor" />
                        </button>
                    </>
                )}

                <button className="player-icon-btn" onClick={handleExit} title="Sair">
                    <CloseIcon width={18} height={18} fill="currentColor" />
                </button>
            </div>

            {isMixerOpen && (
                <MixerPanel
                    layers={layers}
                    volumes={engine.volumes}
                    onVolumeChange={engine.setVolume}
                    onClose={() => setIsMixerOpen(false)}
                />
            )}
        </div>
    );
}