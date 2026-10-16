import { useCallback, useEffect, useMemo, useState } from 'react';
import { favoritedListService } from '../services/favoritedListService';
import type { FavoritedListDetail, FavoritedListSummary, FavoriteEntry } from '../types/favoritedList';

export function useFavoritedLists(userId: string | null) {
    const [lists, setLists] = useState<FavoritedListSummary[]>([]);
    const [detailsByListId, setDetailsByListId] = useState<Record<string, FavoritedListDetail>>({});
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');

    const refresh = useCallback(async () => {
        if (!userId) return;
        setIsLoading(true);
        try {
            const summaries = await favoritedListService.listMine();
            setLists(summaries);

            const results = await Promise.allSettled(summaries.map((list) => favoritedListService.getById(list.id)));
            const detailsMap: Record<string, FavoritedListDetail> = {};
            results.forEach((result) => {
                if (result.status === 'fulfilled' && result.value) {
                    detailsMap[result.value.id] = result.value;
                } else if (result.status === 'rejected') {
                    console.error('Falha ao carregar detalhes de uma lista de favoritos:', result.reason);
                }
            });
            setDetailsByListId(detailsMap);
            setError('');
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Falha ao carregar as listas de favoritos.');
        } finally {
            setIsLoading(false);
        }
    }, [userId]);

    useEffect(() => {
        refresh();
    }, [refresh]);

    const favoritesBySessionId = useMemo(() => {
        const map = new Map<string, FavoriteEntry[]>();
        Object.values(detailsByListId).forEach((list) => {
            list.items.forEach((item) => {
                const entry: FavoriteEntry = {
                    listId: list.id,
                    listName: list.name,
                    favoritedSessionId: item.id,
                    customName: item.customName,
                };
                const existing = map.get(item.stressSessionId) ?? [];
                map.set(item.stressSessionId, [...existing, entry]);
            });
        });
        return map;
    }, [detailsByListId]);

    const createList = useCallback(
        async (name: string): Promise<string> => {
            if (!userId) throw new Error('Usuário não autenticado.');
            const id = await favoritedListService.create({ userId, name });
            await refresh();
            return id;
        },
        [userId, refresh]
    );

    const renameList = useCallback(
        async (id: string, name: string) => {
            await favoritedListService.rename({ id, name });
            await refresh();
        },
        [refresh]
    );

    const deleteList = useCallback(
        async (id: string) => {
            await favoritedListService.remove(id);
            await refresh();
        },
        [refresh]
    );

    const addSessionToList = useCallback(
        async (listId: string, stressSessionId: string, customName: string) => {
            await favoritedListService.addSession(listId, { favoritedListId: listId, stressSessionId, customName });
            await refresh();
        },
        [refresh]
    );

    const removeFavoritedSession = useCallback(
        async (favoritedSessionId: string) => {
            await favoritedListService.removeSession(favoritedSessionId);
            await refresh();
        },
        [refresh]
    );

    const renameFavoritedSession = useCallback(
        async (favoritedSessionId: string, customName: string) => {
            await favoritedListService.renameSession({ id: favoritedSessionId, customName });
            await refresh();
        },
        [refresh]
    );

    return {
        lists,
        detailsByListId,
        favoritesBySessionId,
        isLoading,
        error,
        refresh,
        createList,
        renameList,
        deleteList,
        addSessionToList,
        removeFavoritedSession,
        renameFavoritedSession,
    };
}

export type UseFavoritedListsReturn = ReturnType<typeof useFavoritedLists>;