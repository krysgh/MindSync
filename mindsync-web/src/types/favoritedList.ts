export interface FavoritedListSummary {
    id: string;
    name: string;
    createdAt: string;
    totalItems: number;
}

export interface FavoritedSessionItem {
    id: string;
    stressSessionId: string;
    customName: string;
    targetFrequency: string;
    stressLevelBefore: number;
    stressLevelAfter?: number | null;
    favoritedAt: string;
}

export interface FavoritedListDetail {
    id: string;
    userId: string;
    name: string;
    createdAt: string;
    items: FavoritedSessionItem[];
}

export interface CreateFavoritedListPayload {
    userId: string;
    name: string;
}

export interface UpdateFavoritedListPayload {
    id: string;
    name: string;
}

export interface AddFavoritedSessionPayload {
    favoritedListId: string;
    stressSessionId: string;
    customName: string;
}

export interface UpdateFavoritedSessionPayload {
    id: string;
    customName: string;
}

export interface FavoriteEntry {
    listId: string;
    listName: string;
    favoritedSessionId: string;
    customName: string;
}

export interface FavoritesControls {
    lists: FavoritedListSummary[];
    onCreateList: (name: string) => Promise<string>;
    onAddToList: (listId: string, stressSessionId: string, customName: string) => Promise<void>;
    onRemoveFromList: (favoritedSessionId: string) => Promise<void>;
}
