import { AnimatePresence, motion } from 'framer-motion';
import { STRESS_LEVELS } from '../../constants/stressLevels';

interface StressRangeInputProps {
    label: string;
    value: number;
    onChange: (value: number) => void;
    accentColor?: string;
}

export function StressRangeInput({ label, value, onChange, accentColor }: StressRangeInputProps) {
    const currentLevel = STRESS_LEVELS[value];
    const trackColor = accentColor ?? currentLevel?.color ?? '#6366f1';

    return (
        <div>
            <label className="modal-field-label">
                {label}:{' '}
                <AnimatePresence mode="wait">
                    <motion.strong
                        key={value}
                        initial={{ opacity: 0, y: -4, scale: 0.9 }}
                        animate={{ opacity: 1, y: 0, scale: 1 }}
                        exit={{ opacity: 0, y: 4, scale: 0.9 }}
                        transition={{ duration: 0.18 }}
                        className="stress-range-value"
                        style={{ color: trackColor }}
                    >
                        {currentLevel?.label} ({value})
                    </motion.strong>
                </AnimatePresence>
            </label>
            <input
                type="range"
                min="0"
                max="8"
                value={value}
                onChange={(e) => onChange(Number(e.target.value))}
                className="modal-range"
                style={{ accentColor: trackColor }}
            />
        </div>
    );
}