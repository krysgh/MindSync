import { CalendarIcon, CloseIcon } from '../icons/Icons';

interface SessionToolbarProps {
    selectedDate: string;
    onDateChange: (date: string) => void;
    onCreateSession: () => void;
    createDisabled: boolean;
}

export function SessionToolbar({ selectedDate, onDateChange, onCreateSession, createDisabled }: SessionToolbarProps) {
    return (
        <div className="home-toolbar">
            <h3 className="home-toolbar-title">Minhas Sessões</h3>

            <div className="home-toolbar-actions">
                <div className="date-filter">
                    <CalendarIcon className="date-filter-icon" width={14} height={14} fill="currentColor" />
                    <input
                        type="date"
                        value={selectedDate}
                        onChange={(e) => onDateChange(e.target.value)}
                        className="date-filter-input"
                    />
                    {selectedDate && (
                        <button onClick={() => onDateChange('')} title="Limpar filtro" className="date-filter-clear">
                            <CloseIcon width={11} height={11} fill="currentColor" />
                        </button>
                    )}
                </div>

                <button
                    onClick={onCreateSession}
                    disabled={createDisabled}
                    title={createDisabled ? 'Finalize a sessão pendente para criar uma nova' : ''}
                    className="new-session-btn"
                >
                    + Nova Sessão
                </button>
            </div>
        </div>
    );
}