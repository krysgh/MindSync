import { createContext } from 'react';

export interface AlertOptions {
    title?: string;
    confirmLabel?: string;
}

export interface ConfirmOptions {
    title?: string;
    confirmLabel?: string;
    cancelLabel?: string;
    variant?: 'default' | 'danger';
}

export interface DialogContextType {
    alertUser: (message: string, options?: AlertOptions) => Promise<void>;
    confirmUser: (message: string, options?: ConfirmOptions) => Promise<boolean>;
}

export const DialogContext = createContext<DialogContextType>({} as DialogContextType);