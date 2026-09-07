import { useState, type FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { authService } from '../services/authService';
import { AuthCard } from '../components/auth/AuthCard';
import { FormField } from '../components/auth/FormField';
import { PasswordField } from '../components/auth/PasswordField';
import { MailIcon } from '../components/icons/Icons';

export function Login() {
    const { login } = useAuth();
    const navigate = useNavigate();

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    const clearErrorOnChange = (setter: (value: string) => void) => (value: string) => {
        setter(value);
        if (error) setError('');
    };

    const handleLogin = async (e: FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        setError('');

        try {
            const userId = await authService.login({ email, password });
            login(userId);
            navigate('/home');
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Erro ao realizar login.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AuthCard
            subtitle="Sintonize sua frequência."
            error={error}
            footer={
                <>
                    <span>Ainda não tem uma conta? </span>
                    <Link to="/register" className="link-action">
                        Cadastre-se
                    </Link>
                </>
            }
        >
            <form onSubmit={handleLogin}>
                <FormField
                    id="email"
                    label="E-MAIL"
                    icon={<MailIcon className="icon-label" />}
                    type="email"
                    value={email}
                    onChange={(e) => clearErrorOnChange(setEmail)(e.target.value)}
                    placeholder="frequencia@mente.com"
                    required
                />

                <PasswordField
                    id="password"
                    label="SENHA"
                    value={password}
                    onChange={(e) => clearErrorOnChange(setPassword)(e.target.value)}
                    placeholder="••••••••"
                    required
                />

                <button type="submit" className="btn-login-professional" disabled={isLoading}>
                    {isLoading ? 'Autenticando...' : 'Entrar'}
                </button>
            </form>
        </AuthCard>
    );
}