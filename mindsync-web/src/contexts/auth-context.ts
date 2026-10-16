import { createContext } from 'react';

export interface AuthContextType {
    userId: string | null;
    isLoading: boolean;
    login: (id: string) => void;
    logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType>({} as AuthContextType);
