import type { InputHTMLAttributes, ReactNode } from 'react';

interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
    id: string;
    label: string;
    icon: ReactNode;
}

export function FormField({ id, label, icon, ...inputProps }: FormFieldProps) {
    return (
        <div className="input-group">
            <label htmlFor={id}>
                {icon}
                {label}
            </label>
            <input id={id} {...inputProps} />
        </div>
    );
}
