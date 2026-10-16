export function getLocalDateString(dateString?: string): string {
    if (!dateString) return '';
    if (/^\d{4}-\d{2}-\d{2}$/.test(dateString)) return dateString;

    const hasOffset = /[Zz]$|[+-]\d{2}:?\d{2}$/.test(dateString);
    const isoString = dateString.replace(' ', 'T') + (hasOffset ? '' : 'Z');

    const date = new Date(isoString);
    if (isNaN(date.getTime())) return '';

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
}