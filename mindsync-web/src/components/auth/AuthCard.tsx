import type { ReactNode } from 'react';
import { motion } from 'framer-motion';
import { ErrorIcon, SuccessIcon } from '../icons/Icons';

interface AuthCardProps {
    subtitle: string;
    error?: string;
    success?: string;
    children: ReactNode;
    footer: ReactNode;
}

export function AuthCard({ subtitle, error, success, children, footer }: AuthCardProps) {
    return (
        <div className="main-glass-container">
            <motion.div
                layoutId="auth-card"
                transition={{ type: 'spring', stiffness: 280, damping: 28 }}
                className={`login-card ${error ? 'card-shake' : ''}`}
            >
                <motion.div
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ duration: 0.25 }}
                >
                    <div className="card-header">
                        <h1>MindSync</h1>
                        <p className="subtitle">{subtitle}</p>
                    </div>

                    <div className={`error-wrapper ${error || success ? 'show' : ''}`}>
                        {error && (
                            <div className="error-badge">
                                <ErrorIcon className="icon-error" />
                                <span>{error}</span>
                            </div>
                        )}
                        {success && (
                            <div className="success-badge">
                                <SuccessIcon className="icon-success" />
                                <span>{success}</span>
                            </div>
                        )}
                    </div>

                    {children}

                    <div className="form-footer">{footer}</div>
                </motion.div>
            </motion.div>
        </div>
    );
}
