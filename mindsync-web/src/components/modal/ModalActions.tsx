interface ModalActionsProps {
    onCancel: () => void;
    isLoading: boolean;
    loadingLabel: string;
    submitLabel: string;
    variant?: 'primary' | 'success';
}

export function ModalActions({ onCancel, isLoading, loadingLabel, submitLabel, variant = 'primary' }: ModalActionsProps) {
    return (
        <div className="modal-actions">
            <button type="button" onClick={onCancel} className="modal-btn modal-btn-cancel">
                Cancelar
            </button>
            <button
                type="submit"
                disabled={isLoading}
                className={`modal-btn ${variant === 'success' ? 'modal-btn-success' : 'modal-btn-primary'}`}
            >
                {isLoading ? loadingLabel : submitLabel}
            </button>
        </div>
    );
}
