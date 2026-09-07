import { ChevronLeftIcon, ChevronRightIcon } from '../icons/Icons';

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
}

export function Pagination({ currentPage, totalPages, onPageChange }: PaginationProps) {
    if (totalPages <= 1) return null;

    return (
        <div className="pagination">
            <button
                onClick={() => onPageChange(Math.max(currentPage - 1, 1))}
                disabled={currentPage === 1}
                className="pagination-btn"
            >
                <ChevronLeftIcon width={14} height={14} fill="currentColor" /> Anterior
            </button>

            <span className="pagination-info">
                Página <strong>{currentPage}</strong> de {totalPages}
            </span>

            <button
                onClick={() => onPageChange(Math.min(currentPage + 1, totalPages))}
                disabled={currentPage === totalPages}
                className="pagination-btn"
            >
                Próxima <ChevronRightIcon width={14} height={14} fill="currentColor" />
            </button>
        </div>
    );
}