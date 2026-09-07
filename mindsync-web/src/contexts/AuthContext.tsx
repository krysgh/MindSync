import { useEffect, useState, type ReactNode } from 'react';
import { authService } from '../services/authService';
import { AuthContext } from './auth-context';

export function AuthProvider({ children }: { children: ReactNode }) {
    const [userId, setUserId] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const checkSession = async () => {
            try {
                const userId = await authService.me();
                setUserId(userId);
            } catch {
                setUserId(null);
            } finally {
                setIsLoading(false);
            }
        };

        checkSession();
    }, []);

    const login = (id: string) => setUserId(id);

    const logout = async () => {
        try {
            await authService.logout();
        } finally {
            setUserId(null);
        }
    };

    return (
        <AuthContext.Provider value={{ userId, isLoading, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
}