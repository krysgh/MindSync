import { WarningIcon } from '../icons/Icons';

interface PendingBannerProps {
    visible: boolean;
}

export function PendingBanner({ visible }: PendingBannerProps) {
    return (
        <div className={`pending-banner-wrapper ${visible ? 'is-visible' : ''}`}>
            <div className="pending-banner-inner">
                <div className="pending-banner">
                    <WarningIcon width={16} height={16} fill="currentColor" />
                    <span>Você possui uma sessão em andamento. Avalie e finalize-a para liberar a criação de novas sessões.</span>
                </div>
            </div>
        </div>
    );
}