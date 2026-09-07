import type { ReactNode } from 'react';
import './Modal.css';

interface ModalProps {
    isOpen: boolean;
    size?: 'default' | 'sm';
    children: ReactNode;
}

export function Modal({ isOpen, size = 'default', children }: ModalProps) {
    if (!isOpen) return null;

    return (
        <div className="modal-overlay">
            <div className={`modal-box ${size === 'sm' ? 'modal-box--sm' : ''}`}>{children}</div>
        </div>
    );
}
