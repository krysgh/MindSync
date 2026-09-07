import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useAuth } from '../hooks/useAuth';
import { useSessionInsights } from '../hooks/useSessionInsights';
import { ChevronLeftIcon, LogoMark } from '../components/icons/Icons';
import { ErrorBanner } from '../components/common/ErrorBanner';
import { SummaryCards } from '../components/insights/SummaryCards';
import { TrendChart } from '../components/insights/TrendChart';
import { FrequencyBreakdown } from '../components/insights/FrequencyBreakdown';
import './Insights.css';

export function Insights() {
    const { userId, isLoading: isAuthLoading } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        if (isAuthLoading) return;
        if (!userId) navigate('/');
    }, [userId, isAuthLoading, navigate]);

    const { isLoading, error, summary, trendSeries, frequencyBreakdown } = useSessionInsights(userId);

    if (isAuthLoading) {
        return <div className="home-loading-screen">Carregando sessão...</div>;
    }

    return (
        <div className="app-page insights-page">
            <motion.div
                className="app-container insights-container"
                initial={{ opacity: 0, y: 14 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.35, ease: 'easeOut' }}
            >
                <header className="insights-topbar">
                    <div className="home-brand">
                        <div className="home-brand-badge">
                            <LogoMark />
                        </div>
                        <span className="home-brand-name">
                            Mind<span className="home-brand-accent">Sync</span>
                        </span>
                    </div>

                    <button className="insights-back-btn" onClick={() => navigate('/home')}>
                        <ChevronLeftIcon width={14} height={14} fill="currentColor" /> Voltar
                    </button>
                </header>

                <main className="insights-main">
                    <h2 className="insights-title">Seu Progresso</h2>
                    <p className="insights-subtitle">
                        Um resumo de como suas sessões de estresse têm evoluído ao longo do tempo.
                    </p>

                    {isLoading && <p className="home-status-text">Calculando suas métricas...</p>}
                    {error && <ErrorBanner message={error} />}

                    {!isLoading && !error && (
                        <>
                            <SummaryCards summary={summary} />

                            <section className="insights-section">
                                <h3 className="insights-section-title">Evolução do estresse</h3>
                                <TrendChart points={trendSeries} />
                            </section>

                            <section className="insights-section">
                                <h3 className="insights-section-title">Qual frequência funciona melhor pra você</h3>
                                <FrequencyBreakdown stats={frequencyBreakdown} />
                            </section>
                        </>
                    )}
                </main>
            </motion.div>
        </div>
    );
}