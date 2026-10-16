import { useCallback, useRef, useState, type ReactNode } from 'react';
import { DialogContext, type AlertOptions, type ConfirmOptions } from './dialog-context';
import { ConfirmDialog, type DialogState } from '../components/dialog/ConfirmDialog';

export function DialogProvider({ children }: { children: ReactNode }) {
    const [dialogState, setDialogState] = useState<DialogState | null>(null);
    const resolveRef = useRef<((result: boolean) => void) | null>(null);

    const alertUser = useCallback((message: string, options?: AlertOptions): Promise<void> => {
        return new Promise((resolve) => {
            resolveRef.current = () => resolve();
            setDialogState({
                mode: 'alert',
                title: options?.title ?? 'Aviso',
                message,
                confirmLabel: options?.confirmLabel ?? 'OK',
                cancelLabel: '',
                variant: 'default',
            });
        });
    }, []);

    const confirmUser = useCallback((message: string, options?: ConfirmOptions): Promise<boolean> => {
        return new Promise((resolve) => {
            resolveRef.current = resolve;
            setDialogState({
                mode: 'confirm',
                title: options?.title ?? 'Confirmar ação',
                message,
                confirmLabel: options?.confirmLabel ?? 'Confirmar',
                cancelLabel: options?.cancelLabel ?? 'Cancelar',
                variant: options?.variant ?? 'default',
            });
        });
    }, []);

    const handleResolve = (result: boolean) => {
        resolveRef.current?.(result);
        resolveRef.current = null;
        setDialogState(null);
    };

    return (
        <DialogContext.Provider value={{ alertUser, confirmUser }}>
            {children}
            {dialogState && <ConfirmDialog state={dialogState} onResolve={handleResolve} />}
        </DialogContext.Provider>
    );
}