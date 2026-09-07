import { useState, type FormEvent } from 'react';
import { sessionService } from '../../services/sessionService';
import { FREQUENCY_OPTIONS, TARGET_FREQUENCIES } from '../../constants/frequencies';
import { useDialog } from '../../hooks/useDialog';
import { Modal } from '.././modal/Modal';
import { ModalActions } from '.././modal/ModalActions';
import { StressRangeInput } from '.././modal/StressRangeInput';
import { FrequencyPicker } from '.././modal/FrequencyPicker';

interface CreateSessionModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
}

export function CreateSessionModal({ isOpen, onClose, onSuccess }: CreateSessionModalProps) {
    const [stressBefore, setStressBefore] = useState(4);
    const [frequency, setFrequency] = useState<string>(TARGET_FREQUENCIES[1]);
    const [isLoading, setIsLoading] = useState(false);
    const { alertUser } = useDialog();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        setIsLoading(true);

        try {
            await sessionService.create({ stressLevelBefore: stressBefore, targetFrequency: frequency });
            onSuccess();
            onClose();
        } catch (err) {
            await alertUser(err instanceof Error ? err.message : 'Falha ao criar sessão');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <Modal isOpen={isOpen}>
            <h3 className="modal-title">Nova Sessão MindSync</h3>

            <form onSubmit={handleSubmit} className="modal-form">
                <StressRangeInput label="Estresse Atual" value={stressBefore} onChange={setStressBefore} />

                <div>
                    <label className="modal-field-label">Frequência Alvo:</label>
                    <FrequencyPicker options={FREQUENCY_OPTIONS} value={frequency} onChange={setFrequency} />
                </div>

                <ModalActions
                    onCancel={onClose}
                    isLoading={isLoading}
                    loadingLabel="Salvando..."
                    submitLabel="Iniciar Sessão"
                />
            </form>
        </Modal>
    );
}