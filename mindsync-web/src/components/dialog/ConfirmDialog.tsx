import { Modal } from '../modal/Modal';

export interface DialogState {
    mode: 'alert' | 'confirm';
    title: string;
    message: string;
    confirmLabel: string;
    cancelLabel: string;
    variant: 'default' | 'danger';
}

interface ConfirmDialogProps {
    state: DialogState;
    onResolve: (result: boolean) => void;
}

export function ConfirmDialog({ state, onResolve }: ConfirmDialogProps) {
    return (
        <Modal isOpen size="sm">
            <h3 className="modal-title">{state.title}</h3>
            <p className="modal-subtitle">{state.message}</p>

            <div className="modal-actions">
                {state.mode === 'confirm' && (
                    <button type="button" className="modal-btn modal-btn-cancel" onClick={() => onResolve(false)}>
                        {state.cancelLabel}
                    </button>
                )}
                <button
                    type="button"
                    className={`modal-btn ${state.variant === 'danger' ? 'modal-btn-danger' : 'modal-btn-primary'}`}
                    onClick={() => onResolve(true)}
                    autoFocus
                >
                    {state.confirmLabel}
                </button>
            </div>
        </Modal>
    );
}