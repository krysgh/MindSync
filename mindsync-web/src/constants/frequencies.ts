import { FocusIcon, LeafIcon, MoonIcon, WaveIcon } from '../components/icons/Icons';
import type { ComponentType, SVGProps } from 'react';

export const TARGET_FREQUENCIES = [
    'Delta (2 Hz) - Sono Profundo',
    'Theta (6 Hz) - Meditação',
    'Alpha (10 Hz) - Relaxamento',
    'Beta (20 Hz) - Foco e Atenção',
] as const;

export interface FrequencyOption {
    value: (typeof TARGET_FREQUENCIES)[number];
    shortName: string;
    description: string;
    Icon: ComponentType<SVGProps<SVGSVGElement>>;
}

export const FREQUENCY_OPTIONS: FrequencyOption[] = [
    { value: TARGET_FREQUENCIES[0], shortName: 'Delta', description: 'Sono Profundo', Icon: MoonIcon },
    { value: TARGET_FREQUENCIES[1], shortName: 'Theta', description: 'Meditação', Icon: LeafIcon },
    { value: TARGET_FREQUENCIES[2], shortName: 'Alpha', description: 'Relaxamento', Icon: WaveIcon },
    { value: TARGET_FREQUENCIES[3], shortName: 'Beta', description: 'Foco e Atenção', Icon: FocusIcon },
];