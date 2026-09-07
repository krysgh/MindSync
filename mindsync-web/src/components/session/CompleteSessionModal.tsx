import { useState, type FormEvent } from 'react';
import { sessionService } from '../../services/sessionService';
import { useDialog } from '../../hooks/useDialog';
import { SparkleIcon } from '.././icons/Icons';
import { Modal } from '.././modal/Modal';
import { ModalActions } from '.././modal/ModalActions';
import { StressRangeInput } from '.././modal/StressRangeInput';

interface CompleteSessionModalProps {
    sessionId: string | null;
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
}

export function CompleteSessionModal({ sessionId, isOpen, onClose, onSuccess }: CompleteSessionModalProps) {
    const [stressAfter, setStressAfter] = useState(2);
    const [isLoading, setIsLoading] = useState(false);
    const { alertUser } = useDialog();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (!sessionId) return;
        setIsLoading(true);

        try {
            await sessionService.complete(sessionId, { stressLevelAfter: stressAfter });
            onSuccess();
            onClose();
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao finalizar a sessão');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <Modal isOpen={isOpen && !!sessionId} size="sm">
            <h3 className="modal-title">
                <SparkleIcon width={16} height={16} fill="currentColor" /> Finalizar Sessão
            </h3>
            <p className="modal-subtitle">Como você se sente agora após a frequência sonora?</p>

            <form onSubmit={handleSubmit} className="modal-form">
                <StressRangeInput
                    label="Estresse Atual"
                    value={stressAfter}
                    onChange={setStressAfter}
                    accentColor="#10b981"
                />

                <ModalActions
                    onCancel={onClose}
                    isLoading={isLoading}
                    loadingLabel="Salvando..."
                    submitLabel="Concluir Avaliação"
                    variant="success"
                />
            </form>
        </Modal>
    );
}