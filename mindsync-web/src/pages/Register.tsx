import { useState, type FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '../services/authService';
import { AuthCard } from '../components/auth/AuthCard';
import { FormField } from '../components/auth/FormField';
import { PasswordField } from '../components/auth/PasswordField';
import { LockIcon, MailIcon, UserIcon } from '../components/icons/Icons';

export function Register() {
    const navigate = useNavigate();

    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);

    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    const clearErrorOnChange = (setter: (value: string) => void) => (value: string) => {
        setter(value);
        if (error) setError('');
    };

    const handleRegister = async (e: FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        setError('');
        setSuccess('');

        if (password !== confirmPassword) {
            setIsLoading(false);
            setError('As senhas não coincidem.');
            return;
        }

        try {
            await authService.register({ firstName, lastName, email, password });
            setSuccess('Conta criada com sucesso!');
            setTimeout(() => navigate('/'), 1200);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Não foi possível conectar ao servidor.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AuthCard
            subtitle="Crie sua nova conta."
            error={error}
            success={success}
            footer={
                <>
                    <span>Já tem uma conta? </span>
                    <Link to="/" className="link-action">
                        Fazer login
                    </Link>
                </>
            }
        >
            <form onSubmit={handleRegister}>
                <div className="input-row">
                    <FormField
                        id="firstName"
                        label="NOME"
                        icon={<UserIcon className="icon-label" />}
                        type="text"
                        value={firstName}
                        onChange={(e) => clearErrorOnChange(setFirstName)(e.target.value)}
                        placeholder="Seu nome"
                        required
                    />

                    <FormField
                        id="lastName"
                        label="SOBRENOME"
                        icon={<UserIcon className="icon-label" />}
                        type="text"
                        value={lastName}
                        onChange={(e) => clearErrorOnChange(setLastName)(e.target.value)}
                        placeholder="Sobrenome"
                        required
                    />
                </div>

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
                    visible={showPassword}
                    onToggleVisible={() => setShowPassword((prev) => !prev)}
                    onChange={(e) => clearErrorOnChange(setPassword)(e.target.value)}
                    placeholder="••••••••"
                    required
                />

                <FormField
                    id="confirmPassword"
                    label="CONFIRMAR SENHA"
                    icon={<LockIcon className="icon-label" />}
                    type={showPassword ? 'text' : 'password'}
                    value={confirmPassword}
                    onChange={(e) => clearErrorOnChange(setConfirmPassword)(e.target.value)}
                    placeholder="••••••••"
                    required
                />

                <button type="submit" className="btn-login-professional" disabled={isLoading}>
                    {isLoading ? 'Cadastrando...' : 'Criar Conta'}
                </button>
            </form>
        </AuthCard>
    );
}
