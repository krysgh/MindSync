import { useState, type InputHTMLAttributes } from 'react';
import { EyeClosedIcon, EyeOpenIcon, LockIcon } from '../icons/Icons';

interface PasswordFieldProps extends InputHTMLAttributes<HTMLInputElement> {
    id: string;
    label: string;
    visible?: boolean;
    onToggleVisible?: () => void;
}

export function PasswordField({ id, label, visible, onToggleVisible, ...inputProps }: PasswordFieldProps) {
    const [internalVisible, setInternalVisible] = useState(false);
    const isVisible = visible ?? internalVisible;
    const toggleVisible = onToggleVisible ?? (() => setInternalVisible((prev) => !prev));

    return (
        <div className="input-group">
            <label htmlFor={id}>
                <LockIcon className="icon-label" />
                {label}
            </label>
            <div className="password-input-wrapper">
                <input id={id} type={isVisible ? 'text' : 'password'} {...inputProps} />
                <button
                    type="button"
                    className="toggle-password-btn"
                    onClick={toggleVisible}
                    title={isVisible ? 'Ocultar senha' : 'Ver senha'}
                >
                    {isVisible ? <EyeOpenIcon className="icon-eye" /> : <EyeClosedIcon className="icon-eye" />}
                </button>
            </div>
        </div>
    );
}
