import { ErrorIcon } from '../icons/Icons';

interface ErrorBannerProps {
    message: string;
}

export function ErrorBanner({ message }: ErrorBannerProps) {
    return (
        <div className="error-badge app-error-banner">
            <ErrorIcon className="icon-error" />
            <span>{message}</span>
        </div>
    );
}