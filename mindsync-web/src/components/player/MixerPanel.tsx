import { useRef } from 'react';
import { useClickOutside } from '../../hooks/useClickOutside';
import type { AudioMixLayer } from '../../types/audioMix';
import './MixerPanel.css';

interface MixerPanelProps {
    layers: AudioMixLayer[];
    volumes: number[];
    onVolumeChange: (index: number, value: number) => void;
    onClose: () => void;
}

export function MixerPanel({ layers, volumes, onVolumeChange, onClose }: MixerPanelProps) {
    const panelRef = useRef<HTMLDivElement>(null);
    useClickOutside(panelRef, onClose, true);

    return (
        <div className="mixer-panel" ref={panelRef}>
            <p className="mixer-panel-title">Volume das camadas</p>

            {layers.map((layer, index) => {
                const value = volumes[index] ?? layer.volume;
                return (
                    <div key={layer.audioTrackId} className="mixer-panel-row">
                        <span className="mixer-panel-label">
                            {layer.name}
                            <span className="mixer-panel-label-value">{Math.round(value * 100)}%</span>
                        </span>
                        <input
                            type="range"
                            min={0}
                            max={1}
                            step={0.01}
                            value={value}
                            onChange={(e) => onVolumeChange(index, Number(e.target.value))}
                            style={{ accentColor: '#8b5cf6' }}
                        />
                    </div>
                );
            })}
        </div>
    );
}