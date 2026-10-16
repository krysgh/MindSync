import { motion } from 'framer-motion';
import type { FrequencyOption } from '../../constants/frequencies';

interface FrequencyPickerProps {
    options: FrequencyOption[];
    value: string;
    onChange: (value: string) => void;
}

export function FrequencyPicker({ options, value, onChange }: FrequencyPickerProps) {
    return (
        <div className="frequency-picker">
            {options.map((option) => {
                const isSelected = option.value === value;

                return (
                    <button
                        key={option.value}
                        type="button"
                        className={`frequency-picker-option ${isSelected ? 'is-selected' : ''}`}
                        onClick={() => onChange(option.value)}
                    >
                        {isSelected && (
                            <motion.div
                                layoutId="frequency-picker-highlight"
                                className="frequency-picker-highlight"
                                transition={{ type: 'spring', stiffness: 380, damping: 32 }}
                            />
                        )}

                        <option.Icon className="frequency-picker-icon" width={22} height={22} fill="currentColor" />
                        <span className="frequency-picker-name">{option.shortName}</span>
                        <span className="frequency-picker-description">{option.description}</span>
                    </button>
                );
            })}
        </div>
    );
}